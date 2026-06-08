using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class MasterProjectSwimlaneLists : BaseEntity
    {
        public string SiteId { get; set; }
        public string SwimlaneTypeId { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual DropDown SwimlaneType { get; set; }
    }
}
