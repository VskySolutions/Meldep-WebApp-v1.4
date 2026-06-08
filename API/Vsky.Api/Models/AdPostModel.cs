using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Microsoft.AspNetCore.Http;

namespace Vsky.Api.Models
{
    public record AdPostModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string PictureId { get; set; }
        public string ProjectId { get; set; }
        public string CustomerId { get; set; }
        public string ImageType { get; set; }
        public string ImageProviderClientId { get; set; }
        public string ImageProviderEmpId { get; set; }
        public string ContentType { get; set; }
        public string ContentProviderClientId { get; set; }
        public string ContentProviderEmpId { get; set; }
        public int AdNumber { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string Description { get; set; }
        public string Caption { get; set; }
        public string Tags { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public IFormFile PostDesignPic { get; set; }
        public string PostChangeFlag { get; set; }

        public virtual Picture Picture { get; set; }
        public virtual CompanyClients Customer { get; set; }
        public virtual Project Project { get; set; }
        public virtual DropDown ImageTypeDropDown { get; set; }
        public virtual DropDown ContentTypeDropDown { get; set; }
        public virtual Person ImageProviderClient { get; set; }
        public virtual Person ContentProviderClient { get; set; }
        public virtual Employee ImageProviderEmp { get; set; }
        public virtual Employee ContentProviderEmp { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<AdPostingStatus> AdPostingStatusList { get; set; } = new List<AdPostingStatus>();
    }

    public record AdPostSearchModel : BaseSearchModel
    {
        public List<string> ProjectIds { get; set; }
        public string Name { get; set; }
        public List<string> CustomerIds { get; set; }
        public string SearchText { get; set; }
    }

    public record AdPostListModel : BasePagedListModel<AdPostModel>
    {
        public bool editing { get; set; }
    }
    public record AdPostUploadModel : BaseEntityModel
    {
    }
}
