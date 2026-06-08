using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Vsky.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string PersonId { get; set; }
        public string Type { get; set; }

        public string CreatedById { get; set; }

        public DateTime? CreatedOnUtc { get; set; }

        public string UpdatedById { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }

        public bool Active { get; set; }

        public bool Deleted { get; set; }

        [NotMapped]
        public string EmployeeId { get; set; }
        [NotMapped]
        public EmployeeTicketCount TicketCounts { get; set; }

        [NotMapped]
        public string SupportTeamUserName { get; set; }
        
        public virtual Person Person { get; set; }

        public virtual ICollection<ApplicationUserClaim> Claims { get; set; }

        public virtual ICollection<ApplicationUserLogin> Logins { get; set; }

        public virtual ICollection<ApplicationUserToken> Tokens { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
        public List<VWEmployeeAssignedHours> EmployeeAssignedHours { get; set; }

    }
    public class EmployeeTicketCount
    {
        public int Total { get; set; }
    }
}