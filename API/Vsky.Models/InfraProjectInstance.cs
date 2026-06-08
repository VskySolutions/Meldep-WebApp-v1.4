using System;
using System.Collections.Generic;
using Vsky.Core;

namespace Vsky.Models
{
    public class InfraProjectInstanceList
    {
        public virtual ICollection<InfraProjectInstance> InfraProjectInstancesList { get; set; } = new List<InfraProjectInstance>();
        public int Total { get; set; }
    }
    public class InfraProjectInstance : BaseEntity
    {
        public string SiteId { get; set; }
        public string InfraProjectId { get; set; }
        public string InstanceTypeId { get; set; }
        public string PlatformId { get; set; }
        public string URL { get; set; }
        public string Instructions { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual Project InfraProject { get; set; }
        public virtual DropDown InstanceType { get; set; }
        public virtual DropDown Platform { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<InfraProjectInstanceRole> InfraProjectInstanceRole { get; set; } = new List<InfraProjectInstanceRole>();
    }
    public class InfraProjectInstanceRole : BaseEntity
    {
        public string ProjectInstanceId { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual InfraProjectInstance ProjectInstance { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
        public virtual ICollection<InfraProjectInstanceRoleUsers> InfraProjectInstanceRoleUsers { get; set; } = new List<InfraProjectInstanceRoleUsers>();
    }

    public class InfraProjectInstanceRoleUsers : BaseEntity
    {
        public string ProjectInstanceRoleId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public string UpdatedById { get; set; }
        public bool Deleted { get; set; }

        public virtual InfraProjectInstanceRole ProjectInstanceRole { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public virtual ApplicationUser UpdatedBy { get; set; }
    }
    public class SaveInfraProjectInstance
    {
        public string Id { get; set; }
        public string InfraProjectId { get; set; }
        public string InstanceTypeId { get; set; }
        public string PlatformId { get; set; }
        public string URL { get; set; }
        public string Instructions { get; set; }
        public bool Deleted { get; set; }
    }
    public class SaveInfraProjectInstanceRole
    {
        public string Id { get; set; }
        public string ProjectInstanceId { get; set; }
        public string RoleName { get; set; }
        public bool Deleted { get; set; }
        public string Flag { get; set; }
        public List<SaveInfraProjectInstanceRoleUsers> InfraProjectInstanceRoleUserList { get; set; } = new List<SaveInfraProjectInstanceRoleUsers>();
    }
    public class SaveInfraProjectInstanceRoleUsers
    {
        public string Id { get; set; }
        public string ProjectInstanceRoleId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Deleted { get; set; }
        public string Flag { get; set; }
    }
    public class SaveInfraProjectInstanceRoleRequest
    {
        public string ProjectInstanceId { get; set; }

        public List<SaveInfraProjectInstanceRole> InfraProjectInstanceRoleList { get; set; } = new();
    }
    public class SaveInfraProjectInstanceList
    {
        public List<SaveInfraProjectInstance> InfraProjectInstanceLines { get; set; } = new List<SaveInfraProjectInstance>();
    }
}


