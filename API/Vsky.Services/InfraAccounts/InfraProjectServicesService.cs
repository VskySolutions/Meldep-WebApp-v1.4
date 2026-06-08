using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.InfraAccounts
{
    public class InfraProjectServicesService : IInfraProjectServicesService
    {
        #region Define Services
        private readonly IRepository<InfraProjectServices> _infraProjectServicesRepository;
        #endregion

        #region Services Initializations
        public InfraProjectServicesService(
            IRepository<InfraProjectServices> infraProjectServicesRepository)
        {
            _infraProjectServicesRepository = infraProjectServicesRepository;
        }
        #endregion

        #region Private Methods
        // Title: GetOrderBy
        // Description: This method returns the input string as it is, which can be used as the `ORDER BY` clause in a SQL query.
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetInfraAccountProjectServicesById
        // Title: GetInfraAccountProjectServicesById
        // Description: This method retrieves a InfraAccountProjectServices from the database by its unique identifier (`id`). 
        public async Task<InfraProjectServices> GetInfraAccountProjectServicesById(string id)
        {
            var query = _infraProjectServicesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region HasActiveInfraProjects
        public async Task<bool> HasActiveInfraProjects(string infraServiceId)
        {
            return await _infraProjectServicesRepository.TableNoTracking
                .AnyAsync(x =>
                    !x.Deleted &&
                    !x.Project.Deleted &&
                    x.InfraServiceId == infraServiceId
                );
        }
        #endregion

        // Assign service to project
        #region InsertInfraProjectServices
        public void InsertInfraProjectServices(Models.InfraProjectServices entity)
        {
            _infraProjectServicesRepository.Insert(entity);
        }
        #endregion

        #region DeleteInfraProjectServices
        public void DeleteInfraProjectServices(Models.InfraProjectServices entity)
        {
            entity.Deleted = true;
            _infraProjectServicesRepository.Update(entity);
        }
        #endregion
    }
}
