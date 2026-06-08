using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;


namespace Vsky.Models
{
    public class IssueStatusChangedLog : BaseEntity
    {
        public string IssueId { get; set; }
        public string StatusId { get; set; }
        public string StatusChangedBy { get; set; }
        public DateTime StatusChangedDate { get; set; }
        public bool Deleted { get; set; }

        public virtual Issue Issue { get; set; }
        public virtual DropDown Status { get; set; }
        public virtual Employee StatusChangedByEmployee { get; set; }
    }
}
