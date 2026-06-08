using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record ProjectTaskStatusLogModel : BaseEntityModel
    {
        public string TaskId { get; set; }
        public string StatusId { get; set; }
        public string StatusChangedBy { get; set; }
        public DateTime StatusChangedDate { get; set; }

        public virtual ProjectTask Task { get; set; }
        public virtual DropDown Status { get; set; }
        public virtual Employee StatusChangedByEmployee { get; set; }

        public record ProjectTaskStatusLogSearchModel : BaseSearchModel
        {
        }

        public record ProjectTaskStatusLogListModel : BasePagedListModel<ProjectTaskStatusLogModel>
        {
            public bool editing { get; set; }
        }
        public record ProjectTaskStatusLogUploadModel : BaseEntityModel
        {
        }
    }
}
