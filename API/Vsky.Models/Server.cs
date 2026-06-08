using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class Server : BaseEntity
    {
        public string SiteId { get; set; }
        public string ProviderId { get; set; }
        public string CustomerId { get; set; }
        public string ContractId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Instructions { get; set; }
        public string Notes { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public string FtpPort { get; set; }
        public string FtpHostname { get; set; }
        public DateTime? StartDate { get; set; }
        public string CardDigit { get; set; }
        public string PIN { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public int ServerNotesCount { get; set; }
        
        [NotMapped]
        public int FtpNotesCount { get; set; }

        public virtual DropDown Provider { get; set; }
        public virtual Site Sites { get; set; }
        public virtual ICollection<ServerPayments> ServerPayments { get; set; } = new List<ServerPayments>();

    }
}
