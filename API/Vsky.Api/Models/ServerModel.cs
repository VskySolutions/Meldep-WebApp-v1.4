using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ServerModel : BaseEntityModel
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
        public string StartDateStr { get; set; }
        public string CardDigit { get; set; }
        public string  PIN { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public string Tab { get; set; }
        public int ServerNotesCount { get; set; }
        public int FtpNotesCount { get; set; }
        public virtual DropDownModel Provider { get; set; }
        public virtual SiteModel Sites { get; set; }
        public virtual ICollection<ServerPaymentsModel> ServerPaymentsModel { get; set; } = new List<ServerPaymentsModel>();
        public virtual ICollection<ServerPaymentsModel> ServerPayments { get; set; } = new List<ServerPaymentsModel>();


    }
    public record ServerSearchModel : BaseSearchModel
    {
        public string ProviderId { get; set; }
        public string CustomerId { get; set; }
        public string ContractId { get; set; }
        public string FTPUsername { get; set; }
        public string FTPHostname { get; set; }
        public string FtpPort { get; set; }
        public string SearchText { get; set; }
    }

    public record ServerListModel : BasePagedListModel<ServerModel>
    {
    }
}
