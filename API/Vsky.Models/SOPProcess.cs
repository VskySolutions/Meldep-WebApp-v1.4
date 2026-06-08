using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models
{
    public class SOPProcessList
    {
        public virtual ICollection<SOPProcess> SOPProcessesList { get; set; } = new List<SOPProcess>();
        public int Total { get; set; }
    }
    public class SOPProcess : BaseEntity
    {
        public string SiteId { get; set; }
        public string CategoryId { get; set; }
        public string SubCategoryId { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Purpose { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }
        [NotMapped]
        public string StatusId { get; set; }
        [NotMapped]
        public string StatusText { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual DropDownType Category { get; set; }
        public virtual DropDown SubCategory { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<SOPProcessStatusLog> SOPProcessStatusLog { get; set; } = new List<SOPProcessStatusLog>();
    }
    //public class SOPProcessItems : BaseEntity
    //{
    //    public string SOPProcessId { get; set; }
    //    public string Title { get; set; }
    //    public string Description { get; set; }
    //    public string SortOrder { get; set; }
    //    public DateTime CreatedOnUtc { get; set; }
    //    public string CreatedById { get; set; }
    //    public DateTime UpdatedOnUtc { get; set; }
    //    public string UpdatedById { get; set; }
    //    public bool Deleted { get; set; }

    //    public virtual SOPProcess SOPProcess { get; set; }
    //}
    public class SOPProcessStatusLog : BaseEntity
    {
        public string SOPProcessId { get; set; }
        public string StatusId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual SOPProcess SOPProcess { get; set; }
        public virtual DropDown Status { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
    public class SaveSOPProcess
    {
        public string SiteId { get; set; }
        public string CategoryId { get; set; }
        public string SubCategoryId { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Purpose { get; set; }
        public string StatusId { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }
    }
}

