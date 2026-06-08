using System;
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

namespace Vsky.Services.SOPProcesses
{
    public class SOPProcessService : ISOPProcessService
    {
        #region Define services
        private readonly IRepository<SOPProcess> _sOPProcessRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        public SOPProcessService(IRepository<SOPProcess> sOPProcessRepository,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _sOPProcessRepository = sOPProcessRepository;
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

        public IPagedList<Vsky.Models.SOPProcess> GetAllSOPProcesses(string searchText, string siteId, string logginuser, string title, bool isActive, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue)
        {
            var query = _sOPProcessRepository.TableNoTracking.Where(x => !x.Deleted && x.IsActive == isActive && x.SiteId == siteId);

            var userdata = _userManager.FindByIdAsync(logginuser).GetAwaiter().GetResult();
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

                // If user is NOT SOP Approver or SOP Editor
                if (!normalizedRoles.Contains("sop approver") && !normalizedRoles.Contains("sop editor"))
                {
                    query = query.Where(x =>
                        x.SOPProcessStatusLog
                            .OrderByDescending(p => p.CreatedOnUtc)
                            .Select(p => p.Status.DropDownValue.ToLower())
                            .FirstOrDefault() == "published"
                    );
                }
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim().ToLower();

                // Remove slash
                var normalizedSearch = searchText.Replace("/", "");

                // Full date parse
                var isFullDate = DateTime.TryParse(searchText, out var parsedDate);

                // Numeric partial values
                var isNumber = int.TryParse(normalizedSearch, out var searchNumber);

                query = query.Where(m =>
                    (m.Title ?? "").ToLower().Contains(searchText) ||
                    (m.Purpose ?? "").ToLower().Contains(searchText) ||
                    (m.Version ?? "").ToLower().Contains(searchText) ||
                    (m.Category != null && (m.Category.Type ?? "").ToLower().Contains(searchText)) ||
                    (m.SubCategory != null && (m.SubCategory.DropDownValue ?? "").ToLower().Contains(searchText)) ||
                    (m.ShortDescription ?? "").ToLower().Contains(searchText) ||

                    // First Name
                    (m.UpdatedBy != null &&
                     m.UpdatedBy.Person != null &&
                     (m.UpdatedBy.Person.FirstName ?? "").ToLower().Contains(searchText)) ||

                    // Last Name
                    (m.UpdatedBy != null &&
                     m.UpdatedBy.Person != null &&
                     (m.UpdatedBy.Person.LastName ?? "").ToLower().Contains(searchText)) ||

                    // Full Name
                    (m.UpdatedBy != null &&
                     m.UpdatedBy.Person != null &&
                     (
                        ((m.UpdatedBy.Person.FirstName ?? "") + " " + (m.UpdatedBy.Person.LastName ?? ""))
                            .ToLower()
                            .Contains(searchText)
                     )) ||

                    (m.Description ?? "").ToLower().Contains(searchText) ||
                   (
                        m.SOPProcessStatusLog != null &&
                        (
                            // Current actual status
                            m.SOPProcessStatusLog
                                .OrderByDescending(p => p.CreatedOnUtc)
                                .Select(p => p.Status != null ? p.Status.DropDownValue : "")
                                .FirstOrDefault()
                                .ToLower()
                                .Contains(searchText) ||

                            // Submitted status also searchable by Waiting for Approval
                            (
                                m.SOPProcessStatusLog
                                    .OrderByDescending(p => p.CreatedOnUtc)
                                    .Select(p => p.Status != null ? p.Status.DropDownValue : "")
                                    .FirstOrDefault()
                                    .ToLower() == "submitted"
                                &&
                                "waiting for approval".Contains(searchText)
                            ) ||

                            // Approved status also searchable by Waiting for Published
                            (
                                m.SOPProcessStatusLog
                                    .OrderByDescending(p => p.CreatedOnUtc)
                                    .Select(p => p.Status != null ? p.Status.DropDownValue : "")
                                    .FirstOrDefault()
                                    .ToLower() == "approved"
                                &&
                                "waiting for published".Contains(searchText)
                            )
                        )
                    ) ||

                    (isFullDate && m.UpdatedOnUtc.Date == parsedDate.Date) ||

                    // Partial Date Search
                    (searchText.Contains("/") &&
                        (
                            (m.UpdatedOnUtc.Day.ToString().StartsWith(normalizedSearch)) ||
                            (m.UpdatedOnUtc.Month.ToString().StartsWith(normalizedSearch))
                        )
                    ) ||

                    // Numeric only
                    (isNumber &&
                        (
                            m.UpdatedOnUtc.Day == searchNumber ||
                            m.UpdatedOnUtc.Month == searchNumber ||
                            m.UpdatedOnUtc.Year == searchNumber
                        )
                    )
                );
            }

            if (!string.IsNullOrEmpty(title))
                query = query.Where(x => x.Title.ToLower().Contains(title));

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "statusId")
                {
                    query = descending
                        ? query.OrderByDescending(x =>
                            x.SOPProcessStatusLog
                             .OrderByDescending(p => p.CreatedOnUtc)
                             .Select(p => p.Status.DropDownValue)
                             .FirstOrDefault())
                        : query.OrderBy(x =>
                            x.SOPProcessStatusLog
                             .OrderByDescending(p => p.CreatedOnUtc)
                             .Select(p => p.Status.DropDownValue)
                             .FirstOrDefault());
                }
                else if (sortBy == "updatedByName")
                {
                    query = descending
                        ? query
                            .OrderByDescending(x => x.UpdatedBy.Person.FirstName)
                            .ThenByDescending(x => x.UpdatedBy.Person.LastName)
                        : query
                            .OrderBy(x => x.UpdatedBy.Person.FirstName)
                            .ThenBy(x => x.UpdatedBy.Person.LastName);
                }
                else
                {
                    var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                    query = query.OrderBy(orderBy);
                }
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            query = query.Select(x => new SOPProcess
            {
                Id = x.Id,
                Title = x.Title,
                Version = x.Version,
                ShortDescription = x.ShortDescription,
                Purpose = x.Purpose,
                IsActive = x.IsActive,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Category = new DropDownType
                {
                    Id = x.Category.Id,
                    Type = x.Category.Type
                },
                SubCategory = new DropDown
                {
                    Id = x.SubCategory.Id,
                    DropDownValue = x.SubCategory.DropDownValue
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
                StatusText = x.SOPProcessStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.DropDownValue).FirstOrDefault(),
                StatusId = x.SOPProcessStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.Id).FirstOrDefault(),
            });

