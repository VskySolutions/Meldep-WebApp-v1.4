using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectQuestionsAnswer
{
    public interface IProjectQuestionsAnswersContributorsService
    {
        Task<ProjectQuestionsAnswersContributors> GetProjectQuestionAnswerContributorById(string id);
        Task<List<ProjectQuestionsAnswersContributors>> GetProjectQuestionAnswerContributorsByQuestionAnswerId(
          string siteId,
          string projectQuestionAnswerId);
        //void InsertProjectQuestionAnswerContributor(ProjectQuestionAnswerContributors entity);
        void InsertProjectQuestionAnswerContributorList(IList<ProjectQuestionsAnswersContributors> entities);
        void UpdateProjectQuestionAnswerContributor(ProjectQuestionsAnswersContributors entity);
        void UpdateProjectQuestionAnswerContributorList(IList<ProjectQuestionsAnswersContributors> entities);
        //void DeleteProjectQuestionAnswerContributor(ProjectQuestionsAnswersContributors entity);
    }
}


