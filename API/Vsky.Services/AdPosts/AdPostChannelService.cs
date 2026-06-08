using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.Sites;

namespace Vsky.Services.AdPosts
{
    public class AdPostChannelService : IAdPostChannelService
    {
        #region Define Services
        private readonly IRepository<AdPostChannel> _adPostChannelRepository;
        private readonly ICommonService _commonService;
        //private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations

        public AdPostChannelService(IRepository<AdPostChannel> adPostChannelRepository, ICommonService commonService)
        {
            _adPostChannelRepository = adPostChannelRepository;
            _commonService = commonService;
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

        #region GetAllAdPostChannel
        // Title: GetAllAdPostChannel
        // Description: This method retrieves a paginated list of AdPostChannel based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<AdPostChannel> GetAllAdPostChannel(string SiteId, string SearchText, List<string> projectIds, string name, List<string> customerIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _adPostChannelRepository.TableNoTracking.Where(x => !x.Deleted && x.Project.SiteId == SiteId);

            if (projectIds != null && projectIds.Any())
                query = query.Where(x => projectIds.Contains(x.ProjectId));

            if (customerIds != null && customerIds.Any())
                query = query.Where(x => customerIds.Contains(x.CustomerId));

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(name));
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
                query = query.Where(m =>
                //m.ChannelNumber.ToString().Contains(SearchText) ||
                m.Customer.Company.Name.ToLower().Contains(SearchText.ToLower()) ||
                m.Name.ToLower().Contains(SearchText.ToLower()) ||
                m.GroupMemberCount.ToString().Contains(SearchText)
                );
            }
            query = query.Select(x => new AdPostChannel
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ChannelNumber = x.ChannelNumber,
                GroupMemberCount = x.GroupMemberCount,
                Name = x.Name,
                Description = x.Description,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                Customer = new CompanyClients
                {
                    Id = x.Customer.Id,
                    Name = x.Customer.Company != null ? x.Customer.Company.Name : string.Join(" ", x.Customer.Person.FirstName, x.Customer.Person.LastName).Trim()
                },
            });

            var list = new PagedList<AdPostChannel>(query, page, pageSize);

            return list;
        }
        #endregion

        #region GetAdPostChannelById
        // Title: GetAdPostChannelById
        // Description: This method retrieves a AdPostChannel from the database by its unique identifier (`id`). 
        public async Task<AdPostChannel> GetAdPostChannelById(string id)
        {
            var query = _adPostChannelRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetChannelNumber
        // Title: GetChannelNumber
        // Description: This method retrieves a AdPostChannel from the database. 
        public async Task<AdPostChannel> GetChannelNumber(string SiteId)
        {
            var query = _adPostChannelRepository.TableNoTracking.Where(x => !x.Deleted && x.Project.SiteId == SiteId).OrderByDescending(x => x.ChannelNumber);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAdPostChannelByName
        // Title: GetAdPostChannelByName
        // Description: This method retrieves a AdPostChannel based on its name and Id. It allows an optional exclusion of a AdPostChannel by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific AdPostChannel. The method returns the first matching AdPostChannel or null if no match is found.
        public async Task<AdPostChannel> GetAdPostChannelByName(string name, string ProjectId, string id = null)
        {
            var query = _adPostChannelRepository.TableNoTracking.Where(x => !x.Deleted && x.Name.ToLower() == name.ToLower() && x.ProjectId == ProjectId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion

        #region GetAllAdPostChannelsListForDropdown
        public async Task<List<CommonDropDown>> GetAllAdPostChannelsListForDropdown(string SiteId)
        {
            var query = _adPostChannelRepository.TableNoTracking.Where(m => !m.Deleted && m.Project.SiteId == SiteId);
            var list = await query
                .OrderBy(x => x.Name)
                .Select(x => new CommonDropDown
                {
                    Value = x.Id,
                    Text = x.Name,
                })
            .ToListAsync();
            return list;
        }
        #endregion

        #region GetAdPostChannelDetailsById
        // Title: GetAdPostChannelDetailsById
        // Description: The method selects relevant fields from the AdPostChannel entity.
        public async Task<AdPostChannel> GetAdPostChannelDetailsById(string id)
        {
            var query = _adPostChannelRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            query = query.Select(x => new AdPostChannel
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                CustomerId = x.CustomerId,
                ChannelNumber = x.ChannelNumber,
                GroupMemberCount = x.GroupMemberCount,
                Name = x.Name,
                Description = x.Description,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Project = new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },
                Customer = new CompanyClients
                {
                    Id = x.Customer.Id,
                    Name = x.Customer.Company != null ? x.Customer.Company.Name : string.Join(" ", x.Customer.Person.FirstName, x.Customer.Person.LastName).Trim()
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
                },
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertAdPostChannel
        // Title: InsertAdPostChannel
        // Description: This method inserts a new AdPostChannel entity into the repository. It takes a AdPostChannel object as input and uses the _adPostChannelRepository to handle the insertion operation.
        public void InsertAdPostChannel(AdPostChannel entity)
        {
            _adPostChannelRepository.Insert(entity);
        }
        #endregion

        #region UpdateAdPostChannel
        // Title: UpdateAdPostChannel
        // Description: This method updates the specified AdPostChannel entity in the repository. It takes a AdPostChannel object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateAdPostChannel(AdPostChannel entity)
        {
            _adPostChannelRepository.Update(entity);
        }
        #endregion

        #region DeleteAdPostChannel
        // Title: DeleteAdPostChannel
        // Description: Marks the specified AdPostChannel entity as deleted by setting its `Deleted` property to true. 
        public void DeleteAdPostChannel(AdPostChannel entity)
        {
            entity.Deleted = true;

            _adPostChannelRepository.Update(entity);
        }
        #endregion
    }
}

