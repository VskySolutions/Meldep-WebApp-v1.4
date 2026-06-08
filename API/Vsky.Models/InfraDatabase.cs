using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models
{
    public class InfraDatabaseList
    {
        public virtual ICollection<InfraDatabase> InfraDatabasesList { get; set; } = new List<InfraDatabase>();
        public int Total { get; set; }
    }
    public class InfraDatabase : BaseEntity
    {
        public string SiteId { get; set; }
        public string InfraServiceId { get; set; }
        public string WalletTypeId { get; set; }
        public string WalletNumber { get; set; }
        public string Name { get; set; }
        public string ServerName { get; set; }
        public bool IsReadOrWrite { get; set; }
        public string Instructions { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual Site Site { get; set; }
        public virtual InfraAccountServices InfraService { get; set; }
        public virtual DropDown WalletType { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<InfraDatabaseProjectInstanceMapping> InfraDatabaseProjectInstanceMapping { get; set; } = new List<InfraDatabaseProjectInstanceMapping>();
    }
    public class InfraDatabaseProjectInstanceMapping : BaseEntity
    {
        public string InfraDatabaseId { get; set; }
        public string ProjectInstanceId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual InfraDatabase InfraDatabase { get; set; }
        public virtual InfraProjectInstance InfraProjectInstance { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
    public class SaveInfraDatabase
    {
        public string Id { get; set; }
        public string SiteId { get; set; }
        public string InfraServiceId { get; set; }
        public string WalletTypeId { get; set; }
        public string WalletNumber { get; set; }
        public string Name { get; set; }
        public string ServerName { get; set; }
        public bool IsReadOrWrite { get; set; }
        public bool IsInstruction { get; set; }
        public string Instructions { get; set; }
        public bool Deleted { get; set; }
    }
    public class SaveInfraDatabaseProjectInstanceMapping
    {
        public string Id { get; set; }
        public string InfraDatabaseId { get; set; }
        public string ProjectInstanceId { get; set; }
    }
    public class SaveInfraDatabaseList
    {
        public List<SaveInfraDatabase> InfraDatabaseLines { get; set; } = new List<SaveInfraDatabase>();
    }
}

