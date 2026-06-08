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
using Vsky.Services.Users;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Services.Persons
{
    public class PersonSitesMappingService : IPersonSitesMappingService
    {
        #region Define Services
        private readonly IRepository<Person> _personRepository;
        private readonly IRepository<PersonSitesMapping> _personSitesMappingRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Site> _siteRepository;
        #endregion

        #region Services Initializations
        public PersonSitesMappingService
        (
            IRepository<Person> personRepository,
            IRepository<PersonSitesMapping> personSitesMappingRepository,
            UserManager<ApplicationUser> userManager,
            IRepository<Site> siteRepository
        )
        {
            _personRepository = personRepository;
            _personSitesMappingRepository = personSitesMappingRepository;
            _userManager = userManager;
            _siteRepository = siteRepository;
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
        public IPagedList<PersonSitesMapping> GetAllSiteShare(
            string SiteId,
            string SearchText,
            List<string> personIds,
            string primaryEmailAddress,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _personSitesMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.IsSharedUser && x.SiteId == SiteId);

            if (personIds != null && personIds.Any())
                query = query.Where(x => personIds.Contains(x.PersonId));

            if (!string.IsNullOrWhiteSpace(primaryEmailAddress))
            {
                primaryEmailAddress = primaryEmailAddress.Trim().ToLower(); // Normalize input
                query = query.Where(x => x.Person.PrimaryEmailAddress.ToLower().Contains(primaryEmailAddress)); // Partial match for the name
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(p =>
                    (p.Person.FirstName + " " + p.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    p.Person.PrimaryEmailAddress.ToLower().Contains(SearchText.ToLower())
                    //p.CreatedOnUtc.Value.Date == parsedDate
                );
            }
            query = query.Select(x => new PersonSitesMapping
            {
                Id = x.Id,
                PersonId = x.PersonId,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress
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
                }
            });

            var list = new PagedList<PersonSitesMapping>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetAllSharedSitesByLoggedUserId
        public async Task<IList<PersonSitesMapping>> GetAllSharedSitesByLoggedUserId(string LoggedUserId, string SiteId)
        {
            //Get PersonId from UserId
            var userdata = _userManager.FindByIdAsync(LoggedUserId).GetAwaiter().GetResult();
            var personId = userdata.PersonId;

            // Get shared sites using PersonId
            var query = _personSitesMappingRepository.TableNoTracking
                //.Where(x => !x.Deleted &&
                //        (
                //            (x.IsSharedUser && x.PersonId == personId)
                //            || x.SiteId == SiteId && x.PersonId == personId
                //        ))
                .Where(x => (!x.Deleted && x.PersonId == personId)
                      )
                .Select(x => new PersonSitesMapping
                {
                    Id = x.Id,
                    SiteId = x.SiteId,
                    CreatedOnUtc = x.CreatedOnUtc,
                    Sites = new Site
                    {
                        Id = x.Sites.Id,
                        Name = x.Sites.Name
                    }
                });

            return await query.OrderBy(x => x.CreatedOnUtc).ToListAsync();
        }
        #endregion

        #region GetAllSitesByPersonId
        public async Task<IList<PersonSitesMapping>> GetAllSitesByPersonId(string personId)
        {
            return await _personSitesMappingRepository.Table
                .Where(x => !x.Deleted && x.PersonId == personId)
                .ToListAsync();
        }
        #endregion

        #region Find by id
        public async Task<PersonSitesMapping> GetById(string id)
        {
            var query = _personSitesMappingRepository.TableNoTracking.Where(x => !x.Deleted);

            query = query.Where(x => x.Id == id && !x.Deleted);
            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion

        #region GetPersonInSite
        public async Task<bool> GetPersonInSite(string personId, string siteId)
        {
            return await _personSitesMappingRepository.TableNoTracking
                .AnyAsync(x => x.PersonId == personId
                            && x.SiteId == siteId
                            && !x.Deleted);
        }
        #endregion

        #region GetPersonInOtherSite
        public async Task<bool> GetPersonInOtherSite(string personId, string siteId)
        {
            return await _personSitesMappingRepository.TableNoTracking
                .AnyAsync(x => x.PersonId == personId
                            && x.SiteId != siteId
                            && !x.Deleted);
        }
        #endregion

        #region GetPersonSiteMappingByPersonId
        public async Task<PersonSitesMapping> GetPersonSiteMappingByPersonId(string personId, string siteId)
        {
            var query = _personSitesMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.PersonId == personId);

            if (!string.IsNullOrEmpty(siteId))
                query = query.Where(m => m.SiteId == siteId);

            var item = await query.Include(m => m.Person).Include(m => m.Sites).FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region Find InvitedBy from createdById        
        public async Task<PersonSitesMapping> GetInvitedByCreatedById(string createdById)
        {
            var query = _personSitesMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.CreatedById == createdById)
                .Select(x => new PersonSitesMapping
                {
                    Id = x.Id,
                    CreatedById = x.CreatedById,
                    CreatedBy = new ApplicationUser
                    {
                        Id = x.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = x.CreatedBy.PersonId,
                            FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                        }
                    }
                });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertPersonSites
        public async Task<PersonSitesMapping> GetLastUsedSiteByPersonId(string personId)
        {
            return await _personSitesMappingRepository.TableNoTracking
                .Include(x => x.Sites)
                .FirstOrDefaultAsync(x =>
                    x.PersonId == personId &&
                    x.LastUsed &&
                    !x.Deleted);
        }
        #endregion

        #region InsertPersonSites
        public void InsertPersonSites(PersonSitesMapping entity)
        {
            _personSitesMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdatePersonSites
        public void UpdatePersonSites(PersonSitesMapping entity)
        {
            _personSitesMappingRepository.Update(entity);
        }
        #endregion

        #region DeletePersonSites
        public void DeletePersonSites(PersonSitesMapping entity)
        {
            entity.Deleted = true;
            entity.IsSharedUser = false;

            _personSitesMappingRepository.Update(entity);
        }
        #endregion
    }
}