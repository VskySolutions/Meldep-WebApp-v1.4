using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Common;
using Vsky.Services.Sites;

namespace Vsky.Services.Requirements
{
    public class RequirementService : IRequirementService
    {
        #region Define Services
        private readonly IRepository<Requirement> _requirementRepository;
        private readonly IRepository<Notes> _notesRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<VWProjectRequirementStatusSummary> _vWProjectRequirementStatusSummary;
        private readonly ICommonService _commonService;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations

        public RequirementService(IRepository<Requirement> requirementRepository,
            IRepository<Notes> notesRepository,
            UserManager<ApplicationUser> userManager,
            IRepository<VWProjectRequirementStatusSummary> vWProjectRequirementStatusSummary,
            ICommonService commonService,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _requirementRepository = requirementRepository;
            _notesRepository = notesRepository;
            _userManager = userManager;
            _vWProjectRequirementStatusSummary = vWProjectRequirementStatusSummary;
            _commonService = commonService;
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

        #region GetAllRequirements
        // Title: GetAllRequirements
        // Description: This method retrieves a paginated list of Requirement based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public async Task<IPagedList<Requirement>> GetAllRequirements(
            string SiteId,
            string LoggedUserId,
            string SearchText,
            int requirementNumber,
            List<string> projectIds,
            List<string> projectModuleIds,
            List<string> requirementGroupIds,
            string name,
            string editingStatus,
            List<string> statusIds,
            List<string> requirementTypeIds,
            string identifiedUserTypeId,
            List<string> identifiedCustomerIds,
            List<string> identifiedEmployeeIds,
            List<string> requirementTagIds,
            DateTime? fromDate,
            DateTime? toDate,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {
            //var query = _requirementRepository.TableNoTracking.Where(x => !x.Deleted);
            var query = _requirementRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, SiteId);

            if (!IsAdmin)
                query = query.Where(p => p.Project.ProjectUserMappings.Any(m => !m.Deleted && m.AspNetUserId == LoggedUserId && (m.FullAccess || m.ViewOnly || m.Notes)));

            if (requirementNumber != 0)
            {
                query = query.Where(x => x.RequirementNumber == requirementNumber);
            }

            if (projectIds != null && projectIds.Any())
                query = query.Where(x => projectIds.Contains(x.ProjectId));

            if (projectModuleIds != null && projectModuleIds.Any())
                query = query.Where(x => projectModuleIds.Contains(x.ProjectModuleId));

            if (!string.IsNullOrWhiteSpace(editingStatus))
                query = query.Where(x => editingStatus == "Draft" ? x.EditingStatus == 1 : x.EditingStatus != 1);

            if (statusIds != null && statusIds.Any())
                query = query.Where(x => statusIds.Contains(x.StatusId));

            if (requirementTypeIds != null && requirementTypeIds.Any())
                query = query.Where(x => requirementTypeIds.Contains(x.RequirementTypeId));

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(identifiedUserTypeId))
                query = query.Where(x => x.IdentifiedUserType == identifiedUserTypeId);

            if (identifiedCustomerIds != null && identifiedCustomerIds.Any())
                query = query.Where(x => identifiedCustomerIds.Contains(x.IdentifiedCustomerId));

            if (identifiedEmployeeIds != null && identifiedEmployeeIds.Any())
                query = query.Where(x => identifiedEmployeeIds.Contains(x.IdentifiedEmployeeId));

            if (requirementTagIds?.Any() == true) 
                query = query.Where(x => x.RequirementTags.Any(t => !t.Deleted && t.AspNetUserId == LoggedUserId &&  requirementTagIds.Contains(t.Tags.Id)));

