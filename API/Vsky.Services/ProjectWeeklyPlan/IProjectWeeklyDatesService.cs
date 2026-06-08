using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ProjectWeeklyPlan
{
    public interface IProjectWeeklyDatesService
    {
        Task<ProjectWeeklyPlanDates> GetById(string id);
        Task<ProjectWeeklyPlanDates> GetByIdInDetail(string id);
        Task<List<Models.ProjectWeeklyPlanDates>> GetProjectWeeklyPlanDatesByProjectId(string projectId, string planTypeId, int skipIndex = 0, int takeCount = 4, DateTime? weekEndDate = null);
        Task<List<EmployeeEstimateHoursForWeekSummary>> GetEmployeeHourSummaryByWeekPlanId(string planTypeId, string planWeekId);
        Task<string> CheckIfProjectWeeklyPlanIsCreated(string projectId, string planTypeId, DateTime weekDate);

        void InsertProjectWeeklyPlanDates(Models.ProjectWeeklyPlanDates entity);
        void UpdateProjectWeeklyPlanDates(Models.ProjectWeeklyPlanDates entity);
        void DeleteProjectWeeklyPlanDates(Models.ProjectWeeklyPlanDates entity);
    }
}
