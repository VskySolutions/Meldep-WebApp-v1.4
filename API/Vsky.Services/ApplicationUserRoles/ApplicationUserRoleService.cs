using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ApplicationUserRoles
{
    public class ApplicationUserRoleService : IApplicationUserRoleService
    {

        #region Define Services
        private readonly ApplicationDbContext _db;
        #endregion

        #region Services Initializations
        public ApplicationUserRoleService(
            ApplicationDbContext db
        )
        {
            _db = db;
        }
        #endregion

        #region GetUserRoles
        public async Task<List<ApplicationUserRole>> GetUserRoles(string userId, string siteId)
        {
            return await _db.UserRoles.Where(x => x.UserId == userId && x.SiteId == siteId).ToListAsync();
        }
        #endregion

        #region
        public async Task<bool> IsRoleExists(string userId, string roleId, string siteId)
        {
            return await _db.UserRoles
                .AnyAsync(x =>
                    x.UserId == userId &&
                    x.RoleId == roleId &&
                    x.SiteId == siteId);
        }
        #endregion

        #region GetRoleIdsByUserAndSite
        public async Task<List<string>> GetRoleIdsByUserAndSite(string userId, string siteId)
        {
            return await _db.UserRoles
                .Where(x => x.UserId == userId && x.SiteId == siteId)
                .Select(x => x.RoleId)
                .ToListAsync();
        }

        #endregion

        #region GetNormalizedRoleNamesByUserAndSite
        public async Task<List<string>> GetNormalizedRoleNamesByUserAndSite(string userId, string siteId)
        {
            var roleIds = await _db.UserRoles
                .Where(x => x.UserId == userId && x.SiteId == siteId)
                .Select(x => x.RoleId)
                .ToListAsync();

            return await _db.Roles
                .Where(x => roleIds.Contains(x.Id))
                .Select(x => x.NormalizedName)
                .ToListAsync();
        }
        #endregion

        #region GetRoleNamesByUserAndSite
        public async Task<List<string>> GetRoleNamesByUserAndSite(string userId, string siteId)
        {
            var roleIds = await _db.UserRoles
                .Where(x => x.UserId == userId && x.SiteId == siteId)
                .Select(x => x.RoleId)
                .ToListAsync();

            return await _db.Roles
                .Where(x => roleIds.Contains(x.Id))
                .Select(x => x.Name)
                .ToListAsync();
        }
        #endregion

        #region AddUserRoleAsync
        public async Task AddUserRoleAsync(string userId, string roleId, string siteId)
        {
            var exists = await _db.UserRoles.AnyAsync(x =>
                x.UserId == userId &&
                x.RoleId == roleId &&
                x.SiteId == siteId);

            _db.UserRoles.Add(new ApplicationUserRole
            {
                UserId = userId,
                RoleId = roleId,
                SiteId = siteId
            });

            //if (!exists)
            //{
            //    _db.UserRoles.Add(new ApplicationUserRole
            //    {
            //        UserId = userId,
            //        RoleId = roleId,
            //        SiteId = siteId
            //    });

            //    //await _db.SaveChangesAsync();
            //}
        }
        #endregion

        #region RemoveAllUserRolesAsync
        public async Task RemoveAllUserRolesAsync(string userId, string siteId)
        {
            var userRoles = await _db.UserRoles
                .Where(x => x.UserId == userId && x.SiteId == siteId)
                .ToListAsync();

            if (userRoles.Any())
            {
                _db.UserRoles.RemoveRange(userRoles);
            }
        }
        #endregion

        #region InsertApplicationUserRole
        public void InsertApplicationUserRole(ApplicationUserRole entity)
        {
            _db.Add(entity);
        }
        #endregion

        #region UpdateApplicationUserRole
        public void UpdateApplicationUserRole(ApplicationUserRole entity)
        {
            _db.Update(entity);
        }
        #endregion

        #region DeleteApplicationUserRole
        public void DeleteApplicationUserRole(ApplicationUserRole entity)
        {
            _db.Remove(entity);
        }
        #endregion
    }
}
