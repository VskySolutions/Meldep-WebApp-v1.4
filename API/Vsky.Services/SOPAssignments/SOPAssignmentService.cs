using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;

namespace Vsky.Services.SOPAssignments
{
    public class SOPAssignmentService : ISOPAssignmentService
    {
        #region Define services

        private readonly IRepository<SOPAssignment> _sOPAssignmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IApplicationUserRoleService _applicationUserRoleService;

        public SOPAssignmentService(
            IRepository<SOPAssignment> sOPAssignmentRepository,
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext db,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _sOPAssignmentRepository = sOPAssignmentRepository;
            _userManager = userManager;
            _db = db;
            _applicationUserRoleService = applicationUserRoleService;
        }
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region List

        public IPagedList<SOPAssignment> GetAllSOPAssignments(
            string searchText, 
            string siteId,
            string LoggedUserId,
            string loggeduserEmployeeId, 
            List<string> templateIds,
            List<string> AssignedToEmployeeIds, 
            List<string> ApproverEmployeeIds, 
            List<string> StatusIds, 
            List<string> PriorityIds, 
            string name, 
            bool IsApproved, 
            string sortBy, 
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue)
        {
            var query = _sOPAssignmentRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);

            var userdata = _userManager.FindByIdAsync(LoggedUserId).GetAwaiter().GetResult();
            var user = _userManager.FindByNameAsync(userdata.UserName).GetAwaiter().GetResult();
            if (user != null && !user.Deleted && user.Active)
            {
                //Get user roles
                var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();

                // Fetch the NormalizedName of each role
                //var normalizedRoles = _db.Roles.Where(r => roles.Contains(r.Name)).Select(r => r.NormalizedName).ToArray();
                var normalizedRoles = _applicationUserRoleService
                    .GetNormalizedRoleNamesByUserAndSite(user.Id, siteId)
                    .GetAwaiter()
                    .GetResult()
                    .ToArray();

                if (!normalizedRoles.Any(r => r == "admin"))
                {
                    query = query.Where(x =>
                        x.AssignedToEmployeeId == loggeduserEmployeeId ||
                        (x.ApproverEmployeeId != null && x.ApproverEmployeeId == loggeduserEmployeeId)
                    );
                }
            }

            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(m => m.Name.ToLower().Contains(searchText.ToLower()) || m.Description.ToLower().Contains(searchText.ToLower()));

            if (templateIds?.Any() == true) query = query.Where(x => templateIds.Contains(x.TemplateId));

            if (AssignedToEmployeeIds?.Any() == true) query = query.Where(x => AssignedToEmployeeIds.Contains(x.AssignedToEmployeeId));

            if (ApproverEmployeeIds?.Any() == true) query = query.Where(x => ApproverEmployeeIds.Contains(x.ApproverEmployeeId));

            if (StatusIds?.Any() == true) query = query.Where(x => StatusIds.Contains(x.StatusId));

            if (PriorityIds?.Any() == true) query = query.Where(x => PriorityIds.Contains(x.PriorityId));

            if (!string.IsNullOrEmpty(name))
                query = query.Where(x => x.Name.ToLower().Contains(name));

