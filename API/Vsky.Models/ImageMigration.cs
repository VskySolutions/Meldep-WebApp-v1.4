using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class ImageMigration : BaseEntity
    {
        public string TableName { get; set; }
        public string TableId { get; set; }
        public int TableNumber { get; set; }
        public int ImageNo { get; set; }
        public string Base64Data { get; set; }
        public string BlobURL { get; set; }
        public int IsProcessed { get; set; }
        public bool Deleted { get; set; }
    }
}
