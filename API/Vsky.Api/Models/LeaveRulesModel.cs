using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record LeaveRulesModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public int Year { get; set; }
        public bool IsGenerated { get; set; }
        public virtual Site Site { get; set; }
        public virtual ICollection<LeaveRuleLinesModel> LeaveRuleLines { get; set; } = new List<LeaveRuleLinesModel>();
        public virtual ICollection<LeaveRuleLines> LeaveRuleLinesList { get; set; } = new List<LeaveRuleLines>();
    }

    public record LeaveRulesSearchModel : BaseSearchModel
    {
        public List<int> Years { get; set; }
        public string SearchText { get; set; }
        //public string PrimaryEmailAddress { get; set; }

    }
    public record LeaveRulesListModel : BasePagedListModel<LeaveRulesModel>
    {
        // public bool editing { get; set; }
    }
}