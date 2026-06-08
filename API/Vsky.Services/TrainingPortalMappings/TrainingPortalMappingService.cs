using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.TrainingPortalMappings
{
    public class TrainingPortalMappingService : ITrainingPortalMappingService
    {
        #region Define Service
        private readonly IRepository<Training_Portal_Mapping> _trainingPortalMappingRepository;
        #endregion

        #region Service Initializations
        public TrainingPortalMappingService(IRepository<Training_Portal_Mapping> trainingPortalMappingRepository)
        {
            _trainingPortalMappingRepository = trainingPortalMappingRepository;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetTrainingEmployeesById
        public async Task<Training_Portal_Mapping> GetTrainingEmployeesById(string id)
        {
            var query = _trainingPortalMappingRepository.TableNoTracking.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetTrainingEmployeesById
        public List<Training_Portal_Mapping> GetTrainingByTrainingId(string SiteId, string TrainingId)
        {
            var query = _trainingPortalMappingRepository.TableNoTracking.Where(x => x.TrainingId == TrainingId && x.Training.SiteId == SiteId);
            var list = query.ToList();
            return list;
        }
        #endregion

        #region InsertTrainingEmployees
        public void InsertTrainingEmployees(Training_Portal_Mapping entity)
        {
            _trainingPortalMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdateTrainingEmployees
        // Title : UpdateTrainingEmployees
        // Description: Updates an existing Training_Portal_Mapping entity in the repository.
        public void UpdateTrainingEmployees(Training_Portal_Mapping entity)
        {
            _trainingPortalMappingRepository.Update(entity);
        }
        #endregion

        #region DeleteTrainingEmployees
        // Title : DeleteTrainingEmployees
        // Description: Deletes a Training_Portal_Mapping entity from the repository.
        public void DeleteTrainingEmployees(Training_Portal_Mapping entity)
        {
            _trainingPortalMappingRepository.Delete(entity);
        }
        #endregion

    }
}
