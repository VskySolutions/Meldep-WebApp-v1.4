using System;
using Vsky.Api.Framework.Models;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ProjectTagsModel : BaseEntityModel
    {
        public string ProjectId { get; set; }
        public string AspNetUserId { get; set; }
        public string TagId { get; set; }
        public bool Deleted { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public virtual Project Projects { get; set; }
        public virtual Tags Tags { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
