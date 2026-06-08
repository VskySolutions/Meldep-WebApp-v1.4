using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class InfraFTPList
    {
        public virtual ICollection<InfraFTP> InfraFTPsList { get; set; } = new List<InfraFTP>();
        public int Total { get; set; }
    }
    public class InfraFTP : BaseEntity
    {
        public string SiteId { get; set; }
        public string InfraServiceId { get; set; }
        public string ProtocolTypeId { get; set; }
        public string EncryptionTypeId { get; set; }
        public string WalletTypeId { get; set; }
        public string WalletNumber { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Instructions { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual InfraAccountServices InfraService { get; set; }
        public virtual DropDown ProtocolType { get; set; }
        public virtual DropDown EncryptionType { get; set; }
        public virtual DropDown WalletType { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<InfraFTPsProjectInstanceMapping> InfraFTPsProjectInstanceMapping { get; set; } = new List<InfraFTPsProjectInstanceMapping>();
    }
    public class InfraFTPsProjectInstanceMapping : BaseEntity
    {
        public string InfraFTPId { get; set; }
        public string ProjectInstanceId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual InfraFTP InfraFTP { get; set; }
        public virtual InfraProjectInstance InfraProjectInstance { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
    public class SaveInfraFTP
    {
        public string Id { get; set; }
        public string SiteId { get; set; }
        public string InfraServiceId { get; set; }
        public string ProtocolTypeId { get; set; }
        public string EncryptionTypeId { get; set; }
        public string WalletTypeId { get; set; }
        public string WalletNumber { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Instructions { get; set; }
        public bool IsNew { get; set; }
        public bool IsInstruction { get; set; }
        public bool Deleted { get; set; }
    }
    public class SaveInfraFTPsProjectInstanceMapping
    {
        public string Id { get; set; }
        public string InfraFTPId { get; set; }
        public string ProjectInstanceId { get; set; }
    }
    public class SaveInfraFTPList
    {
        public List<SaveInfraFTP> InfraFTPLines { get; set; } = new List<SaveInfraFTP>();
    }
}

