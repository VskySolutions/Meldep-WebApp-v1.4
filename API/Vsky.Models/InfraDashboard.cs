using System;
using System.Collections.Generic;

namespace Vsky.Models
{
    #region Filter

    // Title: InfraDashboardFilter
    // Description: Shared filter applied to every Infrastructure Dashboard aggregation. Bound from the query string
    // on the controller and passed through to the aggregation service. All dashboard results are additionally
    // site-scoped by the controller-provided SiteId.
    public class InfraDashboardFilter
    {
        public List<string> InfraAccountIds { get; set; }
        public List<string> ProviderIds { get; set; }
        public List<string> ProjectIds { get; set; }
        public List<string> CustomerIds { get; set; }
        public List<string> ItemTypeIds { get; set; }
        public List<string> OwnerShipTypeIds { get; set; }
        public List<string> PaymentTermIds { get; set; }

        // History (Cost trend / historical totals) date range.
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Upcoming renewal window, in days.
        public int? UpcomingDays { get; set; }
    }

    #endregion

    #region Executive Summary

    public class InfraDashboardSummary
    {
        public decimal MonthlyEquivalentTotal { get; set; }
        public decimal AnnualizedTotal { get; set; }
        public decimal OneTimeTotal { get; set; }
        public decimal YtdTotal { get; set; }
        public int AccountCount { get; set; }
        public int ServiceCount { get; set; }
        public int RecurringServiceCount { get; set; }
        public int OneTimeServiceCount { get; set; }
    }

    #endregion

    #region Cost Breakdowns

    public class InfraDashboardBreakdownItem
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public decimal MonthlyEquivalent { get; set; }
        public decimal Annualized { get; set; }
        public decimal Ytd { get; set; }
        public decimal Percentage { get; set; }
        public int ServiceCount { get; set; }
    }

    // Title: InfraDashboardBreakdown
    // Description: One grouped "Cost by" report. Total is the sum of the group amounts under the current filter
    // (the "selected total"); Percentage on each item is that group's share of Total. Labels/Series are chart-ready
    // parallel arrays (annualized amounts) for pie/donut and bar rendering.
    public class InfraDashboardBreakdown
    {
        public string Dimension { get; set; }
        public string Title { get; set; }
        public decimal Total { get; set; }
        public decimal YtdTotal { get; set; }
        public List<InfraDashboardBreakdownItem> Items { get; set; } = new List<InfraDashboardBreakdownItem>();
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Series { get; set; } = new List<decimal>();
        public List<decimal> SeriesYtd { get; set; } = new List<decimal>();
    }

    public class InfraDashboardBreakdowns
    {
        public InfraDashboardBreakdown ByProvider { get; set; }
        public InfraDashboardBreakdown ByItemType { get; set; }
        public InfraDashboardBreakdown ByOwnershipType { get; set; }
    }

    #endregion

    #region Price Changes

    public class InfraDashboardPriceChange
    {
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string AccountName { get; set; }
        public string ProviderLabel { get; set; }
        public string PaymentTermLabel { get; set; }
        public decimal PreviousPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal AbsoluteChange { get; set; }
        public decimal PercentageChange { get; set; }
        public DateTime ChangedOn { get; set; }
        public string Direction { get; set; }
    }

    #endregion

    #region Cost History / Trend

    public class InfraDashboardHistoryPoint
    {
        public string Label { get; set; }
        public DateTime PeriodStart { get; set; }
        public decimal MonthlyEquivalent { get; set; }
        public decimal OneTime { get; set; }
    }

    // Title: InfraDashboardHistory
    // Description: Monthly recurring-cost trend across the selected date range, derived from price history records
    // active during each month. Series (parallel to Labels) is the monthly equivalent recurring burden per month.
    public class InfraDashboardHistory
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal RangeRecurringTotal { get; set; }
        public decimal RangeOneTimeTotal { get; set; }
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Series { get; set; } = new List<decimal>();
        public List<InfraDashboardHistoryPoint> Points { get; set; } = new List<InfraDashboardHistoryPoint>();
    }

    #endregion

    #region Data Quality (Missing Billing Data)

    public class InfraDashboardDataQualityItem
    {
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string AccountName { get; set; }
        public List<string> MissingFields { get; set; } = new List<string>();
        public bool MissingCustomer { get; set; }
        public bool MissingProject { get; set; }
        public bool MissingOwnershipType { get; set; }
        public bool MissingPaymentTerm { get; set; }
        public bool MissingPrice { get; set; }
        public bool MissingEndDate { get; set; }
    }

    public class InfraDashboardDataQuality
    {
        public int TotalFlagged { get; set; }
        public int MissingCustomerCount { get; set; }
        public int MissingProjectCount { get; set; }
        public int MissingOwnershipTypeCount { get; set; }
        public int MissingPaymentTermCount { get; set; }
        public int MissingPriceCount { get; set; }
        public int MissingEndDateCount { get; set; }
        public List<InfraDashboardDataQualityItem> Items { get; set; } = new List<InfraDashboardDataQualityItem>();
    }

    #endregion

    #region Renewals

    public class InfraDashboardRenewalItem
    {
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string AccountName { get; set; }
        public string CustomerLabel { get; set; }
        public string ProviderLabel { get; set; }
        public string PaymentTermLabel { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysUntilRenewal { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal Annualized { get; set; }
    }

    public class InfraDashboardRenewals
    {
        public int UpcomingDays { get; set; }
        public DateTime WindowStart { get; set; }
        public DateTime WindowEnd { get; set; }
        public List<InfraDashboardRenewalItem> Upcoming { get; set; } = new List<InfraDashboardRenewalItem>();
        public List<InfraDashboardRenewalItem> RecurringWithoutEndDate { get; set; } = new List<InfraDashboardRenewalItem>();
    }

    #endregion
}
