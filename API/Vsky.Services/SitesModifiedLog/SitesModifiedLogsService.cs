using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

namespace Vsky.Services.SitesModifiedLog
{
    public class SitesModifiedLogsService : ISitesModifiedLogsService
    {
        private readonly IRepository<SitesModifiedLogs> _sitesModifiedLogRepository;
        public SitesModifiedLogsService(IRepository<SitesModifiedLogs> sitesModifiedLogRepository)
        {
            _sitesModifiedLogRepository = sitesModifiedLogRepository;
        }

        #region Private Methods
        // Title: GetOrderBy
        // Description: This method returns the input string as it is, which can be used as the `ORDER BY` clause in a SQL query.
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        public IPagedList<SitesModifiedLogs> GetAllSitesModifiedLogs(string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _sitesModifiedLogRepository.TableNoTracking;

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }

            query = query.Select(x => new SitesModifiedLogs
            {
                Id = x.Id,
                TableName = x.TableName,
                Module = x.Module,
                ModuleId = x.ModuleId,
                SubModule = x.SubModule,
                SubModuleId = x.SubModuleId,
                ColumnName = x.ColumnName,
                ColumnValue = x.ColumnValue,
                LastModifiedBy = x.LastModifiedBy,
                LastModifiedOnUtc = x.LastModifiedOnUtc,                
                user = new ApplicationUser
                {
                    Person = new Person
                    {
                        Id = x.user.Person.Id,
                        FullName = x.user.Person.FirstName + " " + x.user.Person.LastName,
                    }
                }
            });

            var list = new PagedList<SitesModifiedLogs>(query, page, pageSize);
            return list;
        }

        #region GetSitesModifiedLogsById
        public async Task<SitesModifiedLogs> GetSitesModifiedLogsById(string id)
        {
            var query = _sitesModifiedLogRepository.TableNoTracking.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion


        #region GetSitesModifiedLogDetailsById
        public async Task<SitesModifiedLogs> GetSitesModifiedLogDetailsById(string id)
        {
            var query = _sitesModifiedLogRepository.TableNoTracking.Where(x => x.ModuleId == id);
            query = query.Select(x => new SitesModifiedLogs
            {
                Id = x.Id,
                TableName = x.TableName,
                Module = x.Module,
                ModuleId = x.ModuleId,
                SubModule = x.SubModule,
                SubModuleId = x.SubModuleId,
                ColumnName = x.ColumnName,
                ColumnValue = x.ColumnValue,
                LastModifiedBy = x.LastModifiedBy,
                LastModifiedOnUtc = x.LastModifiedOnUtc,
                LastModifiedonUtcStr = x.LastModifiedOnUtc.ToString("MM/dd/yyyy hh:mm:ss tt"),
                user = new ApplicationUser
                {
                    Person = new Person
                    {
                        Id = x.user.Person.Id,
                        FullName = x.user.Person.FirstName + " " + x.user.Person.LastName,
                    }
                }
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<List<SitesModifiedLogs>> GetAllSitesModifiedLogDetailsById(string SiteId, string subModuleId, string columnName)
        {
            var query = _sitesModifiedLogRepository.Table
                .Where(x => !x.Deleted && x.SiteId == SiteId && x.SubModuleId == subModuleId && x.ColumnName == columnName).OrderByDescending(m => m.LastModifiedOnUtc)
                .Select(x => new SitesModifiedLogs
                {
                    Id = x.Id,
                    TableName = x.TableName,
                    Module = x.Module,
                    ModuleId = x.ModuleId,
                    SubModule = x.SubModule,
                    SubModuleId = x.SubModuleId,
                    ColumnName = x.ColumnName,
                    ColumnValue = x.ColumnValue,
                    LastModifiedBy = x.LastModifiedBy,
                    LastModifiedOnUtc = x.LastModifiedOnUtc,
                    LastModifiedonUtcStr = x.LastModifiedOnUtc.ToString("MM/dd/yyyy hh:mm:ss tt"),
                    user = new ApplicationUser
                    {
                        Person = new Person
                        {
                            Id = x.user.Person.Id,
                            FullName = x.user.Person.FirstName + " " + x.user.Person.LastName,
                        }
                    }
                });

            return await query.ToListAsync();
        }

        #endregion

        public void AddSiteModifiedLogs(string SiteId, string TableName, string ModuleId, string ModuleName, string SubModuleId, string SubModule, string ColumnName, string ColumnValue, string LoggedUserId, DateTime GetDateTime)
        {
            var siteLogs = new SitesModifiedLogs();
            siteLogs.SiteId = SiteId;
            siteLogs.TableName = TableName;
            siteLogs.ModuleId = ModuleId;
            siteLogs.Module = ModuleName;
            siteLogs.SubModuleId = SubModuleId;
            siteLogs.SubModule = SubModule;
            siteLogs.ColumnName = ColumnName;
            siteLogs.ColumnValue = ColumnValue;
            siteLogs.LastModifiedBy = LoggedUserId;
            siteLogs.LastModifiedOnUtc = GetDateTime;

            _sitesModifiedLogRepository.Insert(siteLogs);
        }

        #region InsertSitesModifiedLog
        public void InsertSitesModifiedLog(SitesModifiedLogs entity)
        {
            _sitesModifiedLogRepository.Insert(entity);
        }
        #endregion

        #region UpdateSitesModifiedLog
        public void UpdateSitesModifiedLog(SitesModifiedLogs entity)
        {
            _sitesModifiedLogRepository.Update(entity);
        }
        #endregion

    }
}
