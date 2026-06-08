using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Users
{
    public interface IUserService
    {
        IPagedList<ApplicationUser> GetAllUsersList(string SiteId, string SearchText, string userStatus, string userName, string fullName, string email, List<string> roleIds, string UserId, string sortBy,
            bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        Task<string> GetUserInitialAsync();

        Task<IList<ApplicationUser>> GetAdminUsersAsync();

        Task<IList<ApplicationUser>> GetUsersAsync(string SiteId);

        Task<ApplicationUser> GetById(string SiteId, string Id);
        Task<ApplicationUser> GetUserByEmail(string SiteId, string Email);
        Task<ApplicationUser> GetUserByEmployeeId(
            string siteId,
            string employeeId);

        Task<string> GetUserIdByRole(string SiteId, string role);
        Task<string> GetUserIdByPersonId(string SiteId, string personId);
        Task<List<ApplicationUser>> GetAllUserFirstNameListForDropdown(string SiteId);
        Task<List<ApplicationUser>> GetAllUserLastNameListForDropdown(string SiteId);
        Task<List<ApplicationUser>> GetAllUserListForDropdown(string SiteId, string flag = null);
        Task<string> GetUserFullNameAsync(ApplicationUser user, string SiteId);

        string GeneratePassword();
        Task<List<ApplicationUser>> GetUsersByRole(string SiteId, string role);
        Task<List<ApplicationUser>> GetSupportTeamUsersData(string siteId, string role);
        string GetIdByMigrateUser(string userName);
        string GetAdminUserId();
    }
}