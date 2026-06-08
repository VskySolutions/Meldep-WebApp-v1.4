using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
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
    public class Expense_Purchase_Requests_Service : IExpense_Purchase_Requests_Service
    {
        #region Fields
        private readonly IRepository<Expense> _expensesRepository;
        private readonly IRepository<Expense_Purchase_Requests> _expenses_Purchase_Request_Repository;
        private readonly IRepository<DropDown> _dropdownRepository;
        private readonly IRepository<Expense_BankAccounts> _expenseBankAccountsRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Ctor

        public Expense_Purchase_Requests_Service(
            IRepository<Expense> expensesRepository,
            IRepository<Expense_Purchase_Requests> expenses_Purchase_Request_Repository,
            ApplicationDbContext dbContext,
            IRepository<Expense_BankAccounts> expenseBankAccountsRepository
            , UserManager<ApplicationUser> userManager,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _expensesRepository = expensesRepository;
            _expenses_Purchase_Request_Repository = expenses_Purchase_Request_Repository;
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

        #region GetAllPurchaseExpenseRequests
        public async Task<IPagedList<Expense_Purchase_Requests>> GetAllPurchaseExpenseRequests
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
            var query = _expenses_Purchase_Request_Repository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, SiteId);

            if (!string.IsNullOrEmpty(LoggedUserId) && !IsAdmin && !isApprove)
                query = query.Where(x => x.CreatedById == LoggedUserId);

            if (referenceId != null && referenceId.Any())
                query = query.Where(x => referenceId.Contains(x.Id));

            if (RequestDate != default(DateTime))
            {
                query = query.Where(x => x.RequestDate.Date == RequestDate.Date);
            }

            if (!string.IsNullOrEmpty(statusName))
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
                     m.PurchaseRequestStatus.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                     m.ItemCategory.Type.ToLower().Contains(SearchText.ToLower()) ||
                      (m.RequestedEmployee.Person.FirstName + " " + m.RequestedEmployee.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                     m.ItemName.ToLower().Contains(SearchText.ToLower()) ||
                     m.Quantity.Equals(SearchText.ToLower()) ||
                     m.EstimatedRate.Equals(SearchText.ToLower()) ||
                     m.Discount.Equals(SearchText.ToLower()) ||
                     m.EstimatedAmount.Equals(SearchText.ToLower()) ||
                     m.RequestDate.Date == parsedDate.Date
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
                query = query.Select(x => new Expense_Purchase_Requests
                {
                    Id = x.Id,
                    RequestDate = x.RequestDate,
                });
            }
            else
            {
                // projection
                query = query.Select(x => new Expense_Purchase_Requests
                {
                    Id = x.Id,
                    RequestDate = x.RequestDate,
                    RequestedById = x.RequestedById,
                    ItemName = x.ItemName,
                    ReferenceId = x.ReferenceId,
                    StatusId = x.StatusId,
                    ItemCategoryId = x.ItemCategoryId,
                    Quantity = x.Quantity,
                    EstimatedRate = x.EstimatedRate,
                    Discount = x.Discount,
                    EstimatedAmount = x.EstimatedAmount,
                    IsEdited = x.IsEdited,
                    PurchaseRequestStatus = new DropDown
                    {
                        Id = x.PurchaseRequestStatus.Id,
                        DropDownValue = x.PurchaseRequestStatus.DropDownValue,
                        BgColor = x.PurchaseRequestStatus.BgColor,
                        Color = x.PurchaseRequestStatus.Color,
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
                    RequestedEmployee = new Employee
                    {
                        Id = x.RequestedEmployee.Id,
                        Person = new Person
                        {
                            FullName = x.RequestedEmployee.Person.FirstName + " " + x.RequestedEmployee.Person.LastName
                        }
                    },
                });
            }

            var list = new PagedList<Expense_Purchase_Requests>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetById
        public async Task<Expense_Purchase_Requests> GetById(string id)
        {
            var query = _expenses_Purchase_Request_Repository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion

        #region GetPurchaseExpenseDetailsById
        public async Task<Expense_Purchase_Requests> GetPurchaseExpenseDetailsById(string id)
        {
            var query = _expenses_Purchase_Request_Repository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            // projection
            query = query.Select(x => new Expense_Purchase_Requests
            {
                Id = x.Id,
                RequestDate = x.RequestDate,
                RequestedById = x.RequestedById,
                PurchaserId = x.PurchaserId,
                ItemName = x.ItemName,
                ReferenceId = x.ReferenceId,
                StatusId = x.StatusId,
                ItemCategoryId = x.ItemCategoryId,
                ItemSubCategoryId = x.ItemSubCategoryId,
                VendorId = x.VendorId,
                Quantity = x.Quantity,
                EstimatedRate = x.EstimatedRate,
                Discount = x.Discount,
                EstimatedAmount = x.EstimatedAmount,
                Description = x.Description,
                PostApproverNote = x.PostApproverNote,  
                PreApproverNote = x.PreApproverNote,
                PaidByNote = x.PaidByNote,
                IsEdited = x.IsEdited,
                CreatedOnUtc = x.CreatedOnUtc,
                CreatedById = x.CreatedById,
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                PurchaseRequestStatus = new DropDown
                {
                    Id = x.PurchaseRequestStatus.Id,
                    DropDownValue = x.PurchaseRequestStatus.DropDownValue,
                    BgColor = x.PurchaseRequestStatus.BgColor,
                    Color = x.PurchaseRequestStatus.Color,
                },
                ExpenseVendors = new ExpenseVendors
                {
                    Id = x.ExpenseVendors.Id,
                    VendorName = x.ExpenseVendors.VendorName
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
                RequestedEmployee = new Employee
                {
                    Id = x.RequestedEmployee.Id,
                    Person = new Person
                    {
                        FullName = x.RequestedEmployee.Person.FirstName + " " + x.RequestedEmployee.Person.LastName
                    }
                },
                PurchaserEmployee = new Employee
                {
                    Id = x.PurchaserEmployee.Id,
                    Person = new Person
                    {
                        FullName = x.PurchaserEmployee.Person.FirstName + " " + x.PurchaserEmployee.Person.LastName
                    }
                },
                ExpensePurchaseRequestFileList = x.ExpensePurchaseRequestFileList.Where(x => !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(mapping => new ExpensePurchaseRequestFiles
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

        #region GetExpensePurchaseListForDropdown
        public async Task<List<SelectListItem>> GetExpensePurchaseListForDropdown(string siteId, string LoggedUserId, string statusId = null)
        {
            var query = _expenses_Purchase_Request_Repository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);
            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, siteId);

            if (!IsAdmin && !string.IsNullOrEmpty(LoggedUserId))
                query = query.Where(x => x.CreatedById == LoggedUserId);

            if (!string.IsNullOrEmpty(statusId))
                query = query.Where(x => x.StatusId == statusId);

            return await query.OrderBy(m => m.CreatedOnUtc).Select(x => new SelectListItem { Value = x.Id, Text = x.ReferenceId }).ToListAsync();
        }
        #endregion

        #region GetReferenceId
        public async Task<string> GetReferenceId(string siteId)
        {
            var prefix = "PUR";
            var year = DateTime.UtcNow.Year;
            var query = await _expenses_Purchase_Request_Repository.TableNoTracking.Where(x => x.SiteId == siteId && x.ReferenceId.StartsWith($"{prefix}-{year}-"))
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

        #region InsertExpensePurchaseRequests
        public void InsertExpensePurchaseRequests(Expense_Purchase_Requests entity)
        {
            _expenses_Purchase_Request_Repository.Insert(entity);
        }
        #endregion

        #region UpdateExpensePurchaseRequests
        public void UpdateExpensePurchaseRequests(Expense_Purchase_Requests entity)

        {
            _expenses_Purchase_Request_Repository.Update(entity);
        }
        #endregion

        #region DeleteExpensePurchaseRequests
        public void DeleteExpensePurchaseRequests(Expense_Purchase_Requests entity)
        {
            entity.Deleted = true;
            _expenses_Purchase_Request_Repository.Update(entity);
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
