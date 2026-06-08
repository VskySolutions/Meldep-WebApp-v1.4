using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.TimeInTimeOuts
{
    public class TimeInTimeOutService : ITimeInTimeOutService
    {
        #region Define Services
        private readonly IRepository<TimeInTimeOut> _timeInTimeOutRepository;
        #endregion

        #region Services Initializations

        public TimeInTimeOutService(IRepository<TimeInTimeOut> timeInTimeOutRepository)
        {
            _timeInTimeOutRepository = timeInTimeOutRepository;
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

        #region GetAllTimeInTimeOuts
        // Title: GetAllTimeInTimeOuts
        // Description: This method retrieves a paginated list of TimeInTimeOut based on various search criteria such as name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<TimeInTimeOut> GetAllTimeInTimeOuts(
            string siteId,
            string createdBy,
            string employeeId,
            string searchText,
            DateTime? fromDate,
            DateTime? toDate,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
            )
        {
            var query = _timeInTimeOutRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(x => x.CreatedById == createdBy);

            if (!string.IsNullOrWhiteSpace(employeeId))
                query = query.Where(x => x.EmployeeId == employeeId);

            if (fromDate != null) query = query.Where(x => x.TimeInDate == fromDate);
            if (toDate != null) query = query.Where(a => a.TimeOutDate == toDate);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            // search text
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                DateTime.TryParse(searchText, out var parsedDate);
                decimal parsedDecimalTime;
                decimal.TryParse(searchText, out parsedDecimalTime);
                searchText = searchText.ToLower();

                query = query.Where(m =>
                    (m.Employee.Person.FirstName + " " + m.Employee.Person.LastName).ToLower().Contains(searchText.ToLower()) ||
                    (m.TimeInDate.HasValue && m.TimeInDate.Value.Date == parsedDate.Date));
            }

            query = query.Select(x => new TimeInTimeOut
            {
                Id = x.Id,
                TimeIn = x.TimeIn,
                TimeOut = x.TimeOut,
                TimeInDate = x.TimeInDate,
                TimeOutDate = x.TimeOutDate,
                ActualHours = x.ActualHours,
                ActualHoursStr = FormatTimeSpanToString(x.ActualHours),
                EmployeeId = x.EmployeeId,
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.PersonId,
                        FirstName = x.Employee.Person.FirstName,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    }
                },
            });

            var list = new PagedList<TimeInTimeOut>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetTimeInTimeOutById
        // Title: GetTimeInTimeOutById
        // Description: This method retrieves a TimeInTimeOut from the database by its unique identifier (`id`). 
        public async Task<TimeInTimeOut> GetTimeInTimeOutById(string id)
        {
            var query = _timeInTimeOutRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetTimeInTimeOutByEmployeeId
        // Title: GetTimeInTimeOutByEmployeeId
        // Description: This method retrieves a TimeInTimeOut from the database by its employeeId. 
        public async Task<TimeInTimeOut> GetTimeInTimeOutByEmployeeId(string siteId, string employeeId)
        {
            return await _timeInTimeOutRepository.TableNoTracking
                .Where(x => !x.Deleted
                    && x.SiteId == siteId
                    && x.EmployeeId == employeeId)
                .OrderByDescending(x => x.CreatedOnUtc)
                .Select(x => new TimeInTimeOut
                {
                    Id = x.Id,
                    TimeIn = x.TimeIn,
                    TimeOut = x.TimeOut,
                    //TimeInStr = FormatTimeTo12Hour(x.TimeIn),
                    //TimeOutStr = FormatTimeTo12Hour(x.TimeOut),
                    TimeInDate = x.TimeInDate,
                    TimeOutDate = x.TimeOutDate,
                    ActualHoursStr = FormatTimeSpanToString(x.ActualHours),
                })
                .FirstOrDefaultAsync();
        }
        #endregion

        #region GetTimeInTimeOutDetailsById
        // Title: GetTimeInTimeOutDetailsById
        // Description: The method selects relevant fields from the TimeInTimeOut entity.
        public async Task<TimeInTimeOut> GetTimeInTimeOutDetailsById(string id)
        {
            var query = _timeInTimeOutRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new TimeInTimeOut
            {
                Id = x.Id,
                TimeIn = x.TimeIn,
                TimeOut = x.TimeOut,
                TimeInDate = x.TimeInDate,
                TimeOutDate = x.TimeOutDate,
                ActualHours = x.ActualHours,
                TotalHours = x.TotalHours,
                TotalBreak = x.TotalBreak,
                TimeInStr = FormatTimeTo12Hour(x.TimeIn),
                TimeOutStr = FormatTimeTo12Hour(x.TimeOut),
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.PersonId,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    },
                    EmployeeDesignation = x.Employee.EmployeeDesignation.Where(m => !m.Deleted).Select(m => new EmployeeDesignation
                    {
                        Id = m.Id,
                        LeaveApproverId = m.LeaveApproverId,
                        Shift = new DropDown
                        {
                            Id = m.Shift.Id,
                            DropDownValue = m.Shift.DropDownValue
                        }
                    }).ToList()
                },
                WorkHoursApprovalStatus = new DropDown
                {
                    Id = x.WorkHoursApprovalStatus.Id,
                    DropDownValue = x.WorkHoursApprovalStatus.DropDownValue,
                },
                CreatedBy = new ApplicationUser
                {
                    Person = new Person
                    {
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.FullName,
                    }
                },
                UpdatedBy = new ApplicationUser
                {
                    Person = new Person
                    {
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.FullName,
                    }
                },
                TimeInTimeOutBreakDetailList = x.TimeInTimeOutBreakDetailList.Where(m => !m.Deleted).Select(p => new TimeInTimeOutBreakDetail
                {
                    Id = p.Id,
                    BreakIn = p.BreakIn,
                    BreakOut = p.BreakOut,
                    BreakReason = p.BreakReason,
                    TimeInTimeOutId = p.TimeInTimeOutId,
                    CreatedOnUtc = p.CreatedOnUtc,
                    UpdatedOnUtc = p.UpdatedOnUtc
                }).ToList()
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertTimeInTimeOut
        // Title: InsertTimeInTimeOut
        // Description: This method inserts a new TimeInTimeOut entity into the repository. It takes a TimeInTimeOut object as input and uses the _timeInTimeOutRepository to handle the insertion operation.
        public void InsertTimeInTimeOut(TimeInTimeOut entity)
        {
            _timeInTimeOutRepository.Insert(entity);
        }
        #endregion

        #region UpdateTimeInTimeOut
        // Title: UpdateTimeInTimeOut
        // Description: This method updates the specified TimeInTimeOut entity in the repository. It takes a TimeInTimeOut object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateTimeInTimeOut(TimeInTimeOut entity)
        {
            _timeInTimeOutRepository.Update(entity);
        }
        #endregion

        #region DeleteTimeInTimeOut
        // Title: DeleteTimeInTimeOut
        // Description: Marks the specified TimeInTimeOut entity as deleted by setting its `Deleted` property to true. 
        public void DeleteTimeInTimeOut(TimeInTimeOut entity)
        {
            entity.Deleted = true;

            _timeInTimeOutRepository.Update(entity);
        }
        #endregion

        #region private functions
        private static string FormatTimeTo12Hour(TimeSpan time)
        {
            if (time == TimeSpan.Zero)
                return null;

            return DateTime.Today.Add(time).ToString("hh:mm tt");
        }
        #endregion

        #region private functions
        private static string FormatTimeSpanToString(TimeSpan time)
        {
            if (time == TimeSpan.Zero)
                return null;

            int totalMinutes = (int)Math.Round(time.TotalMinutes);

            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            if (hours > 0 && minutes > 0)
                return $"{hours} hr {minutes} mins";
            else if (hours > 0)
                return $"{hours} hr";
            else
                return $"{minutes} mins";
        }
        #endregion
    }
}

