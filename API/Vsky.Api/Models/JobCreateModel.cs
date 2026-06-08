using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record JobCreateModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string JobDescription { get; set; }
        public string Criteria { get; set; }
        public string JobTitle { get; set; }
        public DateTime? JobCreatedDate { get; set; }
        public DateTime PublishedJobDate { get; set; }
        public string JobReference { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public bool IsActive { get; set; }
        public string PublishedJobDateStr { get; set; }

        public virtual Site Site { get; set; }
    }
    public record JobCreateSearchModel : BaseSearchModel
    {
        public string JobTitle { get; set; }
        public string SearchText { get; set; }
    }

    public record JobCreateListModel : BasePagedListModel<JobCreateModel>
    {
        public bool editing { get; set; }
    }
    public record JobCreateUploadModel : BaseEntityModel
    {
    }
}
