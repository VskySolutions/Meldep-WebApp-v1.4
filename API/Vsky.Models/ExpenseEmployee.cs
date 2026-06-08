using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ExpenseEmployee : BaseEntity
    {
        public string Name { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string CreatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public string UpdatedById { get; set; }

        public bool Deleted { get; set; }
    }
}
