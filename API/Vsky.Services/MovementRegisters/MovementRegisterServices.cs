using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Microsoft.EntityFrameworkCore;
using MailKit.Search;
using System.Globalization;
using Ical.Net;

namespace Vsky.Services.MovementRegisters
{
    public class MovementRegisterServices : IMovementRegisterServices
    {
        #region Define Services
        private readonly IRepository<MovementRegister> _movementRegisterRepository;
        private readonly IRepository<SitesModifiedLogs> _sitesModifiedLogRepository;
        #endregion

        #region Services Initializations

        public MovementRegisterServices(IRepository<MovementRegister> movementRegisterRepository, IRepository<SitesModifiedLogs> sitesModifiedLogRepository)
        {
            _movementRegisterRepository = movementRegisterRepository;
            _sitesModifiedLogRepository = sitesModifiedLogRepository;
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

        #region GetAllMovementRegister
        public IPagedList<Models.MovementRegister> GetAllMovementRegister(
            string SiteId,
            string createdBy,
            string searchText,
            string employeeId,
            string typeId,
            DateTime? fromDate,
            DateTime? toDate,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
         )
        {
            var query = _movementRegisterRepository.TableNoTracking.Include(x => x.MovementRegisterDetails).Where(x => !x.Deleted && x.SiteId == SiteId);

            if (fromDate != null) query = query.Where(x => x.Date >= fromDate);
            if (toDate != null) query = query.Where(a => a.Date <= toDate);

           // query = query.Where(x => x.MovementRegisterDetails.Any(m => !m.Deleted && (string.IsNullOrWhiteSpace(employeeId) || m.EmployeeId == employeeId)));

            if (!string.IsNullOrEmpty(searchText))
            
            {
                var lower = searchText.ToLower();
                DateTime.TryParse(searchText, out var parsedDate);

                query = query.Where(m =>
                    m.MovementRegisterDetails.Any(d =>
                        (d.Employees.Person.FirstName + " " + d.Employees.Person.LastName).ToLower().Contains(lower)) ||
                    m.MovementRegisterDetails.Any(d =>d.Message.ToLower().Contains(lower) ) ||
                    m.MovementRegisterDetails.Any(d =>(d.Approvers.Person.FirstName + " " + d.Approvers.Person.LastName).ToLower().Contains(lower)) ||
                    m.MovementRegisterDetails.Any(d => d.Type.DropDownValue.ToLower().Contains(lower)) ||
                    (m.Date != null && m.Date.Value.Date == parsedDate.Date)
                );
            }

            query = query.Select(x => new Models.MovementRegister
            {
                Id = x.Id,
                Date = x.Date,
                MovementRegisterDetails = x.MovementRegisterDetails.Where(m => !m.Deleted && (string.IsNullOrWhiteSpace(createdBy) || m.CreatedById == createdBy) && (string.IsNullOrWhiteSpace(employeeId) || m.EmployeeId == employeeId) && (string.IsNullOrWhiteSpace(typeId) || m.TypeId == typeId))
                .OrderByDescending(m => m.CreatedOnUtc)
                .Select(d => new MovementRegisterDetails
                {
                    Id = d.Id,
                    EmployeeId = d.EmployeeId,
                    ApproverById = d.ApproverById,
                    Message = d.Message,
                    TimeInMinutes = d.TimeInMinutes,
                    Employees = new Employee
                    {
                        Id = d.EmployeeId,
                        Person = new Person
                        {
                            Id = d.Employees.Person.Id,
                            FullName = d.Employees.Person.FirstName + " " + d.Employees.Person.LastName
                        }
                    },
                    Approvers = new Employee
                    {
                        Id = d.ApproverById,
                        Person = new Person
                        {
                            Id = d.Approvers.Person.Id,
                            FullName = d.Approvers.Person.FirstName + " " + d.Approvers.Person.LastName
                        }
                    },
                    Type = new DropDown
                    {
                        Id = d.Type.Id,
                        DropDownValue = d.Type.DropDownValue,
                    },
                    WFHDuration = new DropDown
                    {
                        Id = d.WFHDuration.Id,
                        DropDownText = d.WFHDuration.DropDownText,
                    },
                })
                .ToList()
            }).Where(r => r.MovementRegisterDetails.Any()).OrderByDescending(x => x.Date); 

            var list = new PagedList<Models.MovementRegister>(query, page, pageSize);

            foreach (var mr in list)
            {
                var details = mr.MovementRegisterDetails.Where(d => !d.Deleted);

                // Sort by Type.DropDownValue if requested
                if (!string.IsNullOrWhiteSpace(sortBy))
                {

                    if (sortBy == "employeeName")
                    {
                        details = descending
                            ? details.OrderByDescending(d => d.Employees.Person.FirstName)
                            : details.OrderBy(d => d.Employees.Person.FirstName);
                    }
                    else if (sortBy == "approverName")
                    {
                        details = descending
                            ? details.OrderByDescending(d => d.Approvers.Person.FirstName)
                            : details.OrderBy(d => d.Approvers.Person.FirstName);
                    }
                    else if (sortBy == "type")
                    {
                        details = descending
                            ? details.OrderByDescending(d => d.Type.DropDownValue)
                            : details.OrderBy(d => d.Type.DropDownValue);
                    }
                    else if (sortBy == "message")
                    {
                        details = descending
                            ? details.OrderByDescending(d => d.Message)
                            : details.OrderBy(d => d.Message);
                    }
                    else if (sortBy == "timeInMinutes")
                    {
                        details = descending
                            ? details.OrderByDescending(d => d.TimeInMinutes)
                            : details.OrderBy(d => d.TimeInMinutes);
                    }
                }

                mr.MovementRegisterDetails = details.ToList();
            }
            return list;
        }
        #endregion

        #region GetAllMovementRegistersForDashboard
        public IPagedList<Models.MovementRegister> GetAllMovementRegistersForDashboard(
            string SiteId,
            string timeZone,
            string createdBy,
            string searchText,
            string employeeId,
            DateTime? fromDate,
            DateTime? toDate,
            bool isViewMore,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
         )
        {
            var query = _movementRegisterRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (fromDate != null) query = query.Where(x => x.Date >= fromDate);
            if (toDate != null) query = query.Where(x => x.Date <= toDate);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                // fallback to dynamic sort for simple fields ONLY
                query = query.OrderBy($"{sortBy} {(descending ? "desc" : "asc")}");
            }
            else
            {
                query = query.OrderByDescending(x => x.Date);
            }

            var createdTimeInZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            query = query.Select(x => new Models.MovementRegister
            {
                Id = x.Id,
                Date = x.Date,
                MovementRegisterDetails = x.MovementRegisterDetails.Where(m => !m.Deleted)
                .OrderByDescending(m => m.CreatedOnUtc)
                .Take(isViewMore ? int.MaxValue : 16)
                .Select(d => new MovementRegisterDetails
                {
                    Id = d.Id,
                    EmployeeId = d.EmployeeId,
                    Message = d.Message,
                    TimeInMinutes = d.TimeInMinutes,
                    CreatedOnUtc = d.CreatedOnUtc,
                    CreatedTimeStr = TimeZoneInfo.ConvertTimeFromUtc(d.CreatedOnUtc, createdTimeInZone).ToString("hh:mm tt", CultureInfo.InvariantCulture),
                    Employees = new Employee
                    {
                        Id = d.EmployeeId,
                        Person = new Person
                        {
                            Id = d.Employees.Person.Id,
                            FullName = d.Employees.Person.FirstName + " " + d.Employees.Person.LastName
                        }
                    },
                    CreatedBy = new ApplicationUser
                    {
                        Id = d.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = d.CreatedBy.PersonId,
                            FullName = d.CreatedBy.Person.FirstName + " " + d.CreatedBy.Person.LastName,
                        }
                    },
                    Type = new DropDown
                    {
                        Id = d.Type.Id,
                        DropDownValue = d.Type.DropDownValue,
                    },
                    WFHDuration = new DropDown
                    {
                        Id = d.WFHDuration.Id,
                        DropDownText = d.WFHDuration.DropDownText,
                    },
                    SiteModifiedLogCount = _sitesModifiedLogRepository.TableNoTracking.Count(m => !m.Deleted && m.SubModuleId == d.Id)
                })
                .ToList()
            });

