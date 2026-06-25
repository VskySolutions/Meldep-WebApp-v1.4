using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;
using Vsky.Core;
using Microsoft.AspNetCore.JsonPatch.Internal;
using System.Linq.Dynamic.Core;

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

        public async Task<IPagedList<object>> GetMovementRegisterDetails(
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
            var query = _movementRegisterDetailsRepository.TableNoTracking
                .Where(x => !x.Deleted && x.MovementRegister.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(x => x.CreatedById == createdBy);

            if (!string.IsNullOrWhiteSpace(employeeId))
                query = query.Where(x => x.EmployeeId == employeeId);

            if (!string.IsNullOrWhiteSpace(typeId))
                query = query.Where(x => x.TypeId == typeId);

            if (!string.IsNullOrWhiteSpace(searchText))
            {

                var lower = searchText.ToLower();
                DateTime.TryParse(searchText, out var parsedDate);

                query = query.Where(x =>
                     (x.Employees.Person.FirstName + " " + x.Employees.Person.LastName).Contains(searchText.ToLower()) ||
                     (x.Approvers.Person.FirstName + " " + x.Approvers.Person.LastName).Contains(searchText.ToLower()) ||
                     x.Message.Contains(searchText.ToLower()) ||
                     x.Type.DropDownValue.Contains(searchText.ToLower()) ||
                     x.TimeInMinutes.ToString().Contains(searchText.ToLower()) ||
                     (x.MovementRegister.Date != null && x.MovementRegister.Date.Value.Date == parsedDate.Date)
                );
            }

            if (fromDate != null)
                query = query.Where(x => x.MovementRegister.Date >= fromDate);

            if (toDate != null)
                query = query.Where(x => x.MovementRegister.Date <= toDate);

           
            var totalCount = await query.CountAsync();

         
            var list = await query
                .OrderByDescending(x => x.CreatedOnUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MovementRegisterDetails
                {
                    Id = m.Id,
                    EmployeeId = m.EmployeeId,
                    ApproverById = m.ApproverById,
                    Message = m.Message,
                    TimeInMinutes = m.TimeInMinutes,

                    Employees = new Employee
                    {
                        Id = m.EmployeeId,
                        Person = new Person
                        {
                            FirstName = m.Employees.Person.FirstName,
                            FullName = m.Employees.Person.FirstName + " " +
                                       m.Employees.Person.LastName
                        }
                    },

                    Approvers = new Employee
                    {
                        Id = m.ApproverById,
                        Person = new Person
                        {
                            FirstName = m.Approvers.Person.FirstName,
                            FullName = m.Approvers.Person.FirstName + " " +
                                       m.Approvers.Person.LastName
                        }
                    },

                    Type = new DropDown
                    {
                        Id = m.Type.Id,
                        DropDownValue = m.Type.DropDownValue
                    },

                    WFHDuration = new DropDown
                    {
                        Id = m.WFHDuration.Id,
                        DropDownText = m.WFHDuration.DropDownText
                    },

                    MovementRegister = new MovementRegister
                    {
                        Date = m.MovementRegister.Date
                    }
                })
                .ToListAsync();

            var groupedResult = list
                .GroupBy(x => x.MovementRegister.Date.Value.Date)
                .Select(g => new MovementRegisterGroup
                {
                    Date = g.Key,
                    Details = g.ToList()
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            foreach (var group in groupedResult)
            {
                var details = group.Details.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    details = sortBy switch
                    {
                        "employeeName" =>
                            descending
                                ? details.OrderByDescending(x => x.Employees.Person.FirstName)
                                : details.OrderBy(x => x.Employees.Person.FirstName),

                        "approverName" =>
                            descending
                                ? details.OrderByDescending(x => x.Approvers.Person.FirstName)
                                : details.OrderBy(x => x.Approvers.Person.FirstName),

                        "type" =>
                            descending
                                ? details.OrderByDescending(x => x.Type.DropDownValue)
                                : details.OrderBy(x => x.Type.DropDownValue),

                        "message" =>
                            descending
                                ? details.OrderByDescending(x => x.Message)
                                : details.OrderBy(x => x.Message),

                        "timeInMinutes" =>
                            descending
                                ? details.OrderByDescending(x => x.TimeInMinutes)
                                : details.OrderBy(x => x.TimeInMinutes),

                        _ => details
                    };
                }

                group.Details = details.ToList();
            }

            return new PagedList<object>(
                groupedResult.Cast<object>().ToList(),
                page,
                pageSize,
                totalCount
            );
        }

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
                    x.MovementRegister.Date.Value.Date == date.Value.Date &&
                    x.MovementRegister.SiteId == siteId);

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
