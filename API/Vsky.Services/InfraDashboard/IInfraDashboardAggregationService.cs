using Vsky.Models;

namespace Vsky.Services.InfraDashboard
{
    // Title: IInfraDashboardAggregationService
    // Description: Read-only financial aggregation over infrastructure accounts, account services, and service price
    // history for the Infrastructure Dashboard & Financial Insights feature. Every method is site-scoped and honors
    // the supplied dashboard filter. This service never mutates infrastructure records.
    public interface IInfraDashboardAggregationService
    {
        #region Executive Summary
        InfraDashboardSummary GetSummary(string siteId, InfraDashboardFilter filter);
        #endregion

        #region Cost Breakdowns
        InfraDashboardBreakdowns GetBreakdowns(string siteId, InfraDashboardFilter filter);
        #endregion

        #region Price Changes
        System.Collections.Generic.List<InfraDashboardPriceChange> GetPriceChanges(string siteId, InfraDashboardFilter filter);
        #endregion

        #region Cost History / Trend
        InfraDashboardHistory GetHistory(string siteId, InfraDashboardFilter filter);
        #endregion

        #region Data Quality
        InfraDashboardDataQuality GetDataQuality(string siteId, InfraDashboardFilter filter);
        #endregion

        #region Renewals
        InfraDashboardRenewals GetRenewals(string siteId, InfraDashboardFilter filter);
        #endregion
    }
}
