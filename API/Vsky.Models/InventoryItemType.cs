using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models;

public class InventoryItemType : BaseEntity
{
    public string SiteId { get; set; }
    public string Name { get; set; }
    public string Prefix { get; set; }
    public int SortOrder { get; set; }
    public virtual Site Sites { get; set; }
}
