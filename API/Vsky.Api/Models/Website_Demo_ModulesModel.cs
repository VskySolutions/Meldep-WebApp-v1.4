using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record Website_Demo_ModulesModel : BaseEntityModel
    {
        public string WebsiteDemoId { get; set; }
        public string ModuleId { get; set; }
        public string[] WebsiteDemoIdsArray { get; set; }
        public virtual Website_Demos Website_Demos { get; set; }
        public virtual Modules Modules { get; set; }
    }
}
