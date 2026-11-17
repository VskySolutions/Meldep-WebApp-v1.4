using System.Threading.Tasks;

namespace Vsky.Services.Authorization
{
    /// <summary>The only place that decides what a user may do in a site.</summary>
    public interface IPermissionService
    {
        Task<PermissionSnapshot> GetSnapshotAsync(string userId, string siteId);
    }
}
