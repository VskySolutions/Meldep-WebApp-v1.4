using System;
using System.Collections.Generic;
using System.Linq;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.InfraAccounts;

namespace Vsky.Services.InfraDashboard
{
    // Title: InfraDashboardAggregationService
    // Description: Server-side financial aggregation for the Infrastructure Dashboard. Normalizes recurring service
    // prices (from the latest price-history record) into monthly-equivalent and annualized values, builds grouped
    // "Cost by" reports with percentage distribution, reads price history for change/trend reporting, and derives
    // chargeability, missing-billing-data, and renewal exception lists. Read-only; performs no writes.
    //
    // Cost normalization contract (per feature blueprint):
    //   monthly service  -> monthlyEquivalent = price,      annualized = price * 12
    //   yearly service   -> monthlyEquivalent = price / 12, annualized = price
    //   one-time service -> reported separately (excluded from recurring monthly/annualized totals)
    public class InfraDashboardAggregationService : IInfraDashboardAggregationService
    {
        #region Define Services
        private readonly IRepository<InfraAccountServices> _infraAccountServicesRepository;
        private readonly IInfraAccountServiceCalculationService _calculationService;
        #endregion

        #region Services Initializations
        public InfraDashboardAggregationService(
            IRepository<InfraAccountServices> infraAccountServicesRepository,
            IInfraAccountServiceCalculationService calculationService)
        {
            _infraAccountServicesRepository = infraAccountServicesRepository;
            _calculationService = calculationService;
        }
        #endregion

        #region Internal Types
        // Normalized, in-memory view of a single active account service used by every dashboard report.
        private class ServiceRow
        {
            public string ServiceId { get; set; }
            public string ServiceName { get; set; }
            public string AccountId { get; set; }
            public string AccountName { get; set; }
            public string CustomerId { get; set; }
            public string ProviderId { get; set; }
            public string ProviderLabel { get; set; }
            public string ItemTypeId { get; set; }
            public string ItemTypeLabel { get; set; }
            public string OwnershipId { get; set; }
            public string OwnershipLabel { get; set; }
            public string PaymentTermId { get; set; }
            public string PaymentTermValue { get; set; }   // lowercased: monthly / yearly / one-time
            public string PaymentTermLabel { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public List<string> ProjectIds { get; set; } = new List<string>();
            public List<string> ProjectNames { get; set; } = new List<string>();
            public List<InfraAccountServicesPriceHistory> PriceHistory { get; set; } = new List<InfraAccountServicesPriceHistory>();

            public decimal CurrentPrice { get; set; }
            public decimal? PreviousPrice { get; set; }
            public DateTime? LatestPriceStart { get; set; }
            public decimal MonthlyEquivalent { get; set; }  // raw (unrounded); recurring only
            public decimal Annualized { get; set; }         // raw (unrounded); recurring only
            public decimal OneTime { get; set; }            // raw (unrounded); one-time only
            public decimal Ytd { get; set; }                // per-service YTD via CalculateYTD (matches Account Services list)
            public bool IsRecurring { get; set; }
            public bool IsOneTime { get; set; }
        }
        #endregion

        #region Constants
        private const string Unassigned = "Unassigned";
        private const string NotSet = "Not Set";
        private const int DefaultUpcomingDays = 30;
        private const int MaxHistoryMonths = 60;
        #endregion

