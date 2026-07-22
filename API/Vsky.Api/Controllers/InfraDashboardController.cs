using System;
using Microsoft.AspNetCore.Mvc;
using Vsky.Models;
using Vsky.Services.InfraDashboard;

namespace Vsky.Api.Controllers
{
    // Title: InfraDashboardController
    // Description: CEO- and finance-focused read-only reporting over infrastructure accounts, account services, and
    // service price history. Exposes executive summary, cost breakdowns, price changes, cost history/trend,
    // chargeability review, missing-billing-data, and upcoming-renewal endpoints. Every endpoint is site-scoped via
    // the active site (X-Site-Id header -> GlobalVariable) and honors the supplied dashboard filter. This controller
    // never mutates infrastructure records.
    [Route("infra-dashboard")]
    public class InfraDashboardController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly IInfraDashboardAggregationService _infraDashboardAggregationService;
        #endregion

        #region Services Initializations
        public InfraDashboardController(
            GlobalVariable globalVariable,
            IInfraDashboardAggregationService infraDashboardAggregationService)
        {
            _globalVariable = globalVariable;
            _infraDashboardAggregationService = infraDashboardAggregationService;
        }
        #endregion

        #region Summary
        // Title: GetSummary
        // Description: Returns total monthly equivalent cost, annualized cost, and one-time cost for the selected scope.
        [HttpGet("summary")]
        public IActionResult GetSummary([FromQuery] InfraDashboardFilter filter)
        {
            try
            {
                var SiteId = _globalVariable.SiteId;
                var model = _infraDashboardAggregationService.GetSummary(SiteId, filter);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Breakdowns
        // Title: GetBreakdowns
        // Description: Returns grouped costs by client/customer, project, provider, item type, ownership type, and
        // payment term, each with amount, percentage distribution, and chart-ready labels/series.
        [HttpGet("breakdowns")]
        public IActionResult GetBreakdowns([FromQuery] InfraDashboardFilter filter)
        {
            try
            {
                var SiteId = _globalVariable.SiteId;
                var model = _infraDashboardAggregationService.GetBreakdowns(SiteId, filter);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Price Changes
        // Title: GetPriceChanges
        // Description: Returns previous price, current price, absolute change, and percentage change per changed service.
        [HttpGet("price-changes")]
        public IActionResult GetPriceChanges([FromQuery] InfraDashboardFilter filter)
        {
            try
            {
                var SiteId = _globalVariable.SiteId;
                var model = _infraDashboardAggregationService.GetPriceChanges(SiteId, filter);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region History
        // Title: GetHistory
        // Description: Returns monthly recurring cost totals across the selected date range, derived from price history.
        [HttpGet("history")]
        public IActionResult GetHistory([FromQuery] InfraDashboardFilter filter)
        {
            try
            {
                var SiteId = _globalVariable.SiteId;
                var model = _infraDashboardAggregationService.GetHistory(SiteId, filter);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Data Quality
        // Title: GetDataQuality
        // Description: Returns services missing billing-critical fields (client/customer, project, ownership type,
        // payment term, price, or end date).
        [HttpGet("data-quality")]
        public IActionResult GetDataQuality([FromQuery] InfraDashboardFilter filter)
        {
            try
            {
                var SiteId = _globalVariable.SiteId;
                var model = _infraDashboardAggregationService.GetDataQuality(SiteId, filter);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Renewals
        // Title: GetRenewals
        // Description: Returns services with end dates in the selected upcoming window and recurring services missing
        // end dates.
        [HttpGet("renewals")]
        public IActionResult GetRenewals([FromQuery] InfraDashboardFilter filter)
        {
            try
            {
                var SiteId = _globalVariable.SiteId;
                var model = _infraDashboardAggregationService.GetRenewals(SiteId, filter);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
