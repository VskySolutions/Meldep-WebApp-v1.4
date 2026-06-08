using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.ProjectModules
{
    public class ProjectModuleService : IProjectModuleService
    {
        #region Define Services
        private readonly IRepository<ProjectModule> _projectModuleRepository;
        private readonly IRepository<Notes> _notesRepository;
        #endregion

        #region Services Initializations
        public ProjectModuleService(IRepository<ProjectModule> projectModuleRepository, IRepository<Notes> notesRepository)
        {
            _projectModuleRepository = projectModuleRepository;
            _notesRepository = notesRepository;
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

        #region GetAllProjectModules
        // Title: GetAllProjectModules
        // Description: This method retrieves a paginated list of Project Modules based on various search criteria such as name, 
        // Project Modules status. It also supports sorting and includes related data such as ProjectModules status. 
        // The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<ProjectModule> GetAllProjectModules(string SiteId, string SearchText, List<string> projectIds, List<string> projectModuleTypeIds, List<string> projectModuleStatusIds, string projectId, List<string> customerIds, List<string> companyContactIds, bool isShowCloseStatus, string pageName, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            //var query = _projectModuleRepository.TableNoTracking.Where(x => !x.Deleted);
            var query = _projectModuleRepository.TableNoTracking.Where(x => !x.Deleted && !x.Project.Deleted && x.Project.Active && x.SiteId == SiteId && !x.Project.IsTemplate);

            if (projectIds != null && projectIds.Any())
                query = query.Where(x => projectIds.Contains(x.ProjectId));

            if (projectModuleTypeIds != null && projectModuleTypeIds.Any())
                query = query.Where(x => projectModuleTypeIds.Contains(x.ProjectModuleTypeId));

            if (projectModuleStatusIds != null && projectModuleStatusIds.Any())
                query = query.Where(x => projectModuleStatusIds.Contains(x.ProjectModuleStatusId));
            else if(pageName != "PL")
                query = query.Where(x => x.ProjectModuleStatus.DropDownValue != "Close");

            if (projectId != null && !string.IsNullOrWhiteSpace(projectId))
                query = query.Where(x => x.ProjectId == projectId);

            if (customerIds != null && customerIds.Any())
                query = query.Where(x => customerIds.Contains(x.Project.CustomerId));

            if (companyContactIds != null && companyContactIds.Any())
                query = query.Where(x => companyContactIds.Contains(x.Project.CompanyContactId));

            if (pageName == "PL") // planner page
            {
                if(isShowCloseStatus == false)                
                    query = query.Where(x => x.ProjectModuleStatus.DropDownValue != "Close");//Show data without close status
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.Project.Name.ToLower().Contains(SearchText.ToLower()) ||
                    (m.CreatedBy.Person.FirstName + " " + m.CreatedBy.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    m.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.ProjectModuleStatus.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.CreatedOnUtc.Date == parsedDate.Date
                );
            }

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "calendar")
                {
                    query = query.OrderBy(t => t.Project.Name).ThenByDescending(t => t.CreatedOnUtc);
                }
                else
                {
                    var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                    query = query.OrderBy(orderBy);
                }
            }
            else
            {
                query = query.OrderBy(x => x.Name);
            }

            if (lookup)
            {
                query = query.Select(x => new ProjectModule
                {
                    Id = x.Id,
                    Name = x.Name,
                });
            }
            else
            {
                query = query.Select(x => new ProjectModule
                {
                    Id = x.Id,
                    Notes = x.Notes,
                    Name = x.Name,
                    ProjectModuleNumber = x.ProjectModuleNumber,
                    Description = x.Description,
                    CloseDate = x.CloseDate,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    TargetDate = x.TargetDate,
                    ProjectId = x.ProjectId,
                    ProjectModuleStatusId = x.ProjectModuleStatusId,
                    ProjectModuleTypeId = x.ProjectModuleTypeId,
                    CreatedOnUtc = x.CreatedOnUtc,
                    CreatedById = x.CreatedById,
                    //SiteId = x.SiteId,
                    Color = x.Color,
                    ProjectTasksCount = x.ProjectTasks.Where(m => !m.Deleted).Count(),
                    Project = new Project
                    {
                        Id = x.Project.Id,
                        Name = x.Project.Name,
                    },
                    ProjectModuleStatus = new DropDown
                    {
                        Id = x.ProjectModuleStatus.Id,
                        DropDownValue = x.ProjectModuleStatus.DropDownValue
                    },
                    ProjectModuleType = new DropDown
                    {
                        Id = x.ProjectModuleType.Id,
                        DropDownValue = x.ProjectModuleType.DropDownValue
                    },
                    CreatedBy = new ApplicationUser
                    {
                        Id = x.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = x.CreatedBy.PersonId,
                            FirstName = x.CreatedBy.Person.FirstName,
                            LastName = x.CreatedBy.Person.LastName,
                        }
                    },
                    ProjectModuleNotesCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Projects Module").Count(),
                });
            }
            var list = new PagedList<ProjectModule>(query, page, pageSize);
            return list;
        }

        public IPagedList<ProjectModule> GetAllProjectModulesForDashboard(string SiteId, string projectId, string pageName, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            //var query = _projectModuleRepository.TableNoTracking.Where(x => !x.Deleted);
            var query = _projectModuleRepository.TableNoTracking.Where(x => !x.Deleted && !x.Project.Deleted && x.ProjectId == projectId && x.SiteId == SiteId);
           
            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.Name);
            }

            if (lookup)
            {
                query = query.Select(x => new ProjectModule
                {
                    Id = x.Id,
                    Name = x.Name,
                });
            }
            else
            {
                query = query.Select(x => new ProjectModule
                {
                    Id = x.Id,
                    Notes = x.Notes,
                    Name = x.Name,
                    ProjectModuleNumber = x.ProjectModuleNumber,
                    Description = x.Description,
                    CloseDate = x.CloseDate,
                    TargetDate = x.TargetDate,
                    ProjectModuleStatusId = x.ProjectModuleStatusId,
                    ProjectModuleTypeId = x.ProjectModuleTypeId,
                    CreatedOnUtc = x.CreatedOnUtc,
                    CreatedById = x.CreatedById,
                    //SiteId = x.SiteId,
                    ProjectTasksCount = x.ProjectTasks.Where(m => !m.Deleted).Count(),
                    Project = new Project
                    {
                        Id = x.Project.Id,
                        Name = x.Project.Name
                    },
                    ProjectModuleStatus = new DropDown
                    {
                        Id = x.ProjectModuleStatus.Id,
                        DropDownValue = x.ProjectModuleStatus.DropDownValue
                    },
                    ProjectModuleType = new DropDown
                    {
                        Id = x.ProjectModuleType.Id,
                        DropDownValue = x.ProjectModuleType.DropDownValue
                    },
                    CreatedBy = new ApplicationUser
                    {
                        Id = x.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = x.CreatedBy.PersonId,
                            FirstName = x.CreatedBy.Person.FirstName,
                            LastName = x.CreatedBy.Person.LastName,
                            FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                        }
                    },
                });
            }
            var list = new PagedList<ProjectModule>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetProjectModuleById
        // Title: GetProjectModuleById
        // Description: This method retrieves a Project Module from the database by its unique identifier (`id`).       
        public async Task<int> GetLastProjectModuleNumber()
        {
            var query = await _projectModuleRepository.TableNoTracking.OrderByDescending(m => m.ProjectModuleNumber).FirstOrDefaultAsync();
            return query == null ? 1 : query.ProjectModuleNumber;
        }
        public async Task<int> GetLastSortOrderOfProjectModules(string projectId)
        {
            var query = await _projectModuleRepository.TableNoTracking.Where(m => !m.Deleted && m.ProjectId == projectId).OrderByDescending(m => m.SortOrder).FirstOrDefaultAsync();
            return query == null ? 0 : query.SortOrder;
        }
        public async Task<ProjectModule> GetProjectModuleById(string id)
        {
            var query = _projectModuleRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        public async Task<ProjectModule> GetAllModulesAndTasksById(string projectModuleId)
        {
            var query = await _projectModuleRepository.TableNoTracking
                .Where(x => !x.Deleted && x.Id == projectModuleId)
                .Include(m => m.ProjectTasks.Where(m => !m.Deleted))
                .FirstOrDefaultAsync();
            return query;
        }
        public async Task<List<ProjectModule>> GetAllModulesByProjectId(string projectId)
        {
            var query = _projectModuleRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectId == projectId);
            query = query.OrderByDescending(x => x.CreatedOnUtc);
            query = query.Select(x => new ProjectModule
            {
                Id = x.Id,
                Name = x.Name,
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion        

        #region GetAllProjectModuleListForDropdown
        public async Task<List<CommonDropDown>> GetAllProjectModuleListForDropdown(string SiteId, bool isTemplate, bool showTaskCount, string projectId = null)
        {
            var query = _projectModuleRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId && !m.IsMoved && m.ProjectModuleStatus.DropDownValue != "Close" && m.Project.IsTemplate == isTemplate);

            if (!string.IsNullOrWhiteSpace(projectId))
            {
                // Split the comma-separated projectIds into an array
                var projectIdArray = projectId.Split(',');
                // Filter query based on the array of project IDs
                query = query.Where(x => projectIdArray.Contains(x.ProjectId) && !x.Project.Deleted);
            }

            var list = await query
                       .OrderBy(x => x.Name.Replace(" ", ""))
                       .Select(x => new CommonDropDown
                       {
                           Text = showTaskCount ? x.Name + " (" + x.ProjectTasks.Count(m =>
                                                    !m.Deleted &&
                                                    !m.IsMoved &&
                                                    !m.ProjectModule.Deleted &&
                                                    !m.ProjectModule.IsMoved &&
                                                    !m.Project.Deleted &&
                                                    m.Project.Active &&
                                                    m.Status.DropDownValue != "Close"
                                              ) + ")": x.Name,
                           Value = x.Id,
                       })
                       .ToListAsync();
            return list;
        }
        #endregion

            #region GetProjectModuleDetailsById
            // Title: GetProjectModuleDetailsById
            // Description: The method selects relevant fields from the ProjectModule entity, including related entities such as ProjectModule status, and returns a `ProjectModule` object with these details. 
        public async Task<ProjectModule> GetProjectModuleDetailsById(string id)
        {
            var query = _projectModuleRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new ProjectModule
            {
                Id = x.Id,
                Notes = x.Notes,
                Name = x.Name,
                ProjectModuleNumber = x.ProjectModuleNumber,
                Description = x.Description,
                CloseDate = x.CloseDate,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                TargetDate = x.TargetDate,
                ProjectModuleStatusId = x.ProjectModuleStatusId,
                ProjectModuleTypeId = x.ProjectModuleTypeId,
                ProjectId = x.ProjectId,
                CreatedOnUtc = x.CreatedOnUtc,
                //SiteId = x.SiteId,
                SortOrder = x.SortOrder,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                ProjectModuleStatus = new DropDown
                {
                    Id = x.ProjectModuleStatus.Id,
                    DropDownValue = x.ProjectModuleStatus.DropDownValue
                },
                ProjectModuleType = new DropDown
                {
                    Id = x.ProjectModuleType.Id,
                    DropDownValue = x.ProjectModuleType.DropDownValue
                },
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    UserName = x.CreatedBy.UserName,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FirstName = x.CreatedBy.Person.FirstName,
                        LastName = x.CreatedBy.Person.LastName,
                    }
                },
                UpdatedBy = new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FirstName = x.UpdatedBy.Person.FirstName,
                        LastName = x.UpdatedBy.Person.LastName,
                    }
                },
                ProjectModuleFilesList = x.ProjectModuleFilesList.Where(x => !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(mapping => new ProjectModuleFiles
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
        #endregion

        #region GetProjectModuleByName
        // Title: GetProjectModuleByName
        // Description: This method retrieves a ProjectModule based on its name and Id. It allows an optional exclusion of a projectModule by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific project Module. The method returns the first matching project Module or null if no match is found.
        public async Task<ProjectModule> GetProjectModuleByName(string name, string ProjectId, string id = null)
        {
            var query = _projectModuleRepository.TableNoTracking.Where(x => !x.Deleted && x.Name.ToLower() == name.ToLower() && x.ProjectId == ProjectId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region IsProjectModuleSortOrderExists
        // Title: IsProjectModuleSortOrderExists
        // Description: This method check Project Module sort order based on its siteId.
        public async Task<bool> IsProjectModuleSortOrderExists(string siteId, string ProjectId, int sortOrder, string moduleId)
        {
            return await _projectModuleRepository.TableNoTracking.AnyAsync(x => !x.Deleted && x.SiteId == siteId && x.ProjectId == ProjectId && x.SortOrder == sortOrder && x.Id != moduleId);
        }
        #endregion

        #region InsertProjectModule
        // Title: InsertProjectModule
        // Description: This method inserts a new ProjectModule entity into the repository. It takes a ProjectModule object as input and uses the _projectModuleRepository to handle the insertion operation.
        public void InsertProjectModule(ProjectModule entity)
        {
            _projectModuleRepository.Insert(entity);
        }
        #endregion

        #region InsertProjectModuleList
        // Title: InsertProjectModuleList
        // Description: This method inserts a list of ProjectModules entity into the repository. It takes a ProjectModule object as input and uses the _projectModuleRepository to handle the insertion operation.
        public void InsertProjectModuleList(IList<ProjectModule> entities)
        {
            _projectModuleRepository.Insert(entities);
        }
        #endregion

        #region UpdateProjectModule
        // Title: UpdateProjectModule
        // Description: This method updates the specified ProjectModule entity in the repository. It takes a ProjectModule object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateProjectModule(ProjectModule entity)
        {
            _projectModuleRepository.Update(entity);
        }
        #endregion

        #region UpdateProjectModuleList
        // Title: UpdateProjectModuleList
        // Description: This method updates the list of ProjectModule entity in the repository. It takes a ProjectModule object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateProjectModuleList(IList<ProjectModule> entities)
        {
            _projectModuleRepository.Update(entities);
        }
        #endregion      

        #region DeleteProjectModule
        // Title: DeleteProjectModule
        // Description: Marks the specified ProjectModule entity as deleted by setting its `Deleted` property to true. 
        public void DeleteProjectModule(ProjectModule entity)
        {
            entity.Deleted = true;

            _projectModuleRepository.Update(entity);
        }
        #endregion
    }
}
