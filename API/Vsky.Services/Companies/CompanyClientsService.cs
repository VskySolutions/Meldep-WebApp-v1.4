using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Vsky.Services.Sites;

namespace Vsky.Services.Companies
{
    public class CompanyClientsService : ICompanyClientsService
    {
        #region Services Initializations
        private readonly IRepository<CompanyClients> _companyClientsRepository;
        private readonly IRepository<Notes> _notesRepository;
        public CompanyClientsService(IRepository<CompanyClients> companyClientsRepository, IRepository<Notes> notesRepository)
        {
            _companyClientsRepository = companyClientsRepository;
            _notesRepository = notesRepository;
        }

        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetById
        public async Task<CompanyClients> GetById(string id)
        {
            var query = _companyClientsRepository.Table;

            query = query.Where(x => x.Id == id);

            query = query.Where(x => !x.Deleted);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }

        public async Task<CompanyClients> GetByPersonId(string personId)
        {
            var query = _companyClientsRepository.TableNoTracking.Where(x => !x.Deleted && x.PersonId == personId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        public async Task<CompanyClients> GetByCompanyId(string companyId)
        {
            var query = _companyClientsRepository.TableNoTracking.Where(x => !x.Deleted && x.CompanyId == companyId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public string GetCompanyContactIdById(string customerId)
        {
            //string CompanyId = null;
            //if (CompanyId == null)
            //{
            //    var customer = _companyClientsRepository.TableNoTracking.Where(x => x.Id == customerId && !x.Deleted).FirstOrDefault();
            //    if (customer != null)
            //    {
            //        CompanyId = customer.CompanyId;
            //    }
            //}
            //return CompanyId;

            if (string.IsNullOrWhiteSpace(customerId)) // Check if customerId is null or empty
                return null;

            var customerIdArray = customerId.Split(',');

            var companyIds = _companyClientsRepository.TableNoTracking.Where(x => customerIdArray.Contains(x.Id) && !x.Deleted).Select(x => x.CompanyId).ToList();

            return companyIds.Any() ? string.Join(",", companyIds) : null;
        }

        //public async Task<CompanyClients> GetCustomerDetailsById(string id)
        //{
        //    var query = _companyClientsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

        //    query = query.Select(x => new CompanyClients
        //    {
        //        Id = x.Id,
        //        CompanyId = x.CompanyId,
        //        PersonId = x.PersonId,
        //        CustomerTypeId = x.CustomerTypeId,
        //        Company = new Company
        //        {
        //            Id = x.Company.Id,
        //            Name = x.Company.Name,
        //            ContactName = x.Company.ContactName,
        //            PhoneNumber = x.Company.PhoneNumber,
        //            EmailAddress = x.Company.EmailAddress,
        //            AlternativeEmailAddress = x.Company.AlternativeEmailAddress,
        //            AlternativePhoneNumber = x.Company.AlternativePhoneNumber,
        //            Website = x.Company.Website,
        //            Active = x.Company.Active,
        //            CreatedById = x.Company.CreatedById,
        //            CreatedOnUtc = x.Company.CreatedOnUtc,
        //            AddressId = x.Company.AddressId,
        //            ServiceProvidedDetails = x.Company.ServiceProvidedDetails,
        //            ServiceProviderDate = x.Company.ServiceProviderDate,
        //            ComapnyCreatedDate = x.Company.ComapnyCreatedDate,
        //            ProductDetails = x.Company.ProductDetails,
        //            EmployeeId = x.Company.EmployeeId,
        //            Employee = new Employee
        //            {
        //                Person = new Person
        //                {
        //                    Id = x.Company.Employee.Person.Id,
        //                    FullName = x.Company.Employee.Person.FirstName + " " + x.Company.Employee.Person.LastName,
        //                    PrimaryPhoneNumber = x.Company.Employee.Person.PrimaryPhoneNumber,
        //                    PrimaryEmailAddress = x.Company.Employee.Person.PrimaryEmailAddress
        //                }
        //            },
        //            Address = new Address
        //            {
        //                Id = x.Company.Address.Id,
        //                City = x.Company.Address.City,
        //                ZipCode = x.Company.Address.ZipCode,
        //                CreatedById = x.Company.Address.CreatedById,
        //                CreatedOnUtc = x.Company.Address.CreatedOnUtc,
        //                AddressLine1 = x.Company.Address.AddressLine1,
        //                AddressLine2 = x.Company.Address.AddressLine2,
        //                AddressCountry = new Country
        //                {
        //                    Name = x.Company.Address.AddressCountry.Name,
        //                    Id = x.Company.Address.AddressCountry.Id,
        //                },
        //                AddressStateProvince = new StateProvince
        //                {
        //                    Name = x.Company.Address.AddressStateProvince.Name,
        //                    Id = x.Company.Address.AddressStateProvince.Id
        //                }
        //            },
        //            BusinessType = new DropDown
        //            {
        //                Id = x.Company.BusinessType.Id,
        //                DropDownValue = x.Company.BusinessType.DropDownValue,
        //            },
        //            Status = new DropDown
        //            {
        //                Id = x.Company.Status.Id,
        //                DropDownValue = x.Company.Status.DropDownValue,
        //            },
        //            CompanyContacts = x.Company.CompanyContacts.Where(m => !m.Deleted).Select(m => new CompanyContacts
        //            {
        //                Id = m.Id,
        //                CreatedById = m.CreatedById,
        //                CreatedOnUtc = m.CreatedOnUtc,
        //                UpdatedById = m.UpdatedById,
        //                UpdatedOnUtc = m.UpdatedOnUtc,
        //                AlternatePhoneNumber = m.AlternatePhoneNumber,
        //                AlternateEmail = m.AlternateEmail,
        //                CompanyId = x.Id,
        //                Person = new Person
        //                {
        //                    Id = m.Person.Id,
        //                    FirstName = m.Person.FirstName,
        //                    LastName = m.Person.LastName,
        //                    PrimaryEmailAddress = m.Person.PrimaryEmailAddress,
        //                    PrimaryPhoneNumber = m.Person.PrimaryPhoneNumber,
        //                }
        //            }).ToList()
        //        },
        //        Person = new Person
        //        {
        //            Id = x.Person.Id,
        //            FullName = x.Person.FirstName + " " + x.Person.LastName,
        //            PrimaryEmailAddress = x.Person.PrimaryEmailAddress,
        //            PrimaryPhoneNumber = x.Person.PrimaryPhoneNumber,
        //            Address = new Address
        //            {
        //                Id = x.Person.Address.Id,
        //                City = x.Person.Address.City,
        //                ZipCode = x.Person.Address.ZipCode,
        //                CreatedById = x.Person.Address.CreatedById,
        //                CreatedOnUtc = x.Person.Address.CreatedOnUtc,
        //                AddressLine1 = x.Person.Address.AddressLine1,
        //                AddressLine2 = x.Person.Address.AddressLine2,
        //                AddressCountry = new Country
        //                {
        //                    Name = x.Person.Address.AddressCountry.Name,
        //                    Id = x.Person.Address.AddressCountry.Id,
        //                },
        //                AddressStateProvince = new StateProvince
        //                {
        //                    Name = x.Person.Address.AddressStateProvince.Name,
        //                    Id = x.Person.Address.AddressStateProvince.Id
        //                }
        //            }
        //        },
        //        CustomerType = new DropDown
        //        {
        //            Id = x.CustomerType.Id,
        //            DropDownValue = x.CustomerType.DropDownValue,
        //        }
        //    });
        //    var item = await query.FirstOrDefaultAsync();
        //    return item;
        //}
        public async Task<CompanyClients> GetCustomerDetailsById(string id)
        {
            var item = await _companyClientsRepository.TableNoTracking
                .Where(x => !x.Deleted && x.Id == id)
                .Include(x => x.CustomerType)
                .Include(x => x.AssignedTo).ThenInclude(x => x.Person)
                .Include(x => x.Company).ThenInclude(c => c.CreatedBy).ThenInclude(m => m.Person)
                .Include(x => x.Company).ThenInclude(c => c.UpdatedBy).ThenInclude(m => m.Person)
                .Include(x => x.Company).ThenInclude(c => c.Address)
                .Include(x => x.Company).ThenInclude(c => c.Address).ThenInclude(a => a.AddressCountry)
                .Include(x => x.Company).ThenInclude(c => c.Address).ThenInclude(a => a.AddressStateProvince)
                .Include(x => x.Company).ThenInclude(c => c.Employee).ThenInclude(e => e.Person)
                .Include(x => x.Company).ThenInclude(c => c.BusinessType)
                .Include(x => x.Company).ThenInclude(c => c.Status)
                .Include(x => x.Company).ThenInclude(c => c.CompanyContacts.Where(cc => !cc.Deleted))
                .Include(x => x.Company).ThenInclude(c => c.CompanyContacts).ThenInclude(cc => cc.Person)
                .Include(x => x.Person)
                .Include(x => x.Person).ThenInclude(p => p.IdentifiedBy)
                .Include(x => x.Person).ThenInclude(p => p.Address)
                .Include(x => x.Person).ThenInclude(p => p.Address).ThenInclude(a => a.AddressCountry)
                .Include(x => x.Person).ThenInclude(p => p.Address).ThenInclude(a => a.AddressStateProvince)
                .Include(x => x.Person).ThenInclude(p => p.AddressType)
                .Include(x => x.Person).ThenInclude(p => p.Gender)
                .FirstOrDefaultAsync();

            if (item?.ParentCustomerId != null)
            {
                var parentCustomer = await _companyClientsRepository.Table
                    .Where(p => p.Id == item.ParentCustomerId && !p.Deleted)
                    .Include(p => p.Company)
                    .Include(p => p.Person)
                    .FirstOrDefaultAsync();

                item.ParentCustomerName = parentCustomer?.Company?.Name
                    ?? (parentCustomer?.Person != null
                        ? parentCustomer.Person.FirstName + " " + parentCustomer.Person.LastName
                        : null);
            }

            return item;
        }

        #endregion

        #region GetAllCustomers
        public IPagedList<CompanyClients> GetAllCustomers(string SiteId, string SearchText, List<string> customerTypeIds, List<string> customerIds, List<string> employeeIds, string emailAddress, string phoneNumber, List<string> parentCustomerIds, string sortBy,
            bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            //var query = _companyClientsRepository.TableNoTracking.Where(x => !x.Deleted && x.Company.SiteId == SiteId);

            var query = _companyClientsRepository.TableNoTracking
                .Where(x => !x.Deleted && x.SiteId == SiteId && (x.CompanyId != null || x.PersonId != null)).OrderByDescending(x => x.CreatedOnUtc)
                .Include(x => x.Person)
                .Include(x => x.CustomerType)
                .Include(x => x.Company)
                .Include(x => x.Company).ThenInclude(x => x.Employee)
                .Include(x => x.Company).ThenInclude(x => x.Employee).ThenInclude(x => x.Person)
                .AsQueryable();
            
            // custom filter
            if (customerTypeIds != null && customerTypeIds.Any())
                query = query.Where(x => customerTypeIds.Contains(x.CustomerTypeId));

            if (customerIds != null && customerIds.Any())
                query = query.Where(x => customerIds.Contains(x.Id));

            if (parentCustomerIds != null && parentCustomerIds.Any())
                query = query.Where(x => parentCustomerIds.Contains(x.ParentCustomerId));

            if (employeeIds != null && employeeIds.Any())
                query = query.Where(x => employeeIds.Contains(x.Company.EmployeeId));

            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                query = query.Where(x =>
                    (x.Company != null && x.Company.EmailAddress.Contains(emailAddress)) ||
                    (x.Person != null && x.Person.PrimaryEmailAddress.Contains(emailAddress))
                );
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                phoneNumber = phoneNumber.Trim().ToLower();
                query = query.Where(x =>
                    (x.Company != null && x.Company.PhoneNumber.ToLower().Contains(phoneNumber)) ||
                    (x.Person != null && x.Person.PrimaryPhoneNumber.ToLower().Contains(phoneNumber)) ||
                    x.AssignedDate.Value.Date == parsedDate.Date
                );
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.CustomerType.DropDownValue.Contains(SearchText.ToLower()) ||
                    //(m.AssignedDate.HasValue && m.AssignedDate.Value.ToString("MM/dd/yyyy").Contains(SearchText.ToLower())) ||
                    (m.Company.EmailAddress.Contains(SearchText.ToLower()) || m.Person.PrimaryEmailAddress.Contains(SearchText.ToLower())) ||
                    (m.Company.PhoneNumber.Contains(SearchText.ToLower()) || m.Person.PrimaryPhoneNumber.Contains(SearchText.ToLower())) ||
                    (m.Company.Employee.Person.FirstName.Contains(SearchText.ToLower()) || m.Company.Employee.Person.LastName.Contains(SearchText.ToLower())) ||
                    (m.Company.Name.Contains(SearchText.ToLower()) || m.Company.Name.Contains(SearchText.ToLower())) ||
                    m.AssignedDate.Value.Date == parsedDate.Date
                );
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                //var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                //query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            //query = query.Select(x => new CompanyClients
            //{
            //    CustomerNoteCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "customer").Count(),
            //});

            var list = new PagedList<CompanyClients>(query, page, pageSize);            

            var parentIds = list.Where(x => x.ParentCustomerId != null)
                            .Select(x => x.ParentCustomerId)
                            .Distinct()
                            .ToList();

            var parentCustomers = _companyClientsRepository.TableNoTracking
                                .Where(x => parentIds.Contains(x.Id) && !x.Deleted)
                                .Include(x => x.Company)
                                .Include(x => x.Person)
                                .ToList();

            foreach (var item in list)
            {
                if (!string.IsNullOrEmpty(item.ParentCustomerId))
                {
                    var parent = parentCustomers.FirstOrDefault(p => p.Id == item.ParentCustomerId);
                    item.ParentCustomerName = parent?.Company?.Name
                        ?? (parent?.Person != null
                            ? parent.Person.FirstName + " " + parent.Person.LastName
                            : null);
                }
            }

            // Notes Count

            var customerIdsInPage = list.Select(x => x.Id).ToList();
            var notesCounts = _notesRepository.TableNoTracking
                .Where(n =>
                    !n.Deleted &&
                    n.Type == "customer" &&
                    customerIdsInPage.Contains(n.SubModuleId))
                .GroupBy(n => n.SubModuleId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    Count = g.Count()
                })
                .ToList();

            foreach (var item in list)
            {
                item.CustomerNoteCount = notesCounts
                    .FirstOrDefault(x => x.CustomerId == item.Id)?.Count ?? 0;
            }


            return list;
        }
        #endregion

        #region GetAllCompanyClients
        public async Task<IList<CompanyClients>> GetAllCompanyClients(string companyId)
        {
            var query = _companyClientsRepository.TableNoTracking;

            query = query.Where(x => x.CompanyId == companyId);

            query = query.Select(x => new CompanyClients
            {
                Id = x.Id,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                CompanyId = x.Company.Id,

                Company = new Company
                {
                    Name = x.Company.Name,
                    ContactName = x.Company.ContactName,
                    PhoneNumber = x.Company.PhoneNumber,
                    EmailAddress = x.Company.EmailAddress,
                    AlternativeEmailAddress = x.Company.AlternativeEmailAddress,
                    AlternativePhoneNumber = x.Company.AlternativePhoneNumber,
                    Website = x.Company.Website,
                    Active = x.Company.Active,
                    CreatedById = x.Company.CreatedById,
                    CreatedOnUtc = x.Company.CreatedOnUtc,
                    AddressId = x.Company.AddressId,
                    ServiceProvidedDetails = x.Company.ServiceProvidedDetails,
                    ServiceProviderDate = x.Company.ServiceProviderDate,
                    ComapnyCreatedDate = x.Company.ComapnyCreatedDate,
                    ProductDetails = x.Company.ProductDetails,
                    Id = x.Company.Id,
                }
            });

            var list = await query.ToListAsync();

            return list;
        }
        #endregion

        #region GetAllClientListForDropdown
        //public async Task<List<CompanyClients>> GetAllClientListForDropdown(string SiteId)
        //{
        //    //var query = _companyClientsRepository.TableNoTracking.Where(x => !x.Deleted && x.Company.SiteId == SiteId);
        //    var query = _companyClientsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
        //    query = query.Select(x => new CompanyClients
        //    {
        //        Id = x.Id,
        //        Company = new Company
        //        {
        //            Name = x.Company.Name,
        //            Id = x.Company.Id,
        //        }
        //    });

        //    var list = await query.ToListAsync();

        //    return list;
        //}

        public async Task<List<CompanyClients>> GetAllClientListForDropdown(string siteId)
        {
            var query = _companyClientsRepository.Table
                .Where(x => !x.Deleted && x.SiteId == siteId && (x.CompanyId != null || x.PersonId != null))
                .Include(x => x.Company)
                .Include(x => x.Person)
                .OrderBy(x => x.CompanyId != null
                    ? x.Company.Name.Replace(" ", "")
                    : (x.Person.FirstName + " " + x.Person.LastName).Replace(" ", ""))
                .Select(x => new CompanyClients
                {
                    Id = x.Id,
                    CompanyId = x.CompanyId,
                    PersonId = x.PersonId,
                    Company = x.CompanyId != null ? new Company
                    {
                        Name = x.Company.Name,
                        Id = x.Company.Id,
                    } : null,
                    Person = x.PersonId != null ? new Person
                    {
                        FirstName = x.Person.FirstName,
                        LastName = x.Person.LastName,
                        Id = x.Person.Id,
                    } : null
                });

            return await query.ToListAsync();
        }

        #endregion

        public async Task<List<CompanyClients>> GetAllParentCustomerList(string SiteId)
        {
            var query = _companyClientsRepository.TableNoTracking.Where(x => !x.Deleted);
            var parentCustomerIds = await query.Where(x => x.ParentCustomerId != null).Select(x => x.ParentCustomerId).Distinct().ToListAsync();
            var parentCustomers = await query.Where(x => parentCustomerIds.Contains(x.Id)).Include(x => x.Company).Include(x => x.Person).OrderBy(x => x.Company != null ? x.Company.Name : x.Person.FirstName).ToListAsync();
            return parentCustomers;
        }

        public async Task<List<CompanyClients>> GetAllParentCustomerListForDropdown(string siteId, string customerId = null)
        {
            var query = _companyClientsRepository.TableNoTracking
            .Where(x => !x.Deleted && x.SiteId == siteId && (x.CompanyId != null || x.PersonId != null))
            .Include(x => x.Company)
            .Include(x => x.Person)
            .OrderBy(x => x.CompanyId != null
                    ? x.Company.Name.Replace(" ", "")
                    : (x.Person.FirstName + " " + x.Person.LastName).Replace(" ", ""))
            .Select(x => new CompanyClients
            {
                Id = x.Id,
                CompanyId = x.CompanyId,
                PersonId = x.PersonId,
                Company = x.CompanyId != null ? new Company
                {
                    Name = x.Company.Name,
                    Id = x.Company.Id,
                } : null,
                Person = x.PersonId != null ? new Person
                {
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName,
                    Id = x.Person.Id,
                } : null
            });


            if (!string.IsNullOrEmpty(customerId))
            {
                query = query.Where(x => x.Id != customerId);
            }

            return await query.ToListAsync();
        }

        #region GetCustomerByCompanyId
        public async Task<CompanyClients> GetCustomerByCompanyId(string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
                return null;

            return await _companyClientsRepository.TableNoTracking
                .FirstOrDefaultAsync(x => x.CompanyId == companyId && !x.Deleted);
        }

        #endregion

        #region InsertCompanyClient
        public void InsertCompanyClient(CompanyClients entity)
        {
            _companyClientsRepository.Insert(entity);
        }
        #endregion

        #region UpdateCompanyClient
        public void UpdateCompanyClient(CompanyClients entity)
        {
            _companyClientsRepository.Update(entity);
        }
        #endregion

        #region DeleteCustomer
        public void DeleteCustomer(CompanyClients entity)
        {
            entity.Deleted = true;

            _companyClientsRepository.Update(entity);
        }

        #endregion

        #region GetAllCompanyListForCustomerDropdown
        public async Task<List<CompanyClients>> GetAllCompanyListForCustomersDropdown(string SiteId)
        {
            var query = _companyClientsRepository.TableNoTracking.Where(m => !m.Deleted && m.Company.SiteId == SiteId);

            query = query.Select(x => new CompanyClients
            {
                Id = x.Id,
                CompanyId = x.Company.Id,
               Company = new Company
               {
                   Id = x.CompanyId,
                   Name = x.Company.Name
               }
            });

            var list = await query.ToListAsync();

            return list;
        }
        #endregion
    }
}
