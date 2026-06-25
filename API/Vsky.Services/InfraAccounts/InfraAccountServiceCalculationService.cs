using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.InfraAccounts
{
    public class InfraAccountServiceCalculationService : IInfraAccountServiceCalculationService
    {
        #region Define Services
        private readonly IRepository<InfraAccountServicesPriceHistory> _infraAccountServicesPriceHistoryRepository;
        #endregion

        #region Services Initializations
        public InfraAccountServiceCalculationService(
            IRepository<InfraAccountServicesPriceHistory> infraAccountServicesPriceHistoryRepository
        )
        {
            _infraAccountServicesPriceHistoryRepository = infraAccountServicesPriceHistoryRepository;
        }
        # endregion

        #region CalculateYTD
        //public decimal CalculateYTD(
        //    ICollection<InfraAccountServicesPriceHistory> priceHistories)
        //{
        //    if (priceHistories == null || !priceHistories.Any())
        //        return 0;

        //    var today = DateTime.UtcNow.Date;

        //    var orderedCycles = priceHistories
        //        .Where(x => !x.Deleted)
        //        .OrderBy(x => x.StartDate)
        //        .ToList();

        //    decimal completedCyclesTotal = 0;
        //    decimal partialCycleCost = 0;

        //    for (int i = 0; i < orderedCycles.Count; i++)
        //    {
        //        var currentCycle = orderedCycles[i];

        //        DateTime cycleEndDate;

        //        // Next cycle start becomes current cycle end
        //        if (i < orderedCycles.Count - 1)
        //        {
        //            cycleEndDate =
        //                orderedCycles[i + 1]
        //                    .StartDate
        //                    .Date;
        //        }
        //        else
        //        {
        //            // Latest cycle fallback = 1 year
        //            cycleEndDate =
        //                currentCycle
        //                    .StartDate
        //                    .AddYears(1)
        //                    .Date;
        //        }

        //        // Fully completed cycles
        //        if (cycleEndDate < today)
        //        {
        //            completedCyclesTotal +=
        //                currentCycle.Price;
        //        }

        //        // Current running partial cycle
        //        else if (
        //            today >= currentCycle.StartDate.Date &&
        //            today <= cycleEndDate
        //        )
        //        {
        //            // Total days in cycle
        //            var totalDays =
        //                (cycleEndDate -
        //                 currentCycle.StartDate.Date)
        //                .Days;

        //            // Days used till today
        //            var usedDays =
        //                (today -
        //                 currentCycle.StartDate.Date)
        //                .Days;

        //            // Price per day
        //            var pricePerDay =
        //                currentCycle.Price / totalDays;

        //            // Partial price till today
        //            partialCycleCost =
        //                usedDays * pricePerDay;
        //        }
        //    }

        //    // Completed cycles + current partial cycle
        //    return Math.Round(
        //        completedCyclesTotal + partialCycleCost,
        //        2);
        //}
        //#endregion

        public decimal CalculateYTD(ICollection<InfraAccountServicesPriceHistory> priceHistories)
        {
            if (priceHistories == null || !priceHistories.Any())
                return 0;

            var today = DateTime.UtcNow.Date;

            var histories = priceHistories
                .Where(x => !x.Deleted)
                .OrderBy(x => x.StartDate)
                .ToList();

            var latestHistory = histories.Last();

            // Sum completed history records
            decimal ytd = histories
                .Where(x => x.Id != latestHistory.Id)
                .Sum(x => x.TotalPrice);

            var endDate = latestHistory.EndDate ?? today;

            // Calculate current active record
            var totalMonths =
                ((endDate.Year - latestHistory.StartDate.Year) * 12) +
                (endDate.Month - latestHistory.StartDate.Month);

            if (endDate.Day < latestHistory.StartDate.Day)
               totalMonths--;

            var years = totalMonths / 12;
            var months = totalMonths % 12;

            var diffInYears = years + (months / 10m);
            ytd += Math.Round(diffInYears * latestHistory.Price, 2);

            return Math.Round(ytd, 2);
        }
        #endregion

        #region GetCurrentCyclePrice
        public decimal GetCurrentCyclePrice(
            ICollection<InfraAccountServicesPriceHistory> priceHistories)
        {
            if (priceHistories == null || !priceHistories.Any())
                return 0;

            var today = DateTime.UtcNow.Date;

            var orderedCycles = priceHistories
                .Where(x => !x.Deleted)
                .OrderBy(x => x.StartDate)
                .ToList();

            for (int i = 0; i < orderedCycles.Count; i++)
            {
                var currentCycle = orderedCycles[i];

                DateTime cycleEndDate;

                // Next cycle start becomes current cycle end
                if (i < orderedCycles.Count - 1)
                {
                    cycleEndDate =
                        orderedCycles[i + 1]
                            .StartDate
                            .Date;
                }
                else
                {
                    // Latest cycle fallback = 1 year
                    cycleEndDate =
                        currentCycle
                            .StartDate
                            .AddYears(1)
                            .Date;
                }

                // Current active cycle
                if (
                    today >= currentCycle.StartDate.Date &&
                    today <= cycleEndDate
                )
                {
                    return currentCycle.Price;
                }
            }

            return 0;
        }
        #endregion

        #region GetInfraAccountServicesPriceHistoryByAccountServiceId
        // Title: GetInfraAccountServicesPriceHistoryByAccountServiceId
        // Description: This method retrieves a InfraAccountServicesPriceHistory from the database by its infraAccountServiceId. 
        public async Task<InfraAccountServicesPriceHistory> GetInfraAccountServicesPriceHistoryByAccountServiceId(string infraAccountServiceId)
        {
            var query = _infraAccountServicesPriceHistoryRepository.TableNoTracking.Where(x => !x.Deleted && x.InfraAccountServiceId == infraAccountServiceId).OrderByDescending(x => x.StartDate);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertInfraAccountServicesPriceHistoryList
        public void InsertInfraAccountServicesPriceHistoryList(IList<Models.InfraAccountServicesPriceHistory> entities)
        {
            _infraAccountServicesPriceHistoryRepository.Insert(entities);
        }
        #endregion

        #region InsertInfraAccountServicesPriceHistory
        public void InsertInfraAccountServicesPriceHistory(Models.InfraAccountServicesPriceHistory entity)
        {
            _infraAccountServicesPriceHistoryRepository.Insert(entity);
        }
        #endregion

        #region UpdateInfraAccountServicesPriceHistory
        public void UpdateInfraAccountServicesPriceHistory(Models.InfraAccountServicesPriceHistory entity)
        {
            _infraAccountServicesPriceHistoryRepository.Update(entity);
        }
        #endregion
    }
}