        #region Load / Normalize
        // Loads the filtered, site-scoped set of active (non-deleted) account services and computes normalized cost
        // values. Site scoping mirrors the existing services list: services carry no SiteId, so they are scoped
        // transitively through their (non-deleted) parent account.
        private List<ServiceRow> LoadServices(string siteId, InfraDashboardFilter filter)
        {
            filter ??= new InfraDashboardFilter();

            var query = _infraAccountServicesRepository.TableNoTracking
                .Where(x => !x.Deleted && x.InfraAccount.SiteId == siteId && !x.InfraAccount.Deleted);

            if (filter.InfraAccountIds?.Any() == true)
                query = query.Where(x => filter.InfraAccountIds.Contains(x.InfraAccountId));
            if (filter.ProviderIds?.Any() == true)
                query = query.Where(x => filter.ProviderIds.Contains(x.InfraAccount.ProviderId));
            if (filter.ItemTypeIds?.Any() == true)
                query = query.Where(x => filter.ItemTypeIds.Contains(x.ItemTypeId));
            if (filter.OwnerShipTypeIds?.Any() == true)
                query = query.Where(x => filter.OwnerShipTypeIds.Contains(x.OwnerShipTypeId));
            if (filter.PaymentTermIds?.Any() == true)
                query = query.Where(x => filter.PaymentTermIds.Contains(x.PaymentTermId));
            if (filter.CustomerIds?.Any() == true)
                query = query.Where(x => filter.CustomerIds.Contains(x.InfraAccount.CustomerId));
            if (filter.ProjectIds?.Any() == true)
                query = query.Where(x => x.InfraProjectServices.Any(p => !p.Deleted && filter.ProjectIds.Contains(p.InfraProjectId)));

            var raw = query.Select(x => new
            {
                ServiceId = x.Id,
                ServiceName = x.Name,
                AccountId = x.InfraAccountId,
                AccountName = x.InfraAccount.Name,
                CustomerId = x.InfraAccount.CustomerId,
                ProviderId = x.InfraAccount.ProviderId,
                ProviderText = x.InfraAccount.Provider.DropDownText,
                ProviderValue = x.InfraAccount.Provider.DropDownValue,
                ItemTypeId = x.ItemTypeId,
                ItemTypeText = x.ItemType.DropDownText,
                ItemTypeValue = x.ItemType.DropDownValue,
                OwnershipId = x.OwnerShipTypeId,
                OwnershipText = x.OwnerShipType.DropDownText,
                OwnershipValue = x.OwnerShipType.DropDownValue,
                PaymentTermId = x.PaymentTermId,
                PaymentTermText = x.PaymentTerm.DropDownText,
                PaymentTermValue = x.PaymentTerm.DropDownValue,
                x.StartDate,
                x.EndDate,
                Projects = x.InfraProjectServices.Where(p => !p.Deleted)
                    .Select(p => new { p.InfraProjectId, ProjectName = p.Project.Name }).ToList(),
                PriceHistory = x.InfraAccountServicesPriceHistory.Where(ph => !ph.Deleted)
                    .Select(ph => new { ph.Id, ph.Price, ph.StartDate, ph.EndDate, ph.TotalPrice }).ToList()
            }).ToList();

            var rows = new List<ServiceRow>(raw.Count);
            foreach (var r in raw)
            {
                var cycles = r.PriceHistory
                    .Select(p => new InfraAccountServicesPriceHistory
                    {
                        Id = p.Id,
                        Price = p.Price,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        TotalPrice = p.TotalPrice
                    })
                    .OrderBy(p => p.StartDate)
                    .ToList();

                var latest = cycles.Count > 0 ? cycles[cycles.Count - 1] : null;
                var previous = cycles.Count >= 2 ? cycles[cycles.Count - 2] : null;
                decimal currentPrice = latest?.Price ?? 0m;
                var ytd = _calculationService.CalculateYTD(cycles);

                var termValue = (r.PaymentTermValue ?? string.Empty).Trim().ToLowerInvariant();

                decimal me = 0m, ann = 0m, ot = 0m;
                bool isRecurring = false, isOneTime = false;
                switch (termValue)
                {
                    case "monthly":
                        me = currentPrice; ann = currentPrice * 12m; isRecurring = true; break;
                    case "yearly":
                        me = currentPrice / 12m; ann = currentPrice; isRecurring = true; break;
                    case "one-time":
                        ot = currentPrice; isOneTime = true; break;
                    default:
                        break; // unclassified payment term -> excluded from recurring & one-time totals
                }

                rows.Add(new ServiceRow
                {
                    ServiceId = r.ServiceId,
                    ServiceName = r.ServiceName,
                    AccountId = r.AccountId,
                    AccountName = r.AccountName,
                    CustomerId = r.CustomerId,
                    ProviderId = r.ProviderId,
                    ProviderLabel = Label(r.ProviderText, r.ProviderValue, Unassigned),
                    ItemTypeId = r.ItemTypeId,
                    ItemTypeLabel = Label(r.ItemTypeText, r.ItemTypeValue, NotSet),
                    OwnershipId = r.OwnershipId,
                    OwnershipLabel = Label(r.OwnershipText, r.OwnershipValue, NotSet),
                    PaymentTermId = r.PaymentTermId,
                    PaymentTermValue = termValue,
                    PaymentTermLabel = Label(r.PaymentTermText, r.PaymentTermValue, NotSet),
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    ProjectIds = r.Projects.Select(p => p.InfraProjectId).ToList(),
                    ProjectNames = r.Projects.Select(p => p.ProjectName).ToList(),
                    PriceHistory = cycles,
                    CurrentPrice = currentPrice,
                    PreviousPrice = previous?.Price,
                    LatestPriceStart = latest?.StartDate,
                    MonthlyEquivalent = me,
                    Annualized = ann,
                    OneTime = ot,
                    Ytd = ytd,
                    IsRecurring = isRecurring,
                    IsOneTime = isOneTime
                });
            }

            return rows;
        }
        #endregion

