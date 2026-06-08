using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class CandidateActivities : BaseEntity
    {
        public string CandidateId { get; set; }
        public string PriorityId { get; set; }
        public string EmployeeOwnerId { get; set; }
        public string ActivityName { get; set; }
        public DateTime? DueDate { get; set; }
        public string CreatedById { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Candidate Candidates { get; set; }
        public virtual DropDown Priority { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
