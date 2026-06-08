using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class CustomerFiles : BaseEntity
    {
        public string CustomerId { get; set; }
        public string SiteId { get; set; }
        public string Note { get; set; }
        // public string FileId { get; set; }
        // public string FileName { get; set; }
        public int Year { get; set; }
        public int SortOrder { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string? CreatedById { get; set; }
        public string? UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public virtual Site Sites { get; set; }
        public virtual CompanyClients? CompanyClients { get; set; }
        public virtual ApplicationUser? CreatedBy { get; set; }
        public virtual ApplicationUser? UpdatedBy { get; set; }
        public virtual ICollection<CustomerFilesLines> CustomerFilesLines { get; set; } = new List<CustomerFilesLines>();

    }

    public class VW_CustomerFiles : BaseEntity
    {
        public string SiteId { get; set;}
        public string CustomerId { get; set;}
        public string CustomerName { get; set;}
        public int Year { get; set;}
        public string FileName { get; set;}
        public int SortOrder { get; set;}
    }
}
