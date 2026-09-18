using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectQuestionsAnswer
{
    public class ProjectQuestionsAnswersContributorsService : IProjectQuestionsAnswersContributorsService
    {
        #region Define services
        private readonly IRepository<ProjectQuestionsAnswersContributors> _projectQuestionAnswerContributorsRepository;
        public ProjectQuestionsAnswersContributorsService(
            IRepository<ProjectQuestionsAnswersContributors> projectQuestionAnswerContributorsRepository
        )
        {
            _projectQuestionAnswerContributorsRepository = projectQuestionAnswerContributorsRepository;
        }
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region Get By Id
        public async Task<ProjectQuestionsAnswersContributors> GetProjectQuestionAnswerContributorById(string id)
        {
            var query = _projectQuestionAnswerContributorsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<List<ProjectQuestionsAnswersContributors>> GetProjectQuestionAnswerContributorsByQuestionAnswerId(
        string siteId,
        string projectQuestionAnswerId)
        {
            var query = _projectQuestionAnswerContributorsRepository
                .TableNoTracking
                .Where(x =>
                    x.ProjectQuestionsAnswers.SiteId == siteId &&
                    x.ProjectQuestionAnswerId == projectQuestionAnswerId);

            return await query.ToListAsync();
        }
        #endregion

        #region Insert Update Delete
        //public void InsertProjectQuestionAnswerContributor(ProjectQuestionAnswerContributors entity)
        //{
        //    _projectQuestionAnswerContributorsRepository.Insert(entity);
        //}
        public void InsertProjectQuestionAnswerContributorList(IList<ProjectQuestionsAnswersContributors> entities)
        {
            _projectQuestionAnswerContributorsRepository.Insert(entities);
        }

        public void UpdateProjectQuestionAnswerContributor(ProjectQuestionsAnswersContributors entity)
        {
            _projectQuestionAnswerContributorsRepository.Update(entity);
        }
        public void UpdateProjectQuestionAnswerContributorList(IList<ProjectQuestionsAnswersContributors> entities)
        {
            _projectQuestionAnswerContributorsRepository.Update(entities);
        }

        //public void DeleteProjectQuestionAnswerContributor(ProjectQuestionsAnswersContributors entity)
        //{
        //    entity.Deleted = true;
        //    _projectQuestionAnswerContributorsRepository.Update(entity);
        //}
        #endregion
    }
}

