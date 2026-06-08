using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectWeeklyPlan
{
    public class ProjectWeeklyPlanDatesReqTaskIssueMappingService : IProjectWeeklyPlanDatesReqTaskIssueMappingService
    {
        #region Initialization
        private readonly IRepository<ProjectWeeklyPlanDatesReqTaskIssueMapping> _ProjectWeeklyPlanDatesReqTaskIssueMappingRepository;
        public ProjectWeeklyPlanDatesReqTaskIssueMappingService(IRepository<ProjectWeeklyPlanDatesReqTaskIssueMapping> projectWeeklyPlanDatesReqTaskIssueMappingRepository)
        {
            _ProjectWeeklyPlanDatesReqTaskIssueMappingRepository = projectWeeklyPlanDatesReqTaskIssueMappingRepository;
        }
        #endregion

        #region GetById
        public async Task<ProjectWeeklyPlanDatesReqTaskIssueMapping> GetById(string id)
        {
            return await _ProjectWeeklyPlanDatesReqTaskIssueMappingRepository.TableNoTracking.FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);
        }
        public async Task<ProjectWeeklyPlanDatesReqTaskIssueMapping> GetByIdInDetail(string id)
        {
            return await _ProjectWeeklyPlanDatesReqTaskIssueMappingRepository.TableNoTracking
                .Where(x => !x.Deleted && x.Id == id)
                .Include(m => m.Requirement)
                .Include(m => m.Task)
                .Include(m => m.Issue)
                .FirstOrDefaultAsync();
        }
        public async Task<List<ProjectWeeklyPlanDatesReqTaskIssueMapping>> GetAllByProjectWeeklyPlanDatesId(string projectWeeklyPlanDatesId)
        {
            return await _ProjectWeeklyPlanDatesReqTaskIssueMappingRepository.TableNoTracking
               .Where(x => !x.Deleted && x.ProjectWeeklyPlanDatesId == projectWeeklyPlanDatesId)
               .Include(m => m.Requirement)
               .Include(m => m.Task)
               .Include(m => m.Issue)
               .ToListAsync();
        }
        #endregion

        #region Insert & Update & Delete
        public void InsertProjectWeeklyPlanDatesReqTaskIssue(ProjectWeeklyPlanDatesReqTaskIssueMapping entity)
        {
            _ProjectWeeklyPlanDatesReqTaskIssueMappingRepository.Insert(entity);
        }
        public void UpdateProjectWeeklyPlanDatesReqTaskIssue(ProjectWeeklyPlanDatesReqTaskIssueMapping entity)
        {
            _ProjectWeeklyPlanDatesReqTaskIssueMappingRepository.Update(entity);
        }
        public void DeleteProjectWeeklyPlanDatesReqTaskIssue(ProjectWeeklyPlanDatesReqTaskIssueMapping entity)
        {
            entity.Deleted = true;
            _ProjectWeeklyPlanDatesReqTaskIssueMappingRepository.Update(entity);
        }
        #endregion
    }
}
