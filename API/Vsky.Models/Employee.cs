using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Vsky.Core;

namespace Vsky.Models;

public class Employee : BaseEntity
{
    public string SiteId { get; set; }

    public string PersonId { get; set; }

    public string EmployeeCode { get; set; }

    public string OfficialEmail { get; set; }

    public string EmergencyContactName { get; set; }

    public string EmergencyPhoneNo { get; set; }    

    public bool? SameASPermanentAddress { get; set; }

    public string AddressId { get; set; }

    public string AadhaarCardNo { get; set; }

    public string PanCardNo { get; set; }

    public string EPFUANNo { get; set; }

    public DateTime? JoiningDate { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public string EducationDetail { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public string CreatedById { get; set; }

    public string UpdatedById { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }

    public bool Active { get; set; }

    public bool Deleted { get; set; }

    [NotMapped]
    public decimal EstimateHrs { get; set; }

    [NotMapped]
    public int YearsCompleted { get; set; }
    public virtual Person Person { get; set; }
    public virtual Address Address { get; set; }

    public virtual ICollection<EmployeeDepartment> EmployeeDepartment { get; set; } = new List<EmployeeDepartment>();
    public virtual ICollection<EmployeeDesignation> EmployeeDesignation { get; set; } = new List<EmployeeDesignation>();
    public virtual ICollection<EmployeeStatus> EmployeeStatuses { get; set; } = new List<EmployeeStatus>();
    public virtual ICollection<EmployeeType> EmployeeType { get; set; } = new List<EmployeeType>();
    public virtual ICollection<EmployeeOrgLocation> EmployeeOrgLocation { get; set; } = new List<EmployeeOrgLocation>();
    public virtual ICollection<EmployeeClientLocation> EmployeeClientLocation { get; set; } = new List<EmployeeClientLocation>();
    public virtual ICollection<ProjectEmployeeMapping> ProjectEmployeeMappings { get; set; } = new List<ProjectEmployeeMapping>();
    //public virtual ICollection<EmployeeDesignation> EmployeeDesignationModel { get; set; } = new List<EmployeeDesignation>();

    public virtual ICollection<ProjectActivity> ProjectActivities { get; set; } = new List<ProjectActivity>();
    public List<VWEmployeeAssignedHours> EmployeeAssignedHours { get; set; }

}


