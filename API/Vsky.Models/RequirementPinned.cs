using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class RequirementPinned : BaseEntity
    {
        public string RequirementId { get; set; }
        public string AspNetUserId { get; set; }
        public bool IsPinned { get; set; }

        public virtual Requirement Requirement { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
