using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.MovementRegisters
{
    public class MovementRegisterDetailsService : IMovementRegisterDetailsService
    {
        #region Define Services
        private readonly IRepository<Models.MovementRegisterDetails> _movementRegisterDetailsRepository;
        #endregion

        #region Services Initializations
        public MovementRegisterDetailsService(IRepository<Models.MovementRegisterDetails> movementRegisterDetailsRepository)
        {
            _movementRegisterDetailsRepository = movementRegisterDetailsRepository;
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

        public async Task<Models.MovementRegisterDetails> GetMovementRegisterDetailsById(string id)
        {
            var query = _movementRegisterDetailsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<Models.MovementRegisterDetails> GetMovementRegisterDetailByTypeId(string siteId, string employeeId, DateTime? date, string typeId)
        {
            var query = _movementRegisterDetailsRepository.TableNoTracking.Where(x => !x.Deleted && x.EmployeeId == employeeId &&
                    x.TypeId == typeId &&
                    x.MomentRegisters.Date.Value.Date == date.Value.Date &&
                    x.MomentRegisters.SiteId == siteId);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }


        #region InsertMovementRegisterDetails
        public void InsertMovementRegisterDetails(Models.MovementRegisterDetails entity)
        {
            _movementRegisterDetailsRepository.Insert(entity);
        }
        #endregion

        #region UpdateMovementRegisterDetails
        public void UpdateMovementRegisterDetails(Models.MovementRegisterDetails entity)
        {
            _movementRegisterDetailsRepository.Update(entity);
        }
        #endregion

        #region DeleteMovementRegisterDetails
        public void DeleteMovementRegisterDetails(Models.MovementRegisterDetails entity)
        {
            //entity.Deleted = true;
            _movementRegisterDetailsRepository.Delete(entity);
        }
        #endregion
    }
}
