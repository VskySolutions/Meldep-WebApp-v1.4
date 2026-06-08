using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;

namespace Vsky.Api.Models
{
    public class MovementRegisterModel
    {
    }
    public record MovementRegisterSearchModel : BaseSearchModel
    {
        public string SearchText { get; set; }
        public string CreatedBy { get; set; }
        public List<string> EmployeeIds { get; set; }
        public string EmployeeId { get; set; }
        public string TypeId { get; set; }
        public bool IsViewMore { get; set; }
        public DateTime? DateStr { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

    }
}
