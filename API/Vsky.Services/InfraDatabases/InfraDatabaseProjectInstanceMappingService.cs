using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.InfraDatabases
{
    public class InfraDatabaseProjectInstanceMappingService : IInfraDatabaseProjectInstanceMappingService
    {
        #region Define Services
        private readonly IRepository<InfraDatabaseProjectInstanceMapping> _infraDatabaseProjectInstanceMappingRepository;
        #endregion

        #region Services Initializations
        public InfraDatabaseProjectInstanceMappingService(
            IRepository<InfraDatabaseProjectInstanceMapping> infraDatabaseProjectInstanceMappingRepository)
        {
            _infraDatabaseProjectInstanceMappingRepository = infraDatabaseProjectInstanceMappingRepository;
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

        #region GetInfraDatabaseProjectInstanceMappingById
        // Title: GetInfraDatabaseProjectInstanceMappingById
        // Description: This method retrieves a Infra Database Project Instance Mapping from the database by its unique identifier (`id`). 
        public async Task<InfraDatabaseProjectInstanceMapping> GetInfraDatabaseProjectInstanceMappingById(string id)
        {
            var query = _infraDatabaseProjectInstanceMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        // Assign ftp to project instance
        #region InsertInfraDatabaseProjectInstanceMapping
        public void InsertInfraDatabaseProjectInstanceMapping(Models.InfraDatabaseProjectInstanceMapping entity)
        {
            _infraDatabaseProjectInstanceMappingRepository.Insert(entity);
        }
        #endregion

        #region DeleteInfraDatabaseProjectInstanceMapping
        public void DeleteInfraDatabaseProjectInstanceMapping(Models.InfraDatabaseProjectInstanceMapping entity)
        {
            entity.Deleted = true;
            _infraDatabaseProjectInstanceMappingRepository.Update(entity);
        }
        #endregion
    }
}


