using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record TrainingPortalMappingModels : BaseEntityModel
    {
        public string TrainingId { get; set; }
        public string EmployeeDesignationId { get; set; }
        public string[] EmployeeDesignationIdsArray { get; set; }
        public virtual DropDown EmployeeDesignationType { get; set; }
        public virtual TrainingPortal Training { get; set; }
    }
}
