using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Common;

namespace Vsky.Services.Projects
{
    public class ProjectService : IProjectService
    {
        #region Define Services
        private readonly ApplicationDbContext _dbContext;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Notes> _notesRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICommonService _commonService;
        private readonly IRepository<InfraProjectServices> _infraProjectServicesRepository;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations
        public ProjectService(
            ApplicationDbContext dbContext,
            IRepository<Project> projectRepository,
            IRepository<Notes> notesRepository,
            UserManager<ApplicationUser> userManager,
            ICommonService commonService,
            IRepository<InfraProjectServices> infraProjectServicesRepository,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _dbContext = dbContext;
            _projectRepository = projectRepository;
            _notesRepository = notesRepository;
            _userManager = userManager;
            _commonService = commonService;
            _infraProjectServicesRepository = infraProjectServicesRepository;
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

        #region GetAllProjects
        // Title: GetAllProjects
        // Description: This method retrieves a paginated list of projects based on various search criteria such as project name, 
        // project status, and team member. It also supports sorting and includes related data such as project status and employee 
        // mappings. The method allows for both full and lookup (limited) data retrieval modes.
        public async Task<IPagedList<Project>> GetAllProjects(
            string siteId,
            bool isTemplate,
            string userId,
            string searchText,
            List<string> projectIds,
            List<string> projectCategoryIds,
            List<string> statusIds,
            List<string> teamMemberIds,
            List<string> coordinatorIds,
            List<string> leadIds,
            List<string> priorityIds,
            List<string> typeIds,
            int status,
            List<string> customerIds,
            List<string> companyContactIds,
            string singleCustomerId,
            List<string> projectTagIds,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.IsTemplate == isTemplate);

            bool isAdmin = await IsCurrentUserAdmin(userId, siteId);
            if (!isAdmin)
                query = query.Where(p => p.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == userId && (m.FullAccess || m.ViewOnly || m.Notes)));

            if (projectIds?.Any() == true) query = query.Where(x => projectIds.Contains(x.Id));
            if (projectCategoryIds?.Any() == true) query = query.Where(x => projectCategoryIds.Contains(x.ProjectCategoryId));
            if (statusIds?.Any() == true) query = query.Where(x => statusIds.Contains(x.ProjectStatusId));
            if (status == 0) query = query.Where(x => !x.Active);
            else if (status == 1) query = query.Where(x => x.Active);
            if (teamMemberIds?.Any() == true) query = query.Where(x => x.ProjectEmployeeMappings.Any(m => teamMemberIds.Contains(m.EmployeeId)));
            if (coordinatorIds?.Any() == true) query = query.Where(x => x.ProjectEmployeeMappings.Any(m => coordinatorIds.Contains(m.EmployeeId) && !m.Deleted && m.EmployeeRoleDropdown.DropDownValue == "Project Coordinator"));
            if (leadIds?.Any() == true) query = query.Where(x => x.ProjectEmployeeMappings.Any(m => leadIds.Contains(m.EmployeeId) && !m.Deleted && m.EmployeeRoleDropdown.DropDownValue == "Project Lead"));
            if (priorityIds?.Any() == true) query = query.Where(x => priorityIds.Contains(x.ProjectPriorityId));
            if (typeIds?.Any() == true) query = query.Where(x => typeIds.Contains(x.ProjectTypeId));
            if (customerIds?.Any() == true) query = query.Where(x => customerIds.Contains(x.CustomerId));
            if (companyContactIds?.Any() == true) query = query.Where(x => companyContactIds.Contains(x.CompanyContactId));
            if (!string.IsNullOrWhiteSpace(singleCustomerId)) query = query.Where(x => x.CustomerId == singleCustomerId);
            if (projectTagIds?.Any() == true) query = query.Where(x => x.ProjectTags.Any(t => !t.Deleted && t.AspNetUserId == userId && projectTagIds.Contains(t.Tags.Id)));

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderByDescending(x => x.IsPinned && x.ProjectPinned.Any(p => p.AspNetUserId == userId)).ThenBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.IsPinned && x.ProjectPinned.Any(p => p.AspNetUserId == userId)).ThenByDescending(x => x.CreatedOnUtc);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                DateTime.TryParse(searchText, out var parsedDate);
                searchText = searchText.ToLower();