        #region Executive Summary
        public InfraDashboardSummary GetSummary(string siteId, InfraDashboardFilter filter)
        {
            var rows = LoadServices(siteId, filter);
            return new InfraDashboardSummary
            {
                MonthlyEquivalentTotal = Round(rows.Sum(r => r.MonthlyEquivalent)),
                AnnualizedTotal = Round(rows.Sum(r => r.Annualized)),
                OneTimeTotal = Round(rows.Sum(r => r.OneTime)),
                YtdTotal = Round(rows.Sum(r => r.Ytd)),
                AccountCount = rows.Select(r => r.AccountId).Distinct().Count(),
                ServiceCount = rows.Count,
                RecurringServiceCount = rows.Count(r => r.IsRecurring),
                OneTimeServiceCount = rows.Count(r => r.IsOneTime)
            };
        }
        #endregion

        #region Cost Breakdowns
        public InfraDashboardBreakdowns GetBreakdowns(string siteId, InfraDashboardFilter filter)
        {
            var rows = LoadServices(siteId, filter);
            return new InfraDashboardBreakdowns
            {
                ByProvider = SingleKeyBreakdown("provider", "Cost by Provider", rows,
                    r => r.ProviderId, r => r.ProviderLabel, Unassigned),
                ByItemType = SingleKeyBreakdown("itemType", "Cost by Service Type", rows,
                    r => r.ItemTypeId, r => r.ItemTypeLabel, NotSet),
                ByOwnershipType = SingleKeyBreakdown("ownershipType", "Cost by Ownership Type", rows,
                    r => r.OwnershipId, r => r.OwnershipLabel, NotSet)
            };
        }

        // Groups recurring services by a single dimension key. One-time services (annualized == 0) are excluded from
        // recurring breakdowns; they are surfaced separately via the summary one-time total.
        private InfraDashboardBreakdown SingleKeyBreakdown(
            string dimension, string title, List<ServiceRow> rows,
            Func<ServiceRow, string> keySelector, Func<ServiceRow, string> labelSelector, string unassignedLabel)
        {
            var recurring = rows.Where(r => r.IsRecurring);

            var items = recurring
                .GroupBy(r =>
                {
                    var k = keySelector(r);
                    return string.IsNullOrWhiteSpace(k) ? string.Empty : k;
                })
                .Select(g => new InfraDashboardBreakdownItem
                {
                    Key = g.Key,
                    Label = string.IsNullOrWhiteSpace(g.Key)
                        ? unassignedLabel
                        : DisplayOrDefault(labelSelector(g.First()), unassignedLabel),
                    MonthlyEquivalent = Round(g.Sum(x => x.MonthlyEquivalent)),
                    Annualized = Round(g.Sum(x => x.Annualized)),
                    Ytd = Round(g.Sum(x => x.Ytd)),
                    ServiceCount = g.Count()
                })
                .ToList();

            return Finalize(dimension, title, items);
        }

        // Computes each item's percentage share of the breakdown total (the selected/filtered total, never a global
        // unfiltered total), sorts by amount desc, and produces chart-ready parallel Labels/Series arrays.
        private static InfraDashboardBreakdown Finalize(string dimension, string title, List<InfraDashboardBreakdownItem> items)
        {
            var total = items.Sum(i => i.Annualized);
            foreach (var i in items)
                i.Percentage = total > 0 ? Round(i.Annualized / total * 100m) : 0m;

            items = items.OrderByDescending(i => i.Annualized).ThenBy(i => i.Label).ToList();

            return new InfraDashboardBreakdown
            {
                Dimension = dimension,
                Title = title,
                Total = Round(total),
                YtdTotal = Round(items.Sum(i => i.Ytd)),
                Items = items,
                Labels = items.Select(i => i.Label).ToList(),
                Series = items.Select(i => i.Annualized).ToList(),
                SeriesYtd = items.Select(i => i.Ytd).ToList()
            };
        }
        #endregion

