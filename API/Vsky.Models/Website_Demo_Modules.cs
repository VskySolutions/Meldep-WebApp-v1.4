using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class Website_Demo_Modules : BaseEntity
    {
        public string WebsiteDemoId { get; set; }
        public string ModuleId { get; set; }
        public virtual Website_Demos Website_Demos {  get; set; }
        public virtual Modules Modules { get; set; }
    }
}
