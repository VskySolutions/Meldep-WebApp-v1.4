using Microsoft.AspNetCore.Identity;

namespace Vsky.Models
{
    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public string? SiteId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ApplicationRole Role { get; set; }
        public virtual Site Site { get; set; }
    }
}