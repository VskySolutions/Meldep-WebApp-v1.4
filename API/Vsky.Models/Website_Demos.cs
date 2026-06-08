using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class Website_Demos : BaseEntity
    {
        public string BusinessSizeId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string CompanyName { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public virtual DropDown BusinessSize { get; set; }
        public virtual ICollection<Website_Demo_Modules> Website_Demo_Modules { get; set; } = new List<Website_Demo_Modules>();
    }
}