            //Search by FromDate and Todate
            if (fromDate != null)
                query = query.Where(x => x.CreatedOnUtc >= fromDate);
            if (toDate != null)
                query = query.Where(a => a.CreatedOnUtc <= toDate);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query
                    .OrderByDescending(x => x.RequirementPinned
                        .Any(p => p.AspNetUserId == LoggedUserId && p.IsPinned))
                    .ThenBy(orderBy);
            }
            else
            {
                query = query
                    .OrderByDescending(x => x.RequirementPinned
                        .Any(p => p.AspNetUserId == LoggedUserId && p.IsPinned))
                    .ThenByDescending(x => x.CreatedOnUtc);
            }
           
            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                       m.RequirementNumber.ToString().Contains(SearchText.ToLower()) ||
                       m.Project.Name.ToLower().Contains(SearchText.ToLower()) ||
                       m.ProjectModule.Name.ToLower().Contains(SearchText.ToLower()) ||
                       m.Title.ToLower().Contains(SearchText.ToLower()) ||
                       m.Status.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                       m.Priority.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                       m.UserType.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                       (m.Employee.Person.FirstName + " " + m.Employee.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                       m.CreatedOnUtc.Date == parsedDate.Date ||
                       m.RequirementTags.Any(t => t.Tags.Name.ToLower().Contains(SearchText.ToLower())) ||
                       m.Area.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                       m.Workspace.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                       m.RequirementType.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                       m.IdentifiedDate == parsedDate.Date ||
                       (m.RequirementEntered.Person.FirstName + " " + m.RequirementEntered.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                       m.ApprovalStatusDropDown.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                       (m.CreatedBy.Person.FirstName + " " + m.CreatedBy.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                       (m.UpdatedBy.Person.FirstName + " " + m.UpdatedBy.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                       (m.CreatedOnUtc.ToString().Contains(SearchText.ToLower())) ||
                       (SearchText.ToLower() == "draft" && m.EditingStatus == 1) ||
                       (SearchText.ToLower() == "confirmed" && m.EditingStatus != 1)
                );
            }
         
            // Apply multi-level dictionary sorting
            if (sorts != null && sorts.Count > 0)
            {
                query = _commonService.ApplySorting(query, sorts);
            }

            var notesQuery = _notesRepository.TableNoTracking.Where(n => !n.Deleted && n.Type == "Requirement");
            query = query.Select(x => new Requirement
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                Title = x.Title,
                StatusId = x.StatusId,
                EditingStatus = x.EditingStatus,
                IdentifiedUserType = x.IdentifiedUserType,
                RequirementNumber = x.RequirementNumber,
                IdentifiedDate = x.IdentifiedDate,
                PriorityId = x.PriorityId,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                Customer = new Person
                {
                    Id = x.Customer.Id,
                    FullName = x.Customer.FirstName + " " + x.Customer.LastName,
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    ProjectUserMappings = x.Project.ProjectUserMappings.Where(m => !m.Deleted && m.ProjectId == x.Project.Id && (IsAdmin || m.AspNetUserId == LoggedUserId)).Select(mapping => new ProjectUserMapping
                    {
                        Id = mapping.Id,
                        FullAccess = mapping.FullAccess,
                        ViewOnly = mapping.ViewOnly,
                        Notes = mapping.Notes
                    }).Take(1).ToList()
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name
                },
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                UserType = new DropDown
                {
                    Id = x.UserType.Id,
                    DropDownValue = x.UserType.DropDownValue
                },
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                Area = new DropDown
                {
                    Id = x.Area.Id,
                    DropDownValue = x.Area.DropDownValue
                },
                Workspace = new DropDown
                {
                    Id = x.Workspace.Id,
                    DropDownValue = x.Workspace.DropDownValue
                },
                RequirementEntered = new Employee
                {
                    Person = new Person
                    {
                        Id = x.RequirementEntered.Person.Id,
                        FullName = x.RequirementEntered.Person.FirstName + " " + x.RequirementEntered.Person.LastName,
                    }
                },
                RequirementType = new DropDown
                {
                    Id = x.RequirementType.Id,
                    DropDownValue = x.RequirementType.DropDownValue
                },
                CreatedBy = new ApplicationUser
                {
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName,
                    }
                },
                UpdatedBy = new ApplicationUser
                {
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName,
                    }
                },
                ApprovalStatusDropDown = new DropDown
                {
                    Id = x.ApprovalStatusDropDown.Id,
                    DropDownValue = x.ApprovalStatusDropDown.DropDownValue
                },
                IsPinned = x.RequirementPinned.Any(p => p.AspNetUserId == LoggedUserId && p.IsPinned),
                RequirementColor = x.RequirementColors.Where(c => c.AspNetUserId == LoggedUserId && !c.Deleted).Select(c => c.Color).FirstOrDefault(),
                RequirementTags = x.RequirementTags.Where(t => !t.Deleted && !t.Tags.Deleted && t.AspNetUserId == LoggedUserId).OrderBy(t => t.Tags.Name).Select(t => new RequirementTags
                {
                    Id = t.Id,
                    TagId = t.TagId,
                    RequirementId = t.RequirementId,
                    Tags = new Tags { Id = t.Tags.Id, Name = t.Tags.Name, Color = t.Tags.Color, BgColor = t.Tags.BgColor }
                }).ToList(),
                ProjectTaskRelatedMappings = x.ProjectTaskRelatedMappings.Where(m => m.RequirementId == x.Id && !m.Deleted && m.RequirementId != null)
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
                RequirementNotesCount = notesQuery.Where(m => m.SubModuleId == x.Id).Count(),
                LastNote = notesQuery.Where(n => n.SubModuleId == x.Id).OrderByDescending(n => n.CreatedOnUtc).Select(n => n.Note).FirstOrDefault()
            });

            var list = new PagedList<Requirement>(query, page, pageSize);
            return list;
        }

        public IPagedList<Requirement> GetAllRequirementsForDashboard(string SiteId, string projectId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _requirementRepository.TableNoTracking.Where(x => !x.Deleted && !x.RequirementGroup.Deleted && x.SiteId == SiteId && x.ProjectId == projectId);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }
            query = query.Select(x => new Requirement
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                Title = x.Title,
                StatusId = x.StatusId,
                IdentifiedDate = x.IdentifiedDate,
                CloseDate = x.CloseDate,
                Description = x.Description,
                Notes = x.Notes,
                EditingStatus = x.EditingStatus,
                ApprovalStatus = x.ApprovalStatus,
                IdentifiedCustomerId = x.IdentifiedCustomerId,
                IdentifiedEmployeeId = x.IdentifiedEmployeeId,
                IdentifiedUserType = x.IdentifiedUserType,
                PriorityId = x.PriorityId,
                RequirementEnteredBy = x.RequirementEnteredBy,
                RequirementGroupId = x.RequirementGroupId,
                RequirementNumber = x.RequirementNumber,
                CreatedOnUtc = x.CreatedOnUtc,
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                RequirementEntered = new Employee
                {
                    Person = new Person
                    {
                        Id = x.RequirementEntered.Person.Id,
                        FullName = x.RequirementEntered.Person.FirstName + " " + x.RequirementEntered.Person.LastName,
                    }
                },
                Customer = new Person
                {
                    Id = x.Customer.Id,
                    FullName = x.Customer.FirstName + " " + x.Customer.LastName,
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                UserType = new DropDown
                {
                    Id = x.UserType.Id,
                    DropDownValue = x.UserType.DropDownValue
                },
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                ApprovalStatusDropDown = new DropDown
                {
                    Id = x.ApprovalStatusDropDown.Id,
                    DropDownValue = x.ApprovalStatusDropDown.DropDownValue
                },
                RequirementGroup = new RequirementGroup
                {
                    Id = x.RequirementGroup.Id,
                    Name = x.RequirementGroup.Name
                },
            });

            var list = new PagedList<Requirement>(query, page, pageSize);
            return list;
        }
        #endregion

        public List<VWProjectRequirementStatusSummary> GetRequirementStatusSummaryByProjectIds(List<string> projectIds)
        {
            var list = _vWProjectRequirementStatusSummary.TableNoTracking.Where(x => projectIds.Contains(x.ProjectId)).ToList();
            return list;
        }

        #region GetRequirementById
        // Title: GetRequirementById
        // Description: This method retrieves a Requirement from the database by its unique identifier (`id`). 
        public async Task<Requirement> GetRequirementById(string id)
        {
            var query = _requirementRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion


        #region GetLastRequirementNumber
        // Title: GetLastRequirementNumber
        // Description: This method retrieves the highest RequirementNumber by ordering the records in descending order, or returns 1 if no record exists.
        public async Task<int> GetLastRequirementNumber()
        {
            var query = await _requirementRepository.TableNoTracking.OrderByDescending(m => m.RequirementNumber).FirstOrDefaultAsync();
            return query == null ? 1 : query.RequirementNumber;
        }
        #endregion

        #region GetRequirementDetailsById
        // Title: GetRequirementDetailsById
        // Description: The method selects relevant fields from the Requirement entity.
        public async Task<Requirement> GetRequirementDetailsById(string id)
        {
            var query = _requirementRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new Requirement
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ProjectModuleId = x.ProjectModuleId,
                Title = x.Title,
                AreaId = x.AreaId,
                WorkspaceId = x.WorkspaceId,
                StatusId = x.StatusId,
                RequirementTypeId = x.RequirementTypeId,
                IdentifiedDate = x.IdentifiedDate,
                CloseDate = x.CloseDate,
                Description = x.Description,
                Notes = x.Notes,
                EditingStatus = x.EditingStatus,
                ApprovalStatus = x.ApprovalStatus,
                IdentifiedCustomerId = x.IdentifiedCustomerId,
                IdentifiedEmployeeId = x.IdentifiedEmployeeId,
                IdentifiedUserType = x.IdentifiedUserType,
                PriorityId = x.PriorityId,
                RequirementEnteredBy = x.RequirementEnteredBy,
                RequirementGroupId = x.RequirementGroupId,
                RequirementNumber = x.RequirementNumber,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Area = new DropDown
                {
                    Id = x.Area.Id,
                    DropDownValue = x.Area.DropDownValue
                },
                Workspace = new DropDown
                {
                    Id = x.Workspace.Id,
                    DropDownValue = x.Workspace.DropDownValue
                },
                Employee = new Employee
                {
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
                RequirementEntered = new Employee
                {
                    Person = new Person
                    {
                        Id = x.RequirementEntered.Person.Id,
                        FullName = x.RequirementEntered.Person.FirstName + " " + x.RequirementEntered.Person.LastName,
                    }
                },
                Customer = new Person
                {
                    Id = x.Customer.Id,
                    FullName = x.Customer.FirstName + " " + x.Customer.LastName,
                },
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    StartDate = x.Project.StartDate,
                    GoLiveDate = x.Project.GoLiveDate
                },
                ProjectModule = new ProjectModule
                {
                    Id = x.ProjectModule.Id,
                    Name = x.ProjectModule.Name,
                    StartDate = x.ProjectModule.StartDate,
                    EndDate = x.ProjectModule.EndDate
                },
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue
                },
                UserType = new DropDown
                {
                    Id = x.UserType.Id,
                    DropDownValue = x.UserType.DropDownValue
                },
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                ApprovalStatusDropDown = new DropDown
                {
                    Id = x.ApprovalStatusDropDown.Id,
                    DropDownValue = x.ApprovalStatusDropDown.DropDownValue
                },
                RequirementType = new DropDown
                {
                    Id = x.RequirementType.Id,
                    DropDownValue = x.RequirementType.DropDownValue
                },
                //RequirementGroup = new RequirementGroup
                //{
                //    Id = x.RequirementGroup.Id,
                //    Name = x.RequirementGroup.Name
                //},
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
                FilePathDetails = x.FilePathDetails.Where(p => !p.Deleted).Select(p => new FilePathDetails
                {
                    Id = p.Id,
                    FileName = p.FileName,
                    FilePath = p.FilePath,
                    Note = p.Note
                }).ToList(),
                RequirementChangeLog = x.RequirementChangeLog.Where(p => !p.Deleted)
                .OrderByDescending(p => p.CreatedOnUtc)
                .Select(p => new RequirementChangeLog
                {
                    Id = p.Id,
                    RequirementLogDate = p.RequirementLogDate,
                    Description = p.Description,
                    RequirementName = p.RequirementName,
                    EmployeeId = p.EmployeeId,
                    CreatedOnUtc = p.CreatedOnUtc,
                    Employee = new Employee
                    {
                        Person = new Person
                        {
                            Id = p.Employee.Person.Id,
                            FullName = p.Employee.Person.FirstName + " " + p.Employee.Person.LastName,
                        }
                    },
                }).ToList(),
                ProjectTaskRelatedMappings = x.ProjectTaskRelatedMappings.Where(m => m.RequirementId == x.Id && !m.Deleted && m.RequirementId != null)
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
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetRequirementDescriptionById
        // Title: GetRequirementDescriptionById
        // Description: This method retrieves a description from the database by its unique identifier (`id`). 
        public async Task<string> GetRequirementDescriptionById(string id)
        {
            var description = await _requirementRepository.TableNoTracking
                .Where(x => !x.Deleted && x.Id == id)
                .Select(x => x.Description)
                .FirstOrDefaultAsync();

            return description;
        }
        #endregion

        #region GetRequirementByName

        public async Task<Requirement> GetRequirementByName(string title, string ProjectId, string ProjectModuleId = null, string id = null)
        {
            var query = _requirementRepository.TableNoTracking.Where(x => !x.Deleted && x.Title.ToLower() == title.ToLower() && x.ProjectId == ProjectId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        #endregion

        #region InsertRequirement
        // Title: InsertRequirement
        // Description: This method inserts a new Requirement entity into the repository. It takes a Requirement object as input and uses the _requirementRepository to handle the insertion operation.
        public void InsertRequirement(Requirement entity)
        {
            _requirementRepository.Insert(entity);
        }
        #endregion

        #region UpdateRequirement
        // Title: UpdateRequirement
        // Description: This method updates the specified Requirement entity in the repository. It takes a Requirement object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateRequirement(Requirement entity)
        {
            _requirementRepository.Update(entity);
        }
        #endregion

        #region DeleteRequirement
        // Title: DeleteRequirement
        // Description: Marks the specified Requirement entity as deleted by setting its `Deleted` property to true. 
        public void DeleteRequirement(Requirement entity)
        {
            entity.Deleted = true;

            _requirementRepository.Update(entity);
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