            var list = new PagedList<Vsky.Models.SOPProcess>(query, page, pageSize);
            return list;
        }

        #endregion

        #region Get By Id
        public SOPProcess GetSOPProcessById(string siteId, string id)
        {
            var query = _sOPProcessRepository.Table
                .Where(x => !x.Deleted && x.SiteId == siteId && x.Id == id)
                .Include(x => x.SOPProcessStatusLog.OrderByDescending(s => s.CreatedOnUtc))
                .FirstOrDefault();

            return query;
        }

        public async Task<Vsky.Models.SOPProcess> GetSOPProcessByIdInDetail(string siteId, string Id)
        {
            var query = _sOPProcessRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.Id == Id);

            query = query.Select(x => new SOPProcess
            {
                Id = x.Id,
                Title = x.Title,
                ShortDescription = x.ShortDescription,
                Description = x.Description,
                Purpose = x.Purpose,
                Version = x.Version,
                IsActive = x.IsActive,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Category = new DropDownType
                {
                    Id = x.Category.Id,
                    Type = x.Category.Type
                },
                SubCategory = new DropDown
                {
                    Id = x.SubCategory.Id,
                    DropDownValue = x.SubCategory.DropDownValue
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
                StatusText = x.SOPProcessStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.DropDownValue).FirstOrDefault(),
                StatusId = x.SOPProcessStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.Id).FirstOrDefault(),
                SOPProcessStatusLog = x.SOPProcessStatusLog.OrderByDescending(m => m.CreatedOnUtc).Select(p => new SOPProcessStatusLog
                {
                    Id = p.Id,
                    CreatedOnUtc = p.CreatedOnUtc,
                    Status = new DropDown
                    {
                        Id = p.Status.Id,
                        DropDownValue = p.Status.DropDownValue
                    },
                    CreatedBy = new ApplicationUser
                    {
                        Id = x.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = x.CreatedBy.PersonId,
                            FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                        }
                    }
                }).ToList(),
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        #endregion

        #region GetSOPProcessByTitle
        // Title: GetSOPProcessByTitle
        // Description: This method retrieves a SOP Process based on its title and Id. It allows an optional exclusion of a SOP Process by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific SOP Process. The method returns the first matching SOP Process or null if no match is found.
        public async Task<SOPProcess> GetSOPProcessByTitle(string SiteId, string title, string id = null)
        {
            var query = _sOPProcessRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Title.ToLower() == title.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion

        #region Insert Update Delete
        public void InsertSOPProcess(SOPProcess entity)
        {
            _sOPProcessRepository.Insert(entity);
        }

        public void UpdateSOPProcess(SOPProcess entity)
        {
            _sOPProcessRepository.Update(entity);
        }

        public void DeleteSOPProcess(SOPProcess entity)
        {
            entity.Deleted = true;
            _sOPProcessRepository.Update(entity);
        }
        #endregion
    }
}