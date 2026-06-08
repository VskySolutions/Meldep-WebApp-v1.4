using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record PicturesModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string Type { get; set; }
        public string ModuleId { get; set; }
        public string Module { get; set; }
        public string SubModuleId { get; set; }
        public string Sub_Module { get; set; }
        public string MimeType { get; set; }

        public string SeoFilename { get; set; }

        public string AltAttribute { get; set; }

        public string TitleAttribute { get; set; }
        public string VirtualPath { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public bool Deleted { get; set; }
        [NotMapped]
        public string CreatedDateStr { get; set; }
        public virtual Site Site { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
    public record PicturesSearchModel : BaseSearchModel
    {
        public string ProjectId { get; set; }
    }
    public record PicturesListModel : BasePagedListModel<PicturesModel>
    {
        public bool editing { get; set; }
    }
}