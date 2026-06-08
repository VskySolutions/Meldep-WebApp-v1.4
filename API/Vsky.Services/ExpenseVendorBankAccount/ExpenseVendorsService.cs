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
    public class ExpenseVendorsService : IExpenseVendorsService
    {
        #region Define Services
        private readonly IRepository<ExpenseVendors> _expenseVendorsRepository;
        private readonly IRepository<ExpenseVendorBankAccounts> _expenseVendorBankAccountsRepository;
        #endregion

        #region Services Initializations
        public ExpenseVendorsService(IRepository<ExpenseVendors> expenseVendorsRepository, IRepository<ExpenseVendorBankAccounts> expenseVendorBankAccountsRepository)
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

        #region GetAllExpenseVendors
        // Title: GetAllExpenseVendors
        // Description: This method retrieves a paginated list of ExpenseVendors based on various search criteria such as title
        public IPagedList<ExpenseVendors> GetAllExpenseVendors(string siteId, string SearchText, string vendorName, List<string> VendorIds, string vendorEmail, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {

            var query = _expenseVendorsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);

            if (!string.IsNullOrWhiteSpace(vendorName))
            {
                vendorName = vendorName.Trim().ToLower();
                query = query.Where(x => x.VendorName.ToLower().Contains(vendorName));
            }

            if (VendorIds != null && VendorIds.Any())
                query = query.Where(x => VendorIds.Contains(x.PersonId));

            if (!string.IsNullOrWhiteSpace(vendorEmail))
            {
                vendorEmail = vendorEmail.Trim().ToLower();
                query = query.Where(x => x.Vendor_Email.ToLower().Contains(vendorEmail));
            }

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
                query = query.Where(m =>
                     m.VendorCode.ToLower().Contains(SearchText.ToLower()) ||
                     m.VendorName.ToLower().Contains(SearchText.ToLower())  ||
                     m.Vendor_Email.ToLower().Contains(SearchText.ToLower()) ||
                     m.Vendor_Phone.Contains(SearchText.ToLower()) ||
                     (m.Person.FirstName + " " + m.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                     m.Address.AddressCountry.Name.ToLower().Contains(SearchText.ToLower())
                );
            }
             query = query.Select(x => new ExpenseVendors
            {
                Id = x.Id,
                VendorName = x.VendorName,
                Vendor_Phone = x.Vendor_Phone,
                Vendor_Email = x.Vendor_Email,
                VendorCode = x.VendorCode,
                PersonId = x.PersonId,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                },
                Address = new Address
                {
                    Id = x.Address.Id,
                    AddressCountry = new Country
                    {
                        Id = x.Address.AddressCountry.Id,
                        Name = x.Address.AddressCountry.Name,
                    },
                },
            });

            var list = new PagedList<ExpenseVendors>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetAllExpenseVendorListForDropdown
        public async Task<List<ExpenseVendors>> GetAllExpenseVendorListForDropdown(string SiteId, bool isOwnerName)
        {
            var query = _expenseVendorsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (isOwnerName)
            {
                query = query
                    .Where(x => x.PersonId != null)
                    .OrderBy(x => x.Person.FirstName)
                    .ThenBy(x => x.Person.LastName);
            }
            else
            {
                query = query
                    .OrderBy(x => x.VendorName);
            }

            var list = await query
                .Select(x => new ExpenseVendors
                {
                    Id = x.Id,
                    Vendor_Email = x.Vendor_Email,
                    VendorName = x.VendorName,
                    Person = x.PersonId != null
                        ? new Person
                        {
                            Id = x.Person.Id,
                            FullName = x.Person.FirstName + " " + x.Person.LastName,
                            FirstName = x.Person.FirstName,
                            LastName = x.Person.LastName
                        }
                        : null
                })
                .ToListAsync();
            if (isOwnerName)
            {
                list = list
                    .GroupBy(x => x.Person.Id)
                    .Select(g => g.First())
                    .ToList();
            }

            return list;
        }
        #endregion

        #region GetExpenseVendorsById
        // Title: GetExpenseVendorsById
        // Description: This method retrieves a ExpenseVendors from the database by its unique identifier (`id`). 
        public async Task<ExpenseVendors> GetExpenseVendorsById(string id)
        {
            var query = _expenseVendorsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetVendorCode
        // Title: GetVendorCode
        // Description: This method retrieves a VendorCode from the database. 
        public async Task<string> GetVendorCode(string SiteId)
        {
            //var query = _expenseVendorsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId).OrderByDescending(x => x.VendorCode);
            //var item = await query.FirstOrDefaultAsync();
            //return item;
            string prefix = "VSV";
            int currentYear = DateTime.UtcNow.Year % 100;

            // Retrieve last vendor code
            var lastVendor = await _expenseVendorsRepository.Table
                .Where(x => !x.Deleted && x.SiteId == SiteId)
                .OrderByDescending(x => x.VendorCode)
                .FirstOrDefaultAsync();

            int nextSeriesNumber = 1; // Default series number

            if (lastVendor?.VendorCode != null && lastVendor.VendorCode.StartsWith(prefix + currentYear))
            {
                // Extract the last 3 digits (series number)
                string lastSeriesPart = lastVendor.VendorCode.Substring(5, 3);
                if (int.TryParse(lastSeriesPart, out int lastSeries))
                {
                    nextSeriesNumber = lastSeries + 1; // Increment the series
                }
            }

            // Format new vendor code (e.g., "VSV25002")
            string newVendorCode = $"{prefix}{currentYear}{nextSeriesNumber:D3}";

            return newVendorCode;
        }
        #endregion

        #region GetExpenseVendorsDetailsById
        // Title: GetExpenseVendorsDetailsById
        // Description: The method selects relevant fields from the ExpenseVendors entity, including related entities such as nd employee mappings, and returns a `ExpenseVendors` object with these details. 
        public async Task<ExpenseVendors> GetExpenseVendorsDetailsById(string id)
        {
            var query = _expenseVendorsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            query = query.Select(x => new ExpenseVendors
            {
                Id = x.Id,
                VendorName = x.VendorName,
                Vendor_Phone = x.Vendor_Phone,
                Vendor_Email = x.Vendor_Email,
                VendorCode = x.VendorCode,
                IsActive = x.IsActive,
                PersonId = x.PersonId,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                },
                Address = new Address
                {
                    Id = x.Address.Id,
                    AddressLine1 = x.Address.AddressLine1,
                    AddressLine2 = x.Address.AddressLine2,
                    StateProvinceId = x.Address.StateProvinceId,
                    CountryId = x.Address.CountryId,
                    City = x.Address.City,
                    ZipCode = x.Address.ZipCode,
                    AddressCountry = new Country
                    {
                        Id = x.Address.AddressCountry.Id,
                        Name = x.Address.AddressCountry.Name,
                    },
                    AddressStateProvince = new StateProvince
                    {
                        Id = x.Address.AddressStateProvince.Id,
                        Name = x.Address.AddressStateProvince.Name,
                    },
                },
                ExpenseVendorBankAccounts = x.ExpenseVendorBankAccounts.Where(p => !p.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(p => new ExpenseVendorBankAccounts
                {
                    Id = p.Id,
                    BankName = p.BankName,
                    AccountNumber = p.AccountNumber,
                    AccountTypeId = p.AccountTypeId,
                    PaymentTypeId = p.PaymentTypeId,
                    IFSCCode = p.IFSCCode,
                    IsActive = p.IsActive,
                    BranchName = p.BranchName,
                    CreatedById = p.CreatedById,
                    IsBankAccount = p.IsBankAccount,
                    UPI_ID = p.UPI_ID,
                    AccountType = new DropDown
                    {
                        Id = p.AccountType.Id,
                        DropDownValue = p.AccountType.DropDownValue
                    },
                    PaymentType = new DropDown
                    {
                        Id = p.PaymentType.Id,
                        DropDownValue = p.PaymentType.DropDownValue
                    }
                }).ToList(),
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetExpenseVendorsByEmail
        // Title: GetExpenseVendorsByEmail
        // Description: This method retrieves a ExpenseVendors based on its email.It allows an optional exclusion of a ExpenseVendors by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific ExpenseVendors. The method returns the first matching ExpenseVendors or null if no match is found.
        public async Task<ExpenseVendors> GetExpenseVendorsByEmail(string siteId, string name, string id = null)
        {
            var query = _expenseVendorsRepository.TableNoTracking.Where(x => !x.Deleted && x.VendorName.ToLower() == name.ToLower() && x.SiteId == siteId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion

        #region InsertExpenseVendor
        // Title: InsertExpenseVendor
        // Description: This method inserts a new ExpenseVendors entity into the repository. It takes a ExpenseVendors object as input and uses the _expenseVendorsRepository to handle the insertion operation.
        public void InsertExpenseVendor(ExpenseVendors entity)
        {
            _expenseVendorsRepository.Insert(entity);
        }
        #endregion

        #region UpdateExpenseVendor
        // Title: UpdateExpenseVendor
        // Description: This method updates the specified ExpenseVendors entity in the repository. It takes a ExpenseVendors object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateExpenseVendor(ExpenseVendors entity)
        {
            _expenseVendorsRepository.Update(entity);
        }
        #endregion

        #region DeleteExpenseVendor
        // Title: DeleteExpenseVendor
        // Description: Marks the specified ExpenseVendors entity as deleted by setting its `Deleted` property to true. 
        public void DeleteExpenseVendor(ExpenseVendors entity)
        {
            entity.Deleted = true;
            _expenseVendorsRepository.Update(entity);
        }
        #endregion
    }
}

