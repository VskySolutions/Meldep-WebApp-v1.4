using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.AdPosts
{
    public class AdPostService : IAdPostService
    {
        #region Define Services
        private readonly IRepository<AdPost> _adPostRepository;
        #endregion

        #region Services Initializations

        public AdPostService(IRepository<AdPost> adPostRepository)
        {
            _adPostRepository = adPostRepository;
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

        #region GetAllAdPosts
        // Title: GetAllAdPosts
        // Description: This method retrieves a paginated list of AdPost based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<AdPost> GetAllAdPosts(string SiteId, string SearchText, List<string> projectIds, string name, List<string> customerIds, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _adPostRepository.TableNoTracking.Where(x => !x.Deleted && x.Project.SiteId == SiteId);

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
                      m.AdNumber.ToString().Contains(SearchText) ||
                      m.Name.ToLower().Contains(SearchText.ToLower()) ||
                      m.Project.Name.ToLower().Contains(SearchText.ToLower()) ||
                      m.Customer.Company.Name.ToLower().Contains(SearchText.ToLower()) ||
                      (m.ImageProviderClient.FirstName + " " + m.ImageProviderClient.LastName).ToLower().Contains(SearchText.ToLower()) ||
                      (m.ContentProviderEmp.Person.FirstName + " " + m.ContentProviderEmp.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                      m.Caption.ToLower().Contains(SearchText.ToLower()) ||
                      m.Tags.ToLower().Contains(SearchText.ToLower())
                );
            }
            query = query.Select(x => new AdPost
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                AdNumber = x.AdNumber,
                URL = x.URL,
                Caption = x.Caption,
                Tags = x.Tags,
                Name = x.Name,
                Description = x.Description,
                Picture = new Picture
                {
                    Id = x.Picture.Id,
                    VirtualPath = x.Picture.VirtualPath,
                    SeoFilename = x.Picture.SeoFilename
                },
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
                ImageTypeDropDown = new DropDown
                {
                    Id = x.ImageTypeDropDown.Id,
                    DropDownValue = x.ImageTypeDropDown.DropDownValue
                },
                ImageProviderClient = new Person
                {
                    Id = x.ImageProviderClient.Id,
                    FullName = x.ImageProviderClient.FirstName + " " + x.ImageProviderClient.LastName,
                },
                ImageProviderEmp = new Employee
                {
                    Person = new Person
                    {
                        Id = x.ImageProviderEmp.Person.Id,
                        FullName = x.ImageProviderEmp.Person.FirstName + " " + x.ImageProviderEmp.Person.LastName,
                    }
                },
                ContentTypeDropDown = new DropDown
                {
                    Id = x.ContentTypeDropDown.Id,
                    DropDownValue = x.ContentTypeDropDown.DropDownValue
                },
                ContentProviderClient = new Person
                {
                    Id = x.ContentProviderClient.Id,
                    FullName = x.ContentProviderClient.FirstName + " " + x.ContentProviderClient.LastName,
                },
                ContentProviderEmp = new Employee
                {
                    Person = new Person
                    {
                        Id = x.ContentProviderEmp.Person.Id,
                        FullName = x.ContentProviderEmp.Person.FirstName + " " + x.ContentProviderEmp.Person.LastName,
                    }
                },
            });

            var list = new PagedList<AdPost>(query, page, pageSize);

            return list;
        }
        #endregion

        #region GetAdPostById
        // Title: GetAdPostById
        // Description: This method retrieves a AdPost from the database by its unique identifier (`id`). 
        public async Task<AdPost> GetAdPostById(string id)
        {
            var query = _adPostRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAdPostNumber
        // Title: GetAdPostNumber
        // Description: This method retrieves a AdPost from the database. 
        public async Task<AdPost> GetAdPostNumber(string SiteId)
        {
            var query = _adPostRepository.TableNoTracking.Where(x => !x.Deleted && x.Project.SiteId == SiteId).OrderByDescending(x => x.AdNumber);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAdPostByName
        // Title: GetAdPostByName
        // Description: This method retrieves a AdPost based on its name and Id. It allows an optional exclusion of a AdPost by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific AdPost. The method returns the first matching AdPost or null if no match is found.
        public async Task<AdPost> GetAdPostByName(string name, string ProjectId, string id = null)
        {
            var query = _adPostRepository.TableNoTracking.Where(x => !x.Deleted && x.Name.ToLower() == name.ToLower() && x.ProjectId == ProjectId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion

        #region GetAdPostDetailsById
        // Title: GetAdPostDetailsById
        // Description: The method selects relevant fields from the AdPost entity.
        public async Task<AdPost> GetAdPostDetailsById(string id)
        {
            var query = _adPostRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            query = query.Select(x => new AdPost
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                CustomerId = x.CustomerId,
                ImageType = x.ImageType,
                ImageProviderClientId = x.ImageProviderClientId,
                ImageProviderEmpId = x.ImageProviderEmpId,
                ContentType = x.ContentType,
                ContentProviderClientId = x.ContentProviderClientId,
                ContentProviderEmpId = x.ContentProviderEmpId,
                PictureId = x.PictureId,
                AdNumber = x.AdNumber,
                URL = x.URL,
                Caption = x.Caption,
                Tags = x.Tags,
                Name = x.Name,
                Description = x.Description,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Picture = new Picture
                {
                    Id = x.Picture.Id,
                    VirtualPath = x.Picture.VirtualPath,
                    SeoFilename = x.Picture.SeoFilename
                },
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
                ImageTypeDropDown = new DropDown
                {
                    Id = x.ImageTypeDropDown.Id,
                    DropDownValue = x.ImageTypeDropDown.DropDownValue
                },
                ImageProviderClient = new Person
                {
                    Id = x.ImageProviderClient.Id,
                    FullName = x.ImageProviderClient.FirstName + " " + x.ImageProviderClient.LastName,
                },
                ImageProviderEmp = new Employee
                {
                    Person = new Person
                    {
                        Id = x.ImageProviderEmp.Person.Id,
                        FullName = x.ImageProviderEmp.Person.FirstName + " " + x.ImageProviderEmp.Person.LastName,
                    }
                },
                ContentTypeDropDown = new DropDown
                {
                    Id = x.ContentTypeDropDown.Id,
                    DropDownValue = x.ContentTypeDropDown.DropDownValue
                },
                ContentProviderClient = new Person
                {
                    Id = x.ContentProviderClient.Id,
                    FullName = x.ContentProviderClient.FirstName + " " + x.ContentProviderClient.LastName,
                },
                ContentProviderEmp = new Employee
                {
                    Person = new Person
                    {
                        Id = x.ContentProviderEmp.Person.Id,
                        FullName = x.ContentProviderEmp.Person.FirstName + " " + x.ContentProviderEmp.Person.LastName,
                    }
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
                AdPostingStatusList = x.AdPostingStatusList.Where(m => !m.Deleted).Select(status => new AdPostingStatus
                {
                    Id = status.Id,
                    Likes = status.Likes,
                    Comments = status.Comments,
                    Shares = status.Shares,
                    Date = status.Date,
                    CreatedOnUtc = status.CreatedOnUtc,
                    AdPostChannel = new AdPostChannel
                    {
                        Id = status.AdPostChannel.Id,
                        Name = status.AdPostChannel.Name
                    }
                }).ToList(),
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertAdPost
        // Title: InsertAdPost
        // Description: This method inserts a new AdPost entity into the repository. It takes a AdPost object as input and uses the _adPostRepository to handle the insertion operation.
        public void InsertAdPost(AdPost entity)
        {
            _adPostRepository.Insert(entity);
        }
        #endregion

        #region UpdateAdPost
        // Title: UpdateAdPost
        // Description: This method updates the specified AdPost entity in the repository. It takes a AdPost object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateAdPost(AdPost entity)
        {
            _adPostRepository.Update(entity);
        }
        #endregion

        #region DeleteAdPost
        // Title: DeleteAdPost
        // Description: Marks the specified AdPost entity as deleted by setting its `Deleted` property to true. 
        public void DeleteAdPost(AdPost entity)
        {
            entity.Deleted = true;

            _adPostRepository.Update(entity);
        }
        #endregion
    }
}
