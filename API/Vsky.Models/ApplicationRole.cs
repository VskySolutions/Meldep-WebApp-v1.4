using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Vsky.Models
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }

        public ApplicationRole(string name) : base(name) { }
        public string NormalizedName { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string SiteId { get; set; }
        public bool IsPrimaryRole { get; set; }
        public DateTime? CreatedOnUtc { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
        public ICollection<ApplicationUserRole> UserRoles { get; set; }

        public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; }
    }
}