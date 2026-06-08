using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record CustomerFilesModel : BaseEntityModel
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

        [NotMapped]
        public virtual CompanyClients? CompanyClients { get; set; }
        [NotMapped]
        public virtual ApplicationUser? CreatedBy { get; set; }
        [NotMapped]
        public virtual ApplicationUser? UpdatedBy { get; set; }

        public List<CustomerFilesModel> Files { get; set; } = new List<CustomerFilesModel>();
        public virtual ICollection<CustomerFilesLinesModel> CustomerFilesLines { get; set; } = new List<CustomerFilesLinesModel>();

    }

    public record CustomerFilesSearchModel : BaseSearchModel
    {
        public string note { get; set; }
        public string CustomerId { get; set; }
        public string CreatedBy { get; set; }
        public int Year { get; set; }
    }

    public record CustomerFilesListModel : BasePagedListModel<CustomerFilesModel>
    {
        public bool editing { get; set; }
    }

    public record VW_CustomerFilesModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int Year { get; set; }
        public string FileName { get; set; }
        public int SortOrder { get; set; }
    }

    public record VW_CustomerFilesListModel : BasePagedListModel<VW_CustomerFilesModel>
    {

    }
}