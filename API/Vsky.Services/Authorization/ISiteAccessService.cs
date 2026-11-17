using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vsky.Services.Authorization
{
    /// <summary>The single rule for whether a user may act in a site.</summary>
    public interface ISiteAccessService
    {
        Task<bool> IsSiteMemberAsync(string userId, string siteId);

        /// <summary>Sites the user may act in, oldest membership first, so the order is stable.</summary>
        Task<List<string>> GetAccessibleSiteIdsAsync(string userId);

        /// <summary>
        /// The site a user works in when no site is named: their last-used site if they may still act
        /// in it, otherwise their oldest accessible site. Login opens this site, and a request that sends
        /// no X-Site-Id header (Swagger, Postman, scripts) uses it, so both see the same site. Null if
        /// the user can act in no site.
        /// </summary>
        Task<string> GetDefaultSiteIdAsync(string userId);
    }
}
