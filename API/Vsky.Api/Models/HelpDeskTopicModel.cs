using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record HelpDeskTopicModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool IsActive { get; set; }
        public bool Deleted { get; set; }
        public virtual Site Site { get; set; }
    }
}
