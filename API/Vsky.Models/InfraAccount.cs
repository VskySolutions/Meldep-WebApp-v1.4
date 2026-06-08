using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class InfraAccountList
    {
        public virtual ICollection<InfraAccount> InfraAccountsList { get; set; } = new List<InfraAccount>();
        public int Total { get; set; }
    }
    public class InfraAccountServiceList
    {
        public virtual ICollection<InfraAccountServices> InfraAccountServicesList { get; set; } = new List<InfraAccountServices>();
        public int Total { get; set; }
    }
    public class InfraAccount : BaseEntity
    {
        public string SiteId { get; set; }
        public string ProviderId { get; set; }
        public string WalletTypeId { get; set; }
        public string WalletNumber { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string CustomerId { get; set; }
        public string CCLast4Digits { get; set; }
        public string Instructions { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        [NotMapped]
        public decimal TotalServicesCost { get; set; }        

        public virtual Site Site { get; set; }
        public virtual DropDown Provider { get; set; }
        public virtual DropDown WalletType { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<InfraAccountServices> InfraAccountServices { get; set; } = new List<InfraAccountServices>();
    }
    public class InfraAccountServices : BaseEntity
    {
        public string InfraAccountId { get; set; }
        public string ItemTypeId { get; set; }
        public string WalletTypeId { get; set; }
        public string InfraAccountServiceId { get; set; }
        public string WalletNumber { get; set; }
        public string OwnerShipTypeId { get; set; }
        public string PaymentTermId { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        [NotMapped]
        public string StartDateStr { get; set; }
        [NotMapped]
        public string EndDateStr { get; set; }
        [NotMapped]
        public string Flag { get; set; }
        [NotMapped]
        public decimal ActualPriceInDollar { get; set; }
        [NotMapped] public decimal YTD { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal PriceInDollar { get; set; }
        public string Instructions { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual InfraAccount InfraAccount { get; set; }
        public virtual DropDown ItemType { get; set; }
        public virtual DropDown WalletType { get; set; }
        public virtual InfraAccountServices InfraAccountService { get; set; }
        public virtual DropDown OwnerShipType { get; set; }
        public virtual DropDown PaymentTerm { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<InfraFTP> InfraFTPList { get; set; } = new List<InfraFTP>();
        public virtual ICollection<InfraDatabase> InfraDatabaseList { get; set; } = new List<InfraDatabase>();
        public virtual ICollection<InfraProjectServices> InfraProjectServices { get; set; } = new List<InfraProjectServices>();
        public virtual ICollection<InfraAccountServicesPriceHistory> PriceHistories { get; set; } = new List<InfraAccountServicesPriceHistory>();
    }
    public class InfraProjectServices : BaseEntity
    {
        public string InfraProjectId { get; set; }
        public string InfraServiceId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual InfraAccountServices InfraAccountServices { get; set; }
        public virtual Project Project { get; set; }
    }

    public class InfraAccountServicesPriceHistory : BaseEntity
    {
        public string InfraAccountServiceId { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual InfraAccountServices InfraAccountServicesHistory { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
    public class SaveInfraAccount
    {
        public string SiteId { get; set; }
        public string ProviderId { get; set; }
        public string WalletTypeId { get; set; }
        public string WalletNumber { get; set; }
        public string URL { get; set; }
        public string Name { get; set; }
        public string CustomerId { get; set; }
        public string CCLast4Digits { get; set; }
        public string Instructions { get; set; }
        public virtual ICollection<InfraAccountServices> InfraAccountServicesList { get; set; } = new List<InfraAccountServices>();
    }

    public class SaveInfraAccountServices
    {
        public string InfraAccountId { get; set; }
        public string ItemTypeId { get; set; }
        public string WalletTypeId { get; set; }
        public string WalletNumber { get; set; }
        public string OwnerShipTypeId { get; set; }
        public string PaymentTermId { get; set; }
        public string InfraAccountServiceId { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public decimal Price { get; set; }
        [NotMapped]
        public string StartDateStr { get; set; }
        [NotMapped]
        public string EndDateStr { get; set; }
        public bool IsInstruction { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal PriceInDollar { get; set; }
        public string Instructions { get; set; }
    }
    public class SaveInfraAccountServicesList
    {
        public string InfraAccountId { get; set; }
        public List<SaveInfraAccountServices> InfraAccountServicesLines { get; set; } = new List<SaveInfraAccountServices>();
    }
}

