using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record HelpDeskFilesModel : BaseEntityModel
    {
        public string ProjectTaskId { get; set; }
        public string FileId { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public virtual ProjectTask ProjectTask { get; set; }
        public virtual Picture File { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
    public record HelpDeskFilesSearchModel : BaseSearchModel
    {
    }

    public record HelpDeskFilesListModel : BasePagedListModel<HelpDeskFilesModel>
    {
        public bool editing { get; set; }
    }
}

