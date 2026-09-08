using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models;

public class FilePathDetails : BaseEntity
{
    public string SiteId { get; set; }
    public string ModuleId { get; set; }
    public string SubModuleId { get; set; }
    public string Module { get; set; }
    public string Sub_Module { get; set; }
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public string Note { get; set; }
    public string Type { get; set; }

    public DateTime CreatedOnUtc { get; set; }
    public string CreatedById { get; set; }
    public string UpdatedById { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool Deleted { get; set; }

    public virtual Requirement Requirement { get; set; }
    public virtual Site Site { get; set; }
}
