using System;
using System.Collections.Generic;
using System.Linq;
//using Vsky.Services.Finance;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Models.Expens;
using Vsky.Services.ApplicationUserRoles;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Services.Expences
{

    public class ExpenseService : IExpenseService
    {
        #region Fields
        private readonly IRepository<Expense> _expensesRepository;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<Expense_Lines> _expenseLinesServiceRepository;
        private readonly IRepository<DropDown> _dropdownRepository;
        private readonly IRepository<Expense_BankAccounts> _expenseBankAccountsRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Ctor

        public ExpenseService(IRepository<Expense> expensesRepository, 
            ApplicationDbContext dbContext, 
            IRepository<Picture> pictureRepository, 
            IRepository<DropDown> dropdownRepository, 
            IRepository<Expense_BankAccounts> expenseBankAccountsRepository, 
            UserManager<ApplicationUser> userManager,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _expensesRepository = expensesRepository;
            _dbContext = dbContext;
            _pictureRepository = pictureRepository;
            _dropdownRepository = dropdownRepository;
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

        #region Public Methods
        public async Task<IPagedList<Expense>> GetAllExpenses(
            string SiteId, 
            bool isApprove, 
            string LoggedUserId, 
            string SearchText, 
            string expenseNumber, 
            List<string> bankAccountIds,
            List<string> payeeIds, 
            DateTime ExpenseDate, 
            string StatusId,
            string createdBy,
            string sortBy, 
            bool descending,
            int page = 1, 
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _expensesRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
            bool IsAdmin = await IsCurrentUserAdmin(LoggedUserId, SiteId);

            if (!string.IsNullOrEmpty(LoggedUserId) && !IsAdmin && !isApprove)
                query = query.Where(x => x.CreatedById == LoggedUserId);

            if (bankAccountIds != null && bankAccountIds.Any())
                query = query.Where(x => bankAccountIds.Contains(x.BackAccountId));
            if (payeeIds != null && payeeIds.Any())
                query = query.Where(x => payeeIds.Contains(x.PayeeId));

            if (ExpenseDate != default(DateTime))
            {
                query = query.Where(x => x.ExpenseDate.Date == ExpenseDate.Date);
            }
            //if (ExpenseDate != null)
            //    query = query.Where(x => x.ExpenseDate == ExpenseDate);
            
            if (!string.IsNullOrEmpty(expenseNumber))
                query = query.Where(x => x.ExpenseNumber.Contains(expenseNumber));

            if (!string.IsNullOrEmpty(StatusId))
                query = query.Where(x => x.StatusId != StatusId);

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(x => x.CreatedById == createdBy);

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

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                     m.ExpenseNumber.ToLower().Contains(SearchText.ToLower()) ||
                     m.ExpenseVendors.VendorName.ToLower().Contains(SearchText.ToLower())  ||
                     m.ExpenseVendorBankAccounts.AccountNumber.ToLower().Contains(SearchText.ToLower()) ||
                     m.ExpenseStatus.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                     (m.CreatedBy.Person.FirstName + " " + m.CreatedBy.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                     m.ExpenseDate.Date == parsedDate.Date
                );
            }

            if (lookup)
            {
                query = query.Select(x => new Expense
                {
                    Id = x.Id,
                    ExpenseDate = x.ExpenseDate,
                });
            }
            else
            {
                // projection
                query = query.Select(x => new Expense
                {
                    Id = x.Id,
                    ExpenseDate = x.ExpenseDate,
                    ExpenseNumber = x.ExpenseNumber,
                    CreatedById = x.CreatedById,
                    IsEdited = x.IsEdited,
                    ExpenseVendors = new ExpenseVendors
                    {
                        Id = x.ExpenseVendors.Id,
                        VendorName = x.ExpenseVendors.VendorName,
                    },
                    ExpenseVendorBankAccounts = new ExpenseVendorBankAccounts
                    {
                       
                        Id = x.ExpenseVendorBankAccounts.Id,
                        AccountNumber = x.ExpenseVendorBankAccounts.AccountNumber,
                        UPI_ID = x.ExpenseVendorBankAccounts.UPI_ID,
                        PaymentType = new DropDown
                        {
                            Id = x.Id,
                            DropDownValue = x.ExpenseVendorBankAccounts.PaymentType.DropDownValue
                        }
                    },
                    ExpenseStatus = new DropDown
                    {
                        Id = x.ExpenseStatus.Id,
                        DropDownValue = x.ExpenseStatus.DropDownValue,
                        BgColor = x.ExpenseStatus.BgColor,
                        Color = x.ExpenseStatus.Color,
                    },
                    CreatedBy = new ApplicationUser
                    {
                        Id = x.CreatedBy.Id,
                        Person = new Person
                        {
                            FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                        }
                    },
                    Amount = x.ExpenseLines
                                .Where(m => !m.Deleted && m.ExpenseId == x.Id)
                                .Sum(d => d.Amount)
                });
            }

            var list = new PagedList<Expense>(query, page, pageSize);
            return list;
        }


        public async Task<Expense> GetExpenseById(string id)
        {
            var query = _expensesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }

        public async Task<Expense> GetById(string id)
        {
            var query = _expensesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            query = query.Select(x => new Expense
            {
                Id = x.Id,
                Ref_no = x.Ref_no,
                Memo = x.Memo,
                ExpenseDate = x.ExpenseDate,
                ExpenseNumber = x.ExpenseNumber,
                RecurringStartDate = x.RecurringStartDate,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                RecurringEndDate = x.RecurringEndDate,
                LocationId = x.LocationId,
                BackAccountId = x.BackAccountId,
                RecurringIntervalId = x.RecurringIntervalId,
                PayeeId = x.PayeeId,
                ExpenseVendorBankAccountId = x.ExpenseVendorBankAccountId,
                StatusId = x.StatusId,
                Active = x.Active,
                SetRecurring = x.SetRecurring,
                IsReImbursement = x.IsReImbursement,
                IsEdited  = x.IsEdited,
                CustomerId = x.CustomerId,
                PostApproverNote = x.PostApproverNote,
                PreApproverNote = x.PreApproverNote,
                PaidByNote = x.PaidByNote,
                ExpenseVendors = new ExpenseVendors
                {
                    Id = x.ExpenseVendors.Id,
                    VendorName = x.ExpenseVendors.VendorName,
                },
                ExpenseVendorBankAccounts = new ExpenseVendorBankAccounts
                {
                    Id = x.ExpenseVendorBankAccounts.Id,
                    PaymentType = new DropDown
                    {
                        Id = x.ExpenseVendorBankAccounts.PaymentType.Id,
                        DropDownValue = x.ExpenseVendorBankAccounts.PaymentType.DropDownValue
                    }
                },
                ExpenseStatus = new DropDown
                {
                    Id = x.ExpenseStatus.Id,
                    DropDownValue = x.ExpenseStatus.DropDownValue
                },
                Location = new DropDown
                {
                    Id = x.Location.Id,
                    DropDownValue = x.Location.DropDownValue
                },
                Picture = new Picture
                {
                    Id = x.Picture.Id,
                    SeoFilename = x.Picture.SeoFilename,
                    VirtualPath = x.Picture.VirtualPath
                },
                ExpenseBankAccounts = new Expense_BankAccounts
                {
                    Id = x.ExpenseBankAccounts.Id,
                    AccountNumber = x.ExpenseBankAccounts.AccountNumber
                },
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                    }
                },
                UpdatedBy = new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = new Person
                    {
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName
                    }
                },
                Amount = _dbContext.ExpenceLines.Where(m => !m.Deleted && m.ExpenseId == x.Id).Sum(d => d.Amount),
                ExpenseLines = x.ExpenseLines.Where(p => !p.Deleted).Select(p => new Expense_Lines
                {
                    Id = p.Id,
                    Description = p.Description,
                    Amount = p.Amount,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    Attachment = p.Attachment,
                    ExpenseCategoryId = p.ExpenseCategoryId,
                    ExpenseSubcategoryId = p.ExpenseSubcategoryId,
                    ExpenseId = p.ExpenseId,
                    CreatedById = p.CreatedById,
                    CreatedOnUtc = p.CreatedOnUtc,
                    Category = new DropDownType
                    {
                        Id = p.Category.Id,
                        Type = p.Category.Type,
                    },
                    ExpenseCategorySubcategory = new DropDown
                    {
                        Id = p.ExpenseCategorySubcategory.Id,
                        DropDownValue = p.ExpenseCategorySubcategory.DropDownValue
                    },
                }).ToList(),
                ExpenseFilesList = x.ExpenseFilesList.Where(x => !x.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(mapping => new Expense_Files
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

        public async Task<string> GetStatusIdByName(string SiteId, string StatusId)
        {
            var item = await _dropdownRepository.Table
                .Where(x => !x.Deleted && x.DropDownValue == StatusId && x.DropDownType.SiteId == SiteId)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            return item;
        }

        public async Task<Picture> GetByPictureId(string Attachment)
        {
            var query = _pictureRepository.TableNoTracking.Where(x => x.Id == Attachment).AsNoTracking();

            //query = query.Where(x => x.Id == Attachment).AsNoTracking();

            var item = await query.FirstOrDefaultAsync();

            return item;
        }

        public async Task<List<Expense_BankAccounts>> GetBankAccountNoList(string SiteId)
        {
            var query = _expenseBankAccountsRepository.TableNoTracking.Where(x => !x.Deleted && x.IsActive && x.SiteId == SiteId);
            query = query.Select(x => new Expense_BankAccounts
            {
                Id = x.Id,
                AccountNumber = x.AccountNumber
            });

            var list = await query.ToListAsync();
            return list;
        }

        #region GetExpenseNumber
        // Title: GetExpenseNumber
        // Description: This method retrieves a ExpenseNumber from the database. 
        public async Task<string> GetExpenseNumber(string SiteId)
        {
            string prefix = "VSEX";
            int currentYear = DateTime.UtcNow.Year % 100;

            // Retrieve last Expense Number
            var lastExpenseNumber = await _expensesRepository.Table
                .Where(x => !x.Deleted && x.SiteId == SiteId)
                .OrderByDescending(x => x.ExpenseNumber)
                .FirstOrDefaultAsync();

            int nextSeriesNumber = 1; // Default series number

            if (lastExpenseNumber?.ExpenseNumber != null && lastExpenseNumber.ExpenseNumber.StartsWith(prefix + currentYear))
            {
                // Extract the last 3 digits (series number)
                //string lastSeriesPart = lastExpenseNumber.ExpenseNumber.Substring(5, 3);
                string lastSeriesPart = lastExpenseNumber.ExpenseNumber.Substring(lastExpenseNumber.ExpenseNumber.Length - 3, 3);

                if (int.TryParse(lastSeriesPart, out int lastSeries))
                {
                    nextSeriesNumber = lastSeries + 1; // Increment the series
                }
            }

            string newExpenseNumber = $"{prefix}{currentYear}{nextSeriesNumber:D3}";

            return newExpenseNumber;
        }
        #endregion

        public void InsertExpenses(Expense entity)
        {
            _expensesRepository.Insert(entity);
        }

        public void UpdateExpenses(Expense entity)

        {
            _expensesRepository.Update(entity);
        }

        public void DeleteExpenses(Expense entity)
        {
            entity.Deleted = true;
            _expensesRepository.Update(entity);
        }

        public async Task UpdatePicture(Picture entity)
        {
            _pictureRepository.Update(entity);
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
