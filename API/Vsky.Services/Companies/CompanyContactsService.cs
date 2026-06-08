using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Vsky.Core;
using Vsky.Services.Sites;

namespace Vsky.Services.Companies
{
    public class CompanyContactsService : ICompanyContactsService
    {
        #region Services Initializations
        private readonly IRepository<CompanyContacts> _companyContactsRepository;
        private readonly ICompanyClientsService _companyClientsService;
        private readonly IRepository<Notes> _notesRepository;
        public CompanyContactsService(
            IRepository<CompanyContacts>
            companyContactsRepository,
            ICompanyClientsService companyClientsService, IRepository<Notes> notesRepository)
        {
            _companyContactsRepository = companyContactsRepository;
            _companyClientsService = companyClientsService;
            _notesRepository = notesRepository;
        }

        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllContactList
        public IPagedList<CompanyContacts> GetAllContactList(string SiteId, string SearchText, string companyId, string sortBy,
            bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _companyContactsRepository.TableNoTracking.Where(x => !x.Deleted && x.Company.SiteId == SiteId && !x.Company.Deleted);

            // custom filter
            if (!string.IsNullOrWhiteSpace(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                    m.Company.Name.Contains(SearchText.ToLower()) ||
                    m.Person.FirstName.ToLower().Contains(SearchText.ToLower()) ||
                    m.Person.LastName.ToLower().Contains(SearchText.ToLower()) ||
                    m.Person.PrimaryEmailAddress.ToLower().Contains(SearchText.ToLower()) ||
                    m.Person.PrimaryPhoneNumber.ToLower().Contains(SearchText.ToLower())
                );
            }

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            query = query.Select(x => new CompanyContacts
            {
                Id = x.Id,
                AlternateEmail = x.AlternateEmail,
                AlternatePhoneNumber = x.AlternatePhoneNumber,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                CompanyId = x.Company.Id,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName,
                    MiddleName = x.Person.MiddleName,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress,
                    PrimaryPhoneNumber = x.Person.PrimaryPhoneNumber,
                },
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
                    //SiteId=x.Company.SiteId,
                    ServiceProvidedDetails = x.Company.ServiceProvidedDetails,
                    ServiceProviderDate = x.Company.ServiceProviderDate,
                    ComapnyCreatedDate = x.Company.ComapnyCreatedDate,
                    ProductDetails = x.Company.ProductDetails,
                    Id = x.Company.Id,
                },
                CompanyContactNotesCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Company Contact").Count(),
            });
            var list = new PagedList<CompanyContacts>(query, page, pageSize);