            var list = new PagedList<Models.MovementRegister>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetById
        public async Task<Models.MovementRegister> GetMovementRegisterById(string id)
        {
            var query = _movementRegisterRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
                //.Include(x => x.MovementRegisterDetails.Where(x => !x.Deleted));

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<Models.MovementRegister> GetMovementRegisterDetailsById(string id, string detailId)
        {
            var query = _movementRegisterRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new Models.MovementRegister
            {
                Id = x.Id,
                Date = x.Date,
                MovementRegisterDetails = x.MovementRegisterDetails.Where(m => !m.Deleted && m.Id == detailId)
                .Select(d => new MovementRegisterDetails
                {
                    Id = d.Id,
                    ApproverById = d.ApproverById,
                    MomentRegisterId = d.MomentRegisterId,
                    EmployeeId = d.EmployeeId,
                    TypeId = d.TypeId,
                    BreakTimeId = d.BreakTimeId,
                    WFHDurationId = d.WFHDurationId,
                    TimeInMinutes = d.TimeInMinutes,
                    Message = d.Message,
                    NotifyToStakeholders = d.NotifyToStakeholders,
                    CreatedOnUtc = d.CreatedOnUtc,
                    UpdatedOnUtc = d.UpdatedOnUtc,
                    Employees = new Employee
                    {
                        Id = d.EmployeeId,
                        Person = new Person
                        {
                            Id = d.Employees.Person.Id,
                            FullName = d.Employees.Person.FirstName + " " + d.Employees.Person.LastName
                        }
                    },
                    Approvers = new Employee
                    {
                        Id = d.ApproverById,
                        Person = new Person
                        {
                            Id = d.Approvers.Person.Id,
                            FullName = d.Approvers.Person.FirstName + " " + d.Approvers.Person.LastName
                        }
                    },
                    Type = new DropDown
                    {
                        Id = d.Type.Id,
                        DropDownValue = d.Type.DropDownValue,
                    },
                    WFHDuration = new DropDown
                    {
                        Id = d.WFHDuration.Id,
                        DropDownText = d.WFHDuration.DropDownText,
                    },
                    CreatedBy = new ApplicationUser
                    {
                        Id = d.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = d.CreatedBy.PersonId,
                            FullName = d.CreatedBy.Person.FirstName + " " + d.CreatedBy.Person.LastName,
                        }
                    },
                })
                .ToList()
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<Models.MovementRegister> GetMovementRegisterByDate(string SiteId, DateTime? Date, string id = null)
        {
            var query = _movementRegisterRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (Date != null)
                query = query.Where(x => x.Date.Value.Date == Date.Value.Date);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<(DateTime? startDate, DateTime? endDate)> GetMovementRegisterDateRange(string siteId)
        {
            // Get all non-deleted movement registers for the site
            var query = _movementRegisterRepository.TableNoTracking
                .Where(x => !x.Deleted && x.SiteId == siteId);

            // Get the earliest date (ascending)
            var startDate = await query
                .OrderBy(x => x.Date)
                .Select(x => x.Date)
                .FirstOrDefaultAsync();

            // Get the latest date (descending)
            var endDate = await query
                .OrderByDescending(x => x.Date)
                .Select(x => x.Date)
                .FirstOrDefaultAsync();

            return (startDate, endDate);
        }

        #endregion

        #region Insert & Update & Delete
        public void InsertMovementRegister(Models.MovementRegister entity)
        {
            _movementRegisterRepository.Insert(entity);
        }
        public void UpdateMovementRegister(Models.MovementRegister entity)
        {
            _movementRegisterRepository.Update(entity);
        }
        public void DeleteMovementRegister(Models.MovementRegister entity)
        {
            entity.Deleted = true;
            _movementRegisterRepository.Update(entity);
        }
        #endregion

    }
}
