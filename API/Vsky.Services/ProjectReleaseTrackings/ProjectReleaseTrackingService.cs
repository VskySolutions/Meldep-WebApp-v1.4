using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;

namespace Vsky.Services.ProjectReleaseTrackings
{
    public class ProjectReleaseTrackingService : IProjectReleaseTrackingService
    {
        #region Define Services
        private readonly IRepository<ProjectReleaseTracking> _projectReleaseTrackingRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations
        public ProjectReleaseTrackingService(
            IRepository<ProjectReleaseTracking> projectReleaseTrackingRepository,
            UserManager<ApplicationUser> userManager,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _projectReleaseTrackingRepository = projectReleaseTrackingRepository;
            _userManager = userManager;
            _applicationUserRoleService = applicationUserRoleService;
        }
        #endregion

        #region Private Methods
        // Title: GetOrderBy
        // Description: This method returns the input string as it is, which can be used as the `ORDER BY` clause in a SQL query.
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllProjectReleaseTrackingList
        public async Task<IPagedList<ProjectReleaseTracking>> GetAllProjectReleaseTrackingList(
            string siteId,
            string searchText,
            string LoggedUserId,
            List<string> projectIds,
            List<string> infraInstanceIds,
            List<string> deploymentOwnerIds,
            List<string> approverIds,
            List<string> testerIds,
            List<string> releaseTypeIds,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {
            var query = _projectReleaseTrackingRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);

            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, siteId);

            if (!IsAdmin)
                query = query.Where(p => p.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId));

            if (projectIds?.Any() == true) query = query.Where(x => projectIds.Contains(x.ProjectId));
            if (infraInstanceIds?.Any() == true) query = query.Where(x => infraInstanceIds.Contains(x.InfraInstanceId));
            if (deploymentOwnerIds?.Any() == true) query = query.Where(x => deploymentOwnerIds.Contains(x.DeploymentOwnerId));
            if (approverIds?.Any() == true) query = query.Where(x => approverIds.Contains(x.ApproverId));
            if (testerIds?.Any() == true) query = query.Where(x => testerIds.Contains(x.TesterId));
            if (releaseTypeIds?.Any() == true) query = query.Where(x => releaseTypeIds.Contains(x.ReleaseTypeId));

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();

