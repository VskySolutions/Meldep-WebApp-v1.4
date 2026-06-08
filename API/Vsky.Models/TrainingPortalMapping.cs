using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;

namespace Vsky.Models
{
    public class Training_Portal_Mapping : BaseEntity
    {
        public string TrainingId { get; set; }
        public string EmployeeDesignationId { get; set; }
        public virtual DropDown EmployeeDesignationType { get; set; }
        public virtual TrainingPortal Training { get; set; }
    }
}
