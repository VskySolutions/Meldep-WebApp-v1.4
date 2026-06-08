using Vsky.Api.Framework.Models;
using Vsky.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System;

namespace Vsky.Api.Models
{
    public record TrainingPortalModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TrainingFileId { get; set; }
        public string EmployeeDesignationId { get; set; }
        public string[] EmployeeDesignationIdsArray { get; set; }
        public int TrainingPortalNumber { get; set; }
        public string Url { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public IFormFile FilePic { get; set; }
        public string FileChangeFlag { get; set; }
        public bool editing { get; set; }
        public virtual Picture File { get; set; }
        public virtual Site Site { get; set; }
        public virtual DropDown EmployeeDesignationType { get; set; }
        public virtual ICollection<Training_Portal_Mapping> TrainingPortalMappings { get; set; } = new List<Training_Portal_Mapping>();
    }
    public record TrainingPortalSearchModels : BaseSearchModel
    {
        public string Name { get; set; }
        public string EmployeeDesignationId { get; set; }
        public List<string> EmployeeDesignationIds { get; set; }
        public string SearchText { get; set; }

    }
    public record TrainingPortalListModel : BasePagedListModel<TrainingPortalModel>
    {
        public bool editing { get; set; }
    }
}
