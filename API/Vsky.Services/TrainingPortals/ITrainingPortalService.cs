using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.TrainingPortals
{
    public interface ITrainingPortalService
    {
        #region GetAllTrainingList
        IPagedList<TrainingPortal> GetAllTrainingList(string SiteId, string SearchText, string name, List<string> EmployeeDesignationIds, string sortBy,
        bool descending, int pageIndex = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetById
        Task<TrainingPortal> GetById(string id, string SiteId);
        #endregion

        #region InsertTraining
        void InsertTraining(TrainingPortal entity);
        #endregion

        #region UpdateTraining
        void UpdateTraining(TrainingPortal entity);
        #endregion

        #region DeleteTraining
        void DeleteTraining(TrainingPortal entity);
        #endregion

        #region InsertPicture
        void InsertPicture(Picture entity);
        #endregion

        #region UpdatePicture
        void UpdatePicture(Picture entity);
        #endregion

        #region GetTrainingByName
        Task<TrainingPortal> GetTrainingByName(string SiteId, string name, string id = null);
        #endregion

        #region GetTrainingDetailsById
        Task<TrainingPortal> GetTrainingDetailsById(string id);
        #endregion
    }
}