            return list;
        }

        #endregion

        #region GetById
        public async Task<CompanyContacts> GetById(string id)
        {
            var query = _companyContactsRepository.TableNoTracking.Where(x => x.Id == id && !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetcompanydetailsById
        public async Task<CompanyContacts> GetCompanyContactdetailsById(string id)
        {
            var query = _companyContactsRepository.TableNoTracking.Where(x => x.Id == id && !x.Deleted);
            query = query.Select(x => new CompanyContacts
            {
                Id = x.Id,
                AlternateEmail = x.AlternateEmail,
                AlternatePhoneNumber = x.AlternatePhoneNumber,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                CompanyId = x.Company.Id,
                PersonId = x.PersonId,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FirstName = x.Person.FirstName,
                    MiddleName = x.Person.MiddleName,
                    LastName = x.Person.LastName,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress,
                    PrimaryPhoneNumber = x.Person.PrimaryPhoneNumber,
                    CreatedById = x.Person.CreatedById,
                    CreatedOnUtc = x.Person.CreatedOnUtc,
                    UpdatedById = x.Person.UpdatedById,
                    UpdatedOnUtc = x.Person.UpdatedOnUtc,
                    Gender = x.Person.Gender,
                    Address = new Address
                    {
                        Id = x.Person.Address.Id,
                        CountryId = x.Person.Address.CountryId,
                        AddressCountry = new Country
                        {
                            Id = x.Person.Address.AddressCountry.Id,
                            Name = x.Person.Address.AddressCountry.Name,
                        }
                    }
                },
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
                    //SiteId=x.Company.SiteId,
                    ServiceProvidedDetails = x.Company.ServiceProvidedDetails,
                    ServiceProviderDate = x.Company.ServiceProviderDate,
                    ComapnyCreatedDate = x.Company.ComapnyCreatedDate,
                    ProductDetails = x.Company.ProductDetails,
                    Id = x.Company.Id,
                }
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllContacts
        public async Task<IList<CompanyContacts>> GetAllContacts(string companyId)
        {
            var query = _companyContactsRepository.TableNoTracking;
            query = query.Where(x => x.CompanyId == companyId);
            query = query.Select(x => new CompanyContacts
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
                    //CompanyId = x.Company.CompanyId,
                    Active = x.Company.Active,
                    CreatedById = x.Company.CreatedById,
                    CreatedOnUtc = x.Company.CreatedOnUtc,
                    AddressId = x.Company.AddressId,
                    ServiceProvidedDetails = x.Company.ServiceProvidedDetails,
                    ServiceProviderDate = x.Company.ServiceProviderDate,
                    ComapnyCreatedDate = x.Company.ComapnyCreatedDate,
                    ProductDetails = x.Company.ProductDetails,
                    Id = x.Company.Id,
                },
                Person = new Person
                {
                    Id = x.Person.Id,
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress,                    
                    PrimaryPhoneNumber = x.Person.PrimaryPhoneNumber,
                },
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllContactListForDropdown
        public async Task<List<CompanyContacts>> GetAllContactListForDropdown(string SiteId, string customerId)
        {
            var query = _companyContactsRepository.TableNoTracking.Where(x => !x.Deleted && x.Company.SiteId == SiteId && !x.Person.Deleted);

            if (!string.IsNullOrWhiteSpace(customerId))
            {
                var customer = await _companyClientsService.GetById(customerId);
                if (customer != null)
                    query = query.Where(x => x.CompanyId == customer.CompanyId);
            }

            query = query.Select(x => new CompanyContacts
            {
                Id = x.Id,
                Company = new Company
                {
                    Name = x.Company.Name
                },
                Person = new Person
                {
                    Id = x.Person.Id,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress
                }
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllContactListByCompanyIdForDropdown
        public async Task<List<CompanyContacts>> GetAllContactListByCompanyIdForDropdown(string SiteId, string companyId = null)
        {
            var query = _companyContactsRepository.TableNoTracking.Where(x => !x.Deleted && x.Company.SiteId == SiteId && !x.Person.Deleted);

            if (!string.IsNullOrWhiteSpace(companyId))
            {
                // Split the comma-separated companyIds into an array
                var companyIdArray = companyId.Split(',');

                // Filter query based on the array of company IDs
                query = query.Where(x => companyIdArray.Contains(x.CompanyId));
            }

            query = query
                .OrderBy(x => (x.Person.FirstName + " " + x.Person.LastName).Replace(" ", ""))
                .Select(x => new CompanyContacts
            {
                Id = x.Id,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress
                }
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllCustomerContactListForDropdown
        public async Task<List<CompanyContacts>> GetAllCustomerContactListForDropdown(string SiteId, string statusId)
        {
            var data = await _companyContactsRepository.TableNoTracking
                    .Where(x => !x.Deleted &&
                                !x.Company.Deleted &&
                                x.Company.SiteId == SiteId &&
                                x.Company.StatusId == statusId)
                    .Select(x => new
                    {
                        PersonId = x.Person.Id,
                        FullName = (x.Person.FirstName ?? "") + " " + (x.Person.LastName ?? ""),
                        CompanyName = x.Company.Name
                    })
                    .ToListAsync();

                        return data
                            .GroupBy(x => new
                            {
                                x.PersonId,
                                x.FullName
                            })
                            .Select(g => new CompanyContacts
                            {
                                Person = new Person
                                {
                                    Id = g.Key.PersonId,
                                    FullName = $"{g.Key.FullName} ({string.Join(", ", g.Select(x => x.CompanyName).Distinct())})"
                                }
                            })
                            .OrderBy(x => x.Person.FullName)
                            .ToList();
        }
        #endregion

        #region GetByPersonAndCompanyId
        public async Task<CompanyContacts> GetByPersonAndCompanyId(string companyId, string personId, string id = null)
        {

            var query = _companyContactsRepository.TableNoTracking.Where(x => x.CompanyId == companyId && x.PersonId == personId && !x.Deleted);

            if (!string.IsNullOrWhiteSpace(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;

        }
        public async Task<CompanyContacts> GetCompanyContactByPersonId(string personId, string id = null)
        {

            var query = _companyContactsRepository.TableNoTracking.Where(x => x.PersonId == personId && !x.Deleted);

            if (!string.IsNullOrWhiteSpace(id))

                query = query.Where(x => x.Id == id);

            var item = await query.FirstOrDefaultAsync();

            return item;

        }
        #endregion


        #region InsertCompanyContact
        public void InsertCompanyContact(CompanyContacts entity)
        {
            _companyContactsRepository.Insert(entity);
        }
        #endregion

        #region UpdateCompanyContact
        public void UpdateCompanyContact(CompanyContacts entity)
        {
            _companyContactsRepository.Update(entity);
        }
        #endregion

        #region DeleteCompanyContact
        public void DeleteCompanyContact(CompanyContacts entity)
        {
            entity.Deleted = true;

            _companyContactsRepository.Update(entity);
        }

        #endregion

        #region InsertCompanyContactList
        public void InsertCompanyContactList(IList<CompanyContacts> entities)
        {
            _companyContactsRepository.Insert(entities);
        }
        #endregion

        #region UpdateCompanyContactList
        public void UpdateCompanyContactList(IList<CompanyContacts> entities)
        {
            _companyContactsRepository.Update(entities);
        }
        #endregion

        #region DeleteCompanyContactList
        public void DeleteCompanyContactList(List<CompanyContacts> entities)
        {
            var companyContacts = new List<CompanyContacts>();
            foreach (var items in entities)
            {
                items.Deleted = true;
                companyContacts.Add(items);
            }
            _companyContactsRepository.Update(companyContacts);
        }
        #endregion
    }
}
