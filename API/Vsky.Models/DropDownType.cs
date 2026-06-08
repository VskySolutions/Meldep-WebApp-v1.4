using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models;

public class DropDownType : BaseEntity
{
    public string Type { get; set; }
    public string SiteId { get; set; }
    public string DisplayName { get; set; }
    public string GroupName { get; set; }
    public string ModuleName { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public string CreatedById { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public bool IsAlphabeticalOrNumerical { get; set; }
    public bool Deleted { get; set; }

    [NotMapped]
    public virtual ICollection<DropDownType> DropDownTypeList { get; set; } = new List<DropDownType>();
}