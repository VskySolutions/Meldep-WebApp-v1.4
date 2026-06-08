using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ProjectFilesModel : BaseEntityModel
    {
        public string ProjectId { get; set; }
        public string FileId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public virtual Project Project { get; set; }
        public virtual Picture File { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
    public record ProjectFilesSearchModel : BaseSearchModel
    {
    }

    public record ProjectFilesListModel : BasePagedListModel<ProjectFilesModel>
    {
        public bool editing { get; set; }
    }
}