        #region Price Changes
        public List<InfraDashboardPriceChange> GetPriceChanges(string siteId, InfraDashboardFilter filter)
        {
            var rows = LoadServices(siteId, filter);
            var result = new List<InfraDashboardPriceChange>();

            foreach (var r in rows)
            {
                if (r.PriceHistory.Count < 2) continue;

                var current = r.PriceHistory[r.PriceHistory.Count - 1];
                var previous = r.PriceHistory[r.PriceHistory.Count - 2];
                var abs = current.Price - previous.Price;
                if (abs == 0m) continue;

                decimal pct;
                if (previous.Price != 0m)
                    pct = Round(abs / previous.Price * 100m);
                else
                    pct = abs > 0m ? 100m : 0m;

                result.Add(new InfraDashboardPriceChange
                {
                    ServiceId = r.ServiceId,
                    ServiceName = r.ServiceName,
                    AccountName = r.AccountName,
                    ProviderLabel = r.ProviderLabel,
                    PaymentTermLabel = r.PaymentTermLabel,
                    PreviousPrice = Round(previous.Price),
                    CurrentPrice = Round(current.Price),
                    AbsoluteChange = Round(abs),
                    PercentageChange = pct,
                    ChangedOn = current.StartDate,
                    Direction = abs > 0m ? "increase" : "decrease"
                });
            }

            return result
                .OrderByDescending(x => x.ChangedOn)
                .ThenByDescending(x => Math.Abs(x.AbsoluteChange))
                .ToList();
        }
        #endregion

        #region Cost History / Trend
        public InfraDashboardHistory GetHistory(string siteId, InfraDashboardFilter filter)
        {
            filter ??= new InfraDashboardFilter();
            var rows = LoadServices(siteId, filter);

            var to = (filter.ToDate ?? DateTime.UtcNow).Date;
            var from = (filter.FromDate ?? to.AddMonths(-11)).Date;
            if (from > to) from = to;

            var cursor = new DateTime(from.Year, from.Month, 1);
            var lastMonth = new DateTime(to.Year, to.Month, 1);

            // Safety cap: never iterate more than MaxHistoryMonths buckets.
            var monthSpan = ((lastMonth.Year - cursor.Year) * 12) + (lastMonth.Month - cursor.Month);
            if (monthSpan >= MaxHistoryMonths)
                cursor = lastMonth.AddMonths(-(MaxHistoryMonths - 1));

            var result = new InfraDashboardHistory { FromDate = cursor, ToDate = lastMonth };

            while (cursor <= lastMonth)
            {
                var monthEnd = cursor.AddMonths(1).AddDays(-1);
                decimal meSum = 0m, otSum = 0m;

                foreach (var r in rows)
                {
                    if (r.IsRecurring)
                    {
                        if (r.StartDate.Date > monthEnd) continue;

                        // Price in effect at month end = the most recent price cycle that had started by then.
                        var cycle = r.PriceHistory
                            .Where(p => p.StartDate.Date <= monthEnd)
                            .OrderByDescending(p => p.StartDate)
                            .FirstOrDefault()
                            ?? r.PriceHistory.OrderBy(p => p.StartDate).FirstOrDefault();

                        if (cycle == null) continue;

                        if (r.PaymentTermValue == "monthly") meSum += cycle.Price;
                        else if (r.PaymentTermValue == "yearly") meSum += cycle.Price / 12m;
                    }
                    else if (r.IsOneTime)
                    {
                        if (r.StartDate.Year == cursor.Year && r.StartDate.Month == cursor.Month)
                            otSum += r.CurrentPrice;
                    }
                }

                result.Points.Add(new InfraDashboardHistoryPoint
                {
                    Label = cursor.ToString("MMM yyyy"),
                    PeriodStart = cursor,
                    MonthlyEquivalent = Round(meSum),
                    OneTime = Round(otSum)
                });

                cursor = cursor.AddMonths(1);
            }

            result.Labels = result.Points.Select(p => p.Label).ToList();
            result.Series = result.Points.Select(p => p.MonthlyEquivalent).ToList();
            result.RangeRecurringTotal = Round(result.Points.Sum(p => p.MonthlyEquivalent));
            result.RangeOneTimeTotal = Round(result.Points.Sum(p => p.OneTime));
            return result;
        }
        #endregion

