using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record LeaveRuleLinesModel : BaseEntityModel
    {
        public string LeaveRuleId { get; set; }

        public string EmploymentTypeId { get; set; }
        //public decimal CreditLeaves { get; set; }
        public decimal CasualLeaves { get; set; }
        public decimal SickLeaves { get; set; }
        public string Flag { get; set; }

        public virtual LeaveRules LeaveRules { get; set; }
        public virtual DropDown EmploymentType { get; set; }
    }

    public record LeaveRuleLinesSearchModel : BaseSearchModel
    {
        //public List<string> PersonIds { get; set; }
        //public string PrimaryEmailAddress { get; set; }

    }
    public record LeaveRuleLinesListModel : BasePagedListModel<LeaveRuleLinesModel>
    {
        // public bool editing { get; set; }
    }
}