using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Models.Expens;
using Vsky.Services.ApplicationUserRoles;

namespace Vsky.Services.Expences
{
    public class Expense_Advance_Requests_Service : IExpense_Advance_Requests_Service
    {
        #region Fields
        private readonly IRepository<Expense> _expensesRepository;
        private readonly IRepository<Expense_Advance_Requests> _expenses_Advance_Request_Repository;
        private readonly IRepository<DropDown> _dropdownRepository;
        private readonly IRepository<Expense_BankAccounts> _expenseBankAccountsRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Ctor

        public Expense_Advance_Requests_Service(
            IRepository<Expense> expensesRepository, 
            IRepository<Expense_Advance_Requests> expenses_Advance_Request_Repository, 
            ApplicationDbContext dbContext,
            IRepository<Expense_BankAccounts> expenseBankAccountsRepository
            , UserManager<ApplicationUser> userManager,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _expensesRepository = expensesRepository;
            _expenses_Advance_Request_Repository = expenses_Advance_Request_Repository;
            _dbContext = dbContext;
            _expenseBankAccountsRepository = expenseBankAccountsRepository;
            _userManager = userManager;
            _applicationUserRoleService = applicationUserRoleService;
        }
        #endregion

        #region Private Methods

        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region GetAllAdvanceExpenseRequests
        public async Task<IPagedList<Expense_Advance_Requests>> GetAllAdvanceExpenseRequests
        (
            string SiteId, 
            bool isApprove,
            string LoggedUserId,
            string SearchText,
            List<string> referenceId,
            DateTime RequestDate, 
            List<string> StatusId,
            string employeeId,
            string sortBy, 
            bool descending, 
            string statusName = "",
            int page = 1, 
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _expenses_Advance_Request_Repository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, SiteId);

            if (!string.IsNullOrEmpty(LoggedUserId) && !IsAdmin && !isApprove)
                query = query.Where(x => x.CreatedById == LoggedUserId);

            if (referenceId != null && referenceId.Any())
                query = query.Where(x => referenceId.Contains(x.Id));

            if (RequestDate != default(DateTime))
            {
                query = query.Where(x => x.RequestDate.Date == RequestDate.Date);
            }

            if(!string.IsNullOrEmpty (statusName))
                query = query.Where(x => x.StatusId != statusName);

            if (StatusId.Any())
                query = query.Where(x => StatusId.Contains(x.StatusId));

            if (!string.IsNullOrWhiteSpace(employeeId))
                query = query.Where(x => x.RequestedEmployee.Id == employeeId);

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                     m.ReferenceId.ToLower().Contains(SearchText.ToLower()) ||
                     m.AdvanceExpenseStatus.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                     m.ItemCategory.Type.ToLower().Contains(SearchText.ToLower()) ||
                     m.PaymentType.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                     (m.RequestedEmployee.Person.FirstName + " " + m.RequestedEmployee.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                     (m.CreatedBy.Person.FirstName + " " + m.CreatedBy.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                     m.RequestDate.Date == parsedDate.Date ||
                     m.Amount.ToString().Contains(SearchText)
                );
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.UpdatedOnUtc);
            }

            if (lookup)
            {
                query = query.Select(x => new Expense_Advance_Requests
                {
                    Id = x.Id,
                    RequestDate = x.RequestDate,
                });
            }
            else
            {
                // projection
                query = query.Select(x => new Expense_Advance_Requests
                {
                    Id = x.Id,
                    RequestDate = x.RequestDate,
                    RequestedBy = x.RequestedBy,
                    PaymentTypeId = x.PaymentTypeId,
                    ReferenceId = x.ReferenceId,
                    StatusId = x.StatusId,
                    Amount = x.Amount,
                    CreatedById = x.CreatedById,
                    ItemCategoryId = x.ItemCategoryId,
                    IsEdited = x.IsEdited,
                    PaymentType = new DropDown
                    {
                        Id = x.PaymentType.Id,
                        DropDownValue = x.PaymentType.DropDownValue,
                        BgColor = x.PaymentType.BgColor,
                        Color = x.PaymentType.Color,
                    },
                    AdvanceExpenseStatus = new DropDown
                    {
                        Id = x.AdvanceExpenseStatus.Id,
                        DropDownValue = x.AdvanceExpenseStatus.DropDownValue,
                        BgColor = x.AdvanceExpenseStatus.BgColor,
                        Color = x.AdvanceExpenseStatus.Color,
                    },
                    CreatedBy = new ApplicationUser
                    {
                        Id = x.CreatedBy.Id,
                        Person = new Person
                        {
                            FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                        }
                    },
                    RequestedEmployee = new Employee
                    {
                        Id = x.RequestedEmployee.Id,
                        Person = new Person
                        {
                            FullName = x.RequestedEmployee.Person.FirstName + " " + x.RequestedEmployee.Person.LastName
                        }
                    },
                    ItemCategory = new DropDownType
                    {
                        Id = x.ItemCategory.Id,
                        Type = x.ItemCategory.Type,
                    }
                });
            }

