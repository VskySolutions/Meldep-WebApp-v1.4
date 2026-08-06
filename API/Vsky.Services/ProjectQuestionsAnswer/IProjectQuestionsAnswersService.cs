using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectQuestionsAnswer
{
    public interface IProjectQuestionsAnswersService
    {
        IPagedList<Vsky.Models.ProjectQuestionsAnswers> GetAllProjectQuestionsAnswers(
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
        );

        Task<ProjectQuestionsAnswers> GetProjectQuestionsAnswerById(string Id);
        Task<Vsky.Models.ProjectQuestionsAnswers> GetProjectQuestionsAnswerByIdInDetail(string siteId, string Id);
        Task<ProjectQuestionsAnswers> GetProjectQuestionsAnswerByTitle(string SiteId, string title, string id = null);
        void InsertProjectQuestionsAnswer(ProjectQuestionsAnswers entity);
        void UpdateProjectQuestionsAnswer(ProjectQuestionsAnswers entity);
        void DeleteProjectQuestionsAnswer(ProjectQuestionsAnswers entity);
    }
}

