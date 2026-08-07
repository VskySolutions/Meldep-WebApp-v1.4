using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ProjectQuestionsAnswer
{
    public interface IProjectQuestionsAnswersResponseLogService
    {
        Task<ProjectQuestionsAnswersResponseLog> GetProjectQuestionsAnswersResponseLogById(string Id);
        void InsertProjectQuestionsAnswersResponseLogList(IList<ProjectQuestionsAnswersResponseLog> entities);
        void UpdateProjectQuestionsAnswersResponseLogList(IList<ProjectQuestionsAnswersResponseLog> entities);
        void DeleteProjectQuestionsAnswersResponseLogList(List<ProjectQuestionsAnswersResponseLog> entity);
    }
}

