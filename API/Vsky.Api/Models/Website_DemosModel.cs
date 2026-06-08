using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record Website_DemosModel : BaseEntityModel
    {
        public string BusinessSizeId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string CompanyName { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public virtual DropDown BusinessSize { get; set; }

        public string[] ModulesIds { get; set; }
        public string RecaptchaToken { get; set; }
        public virtual ICollection<Website_Demo_Modules> Website_Demo_Modules { get; set; } = new List<Website_Demo_Modules>();
    }
}
