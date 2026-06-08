using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.InfraFTPs
{
    public class InfraFTPsProjectInstanceMappingService : IInfraFTPsProjectInstanceMappingService
    {
        #region Define Services
        private readonly IRepository<InfraFTPsProjectInstanceMapping> _infraFTPsProjectInstanceMappingRepository;
        #endregion

        #region Services Initializations
        public InfraFTPsProjectInstanceMappingService(
            IRepository<InfraFTPsProjectInstanceMapping> infraFTPsProjectInstanceMappingRepository)
        {
            _infraFTPsProjectInstanceMappingRepository = infraFTPsProjectInstanceMappingRepository;
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

        #region GetInfraFTPsProjectInstanceMappingById
        // Title: GetInfraFTPsProjectInstanceMappingById
        // Description: This method retrieves a Infra FTPs Project Instance Mapping from the database by its unique identifier (`id`). 
        public async Task<InfraFTPsProjectInstanceMapping> GetInfraFTPsProjectInstanceMappingById(string id)
        {
            var query = _infraFTPsProjectInstanceMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        // Assign ftp to project instance
        #region InsertInfraFTPsProjectInstanceMapping
        public void InsertInfraFTPsProjectInstanceMapping(Models.InfraFTPsProjectInstanceMapping entity)
        {
            _infraFTPsProjectInstanceMappingRepository.Insert(entity);
        }
        #endregion

        #region DeleteInfraFTPsProjectInstanceMapping
        public void DeleteInfraFTPsProjectInstanceMapping(Models.InfraFTPsProjectInstanceMapping entity)
        {
            entity.Deleted = true;
            _infraFTPsProjectInstanceMappingRepository.Update(entity);
        }
        #endregion
    }
}

