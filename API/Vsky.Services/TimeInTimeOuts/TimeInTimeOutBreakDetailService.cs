using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg.Sig;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.TimeInTimeOuts
{
    public class TimeInTimeOutBreakDetailService : ITimeInTimeOutBreakDetailService
    {
        #region Define Services
        private readonly IRepository<TimeInTimeOutBreakDetail> _timeInTimeOutBreakDetailRepository;
        #endregion

        #region Services Initializations
        public TimeInTimeOutBreakDetailService(IRepository<TimeInTimeOutBreakDetail> timeInTimeOutBreakDetailRepository)
        {
            _timeInTimeOutBreakDetailRepository = timeInTimeOutBreakDetailRepository;
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

        #region GetTimeInTimeOutBreakById
        // Title: GetTimeInTimeOutBreakById
        // Description: This method retrieves a TimeInTimeOutBreakDetail from the database by its unique identifier (`id`). 
        public async Task<TimeInTimeOutBreakDetail> GetTimeInTimeOutBreakById(string id)
        {
            var query = _timeInTimeOutBreakDetailRepository.TableNoTracking.Where(x => !x.Deleted);
            query = query.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetBreakInOutByTimeInOutId
        // Title: GetBreakInOutByTimeInOutId
        // Description:This method retrieves the Break In and Break Out time details from the TimeInTimeOutBreakDetail entity for the specified TimeInTimeOutId.It returns the first non-deleted break record containing the Id, BreakIn, and BreakOut values.
        public async Task<List<TimeInTimeOutBreakDetail>> GetBreakInOutByTimeInOutId(string timeInTimeOutId)
        {
            var items = await _timeInTimeOutBreakDetailRepository.TableNoTracking
                .Where(x => x.TimeInTimeOutId == timeInTimeOutId && !x.Deleted)
                .Select(x => new TimeInTimeOutBreakDetail
                {
                    Id = x.Id,
                    BreakIn = x.BreakIn,
                    BreakOut = x.BreakOut
                })
                .ToListAsync();

            return items;
        }
        #endregion

        #region GetTimeInTimeOutBreakDetailByTimeInTimeOutId
        // Title: GetTimeInTimeOutBreakDetailByTimeInTimeOutId
        // Description: This method retrieves a timesheet based on its name and date. It allows an optional exclusion of a timesheet by its ID, which can be useful for scenarios like checking for duplicates.
        public async Task<TimeInTimeOutBreakDetail> GetTimeInTimeOutBreakDetailByTimeInTimeOutId(string timeInTimeOutId, string id = null)
        {
            var query = _timeInTimeOutBreakDetailRepository.TableNoTracking.Where(x => !x.Deleted);

            if (!string.IsNullOrEmpty(timeInTimeOutId))
                query = query.Where(x => x.TimeInTimeOutId == timeInTimeOutId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertTimeInTimeOutBreakDetail
        // Title: InsertTimeInTimeOutBreakDetail
        // Description: This method inserts a new TimeInTimeOutBreakDetail entity into the repository. It takes a TimeInTimeOutBreakDetail object as input and uses the _timeInTimeOutBreakDetailRepository to handle the insertion operation.
        public void InsertTimeInTimeOutBreakDetail(TimeInTimeOutBreakDetail entity)
        {
            _timeInTimeOutBreakDetailRepository.Insert(entity);
        }
        #endregion

        #region UpdateTimeInTimeOutBreakDetail
        // Title: UpdateTimeInTimeOutBreakDetail
        // Description: This method updates the specified TimeInTimeOutBreakDetail entity in the repository. It takes a TimeInTimeOutBreakDetail object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateTimeInTimeOutBreakDetail(TimeInTimeOutBreakDetail entity)
        {
            _timeInTimeOutBreakDetailRepository.Update(entity);
        }
        #endregion

        #region DeleteTimeInTimeOutBreakDetail
        // Title: DeleteTimeInTimeOutBreakDetail
        // Description: Marks the specified TimeInTimeOutBreakDetail entity as deleted by setting its `Deleted` property to true. 
        public void DeleteTimeInTimeOutBreakDetail(TimeInTimeOutBreakDetail entity)
        {
            entity.Deleted = true;
            _timeInTimeOutBreakDetailRepository.Update(entity);
        }
        #endregion

        #region InsertTimeInTimeOutBreakDetailList
        public void InsertTimeInTimeOutBreakDetailList(IList<TimeInTimeOutBreakDetail> entities)
        {
            _timeInTimeOutBreakDetailRepository.Insert(entities);
        }
        #endregion

        #region UpdateTimeInTimeOutBreakDetailList
        // Title: UpdateTimeInTimeOutBreakDetailList
        // Description: This method updates the specified TimeInTimeOutBreakDetail entity in the repository. It takes a TimeInTimeOutBreakDetail object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateTimeInTimeOutBreakDetailList(IList<TimeInTimeOutBreakDetail> entities)
        {
            _timeInTimeOutBreakDetailRepository.Update(entities);
        }
        #endregion

        #region DeleteTimeInTimeOutBreakDetailList
        // Title: DeleteTimeInTimeOutBreakDetailList
        // Description: Marks the specified TimeInTimeOutBreakDetail entity as deleted by setting its `Deleted` property to true. 
        public void DeleteTimeInTimeOutBreakDetailList(List<TimeInTimeOutBreakDetail> entities)
        {
            var list = new List<TimeInTimeOutBreakDetail>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _timeInTimeOutBreakDetailRepository.Update(list);
        }
        #endregion
    }
}


