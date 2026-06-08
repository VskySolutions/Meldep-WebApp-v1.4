using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class DropDown : BaseEntity
{
    public string DropDownTypeId { get; set; }

    public string DropDownValue { get; set; }
    public string DropDownText { get; set; }
    public string Description { get; set; }
    public int SortOrder { get; set; }
    public string BgColor { get; set; }
    public string Color { get; set; }
    public bool Active { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public string UpdatedById { get; set; }
    public virtual DropDownType DropDownType { get; set; }
}
public class CommonDropDown
{
    public string Value { get; set; }
    public string Text { get; set; }
    public string DisplayText { get; set; }
    public string BgColor { get; set; }
    public string Color { get; set; }
    public bool Disable { get; set; }
    public string StatusText { get; set; }
}