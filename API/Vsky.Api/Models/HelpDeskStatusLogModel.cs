using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record HelpDeskStatusLogModel : BaseEntityModel
    {
        public string HelpDeskId { get; set; }
        public string StatusId { get; set; }
        public int DurationInMinutes { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public virtual HelpDesk HelpDesk { get; set; }
        public virtual DropDown Status { get; set; }
    }
}
