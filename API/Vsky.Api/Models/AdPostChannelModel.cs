using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record AdPostChannelModel : BaseEntityModel
    {
        public string ProjectId { get; set; }
        public string SiteId { get; set; }
        public string CustomerId { get; set; }
        public int ChannelNumber { get; set; }
        public string Name { get; set; }
        public int GroupMemberCount { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public virtual CompanyClients Customer { get; set; }
        public virtual Project Project { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
    }

    public record AdPostChannelSearchModel : BaseSearchModel
    {
        public List<string> ProjectIds { get; set; }
        public string Name { get; set; }
        public List<string> CustomerIds { get; set; }
        public string SearchText { get; set; }
    }

    public record AdPostChannelListModel : BasePagedListModel<AdPostChannelModel>
    {
        public bool editing { get; set; }
    }
    public record AdPostChannelUploadModel : BaseEntityModel
    {
    }
}
