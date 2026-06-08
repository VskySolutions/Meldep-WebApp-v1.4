using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class MovementRegisterList
    {
        public virtual ICollection<MovementRegister> MoveRegisterList { get; set; } = new List<MovementRegister>();
        public int Total { get; set; }
    }
    public class MovementRegister : BaseEntity
    {
        public string SiteId { get; set; }
        public DateTime? Date { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public string DateStr { get; set; }

        public virtual Site Sites { get; set; }
        public virtual ICollection<MovementRegisterDetails> MovementRegisterDetails { get; set; } = new List<MovementRegisterDetails>();
    }
    public class MovementRegisterDetails : BaseEntity
    {
        public string MomentRegisterId { get; set; }
        public string EmployeeId { get; set; }
        public string ApproverById { get; set; }
        public string TypeId { get; set; }
        public string WFHDurationId { get; set; }
        public string BreakTimeId { get; set; }
        public string Message { get; set; }
        public bool NotifyToStakeholders { get; set; }
        public int TimeInMinutes { get; set; }
        public string CreatedById { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }

        public virtual MovementRegister MomentRegisters { get; set; }
        public virtual Employee Employees { get; set; }
        public virtual Employee Approvers { get; set; }
        public virtual DropDown Type { get; set; }
        public virtual DropDown WFHDuration { get; set; }
        public virtual DropDown BreakTime { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }

        [NotMapped]
        public string CreatedTimeStr { get; set; }

        [NotMapped]
        public int SiteModifiedLogCount { get; set; }
    }
    public class SaveMovementRegister
    {
        public string MomentRegisterDetailsId { get; set; }
        public string MomentRegisterId { get; set; }
        public string EmployeeId { get; set; }
        public string ApproverById { get; set; }
        public string BreakTimeId { get; set; }
        public string WFHDurationId { get; set; }
        public string DateStr { get; set; }
        public string Message { get; set; }
        public DateTime? Date { get; set; }
        public string Type { get; set; }
        public int TimeInMinutes { get; set; }
        public bool NotifyToStakeholders { get; set; }
        public DateTime? TimeInDate { get; set; }
        //public string Date { get; set; }
    }

}
