using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Common;
using Vsky.Services.Sites;



namespace Vsky.Services.LeaveCredits
{
    public class LeaveCreditService : ILeaveCreditService
    {
        #region Define Services
        private readonly IRepository<LeaveCredit> _leaveCreditRepository;
        private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations

        public LeaveCreditService(
            IRepository<LeaveCredit> leaveCreditRepository, 
            ICommonService commonService)
        {
            _leaveCreditRepository = leaveCreditRepository;
            _commonService = commonService;
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

        #region GetAllLeaveCredits
        // Title: GetAllLeaveCredits
        // Description: This method retrieves a paginated list of LeaveCredit based on various search criteria such as employee name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<LeaveCredit> GetAllLeaveCredits(string SiteId, string SearchText, List<string> employeeIds, int years, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _leaveCreditRepository.TableNoTracking.Where(x => !x.Deleted && x.Employee.SiteId == SiteId);

            if (employeeIds != null && employeeIds.Any())
                query = query.Where(x => employeeIds.Contains(x.EmployeeId));

            if (years > 0)
                query = query.Where(x => x.LeaveCreditsforYear == years);

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                     (m.Employee.Person.FirstName + " " + m.Employee.Person.LastName).ToLower().Contains(SearchText.ToLower())
                       );
            }
             query = query.GroupBy(x => x.EmployeeId).Select(x => new LeaveCredit
            {
                Id = x.FirstOrDefault().Id,

                CasualLeaves = x.Sum(m => m.CasualLeaves + m.SickLeaves),
                CreditReason = x.FirstOrDefault().CreditReason,
                EmployeeId = x.FirstOrDefault().EmployeeId,
                LeaveCreditsforYear = x.FirstOrDefault().LeaveCreditsforYear,
                Note = x.FirstOrDefault().Note,
                UpdatedOnUtc = x.FirstOrDefault().UpdatedOnUtc,
                LeaveTypes = new DropDown
                {
                    Id = x.FirstOrDefault().LeaveTypes.Id,
                    DropDownValue = x.FirstOrDefault().LeaveTypes.DropDownValue
                },
                Employee = new Employee
                {
                    Id = x.FirstOrDefault().Employee.Id,
                    //SiteId = x.FirstOrDefault().Employee.SiteId,
                    PersonId = x.FirstOrDefault().Employee.PersonId,
                    EmployeeCode = x.FirstOrDefault().Employee.EmployeeCode,
                    OfficialEmail = x.FirstOrDefault().Employee.OfficialEmail,
                    EmergencyContactName = x.FirstOrDefault().Employee.EmergencyContactName,
                    EmergencyPhoneNo = x.FirstOrDefault().Employee.EmergencyPhoneNo,
                    SameASPermanentAddress = x.FirstOrDefault().Employee.SameASPermanentAddress,
                    AadhaarCardNo = x.FirstOrDefault().Employee.AadhaarCardNo,
                    PanCardNo = x.FirstOrDefault().Employee.PanCardNo,
                    EPFUANNo = x.FirstOrDefault().Employee.EPFUANNo,
                    JoiningDate = x.FirstOrDefault().Employee.JoiningDate,
                    ReleaseDate = x.FirstOrDefault().Employee.ReleaseDate,
                    EducationDetail = x.FirstOrDefault().Employee.EducationDetail,
                    Person = new Person
                    {
                        Id = x.FirstOrDefault().Employee.Person.Id,
                        FirstName = x.FirstOrDefault().Employee.Person.FirstName,
                        LastName = x.FirstOrDefault().Employee.Person.LastName,
                        FullName = x.FirstOrDefault().Employee.Person.FirstName + " " + x.FirstOrDefault().Employee.Person.LastName,
                        PrimaryEmailAddress = x.FirstOrDefault().Employee.Person.PrimaryEmailAddress,
                    },
                },
            });

