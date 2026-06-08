using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Vsky.Services.Sites;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
namespace Vsky.Services.Domains
{
    public class DomainService : IDomainService
    {
        #region Services Intialization

        private readonly IRepository<Domain> _domainRepository;
        private readonly IRepository<Notes> _notesRepository;
        public DomainService(IRepository<Domain> domainRepository, IRepository<Notes> notesRepository)
        {
            _domainRepository = domainRepository;
            _notesRepository = notesRepository;
        }
        #endregion

        #region GetOrderBy
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region GetAllDomains
        public IPagedList<Domain> GetAllDomains(string SiteId, string SearchText, List<string> projectIds, string url, List<string> domainTypeIds, List<string> domainServerIds, List<string> hostingServerIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _domainRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (projectIds != null && projectIds.Any())
                query = query.Where(x => projectIds.Contains(x.ProjectId));

            if (domainTypeIds != null && domainTypeIds.Any())
                query = query.Where(x => domainTypeIds.Contains(x.DomainTypeId));

            if (domainServerIds != null && domainServerIds.Any())
                query = query.Where(x => domainServerIds.Contains(x.DomainServerId));

            if (hostingServerIds != null && hostingServerIds.Any())
                query = query.Where(x => hostingServerIds.Contains(x.HostingServerId));

            if (!string.IsNullOrWhiteSpace(url))
                query = query.Where(x => x.Url.Contains(url));
            // query = query.Where(x => x.Url.ToLower() == url.ToLower());

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                    m.Project.Name.ToLower().Contains(SearchText.ToLower()) ||
                    m.Url.ToLower().Contains(SearchText.ToLower()) ||
                    m.DomainType.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.DomainServer.Provider.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.HostingServer.Provider.DropDownValue.ToLower().Contains(SearchText.ToLower())
                );
            }
             query = query.Select(x => new Domain
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                DomainServerId = x.DomainServerId,
                HostingServerId = x.HostingServerId,
                DomainTypeId = x.DomainTypeId,
                DomainMappingId = x.DomainMappingId,
                Url = x.Url,
                ExternalMappingNote = x.ExternalMappingNote,
                DatabaseName = x.DatabaseName,
                DatabaseHostname = x.DatabaseHostname,
                DatabaseUsername = x.DatabaseUsername,
                DatabasePassword = x.DatabasePassword,
                FtpUsername = x.FtpUsername,
                FtpPassword = x.FtpPassword,
                FtpHostname = x.FtpHostname,
                FtpPort = x.FtpPort,
                WebsiteLoginUrl = x.WebsiteLoginUrl,
                WebsiteLoginId = x.WebsiteLoginId,
                WebsiteLoginPassword = x.WebsiteLoginPassword,
                Instructions = x.Instructions,
                Notes = x.Notes,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                },
                DomainServer = new Server
                {
                    Id = x.DomainServer.Id,
                    Provider = new DropDown
                    {
                        Id = x.DomainServer.Provider.Id,
                        DropDownValue = x.DomainServer.Provider.DropDownValue,
                    },
                    CustomerId = x.DomainServer.CustomerId,
                    ContractId = x.DomainServer.ContractId,
                },
                HostingServer = new Server
                {
                    Id = x.HostingServer.Id,
                    Provider = new DropDown
                    {
                        Id = x.HostingServer.Provider.Id,
                        DropDownValue = x.HostingServer.Provider.DropDownValue,
                    },
                    CustomerId = x.HostingServer.CustomerId,
                    ContractId = x.HostingServer.ContractId,
                },
                DomainType = new DropDown { 
                    Id = x.DomainType.Id,
                    DropDownValue = x.DomainType.DropDownValue, 
                },
                DomainMapping = new DropDown
                {
                    Id = x.DomainMapping.Id,
                    DropDownValue = x.DomainMapping.DropDownValue,
                },
                DomainAttributes = x.DomainAttributes.Where(m => !m.Deleted).Select(b => new DomainAttributes
                {
                    Id = b.Id,
                    DomainId = b.DomainId,
                    DomainAttributeId = b.DomainAttributeId,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    Amount = b.Amount,
                    Duration = b.Duration,
                    Notes = b.Notes,
                    CreatedById = b.CreatedById,
                    CreatedOnUtc = b.CreatedOnUtc,
                }).ToList(),
                DomainNotesCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Domain").Count(),
            });
            var list = new PagedList<Domain>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetById
        public async Task<Domain> GetById(string id)
        {
            var query = _domainRepository.TableNoTracking.Where(x => x.Id == id && !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetDomainDetailsById
        public async Task<Domain> GetDomainDetailsById(string id)
        {
            var query = _domainRepository.TableNoTracking.Where(x => x.Id == id && !x.Deleted);
            query = query.Select(x => new Domain
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                DomainServerId = x.DomainServerId,
                HostingServerId = x.HostingServerId,
                DomainTypeId = x.DomainTypeId,
                DomainMappingId = x.DomainMappingId,
                Url = x.Url,
                ExternalMappingNote = x.ExternalMappingNote,
                DatabaseName = x.DatabaseName,
                DatabaseHostname = x.DatabaseHostname,
                DatabaseUsername = x.DatabaseUsername,
                DatabasePassword = x.DatabasePassword,
                FtpUsername = x.FtpUsername,
                FtpPassword = x.FtpPassword,
                FtpHostname = x.FtpHostname,
                FtpPort = x.FtpPort,
                WebsiteLoginUrl = x.WebsiteLoginUrl,
                WebsiteLoginId = x.WebsiteLoginId,
                WebsiteLoginPassword = x.WebsiteLoginPassword,
                Instructions = x.Instructions,
                Notes = x.Notes,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                },
                DomainServer = new Server
                {
                    Id = x.DomainServer.Id,
                    Provider = new DropDown
                    {
                        Id = x.DomainServer.Provider.Id,
                        DropDownValue = x.DomainServer.Provider.DropDownValue,
                    },
                    CustomerId = x.DomainServer.CustomerId,
                    ContractId = x.DomainServer.ContractId,
                },
                HostingServer = new Server
                {
                    Id = x.HostingServer.Id,
                    Provider = new DropDown
                    {
                        Id = x.HostingServer.Provider.Id,
                        DropDownValue = x.HostingServer.Provider.DropDownValue,
                    },
                    CustomerId = x.HostingServer.CustomerId,
                    ContractId = x.HostingServer.ContractId,
                },
                DomainType = new DropDown
                {
                    Id = x.DomainType.Id,
                    DropDownValue = x.DomainType.DropDownValue,
                },
                DomainMapping = new DropDown
                {
                    Id = x.DomainMapping.Id,
                    DropDownValue = x.DomainMapping.DropDownValue,
                },
                DomainAttributes = x.DomainAttributes.Where(m => !m.Deleted).Select( b=> new DomainAttributes { 
                    Id = b.Id,
                    DomainId = b.DomainId,
                    DomainAttributeId = b.DomainAttributeId,

                    DomainAttribute = new DropDown
                    {
                        Id = b.DomainAttribute.Id,
                        DropDownValue = b.DomainAttribute.DropDownValue,
                    },
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    Amount = b.Amount,
                    Duration = b.Duration,
                    Notes = b.Notes,
                    CreatedById = b.CreatedById,
                    CreatedOnUtc = b.CreatedOnUtc,
                }).ToList()
            });

            return await query.FirstOrDefaultAsync();
        }
        #endregion

        #region InsertDomain
        public void InsertDomain(Domain entity)
        {
            _domainRepository.Insert(entity);
        }
        #endregion

        #region UpdateDomain
        public void UpdateDomain(Domain entity)
        {
            _domainRepository.Update(entity);
        }
        #endregion

        #region DeleteDomain
        public void DeleteDomain(Domain entity)
        {
            entity.Deleted = true;

            _domainRepository.Update(entity);
        }
        #endregion

    }
}
