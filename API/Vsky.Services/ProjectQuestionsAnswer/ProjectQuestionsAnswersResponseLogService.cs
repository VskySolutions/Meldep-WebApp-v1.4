using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;

namespace Vsky.Services.ProjectQuestionsAnswer
{
    public class ProjectQuestionsAnswersResponseLogService : IProjectQuestionsAnswersResponseLogService
    {
        #region Define services
        private readonly IRepository<ProjectQuestionsAnswersResponseLog> _projectQuestionsAnswersResponseLogRepository;
        private readonly ICommonService _commonService;
        public ProjectQuestionsAnswersResponseLogService(IRepository<ProjectQuestionsAnswersResponseLog> projectQuestionsAnswersResponseLogRepository,
            ICommonService commonService
        )
        {
            _projectQuestionsAnswersResponseLogRepository = projectQuestionsAnswersResponseLogRepository;
            _commonService = commonService;
        }
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region Get By Id
        public async Task<ProjectQuestionsAnswersResponseLog> GetProjectQuestionsAnswersResponseLogById(string id)
        {
            var query = _projectQuestionsAnswersResponseLogRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region Insert Update Delete
        public void InsertProjectQuestionsAnswersResponseLogList(IList<ProjectQuestionsAnswersResponseLog> entities)
        {
            _projectQuestionsAnswersResponseLogRepository.Insert(entities);
        }

        public void UpdateProjectQuestionsAnswersResponseLogList(IList<ProjectQuestionsAnswersResponseLog> entities)
        {
            _projectQuestionsAnswersResponseLogRepository.Update(entities);
        }

        public void DeleteProjectQuestionsAnswersResponseLogList(List<ProjectQuestionsAnswersResponseLog> entities)
        {
            var list = new List<ProjectQuestionsAnswersResponseLog>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _projectQuestionsAnswersResponseLogRepository.Update(list);
        }
        #endregion
    }
}
