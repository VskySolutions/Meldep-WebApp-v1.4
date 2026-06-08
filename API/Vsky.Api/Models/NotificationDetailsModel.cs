using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record NotificationDetailsModel : BaseEntityModel
    {
        public string NotificationId { get; set; }
        public string ToUserId { get; set; }
        public int IsRead { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Notification Notification { get; set; }
    }
    public record NotificationDetailsSearchModel : BaseSearchModel
    {
    }

    public record NotificationDetailsListModel : BasePagedListModel<NotificationDetailsModel>
    {
        public bool editing { get; set; }
    }
    public record NotificationDetailsUploadModel : BaseEntityModel
    {
    }
}
