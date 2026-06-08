using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class AdPostingStatus : BaseEntity
{
    public string AdId { get; set; }
    public string AdPostChannelId { get; set; }
    public DateTime Date { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
    public string CreatedById { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public bool Deleted { get; set; }

    public virtual AdPost AdPost { get; set; }
    public virtual AdPostChannel AdPostChannel { get; set; }
}