            var list = new PagedList<LeaveCredit>(query, page, pageSize);
            return list;
        }
        #endregion

        #region Find leavecredits By employee id
        public decimal GetLeaveCreditsByEmployeeId(string employeeId, int year)
        {
            decimal leavecredits = 0;
            // Check if the employeeId is not provided
            if (employeeId != null)
            {
                leavecredits = _leaveCreditRepository.Table
            .Where(x => !x.Deleted && x.EmployeeId == employeeId && x.LeaveCreditsforYear == year) // Filter by employeeId and not deleted records
            .Sum(x => x.CasualLeaves + x.SickLeaves);
                //.Sum(x => x.CreditLeave);
            }
            // Return leave credits as a string
            return leavecredits;
        }

        public async Task<LeaveCredit> GetLeaveCreditsOfYearByEmployeeId(string employeeId, int year)
        {
            var leaveCredits = _leaveCreditRepository.TableNoTracking.Where(x => !x.Deleted && x.EmployeeId == employeeId && x.LeaveCreditsforYear == year).FirstOrDefault();
            return leaveCredits;
        }
        #endregion

        #region Find all leavecredits By employee id
        public async Task<(decimal TotalLeaves, decimal CasualLeaves, decimal SickLeaves)> GetAllLeaveCreditsByEmployeeId(string employeeId, int year)
        {
            // Calculate total, casual, and sick leave credits
            var totalLeaveCredits = await _leaveCreditRepository.Table
                .Where(x => !x.Deleted && x.EmployeeId == employeeId && x.LeaveCreditsforYear == year)
                .SumAsync(x => x.CasualLeaves + x.SickLeaves);

            var casualLeaveCredits = await _leaveCreditRepository.Table
                .Where(x => !x.Deleted && x.EmployeeId == employeeId && x.LeaveCreditsforYear == year)
                .SumAsync(x => x.CasualLeaves);

            var sickLeaveCredits = await _leaveCreditRepository.Table
                .Where(x => !x.Deleted && x.EmployeeId == employeeId && x.LeaveCreditsforYear == year)
                .SumAsync(x => x.SickLeaves);

            return (totalLeaveCredits, casualLeaveCredits, sickLeaveCredits);
        }

        #endregion

        #region Find PaidLeaveCredits and UnpaidLeaveCredits By employee id
        public (decimal PaidLeaveCredits, decimal UnpaidLeaveCredits) GetLeaveCreditsByEmployeeIdandType(string SiteId, string employeeId, int year)
        {
            decimal paidLeaveCredits = 0;
            decimal unpaidLeaveCredits = 0;
            if (!string.IsNullOrEmpty(employeeId))
            {
                // Get the type IDs for Paid and Unpaid leaves
                var paidLeaveType = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Leave Type", "Paid");
                var unpaidLeaveType = _commonService.GetDrownValueIdByTypeandValue(SiteId, "Leave Type", "Unpaid");

                // Calculate paid leave credits
                paidLeaveCredits = _leaveCreditRepository.Table
                    .Where(x => !x.Deleted && x.EmployeeId == employeeId && x.LeaveCreditsforYear == year && x.LeaveTypeId == paidLeaveType)
                    .Sum(x => (x.CasualLeaves) + (x.SickLeaves));

                // Calculate unpaid leave credits
                unpaidLeaveCredits = _leaveCreditRepository.Table
                    .Where(x => !x.Deleted && x.EmployeeId == employeeId && x.LeaveCreditsforYear == year && x.LeaveTypeId == unpaidLeaveType)
                    .Sum(x => (x.CasualLeaves) + (x.SickLeaves));
            }
            return (paidLeaveCredits, unpaidLeaveCredits);
        }
        #endregion

        #region GetLeaveCreditById
        // Title: GetLeaveCreditById
        // Description: This method retrieves a LeaveCredit from the database by its unique identifier (`id`). 
        public async Task<LeaveCredit> GetLeaveCreditById(string id)
        {
            var query = _leaveCreditRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetLeaveCreditByEmployeeIdandCreditReason
        // Title: GetLeaveCreditByEmployeeIdandCreditReason
        // Description: This method retrieves a LeaveCredit from the database by its unique identifier (`id`). 
        public async Task<LeaveCredit> GetLeaveCreditByEmployeeIdandCreditReason(string employeeId, string CreditReason)
        {
            var query = _leaveCreditRepository.TableNoTracking.Where(x => !x.Deleted && x.EmployeeId == employeeId && x.CreditReason == CreditReason);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetLeaveCreditsByEmployeeData
        // Title: GetLeaveCreditsByEmployeeData
        // Description: This method retrieves a LeaveCredit from the database by employee data. 
        //public async Task<LeaveCredit> GetLeaveCreditsByEmployeeData(string employeeId, string creditReason, int year)
        //{
        //    var query = _leaveCreditRepository.TableNoTracking.Where(x => !x.Deleted && x.EmployeeId == employeeId && x.CreditReason == creditReason && x.LeaveCreditsforYear == year);
        //    var item = await query.FirstOrDefaultAsync();
        //    return item;
        //}
        public async Task<LeaveCredit> GetLeaveCreditsByEmployeeData(string employeeId, int year)
        {
            var query = _leaveCreditRepository.Table.Where(x => !x.Deleted && x.EmployeeId == employeeId && x.LeaveCreditsforYear == year && x.IsDefault);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetLeaveCreditByEmployeeId
        // Title: GetLeaveCreditByEmployeeId
        // Description: This method retrieves a LeaveCredit based on its employeeId.The method returns the first matching LeaveCredit or null if no match is found.
        public async Task<List<LeaveCredit>> GetLeaveCreditByEmployeeId(string employeeId, int year)
        {
            var query = _leaveCreditRepository.TableNoTracking.Where(m => !m.Deleted && m.EmployeeId == employeeId && m.LeaveCreditsforYear == year);
            query = query.Select(x => new LeaveCredit
            {
                Id = x.Id,
                CasualLeaves = x.CasualLeaves,
                SickLeaves = x.SickLeaves,
                CreditReason = x.CreditReason,
                CreatedOnUtc = x.CreatedOnUtc,

                LeaveTypes = new DropDown
                {
                    Id = x.LeaveTypes.Id,
                    DropDownValue = x.LeaveTypes.DropDownValue
                },
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FirstName = x.CreatedBy.Person.FirstName,
                        LastName = x.CreatedBy.Person.LastName,
                    }
                }
            });
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetLeaveCreditByEmployeeIdByType
        // Title: GetLeaveCreditByEmployeeId
        // Description: This method retrieves a LeaveCredit based on its employeeId.The method returns the first matching LeaveCredit or null if no match is found.
        public async Task<List<LeaveCredit>> GetLeaveCreditByEmployeeIdByType(string employeeId, int year, string leaveTypeId)
        {
            var query = _leaveCreditRepository.TableNoTracking.Where(m => m.EmployeeId == employeeId && m.LeaveCreditsforYear == year && m.LeaveTypeId == leaveTypeId);
            query = query.Select(x => new LeaveCredit
            {
                Id = x.Id,
                CasualLeaves = x.CasualLeaves,
                SickLeaves = x.SickLeaves,
                CreditReason = x.CreditReason,
                CreatedOnUtc = x.CreatedOnUtc,

                LeaveTypes = new DropDown
                {
                    Id = x.LeaveTypes.Id,
                    DropDownValue = x.LeaveTypes.DropDownValue
                },
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FirstName = x.CreatedBy.Person.FirstName,
                        LastName = x.CreatedBy.Person.LastName,
                    }
                }
            });
            return await query.ToListAsync();
        }
        #endregion

        #region GetLeaveCreditDetailsById
        // Title: GetLeaveCreditDetailsById
        // Description: The method selects relevant fields from the LeaveCredit entity.
        public async Task<LeaveCredit> GetLeaveCreditDetailsById(string id)
        {
            var query = _leaveCreditRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new LeaveCredit
            {
                Id = x.Id,
                CasualLeaves = x.CasualLeaves,
                SickLeaves = x.SickLeaves,
                CreditReason = x.CreditReason,
                LeaveCreditsforYear = x.LeaveCreditsforYear,
                EmployeeId = x.EmployeeId,
                Note = x.Note,
                IsDefault = x.IsDefault,
                LeaveTypes = new DropDown
                {
                    Id = x.LeaveTypes.Id,
                    DropDownValue = x.LeaveTypes.DropDownValue
                },

                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    //SiteId = x.Employee.SiteId,
                    PersonId = x.Employee.PersonId,
                    EmployeeCode = x.Employee.EmployeeCode,
                    OfficialEmail = x.Employee.OfficialEmail,
                    EmergencyContactName = x.Employee.EmergencyContactName,
                    EmergencyPhoneNo = x.Employee.EmergencyPhoneNo,
                    SameASPermanentAddress = x.Employee.SameASPermanentAddress,
                    AadhaarCardNo = x.Employee.AadhaarCardNo,
                    PanCardNo = x.Employee.PanCardNo,
                    EPFUANNo = x.Employee.EPFUANNo,
                    JoiningDate = x.Employee.JoiningDate,
                    ReleaseDate = x.Employee.ReleaseDate,
                    EducationDetail = x.Employee.EducationDetail,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FirstName = x.Employee.Person.FirstName,
                        LastName = x.Employee.Person.LastName,
                        PrimaryEmailAddress = x.Employee.Person.PrimaryEmailAddress,
                    },
                },
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertLeaveCredit
        // Title: InsertLeaveCredit
        // Description: This method inserts a new LeaveCredit entity into the repository. It takes a LeaveCredit object as input and uses the _leaveCreditRepository to handle the insertion operation.
        public void InsertLeaveCredit(LeaveCredit entity)
        {
            _leaveCreditRepository.Insert(entity);
        }
        #endregion

        #region UpdateLeaveCredit
        // Title: UpdateLeaveCredit
        // Description: This method updates the specified LeaveCredit entity in the repository. It takes a LeaveCredit object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateLeaveCredit(LeaveCredit entity)
        {
            _leaveCreditRepository.Update(entity);
        }
        #endregion

        #region DeleteLeaveCredit
        // Title: DeleteLeaveCredit
        // Description: Marks the specified LeaveCredit entity as deleted by setting its `Deleted` property to true. 
        public void DeleteLeaveCredit(LeaveCredit entity)
        {
            entity.Deleted = true;

            _leaveCreditRepository.Update(entity);
        }
        #endregion
    }
}
