using Vsky.Models;
using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record AdPostingStatusModel : BaseEntityModel
    {
        public string AdId { get; set; }
        public string AdPostChannelId { get; set; }
        public DateTime Date { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public int Shares { get; set; }
        public string Flag { get; set; }
        public string DateStr { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public string UpdatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public bool Deleted { get; set; }

        public virtual AdPost AdPost { get; set; }
        public virtual AdPostChannel AdPostChannel { get; set; }
        public virtual ICollection<AdPostingStatusModel> AdPostingStatuses { get; set; } = new List<AdPostingStatusModel>();
    }

    public record AdPostingStatusSearchModel : BaseSearchModel
    {
    }

    public record AdPostingStatusListModel : BasePagedListModel<AdPostingStatusModel>
    {
        public bool editing { get; set; }
    }
    public record AdPostingStatusUploadModel : BaseEntityModel
    {
    }
}
