using System;
using System.Collections.Generic;
using Vsky.Api.Framework.Models;
using Vsky.Models;

namespace Vsky.Api.Models
{
    public record UserModel : BaseEntityModel
    {
        public string employeeId { get; set; }
        public string employeeEmail { get; set; }
        public string personId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        public string RoleId { get; set; }
        public string Password { get; set; }
        public string[] RoleIds { get; set; }
        public string[] SiteRoleIds { get; set; }
        public bool Active { get; set; }
        public string SiteId { get; set; }
        public bool SendEmail { get; set; }
        public string Type { get; set; }

        public virtual Person Person { get; set; }
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }

    public record UserSearchModel : BaseSearchModel
    {
        public string UserStatus { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string SiteId { get; set; }
        public List<string> SiteRoleIds { get; set; }
        public string SearchText { get; set; }
    }

    public record UserListModel : BasePagedListModel<UserModel>
    {
    }
}