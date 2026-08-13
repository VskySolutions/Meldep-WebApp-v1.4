using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ProjectActionItemsList
    {
        public virtual ICollection<ProjectActionItems> ProjectActionItemList { get; set; } = new List<ProjectActionItems>();
        public int Total { get; set; }
    }
    public class ProjectActionItems : BaseEntity
    {
        public string SiteId { get; set; }
        public string ProjectId { get; set; }
        public string RequirementId { get; set; }
        public string PriorityId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CustomerId { get; set; }
        public string EmployeeId { get; set; }
        public DateTime? DueDate { get; set; }

        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public int ProjectActionItemNotesCount { get; set; }

        public virtual Site Site { get; set; }
        public virtual Project Project { get; set; }
        public virtual Requirement Requirement { get; set; }
        public DropDown Priority { get; set; }
        public CompanyClients Customer { get; set;  }
        public Employee Employee { get; set;  }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
    }
    public class SaveProjectActionItems
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string RequirementId { get; set; }
        public string PriorityId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CustomerId { get; set; }
        public string EmployeeId { get; set; }
        public DateTime? DueDate { get; set; }
        public bool Deleted { get; set; }
    }
}