                query = query.Where(m =>
                    m.Customer.Company.Name.ToLower().Contains(searchText) ||
                    m.Name.ToLower().Contains(searchText) ||
                    (m.ProjectCoordinator.Person.FirstName + " " + m.ProjectCoordinator.Person.LastName).ToLower().Contains(searchText) ||
                    m.ProjectEmployeeMappings.Any(e => (e.Employee.Person.FirstName + " " + e.Employee.Person.LastName).ToLower().Contains(searchText)) ||
                    m.ProjectPriority.DropDownValue.ToLower().Contains(searchText) ||
                    m.ProjectType.DropDownValue.ToLower().Contains(searchText) ||
                    m.ProjectStatus.DropDownValue.ToLower().Contains(searchText) ||
                    m.StartDate.HasValue && m.StartDate.Value.Date == parsedDate.Date ||
                    m.ProjectTags.Any(t => t.Tags.Name.ToLower().Contains(searchText)));
            }

            // Apply multi-level dictionary sorting
            if (sorts != null && sorts.Count > 0)
            {
                query = _commonService.ApplySorting(query, sorts);
            }

            var pagedList = new PagedList<Project>(query.Select(x => new Project
            {
                Id = x.Id,
                Name = x.Name,
                //SiteId = x.SiteId,
                CustomerId = x.CustomerId,
                CompanyContactId = x.CompanyContactId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                GoLiveDate = x.GoLiveDate,
                Website = x.Website,
                Active = x.Active,
                IsPinned = x.IsPinned && x.ProjectPinned.Any(p => p.AspNetUserId == userId),
                IsTemplate = x.IsTemplate,
                CreatedById = x.CreatedById,
                ProjectCategoryId = x.ProjectCategoryId,
                ProjectStatus = new DropDown { Id = x.ProjectStatus.Id, DropDownValue = x.ProjectStatus.DropDownValue },
                ProjectPriority = new DropDown { Id = x.ProjectPriority.Id, DropDownValue = x.ProjectPriority.DropDownValue },
                ProjectType = new DropDown { Id = x.ProjectType.Id, DropDownValue = x.ProjectType.DropDownValue },
                ProjectCoordinator = new Employee
                {
                    Id = x.ProjectCoordinator.Id,
                    Person = new Person
                    {
                        Id = x.ProjectCoordinator.Person.Id,
                        FullName = x.ProjectCoordinator.Person.FirstName + " " + x.ProjectCoordinator.Person.LastName
                    }
                },
                Customer = new CompanyClients
                {
                    Id = x.Customer.Id,
                    Name = x.Customer.Company != null ? x.Customer.Company.Name : string.Join(" ", x.Customer.Person.FirstName, x.Customer.Person.LastName).Trim(),
                    PersonId = x.Customer.PersonId,
                    CompanyId = x.Customer.CompanyId,
                    CustomerTypeId = x.Customer.CustomerTypeId
                },
                ProjectCategories = new DropDownType { Id = x.ProjectCategories.Id, Type = x.ProjectCategories.Type },
                ProjectTags = x.ProjectTags.Where(t => !t.Deleted && !t.Tags.Deleted && t.AspNetUserId == userId).OrderBy(t => t.Tags.Name).Select(t => new ProjectTags
                {
                    Id = t.Id,
                    TagId = t.TagId,
                    ProjectId = t.ProjectId,
                    Tags = new Tags { Id = t.Tags.Id, Name = t.Tags.Name, Color = t.Tags.Color, BgColor = t.Tags.BgColor }
                }).ToList(),
                ProjectColor = x.ProjectColors.Where(c => c.AspNetUserId == userId && !c.Deleted).Select(c => c.Color).FirstOrDefault(),
                //ProjectColors = x.ProjectColors.Where(pc => !pc.Deleted && pc.AspNetUserId == userId)
                //.Select(pc => new ProjectColor
                //{
                //    Id = pc.Id,
                //    Color = pc.Color
                //})
                //.ToList(),
                ProjectEmployeeMappings = x.ProjectEmployeeMappings.Where(m => !m.Deleted).Select(m => new ProjectEmployeeMapping
                {
                    Id = m.Id,
                    Employee = new Employee
                    {
                        Id = m.Employee.Id,
                        Person = new Person
                        {
                            Id = m.Employee.Person.Id,
                            FullName = m.Employee.Person.FirstName + " " + m.Employee.Person.LastName,
                            BgColor = m.Employee.Person.BgColor,
                            Color = m.Employee.Person.Color,
                        }
                    },
                    EmployeeRoleDropdown = new DropDown { Id = m.EmployeeRoleDropdown.Id, DropDownValue = m.EmployeeRoleDropdown.DropDownValue }
                }).ToList(),
                ProjectUserMappings = x.ProjectUserMappings.Where(m => !m.Deleted && m.ProjectId == x.Id && (isAdmin || m.AspNetUserId == userId)).Take(1).Select(m => new ProjectUserMapping
                {
                    Id = m.Id,
                    AspNetUserId = m.AspNetUserId,
                    FullAccess = m.FullAccess,
                    ViewOnly = m.ViewOnly,
                    Notes = m.Notes
                }).ToList(),
                TotalTaskCount = x.ProjectTasks.Count(m => !m.Deleted && !m.IsMoved && !m.ProjectModule.Deleted && !m.Project.Deleted),
                CompletedTaskCount = x.ProjectTasks.Count(m => !m.Deleted && !m.IsMoved && !m.ProjectModule.Deleted && !m.Project.Deleted && (m.Status.DropDownValue == "Completed" || m.Status.DropDownValue == "Close")),
                TotalIssueCount = x.Issue.Count(m => !m.Deleted && !m.Project.Deleted),
                CompletedIssueCount = x.Issue.Count(m => !m.Deleted && !m.Project.Deleted && m.Status.DropDownValue == "Closed"),
                TotalRequirementCount = x.Requirement.Count(m => !m.Deleted && !m.Project.Deleted),
                CompletedRequirementCount = x.Requirement.Count(m => !m.Deleted && !m.Project.Deleted && m.Status.DropDownValue == "Close"),
                ProjectMessageCount = x.ProjectsMessages.Count(m => !m.Deleted),
                ProjectNotesCount = _notesRepository.TableNoTracking.Count(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Projects"),
                TotalTaskEstimateHours = x.ProjectTasks.Where(t => !t.Deleted).Sum(t => (decimal?)t.EstimateTime) ?? 0,
                TotalActivityHours = x.ProjectActivities.Where(a => !a.Deleted).Sum(a => (decimal?)a.EstimateHours) ?? 0
            }), page, pageSize);

            return pagedList;
        }

        public async Task<IPagedList<Project>> GetAllProjectsForNotes(
           string siteId,
           string userId,
           string searchText,
           List<string> projectIds,
           List<string> projectCategoryIds,
           List<string> statusIds,
           List<string> teamMemberIds,
           List<string> coordinatorIds,
           List<string> leadIds,
           List<string> priorityIds,
           List<string> typeIds,
           int status,
           List<string> customerIds,
           List<string> companyContactIds,
           string singleCustomerId,
           string sortBy,
           bool descending,
           int page = 1,
           int pageSize = int.MaxValue,
           bool lookup = false
       )
        {
            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && !x.IsTemplate && x.SiteId == siteId);

            bool isAdmin = await IsCurrentUserAdmin(userId, siteId);
            if (!isAdmin)
                query = query.Where(p => p.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == userId && (m.FullAccess || m.ViewOnly || m.Notes)));

            if (projectIds?.Any() == true) query = query.Where(x => projectIds.Contains(x.Id));
            if (projectCategoryIds?.Any() == true) query = query.Where(x => projectCategoryIds.Contains(x.ProjectCategoryId));
            if (statusIds?.Any() == true) query = query.Where(x => statusIds.Contains(x.ProjectStatusId));
            if (status == 0) query = query.Where(x => !x.Active);
            else if (status == 1) query = query.Where(x => x.Active);
            if (teamMemberIds?.Any() == true) query = query.Where(x => x.ProjectEmployeeMappings.Any(m => teamMemberIds.Contains(m.EmployeeId)));
            if (coordinatorIds?.Any() == true) query = query.Where(x => x.ProjectEmployeeMappings.Any(m => coordinatorIds.Contains(m.EmployeeId) && !m.Deleted && m.EmployeeRoleDropdown.DropDownValue == "Project Coordinator"));
            if (leadIds?.Any() == true) query = query.Where(x => x.ProjectEmployeeMappings.Any(m => leadIds.Contains(m.EmployeeId) && !m.Deleted && m.EmployeeRoleDropdown.DropDownValue == "Project Lead"));
            if (priorityIds?.Any() == true) query = query.Where(x => priorityIds.Contains(x.ProjectPriorityId));
            if (typeIds?.Any() == true) query = query.Where(x => typeIds.Contains(x.ProjectTypeId));
            if (customerIds?.Any() == true) query = query.Where(x => customerIds.Contains(x.CustomerId));
            if (companyContactIds?.Any() == true) query = query.Where(x => companyContactIds.Contains(x.CompanyContactId));
            if (!string.IsNullOrWhiteSpace(singleCustomerId)) query = query.Where(x => x.CustomerId == singleCustomerId);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderByDescending(x => x.IsPinned).ThenBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.CreatedOnUtc);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                DateTime.TryParse(searchText, out var parsedDate);
                searchText = searchText.ToLower();

                query = query.Where(m =>
                    m.Customer.Company.Name.ToLower().Contains(searchText) ||
                    m.Name.ToLower().Contains(searchText) ||
                    m.ProjectStatus.DropDownValue.ToLower().Contains(searchText));
            }

            var pagedList = new PagedList<Project>(query.Select(x => new Project
            {
                Id = x.Id,
                Name = x.Name,
                CustomerId = x.CustomerId,
                CompanyContactId = x.CompanyContactId,
                Active = x.Active,
                CreatedById = x.CreatedById,
                ProjectStatus = new DropDown { Id = x.ProjectStatus.Id, DropDownValue = x.ProjectStatus.DropDownValue },
                ProjectUserMappings = x.ProjectUserMappings.Where(m => !m.Deleted && m.ProjectId == x.Id && (isAdmin || m.AspNetUserId == userId)).Take(1).Select(m => new ProjectUserMapping
                {
                    Id = m.Id,
                    AspNetUserId = m.AspNetUserId,
                    FullAccess = m.FullAccess,
                    ViewOnly = m.ViewOnly,
                    Notes = m.Notes
                }).ToList(),
                TotalTaskCount = x.ProjectTasks.Count(m => !m.Deleted && !m.IsMoved && !m.ProjectModule.Deleted && !m.Project.Deleted),
                ProjectNotesCount = _notesRepository.TableNoTracking.Count(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Projects")
            }), page, pageSize);

            return pagedList;
        }
        #endregion

        #region GetProjectById
        // Title: GetProjectById
        // Description: This method retrieves a project from the database by its unique identifier (`id`). 
        public async Task<Project> GetById(string id)
        {
            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllProjectListForDropdown
        public async Task<List<Project>> GetAllProjectListForDropdown(string SiteId, string LoggedUserId, string[] statuses = null)
        {
            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.Active && !x.IsTemplate && x.SiteId == SiteId);
            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, SiteId);

            if (!IsAdmin)
                query = query.Where(p => p.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId));

            if (statuses != null && statuses.Any())
            {
                query = query.Where(x => statuses.Contains(x.ProjectStatus.DropDownValue));
            }

            query = query.Select(x => new Project
            {
                Id = x.Id,
                Name = x.Name,
                ProjectStatus = new DropDown { Id = x.ProjectStatus.Id, DropDownValue = x.ProjectStatus.DropDownValue },
                StartDate = x.StartDate,
                Customer = new CompanyClients { Id = x.Customer.Id, Name = x.Customer.Company != null ? x.Customer.Company.Name : string.Join(" ", x.Customer.Person.FirstName, x.Customer.Person.LastName).Trim() }, // from bs
                TotalModuleCount = x.ProjectModules.Count(m => !m.Deleted && !m.Project.Deleted && !m.IsMoved && m.ProjectModuleStatus.DropDownValue != "Close"),
                TotalTasksCount = x.ProjectTasks.Count(m => !m.Deleted && !m.IsMoved && !m.ProjectModule.Deleted && !m.ProjectModule.IsMoved && !m.Project.Deleted && m.Project.Active)
            });

            var list = await query.OrderBy(m => m.Name.Replace(" ", "")).ToListAsync();
            return list;
        }

        public async Task<List<CommonDropDown>> GetProjectsListForDropdown(string SiteId, string LoggedUserId, bool isTemplate, string ActiveStatus, bool isAllProject)
        {
            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.IsTemplate == isTemplate);

            bool? isActiveFilter = ActiveStatus == "all"
              ? null
              : Convert.ToBoolean(ActiveStatus);

            // Apply filter only if value exists
            if (isActiveFilter.HasValue)
            {
                query = query.Where(x => x.Active == isActiveFilter.Value);
            }

            if (!isAllProject)
            {
                bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, SiteId);

                if (!IsAdmin)
                    query = query.Where(p => p.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId));
            }
            var list = await query
                       .OrderBy(x => x.Name.Replace(" ", ""))
                       .Select(x => new CommonDropDown
                       {
                           Text = x.Name,
                           Value = x.Id,
                           StatusText = x.ProjectStatus.DropDownValue
                       })
                       .ToListAsync();

            return list;
        }
        #endregion

        #region GetProjectDetailsById
        // Title: GetProjectDetailsById
        // Description: The method selects relevant fields from the project entity, including related entities such as nd employee mappings, and returns a `Project` object with these details. 
        public async Task<Project> GetProjectDetailsById(string id)
        {
            var serviceCounts = _infraProjectServicesRepository.TableNoTracking
            .Where(x => !x.Deleted)
            .GroupBy(x => x.InfraServiceId)
            .Select(g => new
            {
                InfraServiceId = g.Key,
                ProjectCount = g.Count()
            });

            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new Project
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                CompanyContactId = x.CompanyContactId,
                ProjectStatusId = x.ProjectStatusId,
                ProjectPriorityId = x.ProjectPriorityId,
                ProjectTypeId = x.ProjectTypeId,
                ProjectCoordinatorId = x.ProjectCoordinatorId,
                ProjectCategoryId = x.ProjectCategoryId,
                ProjectSubcategoryId = x.ProjectSubcategoryId,
                PlanApproverId = x.PlanApproverId,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                GoLiveDate = x.GoLiveDate,
                Website = x.Website,
                IsTemplate = x.IsTemplate,
                Active = x.Active,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                ProjectStatus = new DropDown
                {
                    Id = x.ProjectStatus.Id,
                    DropDownValue = x.ProjectStatus.DropDownValue,
                },
                ProjectPriority = new DropDown
                {
                    Id = x.ProjectPriority.Id,
                    DropDownValue = x.ProjectPriority.DropDownValue,
                },
                ProjectType = new DropDown
                {
                    Id = x.ProjectType.Id,
                    DropDownValue = x.ProjectType.DropDownValue,
                },
                ProjectCoordinator = new Employee
                {
                    Id = x.ProjectCoordinator.Id,
                    Person = new Person
                    {
                        Id = x.ProjectCoordinator.Person.Id,
                        FullName = x.ProjectCoordinator.Person.LastName + " " + x.ProjectCoordinator.Person.LastName
                    }
                },
                CompanyContact = new CompanyContacts
                {
                    Id = x.CompanyContact.Id,
                    Person = new Person
                    {
                        Id = x.CompanyContact.Person.Id,
                        FullName = x.CompanyContact.Person.FirstName + " " + x.CompanyContact.Person.LastName
                    }
                },
                Customer = new CompanyClients
                {
                    Id = x.Customer.Id,
                    Company = new Company
                    {
                        Id = x.Customer.Company.Id,
                        Name = x.Customer.Company.Name
                    }
                },
                ProjectCategories = new DropDownType
                {
                    Id = x.ProjectCategories.Id,
                    Type = x.ProjectCategories.Type,
                },
                ProjectCategoriesSubCategories = new DropDown
                {
                    Id = x.ProjectCategoriesSubCategories.Id,
                    DropDownValue = x.ProjectCategoriesSubCategories.DropDownValue,
                    Description = x.ProjectCategoriesSubCategories.Description,
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
                ProjectEmployeeMappings = x.ProjectEmployeeMappings.Where(m => !m.Deleted).Select(mapping => new ProjectEmployeeMapping
                {
                    Id = mapping.Id,
                    EmployeeId = mapping.EmployeeId,
                    EmployeeDesignationId = mapping.EmployeeDesignationId,
                    ProductivityFactor = mapping.ProductivityFactor,
                    Employee = new Employee
                    {
                        Id = mapping.Employee.Id,
                        Person = new Person
                        {
                            Id = mapping.Employee.Person.Id,
                            FullName = mapping.Employee.Person.FirstName + " " + mapping.Employee.Person.LastName,
                        },
                    },
                    EmployeeRoleDropdown = new DropDown
                    {
                        Id = mapping.EmployeeRoleDropdown.Id,
                        DropDownValue = mapping.EmployeeRoleDropdown.DropDownValue,
                    }

                }).OrderBy(x => x.EmployeeRoleDropdown.DropDownValue).ThenBy(x => x.Employee.Person.FullName).ToList(),
                ProjectFileList = x.ProjectFileList.Where(x => !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(mapping => new ProjectFiles
                {
                    Id = mapping.Id,
                    CreatedBy = new ApplicationUser
                    {
                        Id = mapping.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = mapping.CreatedBy.Person.Id,
                            FirstName = mapping.CreatedBy.Person.FirstName,
                            LastName = mapping.CreatedBy.Person.LastName,
                        }
                    },
                    CreatedOnUtc = mapping.CreatedOnUtc,
                    File = new Picture
                    {
                        Id = mapping.File.Id,
                        VirtualPath = mapping.File.VirtualPath,
                        MimeType = mapping.File.MimeType,
                        SeoFilename = mapping.File.SeoFilename
                    }
                }).ToList(),
                InfraProjectServices = x.InfraProjectServices.Where(m => !m.Deleted).Select(m => new InfraProjectServices
                {
                    Id = m.Id,
                    InfraAccountServices = new InfraAccountServices
                    {
                        Name = m.InfraAccountServices.Name,
                        URL = m.InfraAccountServices.URL,
                        StartDate = m.InfraAccountServices.StartDate,
                        EndDate = m.InfraAccountServices.EndDate,
                        PriceInDollar = m.InfraAccountServices.PriceInDollar,
                        ActualPriceInDollar = Math.Round(
                            (decimal)m.InfraAccountServices.PriceInDollar /
                            serviceCounts
                                .Where(c => c.InfraServiceId == m.InfraServiceId)
                                .Select(c => c.ProjectCount)
                                .FirstOrDefault(),
                            2
                        ),
                        WalletNumber = m.InfraAccountServices.WalletNumber,
                        Instructions = m.InfraAccountServices.Instructions,
                        ItemType = new DropDown
                        {
                            Id = m.InfraAccountServices.ItemType.Id,
                            DropDownValue = m.InfraAccountServices.ItemType.DropDownValue
                        },
                        OwnerShipType = new DropDown
                        {
                            Id = m.InfraAccountServices.OwnerShipType.Id,
                            DropDownValue = m.InfraAccountServices.OwnerShipType.DropDownValue
                        },
                        PaymentTerm = new DropDown
                        {
                            Id = m.InfraAccountServices.PaymentTerm.Id,
                            DropDownValue = m.InfraAccountServices.PaymentTerm.DropDownValue
                        },
                        WalletType = new DropDown
                        {
                            Id = m.InfraAccountServices.WalletType.Id,
                            DropDownValue = m.InfraAccountServices.WalletType.DropDownValue
                        }
                    }
                }).ToList()
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectSummeryInDetails
        // Title: GetProjectSummeryInDetails
        // Description: The method selects relevant fields from the project entity, and returns a `Project` object with these details. 
        public async Task<Project> GetProjectSummeryInDetails(string projectId, DateTime currentDate, string weeklyTypeId, string monthlyTypeId)
        {
            var startOfYear = new DateTime(currentDate.Year, 1, 1);
            var startOfNextYear = startOfYear.AddYears(1);
            var startOfWeek = currentDate.Date.AddDays(-(int)currentDate.DayOfWeek);

            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == projectId).Select(x => new Project
            {
                Id = x.Id,
                //SiteId = x.SiteId,
                CustomerId = x.CustomerId,
                CompanyContactId = x.CompanyContactId,
                ProjectStatusId = x.ProjectStatusId,
                ProjectPriorityId = x.ProjectPriorityId,
                ProjectTypeId = x.ProjectTypeId,
                ProjectCoordinatorId = x.ProjectCoordinatorId,
                ProjectCategoryId = x.ProjectCategoryId,
                ProjectSubcategoryId = x.ProjectSubcategoryId,
                PlanApproverId = x.PlanApproverId,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                GoLiveDate = x.GoLiveDate,
                Website = x.Website,
                Active = x.Active,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                ProjectStatus = new DropDown
                {
                    Id = x.ProjectStatus.Id,
                    DropDownValue = x.ProjectStatus.DropDownValue,
                },
                ProjectPriority = new DropDown
                {
                    Id = x.ProjectPriority.Id,
                    DropDownValue = x.ProjectPriority.DropDownValue,
                },
                ProjectType = new DropDown
                {
                    Id = x.ProjectType.Id,
                    DropDownValue = x.ProjectType.DropDownValue,
                },
                ProjectCoordinator = new Employee
                {
                    Id = x.ProjectCoordinator.Id,
                    Person = new Person
                    {
                        Id = x.ProjectCoordinator.Person.Id,
                        FullName = x.ProjectCoordinator.Person.LastName + " " + x.ProjectCoordinator.Person.LastName
                    }
                },
                CompanyContact = new CompanyContacts
                {
                    Id = x.CompanyContact.Id,
                    Person = new Person
                    {
                        Id = x.CompanyContact.Person.Id,
                        FullName = x.CompanyContact.Person.FirstName + " " + x.CompanyContact.Person.LastName
                    }
                },
                Customer = new CompanyClients
                {
                    Id = x.Customer.Id,
                    Company = new Company
                    {
                        Id = x.Customer.Company.Id,
                        Name = x.Customer.Company.Name
                    }
                },
                ProjectCategories = new DropDownType
                {
                    Id = x.ProjectCategories.Id,
                    Type = x.ProjectCategories.Type,
                },
                ProjectCategoriesSubCategories = new DropDown
                {
                    Id = x.ProjectCategoriesSubCategories.Id,
                    DropDownValue = x.ProjectCategoriesSubCategories.DropDownValue,
                    Description = x.ProjectCategoriesSubCategories.Description,
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
                ProjectModules = x.ProjectModules.Where(m => !m.Deleted && m.CreatedOnUtc >= startOfYear && m.CreatedOnUtc < startOfNextYear).Select(module => new ProjectModule
                {
                    Id = module.Id,
                    Notes = module.Notes,
                    Name = module.Name,
                    ProjectModuleNumber = module.ProjectModuleNumber,
                    Description = module.Description,
                    CloseDate = module.CloseDate,
                    StartDate = module.StartDate,
                    EndDate = module.EndDate,
                    TargetDate = module.TargetDate,
                    ProjectModuleStatusId = module.ProjectModuleStatusId,
                    ProjectModuleTypeId = module.ProjectModuleTypeId,
                    CreatedOnUtc = module.CreatedOnUtc,
                    //SiteId = module.SiteId,
                    ProjectModuleStatus = new DropDown
                    {
                        Id = module.ProjectModuleStatus.Id,
                        DropDownValue = module.ProjectModuleStatus.DropDownValue
                    },
                    ProjectModuleType = new DropDown
                    {
                        Id = module.ProjectModuleType.Id,
                        DropDownValue = module.ProjectModuleType.DropDownValue
                    },
                    CreatedBy = new ApplicationUser
                    {
                        Id = module.CreatedBy.Id,
                        UserName = module.CreatedBy.UserName,
                        Person = new Person
                        {
                            Id = module.CreatedBy.PersonId,
                            FirstName = module.CreatedBy.Person.FirstName,
                            LastName = module.CreatedBy.Person.LastName,
                        }
                    },
                    UpdatedBy = new ApplicationUser
                    {
                        Id = module.UpdatedBy.Id,
                        Person = new Person
                        {
                            Id = module.UpdatedBy.PersonId,
                            FirstName = module.UpdatedBy.Person.FirstName,
                            LastName = module.UpdatedBy.Person.LastName,
                        }
                    },
                    ProjectModuleFilesList = module.ProjectModuleFilesList.Where(x => !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(mapping => new ProjectModuleFiles
                    {
                        Id = mapping.Id,
                        CreatedBy = new ApplicationUser
                        {
                            Id = mapping.CreatedBy.Id,
                            Person = new Person
                            {
                                Id = mapping.CreatedBy.Person.Id,
                                FirstName = mapping.CreatedBy.Person.FirstName,
                                LastName = mapping.CreatedBy.Person.LastName,
                            }
                        },
                        CreatedOnUtc = mapping.CreatedOnUtc,
                        File = new Picture
                        {
                            Id = mapping.File.Id,
                            VirtualPath = mapping.File.VirtualPath,
                            MimeType = mapping.File.MimeType,
                            SeoFilename = mapping.File.SeoFilename
                        }
                    }).ToList(),
                    ProjectTasks = module.ProjectTasks.Where(x => !x.Deleted).Select(mapping => new ProjectTask
                    {
                        Id = mapping.Id,
                        Name = mapping.Name,
                        ProjectTaskNumber = mapping.ProjectTaskNumber,
                        StatusId = mapping.StatusId,
                        PriorityId = mapping.PriorityId,
                        EstimateTime = mapping.EstimateTime,
                        TaskMonth = mapping.TaskMonth,
                        StartDate = mapping.StartDate,
                        EndDate = mapping.EndDate,
                        AssignedToId = mapping.AssignedToId,
                        CreatedOnUtc = mapping.CreatedOnUtc,
                        UpdatedOnUtc = mapping.UpdatedOnUtc,
                        Description = mapping.Description,
                        Status = new DropDown
                        {
                            Id = mapping.Status.Id,
                            DropDownValue = mapping.Status.DropDownValue
                        },
                        Priority = new DropDown
                        {
                            Id = mapping.Priority.Id,
                            DropDownValue = mapping.Priority.DropDownValue
                        },
                        AssignedTo = new Employee
                        {
                            Id = mapping.AssignedTo.Id,
                            Person = new Person
                            {
                                FirstName = mapping.AssignedTo.Person.FirstName,
                                LastName = mapping.AssignedTo.Person.LastName,
                            }
                        },
                        CreatedBy = new ApplicationUser
                        {
                            Id = mapping.CreatedBy.Id,
                            Person = new Person
                            {
                                Id = mapping.CreatedBy.PersonId,
                                FirstName = mapping.CreatedBy.Person.FirstName,
                                LastName = mapping.CreatedBy.Person.LastName,
                            }
                        },
                        UpdatedBy = new ApplicationUser
                        {
                            Id = mapping.UpdatedBy.Id,
                            Person = new Person
                            {
                                Id = mapping.UpdatedBy.PersonId,
                                FirstName = mapping.UpdatedBy.Person.FirstName,
                                LastName = mapping.UpdatedBy.Person.LastName,
                            }
                        },
                        ProjectActivities = mapping.ProjectActivities.Where(x => !x.Deleted && x.ActivityStatus.DropDownValue.ToLower() != "close").OrderByDescending(x => x.CreatedOnUtc).Select(p => new ProjectActivity
                        {
                            Id = p.Id,
                            Name = p.Name,
                            AssignedToId = p.AssignedToId,
                            ActivityStatusId = p.ActivityStatusId,
                            EstimateHours = p.EstimateHours,
                            StartDate = p.StartDate,
                            EndDate = p.EndDate,
                            Description = p.Description,
                            ActivityStatus = new DropDown
                            {
                                Id = p.ActivityStatus.Id,
                                DropDownValue = p.ActivityStatus.DropDownValue
                            },
                            AssignedTo = new Employee
                            {
                                Id = p.AssignedTo.Id,
                                Person = new Person

                                {
                                    FirstName = p.AssignedTo.Person.FirstName,
                                    LastName = p.AssignedTo.Person.LastName,
                                }
                            },
                        }).ToList(),
                        ProjectTaskFilesList = mapping.ProjectTaskFilesList.Where(x => !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(mapping => new ProjectTaskFiles
                        {
                            Id = mapping.Id,
                            CreatedBy = new ApplicationUser
                            {
                                Id = mapping.CreatedBy.Id,
                                Person = new Person
                                {
                                    Id = mapping.CreatedBy.Person.Id,
                                    FirstName = mapping.CreatedBy.Person.FirstName,
                                    LastName = mapping.CreatedBy.Person.LastName,
                                }
                            },
                            CreatedOnUtc = mapping.CreatedOnUtc,
                            File = new Picture
                            {
                                Id = mapping.File.Id,
                                VirtualPath = mapping.File.VirtualPath,
                                MimeType = mapping.File.MimeType,
                                SeoFilename = mapping.File.SeoFilename
                            }
                        }).ToList(),
                        ProjectTaskRelatedMappings = mapping.ProjectTaskRelatedMappings.Where(m => m.TaskId == mapping.Id && !m.Deleted && (m.RequirementId != null || m.IssueId != null))
                        .Select(m => new ProjectTaskRelatedMapping
                        {
                            Id = m.Id,
                            RequirementId = m.RequirementId,
                            IssueId = m.IssueId,
                            Issue = m.Issue == null ? null : new Issue
                            {
                                IssueNumber = m.Issue.IssueNumber,
                                Status = m.Issue.Status == null ? null : new DropDown { Id = m.Issue.Status.Id, DropDownValue = m.Issue.Status.DropDownValue }
                            },
                            Requirement = m.Requirement == null ? null : new Requirement
                            {
                                RequirementNumber = m.Requirement.RequirementNumber,
                                Status = m.Requirement.Status == null ? null : new DropDown { Id = m.Requirement.Status.Id, DropDownValue = m.Requirement.Status.DropDownValue }
                            }
                        }).ToList(),
                    }).ToList()
                }).ToList(),
                ProjectEmployeeMappings = x.ProjectEmployeeMappings.Where(m => !m.Deleted).OrderByDescending(m => m.CreatedOnUtc).Select(mapping => new ProjectEmployeeMapping
                {
                    Id = mapping.Id,
                    EmployeeId = mapping.EmployeeId,
                    EmployeeDesignationId = mapping.EmployeeDesignationId,
                    ProductivityFactor = mapping.ProductivityFactor,
                    Employee = new Employee
                    {
                        Id = mapping.Employee.Id,
                        Person = new Person
                        {
                            Id = mapping.Employee.Person.Id,
                            FullName = mapping.Employee.Person.FirstName + " " + mapping.Employee.Person.LastName,
                        },
                    },
                    EmployeeRoleDropdown = new DropDown
                    {
                        Id = mapping.EmployeeRoleDropdown.Id,
                        DropDownValue = mapping.EmployeeRoleDropdown.DropDownValue,
                    }

                }).ToList(),
                ProjectFileList = x.ProjectFileList.Where(x => !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(mapping => new ProjectFiles
                {
                    Id = mapping.Id,
                    CreatedBy = new ApplicationUser
                    {
                        Id = mapping.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = mapping.CreatedBy.Person.Id,
                            FirstName = mapping.CreatedBy.Person.FirstName,
                            LastName = mapping.CreatedBy.Person.LastName,
                        }
                    },
                    CreatedOnUtc = mapping.CreatedOnUtc,
                    File = new Picture
                    {
                        Id = mapping.File.Id,
                        VirtualPath = mapping.File.VirtualPath,
                        MimeType = mapping.File.MimeType,
                        SeoFilename = mapping.File.SeoFilename
                    }
                }).ToList()
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        public async Task<Project> GetProjectMyWorkSummeryInDetails(string projectId, DateTime currentDate)
        {
            var startOfYear = new DateTime(currentDate.Year, 1, 1);
            var startOfNextYear = startOfYear.AddYears(1);

            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == projectId).Select(x => new Project
            {
                Id = x.Id,
                //SiteId = x.SiteId,
                CustomerId = x.CustomerId,
                CompanyContactId = x.CompanyContactId,
                ProjectStatusId = x.ProjectStatusId,
                ProjectPriorityId = x.ProjectPriorityId,
                ProjectTypeId = x.ProjectTypeId,
                ProjectCoordinatorId = x.ProjectCoordinatorId,
                ProjectCategoryId = x.ProjectCategoryId,
                ProjectSubcategoryId = x.ProjectSubcategoryId,
                PlanApproverId = x.PlanApproverId,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                GoLiveDate = x.GoLiveDate,
                Website = x.Website,
                Active = x.Active,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                ProjectStatus = new DropDown
                {
                    Id = x.ProjectStatus.Id,
                    DropDownValue = x.ProjectStatus.DropDownValue,
                },
                ProjectPriority = new DropDown
                {
                    Id = x.ProjectPriority.Id,
                    DropDownValue = x.ProjectPriority.DropDownValue,
                },
                ProjectType = new DropDown
                {
                    Id = x.ProjectType.Id,
                    DropDownValue = x.ProjectType.DropDownValue,
                },
                ProjectCoordinator = new Employee
                {
                    Id = x.ProjectCoordinator.Id,
                    Person = new Person
                    {
                        Id = x.ProjectCoordinator.Person.Id,
                        FullName = x.ProjectCoordinator.Person.LastName + " " + x.ProjectCoordinator.Person.LastName
                    }
                },
                CompanyContact = new CompanyContacts
                {
                    Id = x.CompanyContact.Id,
                    Person = new Person
                    {
                        Id = x.CompanyContact.Person.Id,
                        FullName = x.CompanyContact.Person.FirstName + " " + x.CompanyContact.Person.LastName
                    }
                },
                Customer = new CompanyClients
                {
                    Id = x.Customer.Id,
                    Company = new Company
                    {
                        Id = x.Customer.Company.Id,
                        Name = x.Customer.Company.Name
                    }
                },
                ProjectCategories = new DropDownType
                {
                    Id = x.ProjectCategories.Id,
                    Type = x.ProjectCategories.Type,
                },
                ProjectCategoriesSubCategories = new DropDown
                {
                    Id = x.ProjectCategoriesSubCategories.Id,
                    DropDownValue = x.ProjectCategoriesSubCategories.DropDownValue,
                    Description = x.ProjectCategoriesSubCategories.Description,
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
                TimesheetLine = x.TimesheetLine.Where(m => !m.Deleted && m.ProjectId == x.Id && m.CreatedOnUtc >= startOfYear && m.CreatedOnUtc < startOfNextYear).Select(mapping => new TimesheetLines
                {
                    Id = mapping.Id,
                    Hours = mapping.Hours,
                    Description = mapping.Description,
                    Timesheet = new Timesheet
                    {
                        Id = mapping.Timesheet.Id,
                        TimesheetDate = mapping.Timesheet.TimesheetDate,
                        //SiteId = mapping.Timesheet.SiteId,
                        EmployeeId = mapping.Timesheet.EmployeeId,
                        CreatedById = mapping.Timesheet.CreatedById,
                        CreatedOnUtc = mapping.Timesheet.CreatedOnUtc,
                    },
                    ProjectModule = new ProjectModule
                    {
                        Id = mapping.ProjectModule.Id,
                        Name = mapping.ProjectModule.Name,
                    },
                    Task = new ProjectTask
                    {
                        Id = mapping.Task.Id,
                        Name = mapping.Task.Name,
                    },
                    ProjectActivity = new ProjectActivity
                    {
                        Id = mapping.ProjectActivity.Id,
                        Name = mapping.ProjectActivity.Name,
                        TargetMonth = mapping.ProjectActivity.TargetMonth,
                    }
                }).ToList(),
                ProjectEmployeeMappings = x.ProjectEmployeeMappings.Where(m => !m.Deleted).OrderByDescending(m => m.CreatedOnUtc).Select(mapping => new ProjectEmployeeMapping
                {
                    Id = mapping.Id,
                    EmployeeId = mapping.EmployeeId,
                    EmployeeDesignationId = mapping.EmployeeDesignationId,
                    ProductivityFactor = mapping.ProductivityFactor,
                    Employee = new Employee
                    {
                        Id = mapping.Employee.Id,
                        Person = new Person
                        {
                            Id = mapping.Employee.Person.Id,
                            FullName = mapping.Employee.Person.FirstName + " " + mapping.Employee.Person.LastName,
                        },
                    },
                    EmployeeRoleDropdown = new DropDown
                    {
                        Id = mapping.EmployeeRoleDropdown.Id,
                        DropDownValue = mapping.EmployeeRoleDropdown.DropDownValue,
                    }

                }).ToList(),
                ProjectFileList = x.ProjectFileList.Where(x => !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(mapping => new ProjectFiles
                {
                    Id = mapping.Id,
                    CreatedBy = new ApplicationUser
                    {
                        Id = mapping.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = mapping.CreatedBy.Person.Id,
                            FirstName = mapping.CreatedBy.Person.FirstName,
                            LastName = mapping.CreatedBy.Person.LastName,
                        }
                    },
                    CreatedOnUtc = mapping.CreatedOnUtc,
                    File = new Picture
                    {
                        Id = mapping.File.Id,
                        VirtualPath = mapping.File.VirtualPath,
                        MimeType = mapping.File.MimeType,
                        SeoFilename = mapping.File.SeoFilename
                    }
                }).ToList(),
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        public async Task<Project> GetProjectSdlcSummeryInDetails(string projectId, DateTime currentDate)
        {
            var startOfYear = new DateTime(currentDate.Year, 1, 1);
            var startOfNextYear = startOfYear.AddYears(1);

            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == projectId).Select(x => new Project
            {
                Id = x.Id,
                //SiteId = x.SiteId,
                CustomerId = x.CustomerId,
                CompanyContactId = x.CompanyContactId,
                ProjectStatusId = x.ProjectStatusId,
                ProjectPriorityId = x.ProjectPriorityId,
                ProjectTypeId = x.ProjectTypeId,
                ProjectCoordinatorId = x.ProjectCoordinatorId,
                ProjectCategoryId = x.ProjectCategoryId,
                ProjectSubcategoryId = x.ProjectSubcategoryId,
                PlanApproverId = x.PlanApproverId,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                GoLiveDate = x.GoLiveDate,
                Website = x.Website,
                Active = x.Active,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                ProjectStatus = new DropDown
                {
                    Id = x.ProjectStatus.Id,
                    DropDownValue = x.ProjectStatus.DropDownValue,
                },
                ProjectPriority = new DropDown
                {
                    Id = x.ProjectPriority.Id,
                    DropDownValue = x.ProjectPriority.DropDownValue,
                },
                ProjectType = new DropDown
                {
                    Id = x.ProjectType.Id,
                    DropDownValue = x.ProjectType.DropDownValue,
                },
                ProjectCoordinator = new Employee
                {
                    Id = x.ProjectCoordinator.Id,
                    Person = new Person
                    {
                        Id = x.ProjectCoordinator.Person.Id,
                        FullName = x.ProjectCoordinator.Person.LastName + " " + x.ProjectCoordinator.Person.LastName
                    }
                },
                CompanyContact = new CompanyContacts
                {
                    Id = x.CompanyContact.Id,
                    Person = new Person
                    {
                        Id = x.CompanyContact.Person.Id,
                        FullName = x.CompanyContact.Person.FirstName + " " + x.CompanyContact.Person.LastName
                    }
                },
                Customer = new CompanyClients
                {
                    Id = x.Customer.Id,
                    Company = new Company
                    {
                        Id = x.Customer.Company.Id,
                        Name = x.Customer.Company.Name
                    }
                },
                ProjectCategories = new DropDownType
                {
                    Id = x.ProjectCategories.Id,
                    Type = x.ProjectCategories.Type,
                },
                ProjectCategoriesSubCategories = new DropDown
                {
                    Id = x.ProjectCategoriesSubCategories.Id,
                    DropDownValue = x.ProjectCategoriesSubCategories.DropDownValue,
                    Description = x.ProjectCategoriesSubCategories.Description,
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
                TestPlans = x.TestPlans.Where(m => !m.Deleted && m.CreatedOnUtc >= startOfYear && m.CreatedOnUtc < startOfNextYear).Select(tp => new TestPlan
                {
                    Id = tp.Id,
                    PlanMakerId = tp.PlanMakerId,
                    PlanReviewerId = tp.PlanReviewerId,
                    Name = tp.Name,
                    Description = tp.Description,
                    TestPlanNumber = tp.TestPlanNumber,
                    PlanMaker = new Employee
                    {
                        Person = new Person
                        {
                            Id = tp.PlanMaker.Person.Id,
                            FullName = tp.PlanMaker.Person.FirstName + " " + tp.PlanMaker.Person.LastName,
                        }
                    },
                    PlanReviewer = new Employee
                    {
                        Person = new Person
                        {
                            Id = tp.PlanReviewer.Person.Id,
                            FullName = tp.PlanReviewer.Person.FirstName + " " + tp.PlanReviewer.Person.LastName,
                        }
                    },
                    TestCases = tp.TestCases.Where(x => !x.Deleted && x.PlanId == tp.Id).Select(tc => new TestCase
                    {
                        Id = tc.Id,
                        Name = tc.Name,
                        Description = tc.Description,
                        Steps = tc.Steps,
                        ExpectedResult = tc.ExpectedResult,
                        ActualResult = tc.ActualResult,
                        TestResult = tc.TestResult,
                        TestedDate = tc.TestedDate,
                        TestCaseNumber = tc.TestCaseNumber,
                        CreatedById = tc.CreatedById,
                        CreatedOnUtc = tc.CreatedOnUtc,
                        Employee = new Employee
                        {
                            Person = new Person
                            {
                                Id = tc.Employee.Person.Id,
                                FullName = tc.Employee.Person.FirstName + " " + tc.Employee.Person.LastName,
                            }
                        },
                        TestedByEmployee = new Employee
                        {
                            Person = new Person
                            {
                                Id = tc.TestedByEmployee.Person.Id,
                                FullName = tc.TestedByEmployee.Person.FirstName + " " + tc.TestedByEmployee.Person.LastName,
                            }
                        },
                        Status = new DropDown
                        {
                            Id = tc.Status.Id,
                            DropDownValue = tc.Status.DropDownValue
                        },
                        CreatedByUser = new ApplicationUser
                        {
                            Id = tc.CreatedByUser.Id,
                            Person = new Person
                            {
                                FullName = tc.CreatedByUser.Person.FirstName + " " + tc.CreatedByUser.Person.LastName
                            }
                        },
                    }).ToList()
                }).ToList(),
                Requirement = x.Requirement.Where(m => !m.Deleted && m.CreatedOnUtc >= startOfYear && m.CreatedOnUtc < startOfNextYear).Select(req => new Requirement
                {
                    Id = req.Id,
                    ProjectModuleId = req.ProjectModuleId,
                    Title = req.Title,
                    StatusId = req.StatusId,
                    IdentifiedDate = req.IdentifiedDate,
                    CloseDate = req.CloseDate,
                    Description = req.Description,
                    Notes = req.Notes,
                    EditingStatus = req.EditingStatus,
                    ApprovalStatus = req.ApprovalStatus,
                    IdentifiedCustomerId = req.IdentifiedCustomerId,
                    IdentifiedEmployeeId = req.IdentifiedEmployeeId,
                    IdentifiedUserType = req.IdentifiedUserType,
                    PriorityId = req.PriorityId,
                    RequirementEnteredBy = req.RequirementEnteredBy,
                    RequirementGroupId = req.RequirementGroupId,
                    RequirementNumber = req.RequirementNumber,
                    CreatedOnUtc = req.CreatedOnUtc,
                    UpdatedOnUtc = req.UpdatedOnUtc,
                    Employee = new Employee
                    {
                        Person = new Person
                        {
                            Id = req.Employee.Person.Id,
                            FullName = req.Employee.Person.FirstName + " " + req.Employee.Person.LastName,
                        }
                    },
                    RequirementEntered = new Employee
                    {
                        Person = new Person
                        {
                            Id = req.RequirementEntered.Person.Id,
                            FullName = req.RequirementEntered.Person.FirstName + " " + req.RequirementEntered.Person.LastName,
                        }
                    },
                    Customer = new Person
                    {
                        Id = req.Customer.Id,
                        FullName = req.Customer.FirstName + " " + req.Customer.LastName,
                    },
                    ProjectModule = new ProjectModule
                    {
                        Id = req.ProjectModule.Id,
                        Name = req.ProjectModule.Name,
                        StartDate = req.ProjectModule.StartDate,
                        EndDate = req.ProjectModule.EndDate
                    },
                    Status = new DropDown
                    {
                        Id = req.Status.Id,
                        DropDownValue = req.Status.DropDownValue
                    },
                    UserType = new DropDown
                    {
                        Id = req.UserType.Id,
                        DropDownValue = req.UserType.DropDownValue
                    },
                    Priority = new DropDown
                    {
                        Id = req.Priority.Id,
                        DropDownValue = req.Priority.DropDownValue
                    },
                    ApprovalStatusDropDown = new DropDown
                    {
                        Id = req.ApprovalStatusDropDown.Id,
                        DropDownValue = req.ApprovalStatusDropDown.DropDownValue
                    },
                    CreatedBy = new ApplicationUser
                    {
                        Id = req.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = req.CreatedBy.PersonId,
                            FirstName = req.CreatedBy.Person.FirstName,
                            LastName = req.CreatedBy.Person.LastName,
                        }
                    },
                    UpdatedBy = new ApplicationUser
                    {
                        Id = req.UpdatedBy.Id,
                        Person = new Person
                        {
                            Id = req.UpdatedBy.PersonId,
                            FirstName = req.UpdatedBy.Person.FirstName,
                            LastName = req.UpdatedBy.Person.LastName,
                        }
                    },
                    FilePathDetails = req.FilePathDetails.Where(p => !p.Deleted).Select(p => new FilePathDetails
                    {
                        Id = p.Id,
                        FileName = p.FileName,
                        FilePath = p.FilePath,
                        Note = p.Note
                    }).ToList(),
                    ProjectTaskRelatedMappings = req.ProjectTaskRelatedMappings.Where(m => m.RequirementId == req.Id && !m.Deleted && m.RequirementId != null)
                    .Select(m => new ProjectTaskRelatedMapping
                    {
                        Id = m.Id,
                        TaskId = m.TaskId,
                        ProjectTask = new ProjectTask
                        {
                            ProjectTaskNumber = m.ProjectTask.ProjectTaskNumber,
                            Status = new DropDown { Id = m.ProjectTask.Status.Id, DropDownValue = m.ProjectTask.Status.DropDownValue }
                        }
                    }).ToList(),
                }).ToList(),
                Issue = x.Issue.Where(i => !i.Deleted && i.CreatedOnUtc >= startOfYear && i.CreatedOnUtc < startOfNextYear).Select(issue => new Issue
                {
                    Id = issue.Id,
                    Name = issue.Name,
                    LastUpdatedDate = issue.LastUpdatedDate,
                    CloseDate = issue.CloseDate,
                    DueDate = issue.DueDate,
                    IsTaskCreated = issue.IsTaskCreated,
                    IssueNumber = issue.IssueNumber,
                    CreatedOnUtc = issue.CreatedOnUtc,
                    Priority = new DropDown
                    {
                        Id = issue.Priority.Id,
                        DropDownValue = issue.Priority.DropDownValue
                    },
                    Status = new DropDown
                    {
                        Id = issue.Status.Id,
                        DropDownValue = issue.Status.DropDownValue
                    },
                    Type = new DropDown
                    {
                        Id = issue.Type.Id,
                        DropDownValue = issue.Type.DropDownValue
                    },
                    Employee = new Employee
                    {
                        Person = new Person
                        {
                            Id = issue.Employee.Person.Id,
                            FullName = issue.Employee.Person.FirstName + " " + issue.Employee.Person.LastName,
                        }
                    },
                    ClosedByEmployee = new Employee
                    {
                        Person = new Person
                        {
                            Id = issue.ClosedByEmployee.Person.Id,
                            FullName = issue.ClosedByEmployee.Person.FirstName + " " + issue.ClosedByEmployee.Person.LastName,
                        }
                    },
                    LastModifiedByEmployee = new Employee
                    {
                        Person = new Person
                        {
                            Id = issue.LastModifiedByEmployee.Person.Id,
                            FullName = issue.LastModifiedByEmployee.Person.FirstName + " " + issue.LastModifiedByEmployee.Person.LastName,
                        }
                    },
                    ReportedBy = new Employee
                    {
                        Person = new Person
                        {
                            Id = issue.ReportedBy.Person.Id,
                            FullName = issue.ReportedBy.Person.FirstName + " " + issue.ReportedBy.Person.LastName,
                        }
                    },
                    ProjectModule = new ProjectModule
                    {
                        Id = issue.ProjectModule.Id,
                        Name = issue.ProjectModule.Name,
                        StartDate = issue.ProjectModule.StartDate,
                        EndDate = issue.ProjectModule.EndDate
                    },
                    TestCase = issue.TestCase == null ? null : new TestCase
                    {
                        Id = issue.TestCase.Id,
                        Name = issue.TestCase.Name
                    },
                    IssueStatusChangedLog = issue.IssueStatusChangedLog.OrderByDescending(m => m.StatusChangedDate).Select(p => new IssueStatusChangedLog
                    {
                        Id = p.Id,
                        StatusChangedDate = p.StatusChangedDate,
                        Issue = new Issue
                        {
                            Id = p.Issue.Id,
                            Name = p.Issue.Name
                        },
                        Status = new DropDown
                        {
                            Id = p.Status.Id,
                            DropDownValue = p.Status.DropDownValue
                        },
                        StatusChangedByEmployee = new Employee
                        {
                            Person = new Person
                            {
                                Id = p.StatusChangedByEmployee.Person.Id,
                                FullName = p.StatusChangedByEmployee.Person.FirstName + " " + p.StatusChangedByEmployee.Person.LastName
                            }
                        }
                    }).ToList(),
                    ProjectTaskRelatedMappings = issue.ProjectTaskRelatedMappings.Where(m => m.IssueId == issue.Id && !m.Deleted && m.IssueId != null)
                    .Select(m => new ProjectTaskRelatedMapping
                    {
                        Id = m.Id,
                        TaskId = m.TaskId,
                        ProjectTask = new ProjectTask
                        {
                            ProjectTaskNumber = m.ProjectTask.ProjectTaskNumber,
                            Status = new DropDown { Id = m.ProjectTask.Status.Id, DropDownValue = m.ProjectTask.Status.DropDownValue }
                        }
                    }).ToList()
                }).ToList(),
                ProjectEmployeeMappings = x.ProjectEmployeeMappings.Where(m => !m.Deleted).OrderByDescending(m => m.CreatedOnUtc).Select(mapping => new ProjectEmployeeMapping
                {
                    Id = mapping.Id,
                    EmployeeId = mapping.EmployeeId,
                    EmployeeDesignationId = mapping.EmployeeDesignationId,
                    ProductivityFactor = mapping.ProductivityFactor,
                    Employee = new Employee
                    {
                        Id = mapping.Employee.Id,
                        Person = new Person
                        {
                            Id = mapping.Employee.Person.Id,
                            FullName = mapping.Employee.Person.FirstName + " " + mapping.Employee.Person.LastName,
                        },
                    },
                    EmployeeRoleDropdown = new DropDown
                    {
                        Id = mapping.EmployeeRoleDropdown.Id,
                        DropDownValue = mapping.EmployeeRoleDropdown.DropDownValue,
                    }

                }).ToList(),
                ProjectFileList = x.ProjectFileList.Where(x => !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(mapping => new ProjectFiles
                {
                    Id = mapping.Id,
                    CreatedBy = new ApplicationUser
                    {
                        Id = mapping.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = mapping.CreatedBy.Person.Id,
                            FirstName = mapping.CreatedBy.Person.FirstName,
                            LastName = mapping.CreatedBy.Person.LastName,
                        }
                    },
                    CreatedOnUtc = mapping.CreatedOnUtc,
                    File = new Picture
                    {
                        Id = mapping.File.Id,
                        VirtualPath = mapping.File.VirtualPath,
                        MimeType = mapping.File.MimeType,
                        SeoFilename = mapping.File.SeoFilename
                    }
                }).ToList(),
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectByName
        // Title: GetProjectByName
        // Description: This method retrieves a project based on its name and customer ID. It allows an optional exclusion of a project by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific project. The method returns the first matching project or null if no match is found.
        public async Task<Project> GetProjectByName(string SiteId, string name, string customerId, bool isTemplate = false, string id = null)
        {
            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Name.ToLower() == name.ToLower() && x.CustomerId == customerId && x.IsTemplate == isTemplate);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectName
        public async Task<Project> GetProjectName(string SiteId, string name)
        {
            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Name.ToLower() == name.ToLower());
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectByTemplate
        // Title: GetProjectByTemplate
        // Description: This method retrieves a project based on its name and customer ID. It allows an optional exclusion of a project by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific project. The method returns the first matching project or null if no match is found.
        public async Task<Project> GetProjectByTemplate(string SiteId, string name, string customerId, bool template)
        {
            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Name.ToLower() == name.ToLower() && x.CustomerId == customerId);

            if (template)
                query = query.Where(x => x.IsTemplate == template);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectDetailsByIds
        public async Task<List<Project>> GetProjectDetailsByIds(string[] ids, string siteId)
        {
            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && ids.Any(m => m == x.Id));

            if (siteId != null)
                query = query.Where(x => x.SiteId == siteId);

            query = query.Select(x => new Project
            {
                Id = x.Id,
                Name = x.Name,
                Active = x.Active,
                ProjectCoordinatorId = x.ProjectCoordinatorId,
                SiteId = x.SiteId,
                ProjectStatus = new DropDown
                {
                    Id = x.ProjectStatus.Id,
                    DropDownValue = x.ProjectStatus.DropDownValue,
                },
                ProjectPriority = new DropDown
                {
                    Id = x.ProjectPriority.Id,
                    DropDownValue = x.ProjectPriority.DropDownValue,
                },
                ProjectCoordinator = new Employee
                {
                    Id = x.Id,
                    Person = new Person
                    {
                        Id = x.ProjectCoordinator.Id,
                        FullName = x.ProjectCoordinator.Person.FirstName + " " + x.ProjectCoordinator.Person.LastName
                    }
                },
                ProjectEmployeeMappings = x.ProjectEmployeeMappings.Where(x => !x.Deleted).Select(mapping => new ProjectEmployeeMapping
                {
                    Id = mapping.Id,
                    EmployeeId = mapping.EmployeeId,
                    EmployeeDesignationId = mapping.EmployeeDesignationId,
                    ProductivityFactor = mapping.ProductivityFactor,
                    Employee = new Employee
                    {
                        Id = mapping.Employee.Id,
                        Person = new Person
                        {
                            Id = mapping.Employee.Person.Id,
                            FullName = mapping.Employee.Person.FirstName + " " + mapping.Employee.Person.LastName,
                        },
                    },
                    EmployeeRoleDropdown = new DropDown
                    {
                        Id = mapping.EmployeeRoleDropdown.Id,
                        DropDownValue = mapping.EmployeeRoleDropdown.DropDownValue,
                    }

                }).ToList()
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetProjectsAndCharterListForDashboard
        // Title: GetProjectsAndCharterListForDashboard
        // Description: This method retrieves a paginated list of projects based on various search criteria such as project name, 
        // project status, and team member. It also supports sorting and includes related data such as project status and employee 
        // mappings. The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<Project> GetProjectsAndCharterListForDashboard(string SiteId, List<string> projectIds, List<string> projectStatusIds, List<string> projectTeamMemberIds, List<string> projectCoordinatorIds, List<string> projectPriorityIds, List<string> projectTypeIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.Active && !x.IsTemplate && x.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.UpdatedOnUtc);
            }

            query = query.Take(5).Select(x => new Project
            {
                Id = x.Id,
                Name = x.Name,
                ProjectEmployeeMappings = x.ProjectEmployeeMappings.Where(m => !m.Deleted).Select(mapping => new ProjectEmployeeMapping
                {
                    Id = mapping.Id,
                    Employee = new Employee
                    {
                        Id = mapping.Employee.Id,
                        Person = new Person
                        {
                            Id = mapping.Employee.Person.Id,
                            FullName = mapping.Employee.Person.FirstName + " " + mapping.Employee.Person.LastName,
                        },
                    },
                    EmployeeRoleDropdown = new DropDown
                    {
                        Id = mapping.EmployeeRoleDropdown.Id,
                        DropDownValue = mapping.EmployeeRoleDropdown.DropDownValue,
                    }

                }).ToList()
            });

            var list = new PagedList<Project>(query, page, pageSize);
            return list;
        }
        #endregion

        #region WorkBoard Ver 3
        public async Task<Project> GettWorkBoardByProjecId(string SiteId, string ProjectId, DateTime GetDateTime)
        {
            var query = _projectRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Id == ProjectId);

            query = query.Select(a => new Project
            {
                Id = a.Id,
                //SiteId = a.SiteId,
                Name = a.Name,
                SortOrder = a.SortOrder,
                ProjectSwimLanes = a.ProjectSwimLanes.Where(b => !b.Deleted).OrderBy(b => b.SortOrder).Select(b => new Models.ProjectSwimLanes
                {
                    Id = b.Id,
                    ProjectId = b.ProjectId,
                    SwimlaneTypeId = b.SwimlaneTypeId,
                    Name = b.Name,
                    Color = b.Color,
                    SortOrder = b.SortOrder,
                    Deleted = b.Deleted,
                    SwimlaneType = new DropDown
                    {
                        Id = b.SwimlaneType.Id,
                        DropDownValue = b.SwimlaneType.DropDownValue,
                    },
                    ProjectSwimLanesList = b.ProjectSwimLanesList.Where(c => !c.Deleted).OrderBy(c => c.SortOrder).Select(c => new ProjectSwimLanesList
                    {
                        Id = c.Id,
                        ProjectSwimlaneId = c.ProjectSwimlaneId,
                        Name = c.Name,
                        SortOrder = c.SortOrder,
                        Color = c.Color,
                        Deleted = c.Deleted,
                        ProjectSwimLanesListsTasks = c.ProjectSwimLanesListsTasks.Where(d => !d.Deleted).OrderBy(d => d.SortOrder).Select(d => new ProjectSwimLanesListsTasks
                        {
                            Id = d.Id,
                            ProjectSwimlaneListId = d.ProjectSwimlaneListId,
                            ProjectTaskId = d.ProjectTaskId,
                            SortOrder = d.SortOrder,
                            Color = d.Color,
                            Deleted = d.Deleted,
                            ProjectTask = new ProjectTask
                            {
                                Id = d.ProjectTask.Id,
                                ProjectId = d.ProjectTask.ProjectId,
                                ProjectModuleId = d.ProjectTask.ProjectModuleId,
                                Name = d.ProjectTask.Name,
                                StartDate = d.ProjectTask.StartDate,
                                EndDate = d.ProjectTask.EndDate,
                                EstimateTime = d.ProjectTask.EstimateTime,
                                Color = d.ProjectTask.Color,
                                AssignedToId = d.ProjectTask.AssignedToId,
                                ProjectModule = new ProjectModule
                                {
                                    Id = d.ProjectTask.ProjectModule.Id,
                                    Name = d.ProjectTask.ProjectModule.Name,
                                },
                                AssignedTo = new Employee
                                {
                                    Id = d.ProjectTask.AssignedTo.Id,
                                    Person = new Person
                                    {
                                        FirstName = d.ProjectTask.AssignedTo.Person.FirstName,
                                        LastName = d.ProjectTask.AssignedTo.Person.LastName,
                                        FullName = d.ProjectTask.AssignedTo.Person.FullName,
                                        PrimaryEmailAddress = d.ProjectTask.AssignedTo.Person.PrimaryEmailAddress,
                                    }
                                },
                                StatusId = d.ProjectTask.StatusId,
                                Status = new DropDown
                                {
                                    Id = d.ProjectTask.Status.Id,
                                    DropDownValue = d.ProjectTask.Status.DropDownValue,
                                    BgColor = d.ProjectTask.Status.BgColor,
                                    Color = d.ProjectTask.Status.Color,
                                },
                                PriorityId = d.ProjectTask.PriorityId,
                                Priority = new DropDown
                                {
                                    Id = d.ProjectTask.Priority.Id,
                                    DropDownValue = d.ProjectTask.Priority.DropDownValue,
                                    BgColor = d.ProjectTask.Priority.BgColor,
                                    Color = d.ProjectTask.Priority.Color,
                                },
                                ProjectActivities = d.ProjectTask.ProjectActivities
                                .Where(x =>
                                    !x.Deleted &&
                                    x.AssignedTo.Active &&
                                    x.TargetMonth.HasValue &&
                                    x.TargetMonth.Value.Year == GetDateTime.Year &&
                                    x.TargetMonth.Value.Month == GetDateTime.Month
                                ).Select(pa => new ProjectActivity
                                {
                                    Id = pa.Id,
                                    AssignedToId = pa.AssignedToId,
                                    EstimateHours = pa.EstimateHours,
                                    AssignedTo = new Employee
                                    {
                                        Id = pa.AssignedTo.Id,
                                        Active = pa.AssignedTo.Active,
                                        Person = new Person
                                        {
                                            Id = pa.AssignedTo.Person.Id,
                                            FirstName = pa.AssignedTo.Person.FirstName,
                                            LastName = pa.AssignedTo.Person.LastName,
                                            PrimaryEmailAddress = pa.AssignedTo.Person.PrimaryEmailAddress,
                                            PrimaryPhoneNumber = pa.AssignedTo.Person.PrimaryPhoneNumber,
                                        }
                                    }
                                }).ToList(),
                                ProjectTask_Tags = d.ProjectTask.ProjectTask_Tags.Where(e => !e.Deleted && !e.Tags.Deleted).OrderBy(e => e.Tags.Name).Select(za => new ProjectTask_Tags
                                {
                                    Id = za.Id,
                                    Tags = new Tags
                                    {
                                        Id = za.Tags.Id,
                                        Name = za.Tags.Name,
                                        BgColor = za.Tags.BgColor,
                                        Color = za.Tags.Color
                                    }
                                }).ToList()
                            }
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).AsQueryable();

            return await query.FirstOrDefaultAsync();
        }
        #endregion

        #region GetAllProjectDataByProjectId
        public async Task<Project> GetAllProjectDataByProjectId(string SiteId, string projectId)
        {
            var query = _projectRepository.TableNoTracking
            .Where(x => !x.Deleted && x.SiteId == SiteId && x.Id == projectId)
            .Include(x => x.ProjectModules.Where(x => !x.Deleted))
            .ThenInclude(b => b.ProjectTasks.Where(b => !b.Deleted))
            .ThenInclude(c => c.ProjectActivities.Where(c => !c.Deleted))
            .Include(x => x.ProjectEmployeeMappings.Where(pem => !pem.Deleted));

            return await query.FirstOrDefaultAsync();
        }
        #endregion

        #region InsertProject
        // Title: InsertProject
        // Description: This method inserts a new Project entity into the repository. It takes a Project object as input and uses the _projectRepository to handle the insertion operation.
        public void InsertProject(Project entity)
        {
            _projectRepository.Insert(entity);
        }
        #endregion

        #region UpdateProject
        // Title: UpdateProject
        // Description: This method updates the specified Project entity in the repository. It takes a Project object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateProject(Project entity)
        {
            _projectRepository.Update(entity);
        }
        #endregion

        #region DeleteProject
        // Title: DeleteProject
        // Description: Marks the specified project entity as deleted by setting its `Deleted` property to true. 
        public void DeleteProject(Project entity)
        {
            entity.Deleted = true;
            _projectRepository.Update(entity);
        }
        #endregion

        #region InsertProjectList
        public void InsertProjectList(IList<Project> entities)
        {
            _projectRepository.Insert(entities);
        }
        #endregion

        #region UpdateProjectList
        public void UpdateProjectList(IList<Project> entities)
        {
            _projectRepository.Update(entities);
        }
        #endregion

        #region DeleteProjectList
        public void DeleteProjectList(List<Project> entities)
        {
            var list = new List<Project>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _projectRepository.Update(list);
        }
        #endregion

        #region Get Project List From SP

        public async Task<object> GetProjectsAsync(ProjectListRequest request)
        {
            using var conn = _dbContext.Database.GetDbConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@SiteId", request.SiteId);

            parameters.Add("@Deleted", request.Deleted);
            parameters.Add("@IsTemplate", request.IsTemplate);
            parameters.Add("@IsPinned", request.IsPinned);
            parameters.Add("@Active", request.Active);

            parameters.Add("@AspNetUserId", request.AspNetUserId);
            parameters.Add("@CustomerIds", ToCsv(request.CustomerIds));
            parameters.Add("@CompanyContactIds", ToCsv(request.CompanyContactIds));
            parameters.Add("@Ids", ToCsv(request.Ids));
            parameters.Add("@CategoryIds", ToCsv(request.CategoryIds));
            parameters.Add("@SubCategoryIds", ToCsv(request.SubCategoryIds));
            parameters.Add("@StatusIds", ToCsv(request.StatusIds));
            parameters.Add("@PriorityIds", ToCsv(request.PriorityIds));
            parameters.Add("@TypeIds", ToCsv(request.TypeIds));
            parameters.Add("@CoordinatorIds", ToCsv(request.CoordinatorIds));
            parameters.Add("@ManagerIds", ToCsv(request.ManagerIds));
            parameters.Add("@LeadIds", ToCsv(request.LeadIds));
            parameters.Add("@ProjectTagIds", ToCsv(request.ProjectTagIds));

            parameters.Add("@Search", string.IsNullOrEmpty(request.Search) ? null : request.Search);

            parameters.Add("@DefaultSortBy", request.DefaultSortBy ?? "createdOnUtc");
            parameters.Add("@DefaultSortDirection", request.DefaultSortDirection ?? "DESC");
            parameters.Add("@MultiColumnSorting", request.MultiColumnSorting != null && request.MultiColumnSorting.Any() ? JsonSerializer.Serialize(request.MultiColumnSorting) : null);

            parameters.Add("@PageNumber", request.Page);
            parameters.Add("@PageSize", request.PageSize);

            var rawRows = await conn.QueryAsync("SP_GetProjectsList", parameters, commandType: CommandType.StoredProcedure);

            var (rows, totalCount) = ParseDapperResult(rawRows);

            return new { data = rows, count = totalCount };
        }

        private static string? ToCsv(List<string> ids)
        {
            return ids != null && ids.Any()
                ? string.Join(",", ids)
                : null;
        }

        private static (List<Dictionary<string, object>> rows, int totalCount) ParseDapperResult(IEnumerable<dynamic> rawRows)
        {
            int totalCount = 0;
            var rows = new List<Dictionary<string, object>>();

            string[]? keys = null;
            bool[]? isJsonColumn = null;
            bool[]? isSkipped = null;
            string[]? cleanKeys = null;
            int totalCountIndex = -1;

            foreach (IDictionary<string, object> row in rawRows)
            {
                var rowValues = row.Values.ToArray();

                if (keys == null)
                {
                    keys = row.Keys.ToArray();
                    var len = keys.Length;
                    isJsonColumn = new bool[len];
                    isSkipped = new bool[len];
                    cleanKeys = new string[len];

                    for (int i = 0; i < len; i++)
                    {
                        var key = keys[i];

                        if (key == "TotalCount")
                        {
                            isSkipped[i] = true;
                            totalCountIndex = i;
                            continue;
                        }

                        if (key.EndsWith("Json", StringComparison.OrdinalIgnoreCase))
                        {
                            isJsonColumn[i] = true;
                            cleanKeys[i] = key[..^4];
                        }
                        else
                        {
                            cleanKeys[i] = key;
                        }
                    }

                    if (totalCountIndex >= 0 && rowValues[totalCountIndex] != null)
                        totalCount = Convert.ToInt32(rowValues[totalCountIndex]);
                }

                var parsed = new Dictionary<string, object>(keys.Length);

                for (int i = 0; i < keys.Length; i++)
                {
                    if (isSkipped[i]) continue;

                    var value = rowValues[i];
                    parsed[cleanKeys[i]] = isJsonColumn[i] && value is string str
                        ? (string.IsNullOrEmpty(str) ? null : JsonSerializer.Deserialize<JsonElement>(str))
                        : value;
                }

                rows.Add(parsed);
            }

            return (rows, totalCount);
        }
        #endregion

        private async Task<bool> IsCurrentUserAdmin(string CId, string SiteId)
        {
            var userdata = await _userManager.FindByIdAsync(CId);
            var user = await _userManager.FindByNameAsync(userdata.UserName);
            //var roles = await _userManager.GetRolesAsync(user);
            var roles = await _applicationUserRoleService.GetRoleNamesByUserAndSite(user.Id, SiteId);
            var isAdmin = roles.Contains("Admin") || roles.Contains("Site Super Admin") || roles.Contains("System Super Admin");

            return isAdmin;
        }
    }
}