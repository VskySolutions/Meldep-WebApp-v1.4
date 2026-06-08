using System;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record CustomerFilesLinesModel : BaseEntityModel
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
