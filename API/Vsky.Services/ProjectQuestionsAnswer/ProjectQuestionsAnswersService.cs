using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;

namespace Vsky.Services.ProjectQuestionsAnswer
{
    public class ProjectQuestionsAnswersService : IProjectQuestionsAnswersService
    {
        #region Define services
        private readonly IRepository<ProjectQuestionsAnswers> _projectQuestionsAnswersRepository;
        private readonly ICommonService _commonService;
        public ProjectQuestionsAnswersService(IRepository<ProjectQuestionsAnswers> projectQuestionsAnswersRepository,
            ICommonService commonService
        )
        {
            _projectQuestionsAnswersRepository = projectQuestionsAnswersRepository;
            _commonService = commonService;
        }
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region List
        public IPagedList<Vsky.Models.ProjectQuestionsAnswers> GetAllProjectQuestionsAnswers(
            string siteId,
            string searchText,
            string title,
            List<string> projectIds,
            List<string> requirementIds,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue
        )
        {
            var query = _projectQuestionsAnswersRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);

            if (!string.IsNullOrEmpty(title))
                query = query.Where(x => x.Title.ToLower().Contains(title));

            if (projectIds?.Any() == true) query = query.Where(x => projectIds.Contains(x.ProjectId));
            if (requirementIds?.Any() == true) query = query.Where(x => requirementIds.Contains(x.RequirementId));

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.Trim().ToLower();

                query = query.Where(m =>
                    (m.Title ?? "").ToLower().Contains(searchText) ||
                    (m.Project != null && (m.Project.Name ?? "").ToLower().Contains(searchText)) ||
                    (m.Requirement != null && (m.Requirement.Title ?? "").ToLower().Contains(searchText)) ||

                    // First Name
                    (m.UpdatedBy != null &&
                     m.UpdatedBy.Person != null &&
                     (m.UpdatedBy.Person.FirstName ?? "").ToLower().Contains(searchText)) ||

                    // Last Name
                    (m.UpdatedBy != null &&
                     m.UpdatedBy.Person != null &&
                     (m.UpdatedBy.Person.LastName ?? "").ToLower().Contains(searchText)) ||

                    // Full Name
                    (m.UpdatedBy != null &&
                     m.UpdatedBy.Person != null &&
                     (
                        ((m.UpdatedBy.Person.FirstName ?? "") + " " + (m.UpdatedBy.Person.LastName ?? ""))
                            .ToLower()
                            .Contains(searchText)
                     ))
                );
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);


            // Apply multi-level dictionary sorting
            if (sorts != null && sorts.Count > 0)
            {
                query = _commonService.ApplySorting(query, sorts);
            }

            var result = query.Select(x => new ProjectQuestionsAnswers
            {
                Id = x.Id,
                Title = x.Title,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,

                Project = x.Project == null ? null : new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },

                Requirement = x.Requirement == null ? null : new Requirement
                {
                    Id = x.Requirement.Id,
                    Title = x.Requirement.Title
                },

                CreatedBy = x.CreatedBy == null ? null : new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = x.CreatedBy.Person == null ? null : new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FullName = (x.CreatedBy.Person.FirstName ?? "") + " " +
                       (x.CreatedBy.Person.LastName ?? "")
                    }
                },

                UpdatedBy = x.UpdatedBy == null ? null : new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = x.UpdatedBy.Person == null ? null : new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FullName = (x.UpdatedBy.Person.FirstName ?? "") + " " +
                       (x.UpdatedBy.Person.LastName ?? "")
                    }
                }
            });

            return new PagedList<ProjectQuestionsAnswers>(result, page, pageSize);
        }

        #endregion

        #region Get By Id
        public async Task<ProjectQuestionsAnswers> GetProjectQuestionsAnswerById(string id)
        {
            var query = _projectQuestionsAnswersRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<Vsky.Models.ProjectQuestionsAnswers> GetProjectQuestionsAnswerByIdInDetail(string siteId, string Id)
        {
            var query = _projectQuestionsAnswersRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.Id == Id);

            query = query.Select(x => new ProjectQuestionsAnswers
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,

                Project = x.Project == null ? null : new Project
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name
                },

                Requirement = x.Requirement == null ? null : new Requirement
                {
                    Id = x.Requirement.Id,
                    Title = x.Requirement.Title
                },

                CreatedBy = x.CreatedBy == null ? null : new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = x.CreatedBy.Person == null ? null : new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FullName = (x.CreatedBy.Person.FirstName ?? "") + " " +
                       (x.CreatedBy.Person.LastName ?? "")
                    }
                },

                UpdatedBy = x.UpdatedBy == null ? null : new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = x.UpdatedBy.Person == null ? null : new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FullName = (x.UpdatedBy.Person.FirstName ?? "") + " " +
                       (x.UpdatedBy.Person.LastName ?? "")
                    }
                }
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        #endregion

        #region GetProjectQuestionsAnswerByTitle
        public async Task<ProjectQuestionsAnswers> GetProjectQuestionsAnswerByTitle(string siteId, string projectId, string title, string id = null)
        {
            var query = _projectQuestionsAnswersRepository.TableNoTracking
                .Where(x => !x.Deleted
                         && x.SiteId == siteId && x.ProjectId == projectId && x.Title.ToLower() == title.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            return await query.FirstOrDefaultAsync();
        }
        #endregion

        #region Insert Update Delete
        public void InsertProjectQuestionsAnswer(ProjectQuestionsAnswers entity)
        {
            _projectQuestionsAnswersRepository.Insert(entity);
        }

        public void UpdateProjectQuestionsAnswer(ProjectQuestionsAnswers entity)
        {
            _projectQuestionsAnswersRepository.Update(entity);
        }

        public void DeleteProjectQuestionsAnswer(ProjectQuestionsAnswers entity)
        {
            entity.Deleted = true;
            _projectQuestionsAnswersRepository.Update(entity);
        }
        #endregion
    }
}
