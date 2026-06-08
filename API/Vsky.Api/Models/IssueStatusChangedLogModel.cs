using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record IssueStatusChangedLogModel : BaseEntityModel
    {
        public string IssueId { get; set; }
        public string StatusId { get; set; }
        public string StatusChangedBy { get; set; }
        public DateTime StatusChangedDate { get; set; }

        public virtual Issue Issue { get; set; }
        public virtual DropDown Status { get; set; }
        public virtual Employee StatusChangedByEmployee { get; set; }

        public record IssueStatusChangedLogSearchModel : BaseSearchModel
        {
        }

        public record IssueStatusChangedLogListModel : BasePagedListModel<IssueStatusChangedLogModel>
        {
            public bool editing { get; set; }
        }
        public record IssueStatusChangedLogUploadModel : BaseEntityModel
        {
        }
    }
}
