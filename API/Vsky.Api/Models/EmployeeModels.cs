using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record EmployeeModel : BaseEntityModel
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

        public bool Active { get; set; } = true;
        public string StartDateStr { get; set; }
        public string JoiningDateStr { get; set; }
        public string ReleaseDateStr { get; set; }

        public string EndDateStr { get; set; }
        public string Tab { get; set; }
        //public string EmployeeTypeId { get; set; }

        //employee address
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CountryId { get; set; }
        public string StateProvinceId { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }

        public decimal EstimateHrs { get; set; }

        public virtual PersonModel Person { get; set; }

        public virtual AddressModels Address { get; set; }

        public virtual ICollection<EmployeeTypeModel> EmployeeTypeModel { get; set; } = new List<EmployeeTypeModel>();
        public virtual ICollection<EmployeeStatusModel> EmployeeStatusModel { get; set; } = new List<EmployeeStatusModel>();
        public virtual ICollection<EmployeeDepartmentModel> EmployeeDepartmentModel { get; set; } = new List<EmployeeDepartmentModel>();
        public virtual ICollection<EmployeeDesignationModel> EmployeeDesignationModel { get; set; } = new List<EmployeeDesignationModel>();
        public virtual ICollection<EmployeeOrgLocationModel> EmployeeOrgLocationModel { get; set; } = new List<EmployeeOrgLocationModel>();
        public virtual ICollection<EmployeeClientLocationModel> EmployeeClientLocationModel { get; set; } = new List<EmployeeClientLocationModel>();

        public virtual ICollection<EmployeeDepartment> EmployeeDepartment { get; set; } = new List<EmployeeDepartment>();
        public virtual ICollection<EmployeeDesignation> EmployeeDesignation { get; set; } = new List<EmployeeDesignation>();
        public virtual ICollection<EmployeeStatus> EmployeeStatuses { get; set; } = new List<EmployeeStatus>();
        public virtual ICollection<EmployeeType> EmployeeType { get; set; } = new List<EmployeeType>();
        public virtual ICollection<EmployeeOrgLocation> EmployeeOrgLocation { get; set; } = new List<EmployeeOrgLocation>();
        public virtual ICollection<EmployeeClientLocation> EmployeeClientLocation { get; set; } = new List<EmployeeClientLocation>();
        public List<VWEmployeeAssignedHours> EmployeeAssignedHours { get; set; }
    }

    public record EmployeeSearchModel : BaseSearchModel
    {
        public List<string> EmployeeIds { get; set; }
        public List<string> EmployeeTypeIds { get; set; }
        public List<string> EmployeeDepartmentIds { get; set; }
        public List<string> EmployeeDesignationIds { get; set; }
        public List<string> OrgLocationIds { get; set; }
        public string EmployeeStatus { get; set; }
        public string EmployeeCode { get; set; }
        public string PrimaryEmailAddress { get; set; }
        public string AllStatusId { get; set; }
        public string SearchText { get; set; }

    }
    public record EmployeeListModel : BasePagedListModel<EmployeeModel>
    {
       // public bool editing { get; set; }
    }
}