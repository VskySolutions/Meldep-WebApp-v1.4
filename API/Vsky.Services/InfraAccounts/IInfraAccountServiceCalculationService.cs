using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.InfraAccounts
{

    public interface IInfraAccountServiceCalculationService
    {
        #region CalculateYTD
        decimal CalculateYTD(ICollection<InfraAccountServicesPriceHistory> priceHistories);
        #endregion

        #region GetCurrentCyclePrice
        decimal GetCurrentCyclePrice(ICollection<InfraAccountServicesPriceHistory> priceHistories);
        #endregion

        #region GetInfraAccountServicesPriceHistoryByAccountServiceId
        Task<InfraAccountServicesPriceHistory> GetInfraAccountServicesPriceHistoryByAccountServiceId(string infraAccountServiceId);
        #endregion

        #region InsertInfraAccountServicesPriceHistoryList
        void InsertInfraAccountServicesPriceHistoryList(IList<InfraAccountServicesPriceHistory> entities);
        #endregion

        #region UpdateInfraAccountServicesPriceHistory
        void UpdateInfraAccountServicesPriceHistory(Models.InfraAccountServicesPriceHistory entity);
        #endregion
    }
}
