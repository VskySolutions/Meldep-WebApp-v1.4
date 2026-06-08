using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Services.Persons
{
    public class PersonService : IPersonService
    {
        #region Define Services
        /// <summary>
        /// Define Services
        /// </summary>
        private readonly IRepository<Person> _personRepository;
        private readonly IRepository<PersonSitesMapping> _personSitesMappingRepository;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<CompanyClients> _customerRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        #endregion

        #region Services Initializations
        /// <summary>
        /// Services Initializations
        /// </summary>
        /// <param name="personRepository"></param>
        /// <param name="pictureRepository"></param>
        public PersonService(
            IRepository<Person> personRepository, 
            IRepository<Picture> pictureRepository,
            IRepository<PersonSitesMapping> personSitesMappingRepository,
            IRepository<CompanyClients> customerRepository,
            UserManager<ApplicationUser> userManager
        )
        {
            _personRepository = personRepository;
            _pictureRepository = pictureRepository;
            _personSitesMappingRepository = personSitesMappingRepository;
            _customerRepository = customerRepository;
            _userManager = userManager;
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

        #region GetAllPerson
        // Title: GetAllPersons
        // Description: This method retrieves a paginated list of persons based on various search criteria such as person name, 
        // mappings. The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<Person> GetAllPerson(
            string SiteId,
            string SearchText,
            string firstName,
            string lastName,
            string primaryPhoneNumber,
            DateTime? fromDate,
            DateTime? toDate,
            string countryId,
            string stateProvinceId,
            string city,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _personRepository.TableNoTracking.Where(x => !x.Deleted && x.PersonSitesMapping.Any(x => x.SiteId == SiteId && !x.IsSharedUser));

            var customerQuery = _customerRepository.TableNoTracking.Where(c => !c.Deleted && c.SiteId == SiteId && (c.CompanyId != null || c.PersonId != null));
           
            if (!string.IsNullOrWhiteSpace(firstName))
                query = query.Where(x => x.FirstName.ToLower().Contains(firstName.ToLower()));

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(x => x.LastName.ToLower().Contains(lastName.ToLower()));

            if (!string.IsNullOrWhiteSpace(primaryPhoneNumber))
                query = query.Where(x => x.PrimaryPhoneNumber.ToLower() == primaryPhoneNumber.ToLower());

            if (!string.IsNullOrWhiteSpace(countryId))
                query = query.Where(x => x.Address.CountryId.Contains(countryId));

            if (!string.IsNullOrWhiteSpace(stateProvinceId))
                query = query.Where(x => x.Address.StateProvinceId.Contains(stateProvinceId));

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(x => x.Address.City.ToLower().Contains(city.ToLower()));

            if (fromDate != null)
                query = query.Where(x => x.IdentifiedDate >= fromDate);

            if (toDate != null)
                query = query.Where(a => a.IdentifiedDate <= toDate);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            if (!string.IsNullOrEmpty(SearchText) )
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(p =>
                    p.FirstName.ToLower().Contains(SearchText.ToLower()) ||
                    p.LastName.ToLower().Contains(SearchText.ToLower()) ||
                    p.PrimaryEmailAddress.ToLower().Contains(SearchText.ToLower()) ||
                    p.PrimaryPhoneNumber.ToLower().Contains(SearchText.ToLower()) ||
                   (p.IdentifiedBy.FirstName + " " + p.IdentifiedBy.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    p.IdentifiedDate.Value.Date == parsedDate.Date
                );
            }
            query = query.Select(x => new Person
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                FullName = x.FirstName + " " + x.LastName,
                PrimaryEmailAddress = x.PrimaryEmailAddress,
                DOB = x.DOB,
                PrimaryPhoneNumber = x.PrimaryPhoneNumber,
                IdentifiedDate = x.IdentifiedDate,
                IsCustomer = customerQuery.Any(c => c.PersonId == x.Id),
                IdentifiedBy = new Person
                {
                    Id = x.IdentifiedBy.Id,
                    FullName = x.IdentifiedBy.FirstName + " " + x.IdentifiedBy.LastName,
                },
                Address = new Address
                {
                    Id = x.Address.Id,
                    AddressCountry = new Country
                    {
                        Id = x.Address.AddressCountry.Id,
                        Name = x.Address.AddressCountry.Name,
                        TwoLetterIsoCode = x.Address.AddressCountry.TwoLetterIsoCode,
                    }
                },
            });

            var list = new PagedList<Person>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetAllPersonListForDropdown
        public async Task<List<Person>> GetAllPersonListForDropdown(string SiteId)
        {
            var query = _personRepository.TableNoTracking.Where(x => !x.Deleted && x.PersonSitesMapping.Any(x => x.SiteId == SiteId && !x.Deleted && !x.IsSharedUser));
            query = query.OrderBy(m => m.FirstName).Select(x => new Person
            {
                Id = x.Id,
                FullName = x.FirstName + " " + x.LastName,
                PrimaryEmailAddress = x.PrimaryEmailAddress,
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllPersonPrimaryEmailAddressListForDropdown
        //public async Task<List<Person>> GetAllPersonPrimaryEmailAddressListForDropdown(string SiteId)
        //{
        //    var query = _personRepository.TableNoTracking.Where(x => !x.Deleted && !string.IsNullOrWhiteSpace(x.PrimaryEmailAddress) && x.PersonSitesMapping.Any(x => !x.Deleted && x.SiteId == SiteId));
        //    query = query.OrderBy(m => m.PrimaryEmailAddress).Select(x => new Person
        //    {
        //        Id = x.Id,
        //        PrimaryEmailAddress = x.PrimaryEmailAddress,
        //    });

        //    var list = await query.ToListAsync();
        //    return list;
        //}

        public async Task<List<Person>> GetAllPersonPrimaryEmailAddressListForDropdown(string SiteId)
        {
            var list = await _personRepository.TableNoTracking
                .Where(x => !x.Deleted && !string.IsNullOrWhiteSpace(x.PrimaryEmailAddress) && x.PersonSitesMapping.Any(ps => !ps.Deleted && ps.SiteId == SiteId))
                .OrderBy(x => x.PrimaryEmailAddress)
                .ToListAsync();

            return list
                .GroupBy(x => x.PrimaryEmailAddress)
                .Select(g => g.First())
                .Select(x => new Person
                {
                    Id = x.Id,
                    PrimaryEmailAddress = x.PrimaryEmailAddress
                })
                .ToList();
        }
        #endregion

        #region GetAllIsSharedPersonListForDropdown
        public async Task<List<Person>> GetAllIsSharedPersonListForDropdown(string SiteId)
        {
            var query = _personRepository.TableNoTracking.Where(x => !x.Deleted && x.PersonSitesMapping.Any(x => !x.Deleted && x.IsSharedUser && x.SiteId == SiteId));
            query = query.OrderBy(m => m.FirstName).Select(x => new Person
            {
                Id = x.Id,
                FullName = x.FirstName + " " + x.LastName
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetPersonDetailsById
        // Title: GetPersonDetailsById
        // Description: The method selects relevant fields from the person entity, including related entities such as person address, and returns a `Person` object with these details. 
        public async Task<Person> GetPersonDetailsById(string id)
        {
            var query = _personRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new Person
            {
                Id = x.Id,
                FullName = x.FirstName + " " + x.LastName,
                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                LastName = x.LastName,
                PrimaryPhoneNumber = x.PrimaryPhoneNumber,
                PrimaryEmailAddress = x.PrimaryEmailAddress,
                DOB = x.DOB,
                AddressId = x.AddressId,
                GenderId = x.GenderId,
                AddressTypeId = x.AddressTypeId,
                PictureId = x.PictureId,
                IdentifiedDate = x.IdentifiedDate,
                IdentifiedById = x.IdentifiedById,
                IdentificationNote = x.IdentificationNote,
                Relation = x.Relation,
                RelationFullName = x.RelationFullName,
                PhoneNumber = x.PhoneNumber,
                Title = x.Title,
                ProfileLink = x.ProfileLink,
                Color = x.Color,
                BgColor = x.BgColor,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
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
                        ZipCodeLabel = x.Address.AddressCountry.ZipCodeLabel

                    },
                    AddressStateProvince = new StateProvince
                    {
                        Id = x.Address.AddressStateProvince.Id,
                        Name = x.Address.AddressStateProvince.Name,
                    },
                },
                Gender = new DropDown
                {
                    Id = x.Gender.Id,
                    DropDownValue = x.Gender.DropDownValue,
                },
                AddressType = new DropDown
                {
                    Id = x.AddressType.Id,
                    DropDownValue = x.AddressType.DropDownValue,
                },
                IdentifiedBy = new Person
                {
                    Id = x.IdentifiedBy.Id,
                    FullName = x.IdentifiedBy.FirstName + " " + x.IdentifiedBy.LastName,
                },
                Picture = new Picture
                {
                    Id = x.Picture.Id,
                    VirtualPath = x.Picture.VirtualPath,
                    SeoFilename = x.Picture.SeoFilename
                }
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetPersonById
        // Title: GetPersonById
        // Description: This method retrieves a person from the database by its unique identifier (`id`). 
        public async Task<Person> GetPersonById(string id)
        {
            var query = _personRepository.Table.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetPersonByEmail
        // Title: GetProjectByEmail
        // Description: This method retrieves a person based on its name and PrimaryEmailAddress. It allows an optional exclusion of a person by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific person. The method returns the first matching person or null if no match is found.
        public async Task<Person> GetPersonByEmail(string primaryEmailAddress, string id = null, string SiteId = null)
        {
            //var query = _personRepository.TableNoTracking.Where(x => !x.Deleted && x.PrimaryEmailAddress == primaryEmailAddress && x.PersonSitesMapping.Any(x => x.SiteId == SiteId));

            var query = _personRepository.TableNoTracking.Where(x => !x.Deleted && x.PrimaryEmailAddress == primaryEmailAddress);

            if (!string.IsNullOrEmpty(SiteId))
            {
                query = query.Where(x => x.PersonSitesMapping.Any(p => p.SiteId == SiteId && !p.IsSharedUser));
            }

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetPersonByUserEmail
        public async Task<Person> GetPersonByUserEmail(string email, string SiteId = null)
        {
            var query = _userManager.Users.Where(x => !x.Deleted && x.Email == email);

            if (!string.IsNullOrEmpty(SiteId))
            {
                query = query.Where(x => x.Person.PersonSitesMapping.Any(p => p.SiteId == SiteId && !p.IsSharedUser));
            }

            var item = await query.Select(x => x.Person).FirstOrDefaultAsync();
            return item;
        }
        #endregion

        public async Task<Person> GetPersonByColor(string id, string bgColor, string color, string SiteId = null)
        {
            var query = _personRepository.TableNoTracking.Where(x => !x.Deleted && x.BgColor == bgColor && x.Color == color && x.Id != id);
            if (!string.IsNullOrEmpty(SiteId))
            {
                query = query.Where(x => x.PersonSitesMapping.Any(p => p.SiteId == SiteId));
            }
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        #region GetPersonByEmailAddress
        public async Task<Person> GetPersonByEmailAddress(string primaryEmailAddress, string id = null, string SiteId = null)
        {
            var query = _personRepository.TableNoTracking.Where(x => !x.Deleted && x.PrimaryEmailAddress == primaryEmailAddress);

            if (!string.IsNullOrEmpty(SiteId))
            {
                query = query.Where(x => x.PersonSitesMapping.Any(p => p.SiteId == SiteId));
            }

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region PersonSiteMapping

        #region GetPersonSiteMappingByPersonId
        public async Task<PersonSitesMapping> GetPersonSiteMappingByPersonId(string personId, string siteId)
        {
            var query = _personSitesMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.PersonId == personId);

            if (!string.IsNullOrEmpty(siteId))
                query = query.Where(m => m.SiteId == siteId);

            var item = await query.Include(m => m.Person).Include(m => m.Sites).OrderBy(x => x.CreatedOnUtc).FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertPersonSites
        // Title: InsertPersonSites
        // Description: This method inserts a new person site entity into the repository. It takes a person object as input and uses the _personRepository to handle the insertion operation.
        public void InsertPersonSites(PersonSitesMapping entity)
        {
            _personSitesMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdatePersonSites
        // Title: UpdatePersonSites
        // Description: This method updates the specified person site entity in the repository. It takes a person object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdatePersonSites(PersonSitesMapping entity)
        {
            _personSitesMappingRepository.Update(entity);
        }
        #endregion

        #endregion

        #region InsertPerson
        // Title: InsertPerson
        // Description: This method inserts a new person entity into the repository. It takes a person object as input and uses the _personRepository to handle the insertion operation.
        public void InsertPerson(Person entity)
        {
            _personRepository.Insert(entity);
        }
        #endregion

        #region UpdatePerson
        // Title: UpdatePerson
        // Description: This method updates the specified person entity in the repository. It takes a person object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdatePerson(Person entity)
        {
            _personRepository.Update(entity);
        }
        #endregion

        #region DeletePerson
        // Title: DeletePerson
        // Description: Marks the specified person entity as deleted by setting its `Deleted` property to true.
        public void DeletePerson(Person entity)
        {
            entity.Deleted = true;

            _personRepository.Update(entity);
        }
        #endregion
    }
}