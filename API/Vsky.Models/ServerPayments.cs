using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ServerPayments : BaseEntity
    {
        public string ServerId { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public DateTime? RenewDate { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public virtual Server Server { get; set; }
    }
}
