using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record NoteModel : BaseEntityModel
    {
        public string SiteId { get; set; }
        public string Note { get; set; }
        public string Type { get; set; }
        public string SubModuleId { get; set; }
        public string Module { get; set; }
        public string ModuleId { get; set; }
        public string Sub_Module { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        [NotMapped]
        public string CreatedDateStr { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }
        public string TaggedPersonId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Site Site { get; set; }
    }
    public record NoteSearchModel : BaseSearchModel
    {
        public string ProjectId { get; set; }
    }

    public record NoteListModel : BasePagedListModel<NoteModel>
    {
        public bool editing { get; set; }
    }
}