                query = query.Where(m =>
                    m.Project.Name.ToLower().Contains(searchText) ||
                    m.Name.ToLower().Contains(searchText) ||
                    m.InfraInstance.InstanceType.DropDownValue.ToLower().Contains(searchText) ||
                    (m.DeploymentOwner.Person.FirstName + " " + m.DeploymentOwner.Person.LastName)
                        .ToLower()
                        .Contains(searchText) ||
                    (m.ProjectReleaseTrackingStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.DropDownValue).FirstOrDefault() ?? "").ToLower().Contains(searchText) ||
                    m.ReleaseType.DropDownValue.ToLower().Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "statusId")
                {
                    query = descending
                        ? query.OrderByDescending(x =>
                            x.ProjectReleaseTrackingStatusLog
                             .OrderByDescending(p => p.CreatedOnUtc)
                             .Select(p => p.Status.Id)
                             .FirstOrDefault())
                        : query.OrderBy(x =>
                            x.ProjectReleaseTrackingStatusLog
                             .OrderByDescending(p => p.CreatedOnUtc)
                             .Select(p => p.Status.Id)
                             .FirstOrDefault());
                }
                else
                {
                    var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                    query = query.OrderBy(orderBy);
                }
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            query = query.Select(x => new ProjectReleaseTracking
            {
                Id = x.Id,
                VersionNumber = x.VersionNumber,
                Name = x.Name,
                PlannedReleaseDate = x.PlannedReleaseDate,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    ProjectUserMappings = x.Project.ProjectUserMappings.Where(m => !m.Deleted && m.ProjectId == x.Project.Id && (IsAdmin || m.AspNetUserId == LoggedUserId)).Take(1).Select(m => new ProjectUserMapping
                    {
                        Id = m.Id,
                        FullAccess = m.FullAccess,
                        ViewOnly = m.ViewOnly,
                        Notes = m.Notes
                    }).ToList(),
                },
                InfraInstance = new InfraProjectInstance
                {
                    Id = x.InfraInstance.Id,
                    URL = x.InfraInstance.URL,
                    InstanceType = new DropDown
                    {
                        DropDownValue = x.InfraInstance.InstanceType.DropDownValue
                    }
                },
                DeploymentOwner = new Employee
                {
                    Person = new Person
                    {
                        FullName = x.DeploymentOwner.Person.FirstName + " " + x.DeploymentOwner.Person.LastName
                    }
                },
                StatusText = x.ProjectReleaseTrackingStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.DropDownValue).FirstOrDefault(),
                StatusId = x.ProjectReleaseTrackingStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.Id).FirstOrDefault(),
            });

            var list = new PagedList<ProjectReleaseTracking>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetProjectReleaseTrackingById
        // Title: GetProjectReleaseTrackingById
        // Description: This method retrieves a ProjectReleaseTracking from the database by its unique identifier (`id`). 
        public async Task<ProjectReleaseTracking> GetProjectReleaseTrackingById(string id)
        {
            var query = _projectReleaseTrackingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GenerateNextVersion
        public string GenerateNextVersion(string lastVersion, string releaseType)
        {
            if (string.IsNullOrEmpty(lastVersion))
                return "1.0.0";

            var parts = lastVersion.Split('.')
                .Select(x => int.TryParse(x, out var val) ? val : 0)
                .ToArray();

            int major = parts.Length > 0 ? parts[0] : 0;
            int minor = parts.Length > 1 ? parts[1] : 0;
            int patch = parts.Length > 2 ? parts[2] : 0;

            switch (releaseType.ToLower())
            {
                case "major":
                    major += 1;
                    minor = 0;
                    patch = 0;
                    break;

                case "minor":
                case "patch":
                    minor += 1;
                    patch = 0;
                    break;

                case "hotfix":
                    patch += 1;
                    break;

                default:
                    throw new Exception("Invalid release type");
            }

            return $"{major}.{minor}.{patch}";
        }
        public async Task<string> GetLastVersion(string projectId)
        {
            return await _projectReleaseTrackingRepository.TableNoTracking
                .Where(x => !x.Deleted && x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedOnUtc)
                .Select(x => x.VersionNumber)
                .FirstOrDefaultAsync();
        }
        public async Task<string> GenerateVersionNumber(string projectId, string releaseType)
        {
            var lastVersion = await GetLastVersion(projectId);

            var nextVersion = GenerateNextVersion(lastVersion, releaseType);

            return nextVersion;
        }
        #endregion

        #region GetProjectReleaseTrackingInDetailsById
        // Title: GetProjectReleaseTrackingInDetailsById
        // Description: The method selects relevant fields from the Infra Account entity
        public async Task<ProjectReleaseTracking> GetProjectReleaseTrackingInDetailsById(string id)
        {
            var query = _projectReleaseTrackingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id)
                .Select(x => new ProjectReleaseTracking
                {
                    Id = x.Id,
                    Name = x.Name,
                    VersionNumber = x.VersionNumber,
                    PlannedReleaseDate = x.PlannedReleaseDate,
                    Description = x.Description,
                    CreatedOnUtc = x.CreatedOnUtc,
                    UpdatedOnUtc = x.UpdatedOnUtc,
                    Project = new Project
                    {
                        Id = x.Project.Id,
                        Name = x.Project.Name
                    },
                    InfraInstance = new InfraProjectInstance
                    {
                        Id = x.InfraInstance.Id,
                        URL = x.InfraInstance.URL
                    },
                    DeploymentOwner = new Employee
                    {
                        Id = x.DeploymentOwner.Id,
                        Person = new Person
                        {
                            FullName = x.DeploymentOwner.Person.FirstName + " " + x.DeploymentOwner.Person.LastName
                        }
                    },
                    Approver = new Employee
                    {
                        Id = x.Approver.Id,
                        Person = new Person
                        {
                            FullName = x.Approver.Person.FirstName + " " + x.Approver.Person.LastName
                        }
                    },
                    Tester = new Employee
                    {
                        Id = x.Tester.Id,
                        Person = new Person
                        {
                            FullName = x.Tester.Person.FirstName + " " + x.Tester.Person.LastName
                        }
                    },
                    ReleaseType = new DropDown
                    {
                        Id = x.ReleaseType.Id,
                        DropDownValue = x.ReleaseType.DropDownValue
                    },
                    CreatedBy = new ApplicationUser
                    {
                        Id = x.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = x.CreatedBy.PersonId,
                            FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                        }
                    },
                    UpdatedBy = new ApplicationUser
                    {
                        Id = x.UpdatedBy.Id,
                        Person = new Person
                        {
                            Id = x.UpdatedBy.PersonId,
                            FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName
                        }
                    },
                    StatusText = x.ProjectReleaseTrackingStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.DropDownValue).FirstOrDefault(),
                });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertProjectReleaseTracking
        public void InsertProjectReleaseTracking(Models.ProjectReleaseTracking entity)
        {
            _projectReleaseTrackingRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectReleaseTracking
        public void UpdateProjectReleaseTracking(Models.ProjectReleaseTracking entity)
        {
            _projectReleaseTrackingRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectReleaseTracking
        public void DeleteProjectReleaseTracking(Models.ProjectReleaseTracking entity)
        {
            entity.Deleted = true;
            _projectReleaseTrackingRepository.Update(entity);
        }
        #endregion

        #region private methods
        private async Task<bool> IsCurrentUserAdmin(string CId, string SiteId)
        {
            var userdata = await _userManager.FindByIdAsync(CId);
            var user = await _userManager.FindByNameAsync(userdata.UserName);
            //var roles = await _userManager.GetRolesAsync(user);
            var roles = await _applicationUserRoleService.GetRoleNamesByUserAndSite(user.Id, SiteId);
            var isAdmin = roles.Contains("Admin") || roles.Contains("Site Super Admin") || roles.Contains("System Super Admin");

            return isAdmin;
        }
        #endregion
    }
}
