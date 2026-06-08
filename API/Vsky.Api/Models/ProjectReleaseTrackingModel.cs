using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Core;

namespace Vsky.Api.Models
{
    public record ProjectReleaseTrackingModel : BaseEntityModel
    {
    }

    public record ProjectReleaseTrackingSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public List<string> ProjectIds { get; set; }
        public List<string> InfraInstanceIds { get; set; }
        public List<string> DeploymentOwnerIds { get; set; }
        public List<string> ApproverIds { get; set; }
        public List<string> TesterIds { get; set; }
        public List<string> ReleaseTypeIds { get; set; }
    }
}
