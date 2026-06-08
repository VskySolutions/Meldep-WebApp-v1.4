using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record EmailNotificationsModel : BaseEntityModel
    {
    }

    public record MessageTemplateSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
    }

    public record SiteEmailNotificationsSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string UserId { get; set; }
    }
}
