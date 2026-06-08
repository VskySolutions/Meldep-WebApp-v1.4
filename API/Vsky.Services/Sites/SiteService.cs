using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Sites
{
    public class SiteService : ISiteService
    {
        #region Define Repositories
        private readonly IRepository<Site> _siteRepository;
        #endregion

        #region Repositories Initialization
        public SiteService(IRepository<Site> siteRepository)
        {
            _siteRepository = siteRepository;
        }
        #endregion

        #region Common order by
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllSitesList
        public async Task<IList<Site>> GetAllSitesList()
        {
            var sites = await _siteRepository.TableNoTracking.Where(x => !x.Deleted).ToListAsync();
            return sites;
        }
        #endregion

        #region Get all sites
        public IPagedList<Site> GetAllSites(string SearchText, string name, string fullName, string emailAddress, string siteStatus, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _siteRepository.TableNoTracking.Where(x => !x.Deleted);

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                fullName = fullName.Trim().ToLower();
                query = query.Where(x => (x.Person.FirstName.ToLower() + " " + x.Person.LastName.ToLower()).Contains(fullName));
            }

            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                emailAddress = emailAddress.Trim().ToLower();
                query = query.Where(x => x.Person.PrimaryEmailAddress.ToLower().Contains(emailAddress));
            }

            if (!string.IsNullOrWhiteSpace(siteStatus))
                query = query.Where(x => siteStatus == "Active" ? x.Active : !x.Active);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                string orderBy;
                if (sortBy == "person.fullName")
                {
                    orderBy = $"{GetOrderBy("Person.FirstName")} {(descending ? "desc" : "asc")}, {GetOrderBy("Person.LastName")} {(descending ? "desc" : "asc")}";
                }
                else
                {
                    orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                }
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.UpdatedOnUtc);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                    m.Name.ToLower().Contains(SearchText.ToLower()) ||
                    (m.Person.FirstName + " " + m.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    m.Person.PrimaryPhoneNumber.ToLower().Contains(SearchText.ToLower()) ||
                    m.Person.PrimaryEmailAddress.ToLower().Contains(SearchText.ToLower())
                );
            }
            query = query.Select(x => new Site
            {
                Id = x.Id,
                Name = x.Name,
                Active = x.Active,
                IsDropdownGenerated = x.IsDropdownGenerated,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress,
                    PrimaryPhoneNumber = x.Person.PrimaryPhoneNumber,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                },
            });

            var list = new PagedList<Site>(query, page, pageSize);
            return list;
        }
        #endregion

        #region Find site by id
        public async Task<Site> GetById(string id)
        {
            var query = _siteRepository.TableNoTracking.Where(x => !x.Deleted);
            query = query.Where(x => x.Id == id && !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<Site> GetSiteDetailsById(string id)
        {
            var query = _siteRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            query = query.Select(x => new Site
            {
                Id = x.Id,
                Name = x.Name,
                PersonId = x.PersonId,
                Active = x.Active,
                UserName = x.UserName,
                SiteLogoId = x.SiteLogoId,
                SiteLogoPath = x.SiteLogoPath,
                SiteFaviconId = x.SiteFaviconId,
                SiteFaviconPath = x.SiteFaviconPath,
                TicketNoPrefix = x.TicketNoPrefix,
                TicketGenerationEmail = x.TicketGenerationEmail,
                TimeZone = x.TimeZone,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    PrimaryPhoneNumber = x.Person.PrimaryPhoneNumber,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress,
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
                SitesRoles = x.SitesRoles.Where(mapping => !mapping.Deleted && !mapping.ApplicationRole.Deleted).Select(mapping => new SitesRoles
                {
                    Id = mapping.Id,
                    SiteId = mapping.SiteId,
                    RoleId = mapping.RoleId,
                    Deleted = mapping.Deleted,
                    ApplicationRole = new ApplicationRole
                    {
                        Id = mapping.ApplicationRole.Id,
                        Name = mapping.ApplicationRole.Name
                    },
                }).ToList()
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<string> GetSiteIdFromTicketGenerationEmail(string Email)
        {
            var SiteData = await _siteRepository.TableNoTracking.Where(x => !x.Deleted && x.Active && x.TicketGenerationEmail == Email).FirstOrDefaultAsync();
            return SiteData != null ? SiteData.Id : "";
        }

        public async Task<Site> GetSiteTicketNoPrefixById(string siteId)
        {
            return await _siteRepository.TableNoTracking.Where(x => x.Id == siteId)
                .Select(x => new Site
                {
                    Name = x.Name,
                    TicketNoPrefix = x.TicketNoPrefix
                })
                .FirstOrDefaultAsync();
        }

        #endregion

        #region Find site by name
        public async Task<Site> GetBySiteName(string name, string id = null)
        {
            var query = _siteRepository.TableNoTracking.Where(x => !x.Deleted && x.Name.ToLower() == name.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region Find Site Name By Id        
        public async Task<Site> GetSiteNameById(string id)
        {
            var query = _siteRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region Insert site
        public void InsertSite(Site entity)
        {
            _siteRepository.Insert(entity);
        }
        #endregion

        #region Update site
        public void UpdateSite(Site entity)
        {
            _siteRepository.Update(entity);
        }
        #endregion

        #region Delete site
        public void DeleteSite(Site entity)
        {
            entity.Deleted = true;
            entity.Active = false;

            _siteRepository.Update(entity);
        }
        #endregion

        #region GetSiteDetails
        public Site GetSiteDetails()
        {
            return _siteRepository.TableNoTracking.Where(m => m.Deleted != true && m.Active == true).FirstOrDefault();
        }
        public DateTime GetDateTime(string siteTimeZone = "India Standard Time")
        {
            return DateTime.UtcNow;
            //try
            //{
            //    DateTime utcTime = DateTime.UtcNow;
            //    TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(siteTimeZone);
            //    return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
            //}
            //catch
            //{
            //    return DateTime.UtcNow;
            //}
        }
        public string GetTimeZoneFromSiteId(string Id)
        {
            var SiteData = _siteRepository.TableNoTracking.FirstOrDefault(m => m.Id == Id);
            return SiteData != null ? SiteData.TimeZone : "India Standard Time";
        }


        #endregion

        #region GetVskySiteId
        public string GetVskySiteId()
        {
            string siteId = null;
            var site = _siteRepository.TableNoTracking.Where(x => !x.Deleted && x.Active && x.Name == "Vsky Solutions").FirstOrDefault();
            if (site != null)
            {
                siteId = site.Id;
            }
            return siteId;
        }
        #endregion
    }
}