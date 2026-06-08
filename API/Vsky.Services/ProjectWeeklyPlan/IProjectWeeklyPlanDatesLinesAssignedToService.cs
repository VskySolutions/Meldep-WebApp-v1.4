using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectWeeklyPlan
{
    public interface IProjectWeeklyPlanDatesLinesAssignedToService
    {
        #region GetById & GetAllByProjectWeeklyPlanDatesLineId
        Task<ProjectWeeklyPlanDatesLinesAssignedTo> GetById(string Id);
        Task<ProjectWeeklyPlanDatesLinesAssignedTo> GetByIdInDetail(string Id);
        Task<List<ProjectWeeklyPlanDatesLinesAssignedTo>> GetAllAssignToAsList(string ProjectWeeklyPlanDatesLineId);
        Task<List<ProjectWeeklyPlanDatesLinesAssignedTo>> GetAllByProjectWeeklyPlanDatesLineId(string ProjectIdProjectWeeklyPlanDatesLineId);
        Task<bool> CheckIfEmployeeIdExistsInWeeklyPlanLine(string LineId, string EmployeeId);
        #endregion

        #region Insert & Update & Delete
        void InsertProjectWeeklyPlanDatesLinesAssignedTo(ProjectWeeklyPlanDatesLinesAssignedTo entity);
        void UpdateProjectWeeklyPlanDatesLinesAssignedTo(ProjectWeeklyPlanDatesLinesAssignedTo entity);
        void DeleteProjectWeeklyPlanDatesLinesAssignedTo(ProjectWeeklyPlanDatesLinesAssignedTo entity);
        #endregion
    }
}
