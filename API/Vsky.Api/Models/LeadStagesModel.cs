using System;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public record LeadStagesModel : BaseEntityModel
    {
        public string StageName { get; set; }
        public string StageDescription { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
    }
}