            if (IsApproved)
                query = query.Where(x => x.IsApproved == IsApproved);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            query = query.Select(x => new SOPAssignment
            {
                Id = x.Id,
                SiteId = x.SiteId,
                TemplateId = x.TemplateId,
                Template = new SOPTemplate
                {
                    Id = x.Template.Id,
                    Name = x.Template.Name,
                    Version = x.Template.Version,
                },
                AssignedToEmployeeId = x.AssignedToEmployeeId,
                AssignedToEmployee = new Employee
                {
                    Id = x.AssignedToEmployee.Id,
                    Person = new Person
                    {
                        Id = x.AssignedToEmployee.Person.Id,
                        FullName = x.AssignedToEmployee.Person.FirstName + " " + x.AssignedToEmployee.Person.LastName
                    }
                },
                ApproverEmployeeId = x.ApproverEmployeeId,
                ApproverEmployee = new Employee
                {
                    Id = x.ApproverEmployee.Id,
                    Person = new Person
                    {
                        Id = x.ApproverEmployee.Person.Id,
                        FullName = x.ApproverEmployee.Person.FirstName + " " + x.ApproverEmployee.Person.LastName
                    }
                },
                StatusId = x.StatusId,
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownText = x.Status.DropDownText,
                    DropDownValue = x.Status.DropDownValue,
                    BgColor = x.Status.BgColor,
                    Color = x.Status.Color,
                    Description = x.Status.Description,
                },
                PriorityId = x.PriorityId,
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownText = x.Priority.DropDownText,
                    DropDownValue = x.Priority.DropDownValue,
                    BgColor = x.Priority.BgColor,
                    Color = x.Priority.Color,
                    Description = x.Priority.Description,
                },
                Name = x.Name,
                Description = x.Description,
                AssignedDate = x.AssignedDate,
                DueDate = x.DueDate,
                IsApproved = x.IsApproved,
                ApprovedDate = x.ApprovedDate,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                    }
                },
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                UpdatedBy = new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName
                    }
                },
                TotalCount = x.SOPAssignmentResponses.Count(r => !r.Deleted),
                ApprovedCount = x.SOPAssignmentResponses.Count(r => !r.Deleted && r.IsApproved)
            });

            var list = new PagedList<SOPAssignment>(query, page, pageSize);
            return list;
        }

        #endregion

        #region Get By Id

        public SOPAssignment GetSOPAssignmentById(string siteId, string Id)
        {
            var query = _sOPAssignmentRepository.TableNoTracking.FirstOrDefault(x => !x.Deleted && x.SiteId == siteId && x.Id == Id);
            return query ?? null;
        }

        public SOPAssignment GetSOPAssignmentByIdInDetail(string siteId, string Id)
        {
            var query = _sOPAssignmentRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.Id == Id);

            query = query.Select(x => new SOPAssignment
            {
                Id = x.Id,
                SiteId = x.SiteId,
                TemplateId = x.TemplateId,
                Template = new SOPTemplate
                {
                    Id = x.Template.Id,
                    Name = x.Template.Name,
                    Description = x.Template.Description,
                    Version = x.Template.Version,
                    SOPTemplateSections = x.Template.SOPTemplateSections.Where(x => !x.Deleted).OrderBy(o => o.SortOrder).Select(y => new SOPTemplateSection
                    {
                        Id = y.Id,
                        Name = y.Name,
                        Description = y.Description,
                        SOPTemplateSectionItems = y.SOPTemplateSectionItems.Where(x => !x.Deleted).OrderBy(o => o.SortOrder).Select(z => new SOPTemplateSectionItems
                        {
                            Id = z.Id,
                            Name = z.Name,
                            Description = z.Description,
                            InputTypeId = z.InputTypeId,
                            InputType = new DropDown
                            {
                                Id = z.InputTypeId,
                                DropDownText = z.InputType.DropDownText,
                                DropDownValue = z.InputType.DropDownValue,
                            },
                            IsMandatory = z.IsMandatory,
                            IsRequiredEvidence = z.IsRequiredEvidence,
                            ValidationJson = z.ValidationJson,
                            SortOrder = z.SortOrder,
                            UpdatedDateStr = y.UpdatedOnUtc.ToString("MM/dd/yyyy hh:mm tt"),
                            UpdatedBy = new ApplicationUser
                            {
                                Id = z.UpdatedBy.Id,
                                Person = new Person
                                {
                                    Id = z.UpdatedBy.PersonId,
                                    FullName = z.UpdatedBy.Person.FirstName + " " + z.UpdatedBy.Person.LastName
                                }
                            },
                        }).ToList(),
                    }).ToList()
                },
                AssignedToEmployeeId = x.AssignedToEmployeeId,
                AssignedToEmployee = new Employee
                {
                    Id = x.AssignedToEmployee.Id,
                    Person = new Person
                    {
                        Id = x.AssignedToEmployee.Person.Id,
                        FullName = x.AssignedToEmployee.Person.FirstName + " " + x.AssignedToEmployee.Person.LastName
                    }
                },
                ApproverEmployeeId = x.ApproverEmployeeId,
                ApproverEmployee = new Employee
                {
                    Id = x.ApproverEmployee.Id,
                    Person = new Person
                    {
                        Id = x.ApproverEmployee.Person.Id,
                        FullName = x.ApproverEmployee.Person.FirstName + " " + x.ApproverEmployee.Person.LastName
                    }
                },
                StatusId = x.StatusId,
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownText = x.Status.DropDownText,
                    DropDownValue = x.Status.DropDownValue,
                    BgColor = x.Status.BgColor,
                    Color = x.Status.Color,
                    Description = x.Status.Description,
                },
                PriorityId = x.PriorityId,
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownText = x.Priority.DropDownText,
                    DropDownValue = x.Priority.DropDownValue,
                    BgColor = x.Priority.BgColor,
                    Color = x.Priority.Color,
                    Description = x.Priority.Description,
                },
                Name = x.Name,
                Description = x.Description,
                AssignedDate = x.AssignedDate,
                DueDate = x.DueDate,
                IsApproved = x.IsApproved,
                ApprovedDate = x.ApprovedDate,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                    }
                },
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                UpdatedBy = new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName
                    }
                },
                SOPAssignmentResponses = x.SOPAssignmentResponses.Where(x => !x.Deleted).Select(y => new SOPAssignmentResponse
                {
                    Id = y.Id,
                    AssignementId = y.AssignementId,
                    SectionItemId = y.SectionItemId,
                    IsApproved = y.IsApproved,
                    ApprovedComment = y.ApprovedComment,
                    SectionItem = new SOPTemplateSectionItems
                    {
                        Id  = y.SectionItem.Id,
                        Name  = y.SectionItem.Name,
                        Description  = y.SectionItem.Description,
                        InputTypeId = y.SectionItem.InputTypeId,
                        InputType = new DropDown
                        {
                            Id = y.SectionItem.InputTypeId,
                            DropDownText = y.SectionItem.InputType.DropDownText,
                            DropDownValue = y.SectionItem.InputType.DropDownValue,
                        },
                        IsMandatory = y.SectionItem.IsMandatory,
                        IsRequiredEvidence = y.SectionItem.IsRequiredEvidence,
                        ValidationJson = y.SectionItem.ValidationJson,
                    },
                    IsChecked = y.IsChecked,
                    Response = y.Response,
                    CreatedById = y.CreatedById,
                    CreatedOnUtc = y.CreatedOnUtc,
                    CreatedBy = new ApplicationUser
                    {
                        Id = y.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = y.CreatedBy.PersonId,
                            FullName = y.CreatedBy.Person.FirstName + " " + y.CreatedBy.Person.LastName
                        }
                    },
                    UpdatedById = y.UpdatedById,
                    UpdatedDateStr = y.UpdatedOnUtc.ToString("MM/dd/yyyy hh:mm tt"),
                    UpdatedBy = new ApplicationUser
                    {
                        Id = y.UpdatedBy.Id,
                        Person = new Person
                        {
                            Id = y.UpdatedBy.PersonId,
                            FullName = y.UpdatedBy.Person.FirstName + " " + y.UpdatedBy.Person.LastName
                        }
                    },
                    SOPAssignmentResponseEvidences = y.SOPAssignmentResponseEvidences.Where(x => !x.Deleted).Select(z => new SOPAssignmentResponseEvidences
                    {
                        Id = z.Id,
                        ResponseId = z.ResponseId,
                        FileId = z.FileId,
                        File = new Picture
                        {
                            Id = z.File.Id,
                            SeoFilename = z.File.SeoFilename,
                            VirtualPath = z.File.VirtualPath,
                        },
                        CreatedById = z.CreatedById,
                        CreatedOnUtc = z.CreatedOnUtc,
                        CreatedBy = new ApplicationUser
                        {
                            Id = z.CreatedBy.Id,
                            Person = new Person
                            {
                                Id = z.CreatedBy.PersonId,
                                FullName = z.CreatedBy.Person.FirstName + " " + z.CreatedBy.Person.LastName
                            }
                        },
                    }).ToList()
                }).ToList(),
            });

            return query.FirstOrDefault();
        }

        #endregion

        #region GetSOPAssignmentByName
        // Title: GetSOPAssignmentByName
        // Description: This method retrieves a SOPAssignment based on its name. It allows an optional exclusion of a SOPAssignment by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific SOPAssignment. The method returns the first matching SOPAssignment or null if no match is found.
        public async Task<SOPAssignment> GetSOPAssignmentByName(string SiteId, string name, string id = null)
        {
            var query = _sOPAssignmentRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Name.ToLower() == name.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region Insert Update Delete
        public void InsertSOPAssignment(SOPAssignment entity)
        {
            _sOPAssignmentRepository.Insert(entity);
        }

        public void UpdateSOPAssignment(SOPAssignment entity)
        {
            _sOPAssignmentRepository.Update(entity);
        }

        public void DeleteSOPAssignment(SOPAssignment entity)
        {
            entity.Deleted = true;
            _sOPAssignmentRepository.Update(entity);
        }
        #endregion
    }
}