        #region Data Quality
        public InfraDashboardDataQuality GetDataQuality(string siteId, InfraDashboardFilter filter)
        {
            var rows = LoadServices(siteId, filter);
            var result = new InfraDashboardDataQuality();

            foreach (var r in rows)
            {
                var missingCustomer = string.IsNullOrWhiteSpace(r.CustomerId);
                var missingProject = r.ProjectIds == null || r.ProjectIds.Count == 0;
                var missingOwnership = string.IsNullOrWhiteSpace(r.OwnershipId);
                var missingPaymentTerm = string.IsNullOrWhiteSpace(r.PaymentTermId)
                    || (r.PaymentTermValue != "monthly" && r.PaymentTermValue != "yearly" && r.PaymentTermValue != "one-time");
                var missingPrice = r.CurrentPrice <= 0m;
                var missingEndDate = r.IsRecurring && r.EndDate == null;

                if (!(missingCustomer || missingProject || missingOwnership || missingPaymentTerm || missingPrice || missingEndDate))
                    continue;

                var missingFields = new List<string>();
                if (missingCustomer) { missingFields.Add("Client / Customer"); result.MissingCustomerCount++; }
                if (missingProject) { missingFields.Add("Project"); result.MissingProjectCount++; }
                if (missingOwnership) { missingFields.Add("Ownership Type"); result.MissingOwnershipTypeCount++; }
                if (missingPaymentTerm) { missingFields.Add("Payment Term"); result.MissingPaymentTermCount++; }
                if (missingPrice) { missingFields.Add("Price"); result.MissingPriceCount++; }
                if (missingEndDate) { missingFields.Add("End Date"); result.MissingEndDateCount++; }

                result.Items.Add(new InfraDashboardDataQualityItem
                {
                    ServiceId = r.ServiceId,
                    ServiceName = r.ServiceName,
                    AccountName = r.AccountName,
                    MissingFields = missingFields,
                    MissingCustomer = missingCustomer,
                    MissingProject = missingProject,
                    MissingOwnershipType = missingOwnership,
                    MissingPaymentTerm = missingPaymentTerm,
                    MissingPrice = missingPrice,
                    MissingEndDate = missingEndDate
                });
            }

            result.TotalFlagged = result.Items.Count;
            result.Items = result.Items.OrderByDescending(i => i.MissingFields.Count).ThenBy(i => i.ServiceName).ToList();
            return result;
        }
        #endregion

        #region Renewals
        public InfraDashboardRenewals GetRenewals(string siteId, InfraDashboardFilter filter)
        {
            filter ??= new InfraDashboardFilter();
            var rows = LoadServices(siteId, filter);

            var window = filter.UpcomingDays.GetValueOrDefault(DefaultUpcomingDays);
            if (window <= 0) window = DefaultUpcomingDays;

            var today = DateTime.UtcNow.Date;
            var windowEnd = today.AddDays(window);

            var upcoming = rows
                .Where(r => r.EndDate.HasValue && r.EndDate.Value.Date >= today && r.EndDate.Value.Date <= windowEnd)
                .OrderBy(r => r.EndDate)
                .Select(r => ToRenewalItem(r, today))
                .ToList();

            var recurringWithoutEndDate = rows
                .Where(r => r.IsRecurring && r.EndDate == null)
                .OrderBy(r => r.AccountName).ThenBy(r => r.ServiceName)
                .Select(r => ToRenewalItem(r, today))
                .ToList();

            return new InfraDashboardRenewals
            {
                UpcomingDays = window,
                WindowStart = today,
                WindowEnd = windowEnd,
                Upcoming = upcoming,
                RecurringWithoutEndDate = recurringWithoutEndDate
            };
        }

        private static InfraDashboardRenewalItem ToRenewalItem(ServiceRow r, DateTime today)
        {
            return new InfraDashboardRenewalItem
            {
                ServiceId = r.ServiceId,
                ServiceName = r.ServiceName,
                AccountName = r.AccountName,
                CustomerLabel = DisplayOrDefault(r.CustomerId, Unassigned),
                ProviderLabel = r.ProviderLabel,
                PaymentTermLabel = r.PaymentTermLabel,
                EndDate = r.EndDate,
                DaysUntilRenewal = r.EndDate.HasValue ? (int?)(r.EndDate.Value.Date - today).Days : null,
                CurrentPrice = Round(r.CurrentPrice),
                Annualized = Round(r.Annualized)
            };
        }
        #endregion

        #region Helpers
        private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        private static string Label(string text, string value, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(text)) return text;
            if (!string.IsNullOrWhiteSpace(value)) return value;
            return fallback;
        }

        private static string DisplayOrDefault(string value, string fallback)
            => string.IsNullOrWhiteSpace(value) ? fallback : value;
        #endregion
    }
}
