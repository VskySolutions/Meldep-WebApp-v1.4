using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ProjectActionItem
{
    public interface IProjectActionItemsService
    {
        #region GetAllProjectActionItems
        Task<IPagedList<ProjectActionItems>> GetAllProjectActionItems(
            string SiteId,
            string LoggedUserId,
            string SearchText,
            List<string> projectIds,
            List<string> requirementIds,
            List<string> priorityIds,
            string title,
            List<string> customerIds,
            List<string> employeeIds,
            DateTime? dueDate,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetProjectActionItemById
        Task<ProjectActionItems> GetProjectActionItemById(string id);
        #endregion

        #region GetProjectActionItemDetailsById
        Task<ProjectActionItems> GetProjectActionItemDetailsById(string id);
        #endregion

        #region InsertProjectActionItems
        void InsertProjectActionItems(ProjectActionItems entity);
        #endregion

        #region UpdateProjectActionItems
        void UpdateProjectActionItems(ProjectActionItems entity);
        #endregion

        #region DeleteProjectActionItems
        void DeleteProjectActionItems(ProjectActionItems entity);
        #endregion
    }
}
