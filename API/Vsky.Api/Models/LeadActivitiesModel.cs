using System;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record LeadActivitiesModel : BaseEntityModel
    {
        public string ActivityName { get; set; }
        public string ActivityDescription { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
    }
}
