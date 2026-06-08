using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vsky.Services.ProjectWeeklyPlan
{
    public interface IProjectWeeklyDatesLinesService
    {
        Task<Models.ProjectWeeklyPlanDatesLines> GetById(string id);
        Task<Models.ProjectWeeklyPlanDatesLines> GetInDetailById(string id);
        void InsertProjectWeeklyPlanDatesLines(Models.ProjectWeeklyPlanDatesLines entity);
        void UpdateProjectWeeklyPlanDatesLines(Models.ProjectWeeklyPlanDatesLines entity);
        void DeleteProjectWeeklyPlanDatesLines(Models.ProjectWeeklyPlanDatesLines entity);
    }
}
