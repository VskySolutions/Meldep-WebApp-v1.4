using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.TrainingPortalMappings
{
    public interface ITrainingPortalMappingService
    {
        #region GetTrainingEmployeesById
        Task<Training_Portal_Mapping> GetTrainingEmployeesById(string id);
        #endregion

        #region GetTrainingByTrainingId
        List<Training_Portal_Mapping> GetTrainingByTrainingId(string SiteId, string TrainingId);
        #endregion

        #region InsertTrainingEmployees
        void InsertTrainingEmployees(Training_Portal_Mapping entity);
        #endregion
       
        #region UpdateTrainingEmployees
        void UpdateTrainingEmployees(Training_Portal_Mapping entity);
        #endregion

        #region DeleteTrainingEmployees
        void DeleteTrainingEmployees(Training_Portal_Mapping entity);
        #endregion
    }
}
