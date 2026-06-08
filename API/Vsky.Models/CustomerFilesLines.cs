using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class CustomerFilesLines : BaseEntity
    {
        public string CustomerFileId { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public int SortOrder { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string? CreatedById { get; set; }
        public string? UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public virtual CustomerFiles? CustomerFiles { get; set; }
    }
}
