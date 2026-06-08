using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.JobsCreate
{
    public class JobCreateService : IJobCreateService
    {
        #region Define Services
        private readonly IRepository<JobCreate> _jobCreateRepository;
        #endregion

        #region Services Initializations
        public JobCreateService(IRepository<JobCreate> jobCreateRepository)
        {
            _jobCreateRepository = jobCreateRepository;
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

        #region GetAllJobPosts
        // Title: GetAllJobPosts
        // Description: This method retrieves a paginated list of JobCreate based on various search criteria such as job name, 
        // project status, and team member. It also supports sorting and includes related data such as job status and employee 
        // mappings. The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<JobCreate> GetAllJobPosts(string SiteId, string SearchText, string jobTitle, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {

            var query = _jobCreateRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(jobTitle))
            {
                jobTitle = jobTitle.Trim().ToLower();
                query = query.Where(x => x.JobTitle.ToLower().Contains(jobTitle));
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
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                 m.JobTitle.ToLower().Contains(SearchText.ToLower()) ||
                 m.Criteria.ToLower().Contains(SearchText.ToLower()) ||
                 m.PublishedJobDate.Date == parsedDate.Date ||
                 m.JobReference.ToLower().Contains(SearchText.ToLower())
                );
            }
             query = query.Select(x => new JobCreate
            {
                Id = x.Id,
                //SiteId = x.SiteId,
                JobTitle = x.JobTitle,
                JobDescription = x.JobDescription,
                Criteria = x.Criteria,
                JobCreatedDate = x.JobCreatedDate,
                PublishedJobDate = x.PublishedJobDate,
                JobReference = x.JobReference,
                IsActive = x.IsActive,
            });

            var list = new PagedList<JobCreate>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetJobPostById
        // Title: GetJobPostById
        // Description: This method retrieves a JobCreate from the database by its unique identifier (`id`). 
        public async Task<JobCreate> GetJobPostById(string id)
        {
            var query = _jobCreateRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllJobPostListForDropdown
        public async Task<List<CommonDropDown>> GetAllJobPostListForDropdown(string SiteId)
        {
            var query = _jobCreateRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            var list = await query
                  .OrderBy(x => x.JobTitle)
                  .Select(x => new CommonDropDown
                  {
                      Value = x.Id,
                      Text = x.JobTitle,
                  }).ToListAsync();

            return list;
        }
        #endregion

        #region GetJobPostDetailsById
        // Title: GetJobPostDetailsById
        // Description: The method selects relevant fields from the JobCreate entity, including related entities such as nd employee mappings, and returns a `JobCreate` object with these details. 
        public async Task<JobCreate> GetJobPostDetailsById(string id)
        {
            var query = _jobCreateRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new JobCreate
            {
                Id = x.Id,
                //SiteId = x.SiteId,
                JobTitle = x.JobTitle,
                JobDescription = x.JobDescription,
                Criteria = x.Criteria,
                JobCreatedDate = x.JobCreatedDate,
                PublishedJobDate = x.PublishedJobDate,
                JobReference = x.JobReference,
                IsActive = x.IsActive
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetJobPostByTitle
        // Title: GetJobPostByTitle
        // Description: This method retrieves a JobCreate based on its title.It allows an optional exclusion of a JobCreate by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific JobCreate. The method returns the first matching JobCreate or null if no match is found.
        public async Task<JobCreate> GetJobPostByTitle(string SiteId, string title, string id = null)
        {
            var query = _jobCreateRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.JobTitle.ToLower() == title.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertJobPost
        // Title: InsertJobPost
        // Description: This method inserts a new JobCreate entity into the repository. It takes a JobCreate object as input and uses the _jobCreateRepository to handle the insertion operation.
        public void InsertJobPost(JobCreate entity)
        {
            _jobCreateRepository.Insert(entity);
        }
        #endregion

        #region UpdateJobPost
        // Title: UpdateJobPost
        // Description: This method updates the specified JobCreate entity in the repository. It takes a JobCreate object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateJobPost(JobCreate entity)
        {
            _jobCreateRepository.Update(entity);
        }
        #endregion

        #region DeleteJobPost
        // Title: DeleteJobPost
        // Description: Marks the specified JobCreate entity as deleted by setting its `Deleted` property to true. 
        public void DeleteJobPost(JobCreate entity)
        {
            entity.Deleted = true;
            _jobCreateRepository.Update(entity);
        }
        #endregion
    }
}
