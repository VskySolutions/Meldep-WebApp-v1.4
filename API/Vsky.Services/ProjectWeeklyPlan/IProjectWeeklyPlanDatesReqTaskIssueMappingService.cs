using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ProjectWeeklyPlan
{
    public interface IProjectWeeklyPlanDatesReqTaskIssueMappingService
    {
        Task<ProjectWeeklyPlanDatesReqTaskIssueMapping> GetById(string id);
        Task<ProjectWeeklyPlanDatesReqTaskIssueMapping> GetByIdInDetail(string id);
        Task<List<ProjectWeeklyPlanDatesReqTaskIssueMapping>> GetAllByProjectWeeklyPlanDatesId(string projectWeeklyPlanDatesId);

        void InsertProjectWeeklyPlanDatesReqTaskIssue(ProjectWeeklyPlanDatesReqTaskIssueMapping entity);
        void UpdateProjectWeeklyPlanDatesReqTaskIssue(ProjectWeeklyPlanDatesReqTaskIssueMapping entity);
        void DeleteProjectWeeklyPlanDatesReqTaskIssue(ProjectWeeklyPlanDatesReqTaskIssueMapping entity);
    }
}
