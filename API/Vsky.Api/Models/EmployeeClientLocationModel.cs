using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record EmployeeClientLocationModel : BaseEntityModel
    {
        public string EmployeeId { get; set; }
        public string ClientLocationId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string ClientLocationStartDateStr { get; set; }

        public string ClientLocationEndDateStr { get; set; }
        public string Flag { get; set; }

        public bool Active { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual DropDown ClientLocation { get; set; }

        public virtual ICollection<EmployeeClientLocationModel> ClientLocationEntities { get; set; } = new List<EmployeeClientLocationModel>();
    }
}

