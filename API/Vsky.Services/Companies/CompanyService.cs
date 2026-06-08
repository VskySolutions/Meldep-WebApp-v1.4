using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.Companies
{
    public class CompanyService : ICompanyService
    {
        #region Service Initialization
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Notes> _notesRepository;
        public CompanyService(IRepository<Company> customerRepository, IRepository<Notes> notesRepository)
        {
            _companyRepository = customerRepository;
            _notesRepository = notesRepository;
        }
        #endregion

        #region Private Methods

        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region Public Methods
        public IPagedList<Company> GetAllCompanies(string SiteId, string SearchText, string companyId, string businessTypeId,string employeeId, string sortBy,
            bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _companyRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            // custom filter
            if (!string.IsNullOrWhiteSpace(companyId))
                query = query.Where(x => x.Id == companyId);

            if (!string.IsNullOrWhiteSpace(employeeId))
                query = query.Where(x => x.EmployeeId == employeeId);

            if (!string.IsNullOrWhiteSpace(businessTypeId))
                query = query.Where(x => x.BusinessTypeId == businessTypeId);

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                    m.Name.Contains(SearchText.ToLower()) ||
                    m.Website.Contains(SearchText.ToLower()) ||
                    m.EmailAddress.Contains(SearchText.ToLower()) ||
                    m.PhoneNumber.Contains(SearchText.ToLower()) ||
                    m.BusinessType.DropDownValue.Contains(SearchText.ToLower()) ||
                    m.Status.DropDownValue.Contains(SearchText.ToLower()) ||
                    (m.Employee.Person.FirstName.Contains(SearchText.ToLower()) || m.Employee.Person.LastName.Contains(SearchText.ToLower()))
                );
            }

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
                query = query.Select(x => new Company
                {
                    Id = x.Id,
                    Name = x.Name,
                });
            }
            else
            {
                query = query.Select(x => new Company
                {
                    Id = x.Id,
                    Name = x.Name,
                    ContactName = x.ContactName,
                    PhoneNumber = x.PhoneNumber,
                    EmailAddress = x.EmailAddress,
                    AlternativeEmailAddress = x.AlternativeEmailAddress,
                    AlternativePhoneNumber = x.AlternativePhoneNumber,
                    Website = x.Website,
                    Active = x.Active,
                    CreatedById = x.CreatedById,
                    CreatedOnUtc = x.CreatedOnUtc,
                    AddressId = x.AddressId,
                    ServiceProvidedDetails = x.ServiceProvidedDetails,
                    ServiceProviderDate = x.ServiceProviderDate,
                    ComapnyCreatedDate = x.ComapnyCreatedDate,
                    EmployeeId = x.EmployeeId,
                    BusinessTypeId = x.BusinessTypeId,
                    StatusId = x.StatusId,
                    ProductDetails = x.ProductDetails,
                    //SiteId = x.SiteId,
                    Address = new Address
                    {
                        Id = x.Address.Id,
                        City = x.Address.City,
                        ZipCode = x.Address.ZipCode,
                        CreatedById = x.Address.CreatedById,
                        CreatedOnUtc = x.Address.CreatedOnUtc,
                        AddressLine1 = x.Address.AddressLine1,
                        AddressLine2 = x.Address.AddressLine2,
                        AddressCountry = new Country
                        {
                            Name = x.Address.AddressCountry.Name,
                            Id = x.Address.AddressCountry.Id,
                        },
                        AddressStateProvince = new StateProvince
                        {
                            Name = x.Address.AddressStateProvince.Name,
                            Id = x.Address.AddressStateProvince.Id
                        }
                    },
                    BusinessType = new DropDown
                    {
                        Id = x.BusinessType.Id,
                        DropDownValue = x.BusinessType.DropDownValue,
                    },
                    Status = new DropDown
                    {
                        Id = x.Status.Id,
                        DropDownValue = x.Status.DropDownValue,
                    },
                    Employee = new Employee
                    {
                        Id = x.Employee.Id,
                        Person = new Person
                        {
                            Id = x.Employee.Person.Id,
                            FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                            FirstName = x.Employee.Person.FirstName,
                            LastName = x.Employee.Person.LastName
                        },
                    },
                    CompanyContacts = x.CompanyContacts.Where(m => !m.Deleted).Select(m => new CompanyContacts
                    {
                        Id = m.Id,
                        CreatedById = m.CreatedById,
                        CreatedOnUtc = m.CreatedOnUtc,
                        UpdatedById = m.UpdatedById,
                        UpdatedOnUtc = m.UpdatedOnUtc,
                        AlternatePhoneNumber = m.AlternatePhoneNumber,
                        AlternateEmail = m.AlternateEmail,
                        CompanyId = x.Id,
                        Person = new Person
                        {
                            Id = m.Person.Id,
                            FirstName = m.Person.FirstName,
                            LastName = m.Person.LastName,
                            PrimaryEmailAddress = m.Person.PrimaryEmailAddress,
                            PrimaryPhoneNumber = m.Person.PrimaryPhoneNumber,
                        }
                    }).ToList(),
                    CompanyCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Company").Count(),
                });
            }

            var list = new PagedList<Company>(query, page, pageSize);
            return list;
        }
        #endregion
        
        #region GetById
        public async Task<Company> GetById(string id)
        {
            var query = _companyRepository.Table;
            query = query.Where(x => x.Id == id);
            query = query.Where(x => !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetcompanydetailsById
        public async Task<Company> GetcompanydetailsById(string id)
        {
            var query = _companyRepository.Table;
            query = query.Where(x => x.Id == id);
            query = query.Where(x => !x.Deleted);

            query = query.Select(x => new Company
            {
                Id = x.Id,
                Name = x.Name,
                ContactName = x.ContactName,
                PhoneNumber = x.PhoneNumber,
                EmailAddress = x.EmailAddress,
                AlternativeEmailAddress = x.AlternativeEmailAddress,
                AlternativePhoneNumber = x.AlternativePhoneNumber,
                Website = x.Website,
                Active = x.Active,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                AddressId = x.AddressId,
                ServiceProvidedDetails = x.ServiceProvidedDetails,
                ServiceProviderDate = x.ServiceProviderDate,
                ComapnyCreatedDate = x.ComapnyCreatedDate,
                EmployeeId = x.EmployeeId,
                BusinessTypeId = x.BusinessTypeId,
                StatusId = x.StatusId,
                ProductDetails = x.ProductDetails,
                //SiteId = x.SiteId,
                ProfileLink = x.ProfileLink,
                Description = x.Description,
                Address = new Address
                {
                    Id = x.Address.Id,
                    City = x.Address.City,
                    ZipCode = x.Address.ZipCode,
                    CreatedById = x.Address.CreatedById,
                    CreatedOnUtc = x.Address.CreatedOnUtc,
                    AddressLine1 = x.Address.AddressLine1,
                    AddressLine2 = x.Address.AddressLine2,
                    AddressCountry = new Country
                    {
                        Name = x.Address.AddressCountry.Name,
                        Id = x.Address.AddressCountry.Id,
                    },
                    AddressStateProvince = new StateProvince
                    {
                        Name = x.Address.AddressStateProvince.Name,
                        Id = x.Address.AddressStateProvince.Id
                    }
                },
                BusinessType = new DropDown
                {
                    Id = x.BusinessType.Id,
                    DropDownValue = x.BusinessType.DropDownValue,
                },
                Status = new DropDown
                {
                    Id = x.Status.Id,
                    DropDownValue = x.Status.DropDownValue,
                },
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                        FirstName = x.Employee.Person.FirstName,
                        LastName = x.Employee.Person.LastName
                    },
                },
                CompanyContacts = x.CompanyContacts.Where(m => !m.Deleted).Select(m => new CompanyContacts
                {
                    Id = m.Id,
                    CreatedById = m.CreatedById,
                    CreatedOnUtc = m.CreatedOnUtc,
                    UpdatedById = m.UpdatedById,
                    UpdatedOnUtc = m.UpdatedOnUtc,
                    AlternatePhoneNumber = m.AlternatePhoneNumber,
                    AlternateEmail = m.AlternateEmail,
                    CompanyId = x.Id,
                    Person = new Person
                    {
                        Id = m.Person.Id,
                        FirstName = m.Person.FirstName,
                        LastName = m.Person.LastName,
                        PrimaryEmailAddress = m.Person.PrimaryEmailAddress,
                        PrimaryPhoneNumber = m.Person.PrimaryPhoneNumber,
                    }
                }).ToList()
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        
        //public async Task<Company> GetCompanyDetailsForCustomer(string customerId)
        //{ }

        #endregion

        #region GetAllCompanyListForDropdown
        public async Task<List<CommonDropDown>> GetAllCompanyListForDropdown(string SiteId)
        {
            var query = _companyRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            var list = await query
                .OrderBy(x => x.Name.Replace(" ", ""))
                .Select(x => new CommonDropDown
                {
                    Text = x.Name,
                    Value = x.Id,
                })
                .ToListAsync(); 

            return list;
        }
        #endregion

        #region GetAllPrimaryEmployeeListForDropdown
        public async Task<List<Company>> GetAllPrimaryEmployeeListForDropdown(string SiteId)
        {
            var query = _companyRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Employee != null && !x.Employee.Deleted && !x.Employee.Person.Deleted && x.Employee.Person != null);
            query = query.Select(x => new Company
            {
                Id = x.Id,
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName +" "+ x.Employee.Person.LastName,
                    }
                }
            });

            var list = await query.ToListAsync();
            // Group by Person.Id to remove duplicates and select the first from each group

            // Group by Person.Id to remove duplicates
            var distinctList = list
                .GroupBy(x => x.Employee.Person.Id)
                .Select(group => group.First())
                .OrderBy(x => x.Employee.Person.FullName)  // <-- Alphabetical ascending
                .ToList();

            return distinctList;
        }
        #endregion

        #region GetCompanyByNameAndSiteId
        public async Task<Company> GetCompanyByNameAndSiteId(string companyName, string siteId)
        {
            if (string.IsNullOrEmpty(companyName) || string.IsNullOrEmpty(siteId))
                return null;

            companyName = companyName.Trim().ToLower();

            var companies = await _companyRepository.TableNoTracking
                .Where(x => x.SiteId == siteId && !x.Deleted)
                .ToListAsync();

            //return companies
            //    .FirstOrDefault(x =>
            //        companyName.StartsWith(x.Name?.Trim().ToLower()));

            return companies
                .FirstOrDefault(x =>
                    companyName.Split(' ')
                    .FirstOrDefault()
                    == x.Name?.Trim().ToLower());
        }
        #endregion

        #region InsertCompany
        public void InsertCompany(Company entity)
        {
            _companyRepository.Insert(entity);
        }
        #endregion

        #region UpdateCompany
        public void UpdateCompany(Company entity)
        {
            _companyRepository.Update(entity);
        }
        #endregion

        #region DeleteCompany
        public void DeleteCompany(Company entity)
        {
            entity.Deleted = true;

            _companyRepository.Update(entity);
        }

        #endregion
    }
}