            var list = new PagedList<Expense_Advance_Requests>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetById
        public async Task<Expense_Advance_Requests> GetById(string id)
        {
            var query = _expenses_Advance_Request_Repository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion

        #region GetAdvanceExpenseDetailsById
        public async Task<Expense_Advance_Requests> GetAdvanceExpenseDetailsById(string id)
        {
            var query = _expenses_Advance_Request_Repository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            query = query.Select(x => new Expense_Advance_Requests
            {
                Id = x.Id,
                RequestDate = x.RequestDate,
                RequestedBy = x.RequestedBy,
                PaymentTypeId = x.PaymentTypeId,
                LocationId = x.LocationId,
                ReferenceId = x.ReferenceId,
                StatusId = x.StatusId,
                Amount = Math.Round(x.Amount, 2),
                ApplyToTrip = x.ApplyToTrip,
                AdvanceDetails = x.AdvanceDetails,
                Notes = x.Notes,
                IsEdited = x.IsEdited,
                CreatedOnUtc = x.CreatedOnUtc,
                CreatedById = x.CreatedById,
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                ItemCategoryId = x.ItemCategoryId,
                ItemSubCategoryId = x.ItemSubCategoryId,
                PostApproverNote = x.PostApproverNote,
                PreApproverNote = x.PreApproverNote,
                PaidByNote = x.PaidByNote,
                PaymentType = new DropDown
                {
                    Id = x.PaymentType.Id,
                    DropDownValue = x.PaymentType.DropDownValue,
                    BgColor = x.PaymentType.BgColor,
                    Color = x.PaymentType.Color,
                },
                AdvanceExpenseStatus = new DropDown
                {
                    Id = x.AdvanceExpenseStatus.Id,
                    DropDownValue = x.AdvanceExpenseStatus.DropDownValue,
                    BgColor = x.AdvanceExpenseStatus.BgColor,
                    Color = x.AdvanceExpenseStatus.Color,
                },
                Location = new DropDown
                {
                    Id = x.Location.Id,
                    DropDownValue = x.Location.DropDownValue
                },
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                    }
                },
                RequestedEmployee = new Employee
                {
                    Id = x.RequestedEmployee.Id,
                    Person = new Person
                    {
                        FullName = x.RequestedEmployee.Person.FirstName + " " + x.RequestedEmployee.Person.LastName
                    }
                },
                ItemCategory = new DropDownType
                {
                    Id = x.ItemCategory.Id,
                    Type = x.ItemCategory.Type,
                },
                ItemSubCategory = new DropDown
                {
                    Id = x.ItemSubCategory.Id,
                    DropDownValue = x.ItemSubCategory.DropDownValue,
                },
                ExpenseAdvanceRequestFileList = x.ExpenseAdvanceRequestFileList.Where(x => !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(mapping => new ExpenseAdvanceRequestFiles
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

        #region GetExpenseAdvanceListForDropdown
        public async Task<List<SelectListItem>> GetExpenseAdvanceListForDropdown(string siteId, string LoggedUserId, string statusId = null)
        {
            var query = _expenses_Advance_Request_Repository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);
            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, siteId);

            if(!IsAdmin && !string.IsNullOrEmpty(LoggedUserId))
                query = query.Where(x => x.CreatedById == LoggedUserId);

            if (!string.IsNullOrEmpty(statusId))
                query = query.Where(x => x.StatusId == statusId);

            return await query.OrderBy(m => m.CreatedOnUtc).Select(x => new SelectListItem { Value = x.Id, Text = x.ReferenceId }).ToListAsync();
        }
        #endregion

        #region GetReferenceId
        public async Task<string> GetReferenceId(string siteId)
        {
            var prefix = "ADV";
            var year = DateTime.UtcNow.Year;
            var query =await _expenses_Advance_Request_Repository.TableNoTracking.Where(x => x.SiteId == siteId && x.ReferenceId.StartsWith($"{prefix}-{year}-"))
                        .OrderByDescending(x => x.ReferenceId)
                        .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (query != null)
            {
                var parts = query.ReferenceId.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}-{year}-{nextNumber:D3}";
        }
        #endregion

        #region InsertExpenseAdvanceRequests
        public void InsertExpenseAdvanceRequests(Expense_Advance_Requests entity)
        {
            _expenses_Advance_Request_Repository.Insert(entity);
        }
        #endregion

        #region UpdateExpenseAdvanceRequests
        public void UpdateExpenseAdvanceRequests(Expense_Advance_Requests entity)

        {
            _expenses_Advance_Request_Repository.Update(entity);
        }
        #endregion

        #region DeleteExpenseAdvanceRequests
        public void DeleteExpenseAdvanceRequests(Expense_Advance_Requests entity)
        {
            entity.Deleted = true;
            _expenses_Advance_Request_Repository.Update(entity);
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
