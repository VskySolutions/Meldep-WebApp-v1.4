using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.ExpenseVendorBankAccount
{
    public class ExpenseVendorBankAccountsService : IExpenseVendorBankAccountsService
    {
        #region Define Services
        private readonly IRepository<ExpenseVendors> _expenseVendorsRepository;
        private readonly IRepository<ExpenseVendorBankAccounts> _expenseVendorBankAccountsRepository;
        #endregion

        #region Services Initializations
        public ExpenseVendorBankAccountsService(IRepository<ExpenseVendors> expenseVendorsRepository, IRepository<ExpenseVendorBankAccounts> expenseVendorBankAccountsRepository)
        {
            _expenseVendorsRepository = expenseVendorsRepository;
            _expenseVendorBankAccountsRepository = expenseVendorBankAccountsRepository;
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

        #region GetAllExpenseVendorBankAccounts
        // Title: GetAllExpenseVendorBankAccounts
        // Description: This method retrieves a paginated list of ExpenseVendors based on various search criteria such as title
        public IPagedList<ExpenseVendorBankAccounts> GetAllExpenseVendorBankAccounts(string siteId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {

            var query = _expenseVendorBankAccountsRepository.TableNoTracking.Where(x => !x.Deleted && x.Vendor.SiteId == siteId);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.UpdatedOnUtc);
            }

            query = query.Select(x => new ExpenseVendorBankAccounts
            {
                Id = x.Id,
                AccountNumber = x.AccountNumber,
                BankName = x.BankName,
                IFSCCode = x.IFSCCode,
                BranchName = x.BranchName,
                UPI_ID = x.UPI_ID,
                IsBankAccount = x.IsBankAccount,
                IsActive = x.IsActive,
                Vendor = new ExpenseVendors
                {
                    Id = x.Vendor.Id,
                    VendorName = x.Vendor.VendorName,
                    Vendor_Email = x.Vendor.Vendor_Email,
                    Person = new Person
                    {
                        Id = x.Vendor.Person.Id,
                        FullName = x.Vendor.Person.FirstName + " " + x.Vendor.Person.LastName,
                    }
                },
            });

            var list = new PagedList<ExpenseVendorBankAccounts>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetAllVendorsAccountListForDropdown
        public async Task<List<ExpenseVendorBankAccounts>> GetAllVendorsAccountListForDropdown(string SiteId, string vendorId = null)
        {
            var query = _expenseVendorBankAccountsRepository.TableNoTracking.Where(m => !m.Deleted && m.Vendor.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(vendorId))
            {
                query = query.Where(x => x.VendorId == vendorId && !x.Vendor.Deleted);
            }

            query = query.OrderBy(x => x.PaymentType.DropDownValue).Select(x => new ExpenseVendorBankAccounts
            {
                Id = x.Id,
                AccountNumber = x.AccountNumber,
                BankName = x.BankName,
                UPI_ID = x.UPI_ID,
                PaymentType = new DropDown
                {
                    Id = x.PaymentType.Id,
                    DropDownValue = x.PaymentType.DropDownValue
                }
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetExpenseVendorsBankAccountById
        // Title: GetExpenseVendorsBankAccountById
        // Description: This method retrieves a ExpenseVendors from the database by its unique identifier (`id`). 
        public async Task<ExpenseVendorBankAccounts> GetExpenseVendorsBankAccountById(string id)
        {
            var query = _expenseVendorBankAccountsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetExpenseVendorsBankAccountDetailsById
        // Title: GetExpenseVendorsDetailsById
        // Description: The method selects relevant fields from the ExpenseVendors entity, including related entities such as nd employee mappings, and returns a `ExpenseVendors` object with these details. 
        public async Task<ExpenseVendorBankAccounts> GetExpenseVendorsBankAccountDetailsById(string id)
        {
            var query = _expenseVendorBankAccountsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new ExpenseVendorBankAccounts
            {
                Id = x.Id,
                AccountNumber = x.AccountNumber,
                BankName = x.BankName,
                IFSCCode = x.IFSCCode,
                BranchName = x.BranchName,
                UPI_ID = x.UPI_ID,
                IsBankAccount = x.IsBankAccount,
                IsActive = x.IsActive,
                Vendor = new ExpenseVendors
                {
                    Id = x.Vendor.Id,
                    VendorName = x.Vendor.VendorName,
                    Vendor_Email = x.Vendor.Vendor_Email,
                    Person = new Person
                    {
                        Id = x.Vendor.Person.Id,
                        FullName = x.Vendor.Person.FirstName + " " + x.Vendor.Person.LastName,
                    }
                },
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertExpenseVendorBankAccounts
        // Title: InsertExpenseVendorBankAccounts
        // Description: This method inserts a new ExpenseVendorBankAccounts entity into the repository. It takes a ExpenseVendorBankAccounts object as input and uses the _expenseVendorBankAccountsRepository to handle the insertion operation.
        public void InsertExpenseVendorBankAccounts(ExpenseVendorBankAccounts entity)
        {
            _expenseVendorBankAccountsRepository.Insert(entity);
        }
        #endregion

        #region UpdateExpenseVendorBankAccounts
        // Title: UpdateExpenseVendorBankAccounts
        // Description: This method updates the specified ExpenseVendorBankAccounts entity in the repository. It takes a ExpenseVendorBankAccounts object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateExpenseVendorBankAccounts(ExpenseVendorBankAccounts entity)
        {
            _expenseVendorBankAccountsRepository.Update(entity);
        }
        #endregion

        #region DeleteExpenseVendorBankAccounts
        // Title: DeleteExpenseVendorBankAccounts
        // Description: Marks the specified ExpenseVendorBankAccounts entity as deleted by setting its `Deleted` property to true. 
        public void DeleteExpenseVendorBankAccounts(ExpenseVendorBankAccounts entity)
        {
            entity.Deleted = true;
            _expenseVendorBankAccountsRepository.Update(entity);
        }
        #endregion
    }
}

