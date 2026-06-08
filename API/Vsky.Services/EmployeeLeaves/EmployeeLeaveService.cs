using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Employees;
using Vsky.Services.Sites;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Services.EmployeeLeaves
{
    public class EmployeeLeaveService : IEmployeeLeaveService
    {
        #region Define Services
        private readonly IRepository<EmployeeLeave> _employeeLeaveRepository;
        private readonly IRepository<LeaveSchedules> _leaveSchedulesRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISiteService _siteService;
        private readonly ApplicationDbContext _db;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations

        public EmployeeLeaveService(
            IRepository<EmployeeLeave> employeeLeaveRepository,
            IRepository<LeaveSchedules> leaveSchedulesRepository,
            UserManager<ApplicationUser> userManager,
            ISiteService siteService,
            ApplicationDbContext db,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _employeeLeaveRepository = employeeLeaveRepository;
            _leaveSchedulesRepository = leaveSchedulesRepository;
            _userManager = userManager;
            _siteService = siteService;
            _db = db;
            _applicationUserRoleService = applicationUserRoleService;
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

        #region GetAllEmployeeLeave
        // Title: GetAllEmployeeLeave
        // Description: This method retrieves a paginated list of EmployeeLeave based on various search criteria such as employee name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<EmployeeLeave> GetAllEmployeeLeave(string SiteId, string logginuser, string createdBy, string SearchText, string Flag, List<string> employeeIds, List<string> statusIds, List<string> leaveCategoryId, DateTime? createdOnUtc, string leaveMonthStr, int years, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {

            var query = _employeeLeaveRepository.TableNoTracking.Where(x => !x.Deleted && x.Employee.SiteId == SiteId);
            //Get user by userid
            var userdata = _userManager.FindByIdAsync(logginuser).GetAwaiter().GetResult();
            var user = _userManager.FindByNameAsync(userdata.UserName).GetAwaiter().GetResult();
            if (user != null && !user.Deleted && user.Active)
            {
                //Get user roles
                var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
                // Fetch the NormalizedName of each role
                //var normalizedRoles = _db.Roles.Where(r => roles.Contains(r.Name)).Select(r => r.NormalizedName).ToArray();
                var normalizedRoles = _applicationUserRoleService
                    .GetNormalizedRoleNamesByUserAndSite(user.Id, SiteId)
                    .GetAwaiter()
                    .GetResult()
                    .ToArray();

                if (Flag != "FD")
                {
                    if (!normalizedRoles.Contains("admin"))
                    {
                        if (createdBy != null)
                            query = query.Where(x => x.EmployeeId == createdBy);
                    }
                }
                else
                {
                    if (!normalizedRoles.Contains("hr"))
                    {
                        if (createdBy != null)
                            query = query.Where(x => x.EmployeeId == createdBy);
                    }
                }
            }

            if (employeeIds != null && employeeIds.Any())
                query = query.Where(x => employeeIds.Contains(x.EmployeeId));

            if (statusIds != null && statusIds.Any())
                query = query.Where(x => statusIds.Contains(x.LeaveStatusId));

            if (leaveCategoryId != null && leaveCategoryId.Any())
                query = query.Where(x => leaveCategoryId.Contains(x.LeaveCategoryId));

            if (createdOnUtc != null)
                query = query.Where(x => x.CreatedOnUtc.Date == createdOnUtc);

            if (years != 0)
                query = query.Where(x => x.FromDate.Year == years);
            if (!string.IsNullOrWhiteSpace(leaveMonthStr))
            {
                // Combine month + year
                DateTime monthStart = DateTime.ParseExact($"{leaveMonthStr} {years}", "MMMM yyyy", null);
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                query = query.Where(x =>
                    (x.FromDate <= monthEnd && x.ToDate >= monthStart)
                );
            }
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                (m.Employee.Person.FirstName + " " + m.Employee.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                m.FromDate.Date == parsedDate.Date ||
                m.ToDate.Date == parsedDate.Date ||
                m.NoofLeaves.ToString().ToLower().Contains(SearchText.ToLower()) ||
                m.LeaveStatuses.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                m.HalfDayType.ToLower().Contains(SearchText.ToLower()) ||
                m.LeaveCategories.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                m.CreatedOnUtc.Date == parsedDate.Date ||
                m.Reason.Contains(SearchText.ToLower())
                );
            }
            query = query.Select(x => new EmployeeLeave
            {
                Id = x.Id,
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                NoofLeaves = x.NoofLeaves,
                Reason = x.Reason,
                ApproverNote = x.ApproverNote,
                HRNote = x.HRNote,
                IsPaidLeave = x.IsPaidLeave,
                HalfDayType = x.HalfDayType,
                IsSandwich = x.IsSandwich,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                LeaveStatuses = new DropDown
                {
                    Id = x.LeaveStatuses.Id,
                    DropDownValue = x.LeaveStatuses.DropDownValue
                },
                LeaveCategories = new DropDown
                {
                    Id = x.LeaveCategories.Id,
                    DropDownValue = x.LeaveCategories.DropDownValue
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
                LeaveApprover = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FirstName = x.Employee.Person.FirstName,
                        LastName = x.Employee.Person.LastName,
                    },
                },
                File = new Picture
                {
                    Id = x.File.Id,
                    VirtualPath = x.File.VirtualPath,
                    SeoFilename = x.File.SeoFilename
                }
            });

            var list = new PagedList<EmployeeLeave>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetAllEmployeeLeaveForApprove
        // Title: Get All EmployeeLeave For Approve
        // Description: This method retrieves a paginated list of EmployeeLeave based on various search criteria such as employee name, status 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<EmployeeLeave> GetAllEmployeeLeaveForApprove(
            string SiteId,
            string employeeId,
            string SearchText,
            List<string> personIds,
            List<string> statusIds,
            DateTime? createdOnUtc,
            string leaveMonthStr,
            int years,
            List<string> leaveCategoryId,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _employeeLeaveRepository.TableNoTracking.Where(x => !x.Deleted && x.Employee.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(employeeId))
                query = query.Where(x => x.LeaveApproverId == employeeId);

            if (personIds != null && personIds.Any())
                query = query.Where(x => personIds.Contains(x.Employee.Id));


            if (statusIds != null && statusIds.Any())
                query = query.Where(x => statusIds.Contains(x.LeaveStatusId));

            if (createdOnUtc != null)
                query = query.Where(x => x.CreatedOnUtc.Date == createdOnUtc);

            if (years != 0)
                query = query.Where(x => x.CreatedOnUtc.Year == years);

            if (leaveCategoryId != null && leaveCategoryId.Any())
                query = query.Where(x => leaveCategoryId.Contains(x.LeaveCategoryId));

            if (!string.IsNullOrWhiteSpace(leaveMonthStr))
            {
                DateTime monthStart = DateTime.ParseExact(leaveMonthStr, "MMMM", null);
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                query = query.Where(x =>
                    (x.FromDate <= monthEnd && x.ToDate >= monthStart)
                );
            }


            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                m.Employee.EmployeeCode.Contains(SearchText.ToLower()) ||
                (m.Employee.Person.FirstName + " " + m.Employee.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                m.FromDate.Date == parsedDate.Date ||
                m.ToDate.Date == parsedDate.Date ||
                m.LeaveStatuses.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                m.LeaveCategories.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                m.HalfDayType.ToLower().Contains(SearchText.ToLower()) ||
                m.CreatedOnUtc.Date == parsedDate.Date ||
                m.Reason.ToLower().Contains(SearchText.ToLower())
               );
            }
            query = query.Select(x => new EmployeeLeave
            {
                Id = x.Id,
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                NoofLeaves = x.NoofLeaves,
                Reason = x.Reason,
                ApproverNote = x.ApproverNote,
                HRNote = x.HRNote,
                IsPaidLeave = x.IsPaidLeave,
                HalfDayType = x.HalfDayType,
                IsSandwich = x.IsSandwich,
                CreatedOnUtc = x.CreatedOnUtc,

                LeaveStatuses = new DropDown
                {
                    Id = x.LeaveStatuses.Id,
                    DropDownValue = x.LeaveStatuses.DropDownValue
                },
                LeaveCategories = new DropDown
                {
                    Id = x.LeaveCategories.Id,
                    DropDownValue = x.LeaveCategories.DropDownValue
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
                LeaveApprover = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FirstName = x.Employee.Person.FirstName,
                        LastName = x.Employee.Person.LastName,
                    },
                },
                File = new Picture
                {
                    Id = x.File.Id,
                    SeoFilename = x.File.SeoFilename
                }
            });

            var list = new PagedList<EmployeeLeave>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetFiveEmployeeLeaveForApprove
        public async Task<List<EmployeeLeave>> GetFiveEmployeeLeaveForApprove(string SiteId, string employeeId, string leavestatus, string activeEmployeeStatus, string exEmployeeStatus)
        {
            //var query = _employeeLeaveRepository.TableNoTracking.Where(x => !x.Deleted && x.LeaveStatusId == leavestatus);
            var query = _employeeLeaveRepository.TableNoTracking.Where(x => !x.Deleted && x.Employee.SiteId == SiteId && x.FromDate.Year >= DateTime.UtcNow.Year && x.Employee.ReleaseDate == null && x.LeaveStatusId == leavestatus);

            if (!string.IsNullOrWhiteSpace(employeeId))
                query = query.Where(x => x.LeaveApproverId == employeeId);

            query = query.Where(x => x.Employee.EmployeeStatuses.Where(s => !s.Deleted).OrderByDescending(s => s.StartDate).FirstOrDefault().EmployeeStatusId == activeEmployeeStatus);

            query = query.Where(x => !x.Employee.EmployeeStatuses.Where(m => !m.Deleted).Any(m => m.EmployeeStatusId == exEmployeeStatus && m.StartDate > x.Employee.EmployeeStatuses.Where(s => !s.Deleted).OrderByDescending(s => s.StartDate).FirstOrDefault().StartDate));

            query = query.Select(x => new EmployeeLeave
            {
                Id = x.Id,
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                NoofLeaves = x.NoofLeaves,
                Reason = x.Reason,
                HRNote = x.HRNote,
                HalfDayType = x.HalfDayType,
                IsPaidLeave = x.IsPaidLeave,
                CreatedOnUtc = x.CreatedOnUtc,
                LeaveStatuses = new DropDown
                {
                    Id = x.LeaveStatuses.Id,
                    DropDownValue = x.LeaveStatuses.DropDownValue
                },
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    PersonId = x.Employee.PersonId,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FirstName = x.Employee.Person.FirstName,
                        LastName = x.Employee.Person.LastName
                    }
                },
                LeaveApprover = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FirstName = x.Employee.Person.FirstName,
                        LastName = x.Employee.Person.LastName,
                    },
                },
                File = new Picture
                {
                    Id = x.File.Id,
                    VirtualPath = x.File.VirtualPath,
                    SeoFilename = x.File.SeoFilename
                }
            });

            // Use .Take(5) to limit the query to only 5 records
            query = query.Take(4);
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetEmployeeLeaveListForDashboard
        public async Task<List<EmployeeLeave>> GetEmployeeLeaveListForDashboard(string SiteId, DateTime GetDateTime)
        {
            var endDate = GetDateTime.AddDays(7);
            var query = _employeeLeaveRepository.TableNoTracking.Where(x => !x.Deleted && x.Employee.SiteId == SiteId && x.LeaveStatuses.DropDownValue != "Cancelled" && x.LeaveStatuses.DropDownValue != "Decline");
            query = query.Where(x => x.FromDate.Date <= endDate && x.ToDate.Date >= GetDateTime);
            query = query.Where(x => x.Employee.EmployeeStatuses.Where(s => !s.Deleted).OrderByDescending(s => s.StartDate).FirstOrDefault().Status.DropDownValue == "Current");
            query = query.Where(x => !x.Employee.EmployeeStatuses.Where(m => !m.Deleted).Any(m => m.Status.DropDownValue == "Ex-Employee" && m.StartDate > x.Employee.EmployeeStatuses.Where(s => !s.Deleted).OrderByDescending(s => s.StartDate).FirstOrDefault().StartDate));

            query = query.Select(x => new EmployeeLeave
            {
                Id = x.Id,
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                NoofLeaves = x.NoofLeaves,
                HalfDayType = x.HalfDayType,
                Reason = x.Reason,
                CreatedOnUtc = x.CreatedOnUtc,
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    PersonId = x.Employee.PersonId,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FirstName = x.Employee.Person.FirstName,
                        LastName = x.Employee.Person.LastName
                    }
                },
                LeaveStatuses = new DropDown
                {
                    Id = x.LeaveStatuses.Id,
                    DropDownValue = x.LeaveStatuses.DropDownValue
                }
            });

            query = query.OrderBy(x => x.FromDate).ThenByDescending(x => x.LeaveStatuses.DropDownValue == "Approved");

            var list = await query.ToListAsync();
            return list;
        }
        public async Task<List<EmployeeLeave>> GetEmployeeLeaveListForMovReg(string siteId, string dateStr, DateTime GetDateTime)
        {
            if (!string.IsNullOrWhiteSpace(dateStr) && DateTime.TryParse(dateStr, out var parsedDate))
            {
                GetDateTime = parsedDate.Date;
            }

            var invalidStatuses = new[] { "Cancelled", "Decline" };

            // Get latest leave Id per employee
            var latestLeaveIds = await _employeeLeaveRepository.TableNoTracking
                .Where(l =>
                    !l.Deleted &&
                    l.Employee.SiteId == siteId &&
                    !invalidStatuses.Contains(l.LeaveStatuses.DropDownValue) &&
                    l.FromDate.Date <= GetDateTime &&
                    l.ToDate.Date >= GetDateTime &&

                    // Latest employee status must be Active
                    l.Employee.EmployeeStatuses.Any(s =>
                        !s.Deleted &&
                        s.Status.DropDownValue == "Current" &&
                        s.StartDate ==
                            l.Employee.EmployeeStatuses
                                .Where(es => !es.Deleted)
                                .Max(es => es.StartDate)
                    )
                )
                .GroupBy(l => l.Employee.Id)
                .Select(g => g
                    .OrderBy(x => x.CreatedOnUtc)
                    .Select(x => x.Id)
                    .First()
                )
                .ToListAsync();

            if (!latestLeaveIds.Any())
                return new List<EmployeeLeave>();

            // Load full leave details
            var result = await _employeeLeaveRepository.TableNoTracking
                .Where(l => latestLeaveIds.Contains(l.Id))
                .Select(l => new EmployeeLeave
                {
                    Id = l.Id,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    NoofLeaves = l.NoofLeaves,
                    HalfDayType = l.HalfDayType,
                    Reason = l.Reason,

                    Employee = new Employee
                    {
                        Id = l.Employee.Id,
                        PersonId = l.Employee.PersonId,
                        Person = new Person
                        {
                            Id = l.Employee.Person.Id,
                            FirstName = l.Employee.Person.FirstName,
                            LastName = l.Employee.Person.LastName
                        }
                    },

                    LeaveStatuses = new DropDown
                    {
                        Id = l.LeaveStatuses.Id,
                        DropDownValue = l.LeaveStatuses.DropDownValue
                    }
                })
                .OrderBy(l => l.FromDate)
                .ThenByDescending(l => l.LeaveStatuses.DropDownValue == "Approved")
                .ToListAsync();

            return result;
        }
        #endregion

        #region Find usedleave By employee id
        public decimal GetUsedLeaveByEmployeeId(string employeeId, int year)
        {
            decimal usedleave = 0;

            if (employeeId != null)
            {
                usedleave = _employeeLeaveRepository.Table
            .Where(x => !x.Deleted && x.EmployeeId == employeeId && x.ToDate.Year == year && x.LeaveStatuses != null && x.LeaveStatuses.DropDownValue != "Decline"
            && x.LeaveStatuses.DropDownValue != "Cancelled") // Filter by employeeId and not deleted records
            .Sum(x => x.NoofLeaves);
            }
            // Return leave credits as a string
            return usedleave;
        }
        #endregion

        #region GetUsedLeaveByEmployeeIdAndLeaveCategoryId
        public decimal GetUsedLeaveByEmployeeIdAndLeaveCategoryId(string employeeId, int year, string leaveCategoryId, string id = null)
        {
            decimal usedleave = 0;

            if (employeeId != null)
            {
                usedleave = _employeeLeaveRepository.Table
            .Where(x => !x.Deleted && x.EmployeeId == employeeId && x.ToDate.Year == year && x.LeaveCategoryId == leaveCategoryId && x.LeaveStatuses != null && x.LeaveStatuses.DropDownValue != "Decline"
            && x.LeaveStatuses.DropDownValue != "Cancelled" && (string.IsNullOrWhiteSpace(id) || x.Id != id)) // Filter by employeeId and not deleted records
            .Sum(x => x.NoofLeaves);
            }
            // Return leave credits as a string
            return usedleave;
        }
        #endregion

        #region GetEmployeeLeaveById
        // Title: GetEmployeeLeaveById
        // Description: This method retrieves a EmployeeLeave from the database by its unique identifier (`id`). 
        public async Task<EmployeeLeave> GetEmployeeLeaveById(string id)
        {
            var query = _employeeLeaveRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetEmployeeLeaveByEmployeeId
        // Title: GetEmployeeLeaveByEmployeeId
        // Description: This method retrieves a EmployeeLeave based on its employeeId.The method returns the first matching EmployeeLeave or null if no match is found.
        public async Task<List<EmployeeLeave>> GetEmployeeLeaveByEmployeeId(string employeeId)
        {
            var query = _employeeLeaveRepository.TableNoTracking.Where(m => m.EmployeeId == employeeId);
            query = query.Select(x => new EmployeeLeave
            {
                Id = x.Id,
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                NoofLeaves = x.NoofLeaves,
                Reason = x.Reason,
                ApproverNote = x.ApproverNote,
                HRNote = x.HRNote,
                IsPaidLeave = x.IsPaidLeave,
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetEmployeeLeaveDetailsById
        // Title: GetEmployeeLeaveDetailsById
        // Description: The method selects relevant fields from the EmployeeLeave entity.
        public async Task<EmployeeLeave> GetEmployeeLeaveDetailsById(string id)
        {
            var query = _employeeLeaveRepository.Table;
            query = query.Where(x => x.Id == id);
            query = query.Where(x => !x.Deleted && x.Employee.Active);
            query = query.Select(x => new EmployeeLeave
            {
                Id = x.Id,
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                NoofLeaves = x.NoofLeaves,
                Reason = x.Reason,
                ApproverNote = x.ApproverNote,
                HRNote = x.HRNote,
                IsPaidLeave = x.IsPaidLeave,
                IsHalfDay = x.IsHalfDay,
                HalfDayType = x.HalfDayType,
                IsSandwich = x.IsSandwich,
                CreatedOnUtc = x.CreatedOnUtc,

                LeaveStatuses = new DropDown
                {
                    Id = x.LeaveStatuses.Id,
                    DropDownValue = x.LeaveStatuses.DropDownValue
                },
                LeaveCategories = new DropDown
                {
                    Id = x.LeaveCategories.Id,
                    DropDownValue = x.LeaveCategories.DropDownValue
                },
                LeaveApprover = new Employee
                {
                    Person = new Person
                    {
                        Id = x.LeaveApprover.Person.Id,
                        FirstName = x.LeaveApprover.Person.FirstName,
                        LastName = x.LeaveApprover.Person.LastName,
                    }
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
                        PrimaryPhoneNumber = x.Employee.Person.PrimaryPhoneNumber,
                    }
                },
                File = new Picture
                {
                    Id = x.File.Id,
                    VirtualPath = x.File.VirtualPath,
                    SeoFilename = x.File.SeoFilename
                }
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllEmployeeLeaves
        public async Task<List<EmployeeLeave>> GetAllEmployeeLeaves(string employeeId)
        {
            var employeeLeaves = await _employeeLeaveRepository
                .TableNoTracking
                .Where(x =>
                    !x.Deleted &&
                    x.EmployeeId == employeeId &&
                    x.LeaveStatuses != null &&
                    x.LeaveStatuses.DropDownValue != "Decline" &&
                    x.LeaveStatuses.DropDownValue != "Cancelled" &&
                            Convert.ToDecimal(x.NoofLeaves) != 0.5m)
                .ToListAsync();

            var expandedLeaves = new List<EmployeeLeave>();

            foreach (var leave in employeeLeaves)
            {
                if (leave.FromDate == null || leave.ToDate == null)
                    continue;

                for (var date = leave.FromDate.Date; date <= leave.ToDate.Date; date = date.AddDays(1))
                {
                    expandedLeaves.Add(new EmployeeLeave
                    {
                        Id = leave.Id,
                        FromDate = date,
                        ToDate = date
                    });
                }
            }

            return expandedLeaves
                .OrderBy(x => x.FromDate)
                .ToList();
        }
        #endregion

        #region GetEmployeeLeavesThatIncludeDates
        /// GetEmployeeLeavesThatIncludeDates
        /// Returns all employee leave entries whose date range includes
        /// ANY date from the provided date list.
        public async Task<List<EmployeeLeave>> GetEmployeeLeavesThatIncludeDates(string employeeId, List<DateTime> targetDates, string id = null)
        {
            targetDates = targetDates.Select(d => d.Date).ToList();

            var employeeLeaves = await _employeeLeaveRepository
                .TableNoTracking
                .Where(x =>
                    !x.Deleted &&
                    x.EmployeeId == employeeId &&
                    x.LeaveStatuses != null &&
                    x.LeaveStatuses.DropDownValue != "Decline" &&
                    x.LeaveStatuses.DropDownValue != "Cancelled" &&
                    x.IsSandwich)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(id))
                employeeLeaves = employeeLeaves.Where(x => x.Id != id).ToList();

            var matchingLeaves = employeeLeaves
                .Where(l =>
                {
                    var leaveFrom = l.FromDate.Date;
                    var leaveTo = l.ToDate.Date;

                    return targetDates.Any(d => d >= leaveFrom && d <= leaveTo);
                })
                .OrderBy(l => l.CreatedOnUtc).Select(l => new EmployeeLeave
                {
                    Id = l.Id,
                    NoofLeaves = l.NoofLeaves,
                    FromDate = l.FromDate.Date,
                    ToDate = l.ToDate.Date
                })
                .ToList();

            return matchingLeaves;
        }
        #endregion

        #region
        //public async Task<bool> CheckPreviousHoliday(string siteId, string employeeId, DateTime startDate)
        //{
        //    var prevDate = startDate.AddDays(-1);

        //    bool isEmployeeLeave = await _employeeLeaveRepository.TableNoTracking
        //        .AnyAsync(x => !x.Deleted &&
        //                       x.EmployeeId == employeeId &&
        //                       x.LeaveStatuses != null &&
        //                       x.LeaveStatuses.DropDownValue != "Decline" &&
        //                       x.LeaveStatuses.DropDownValue != "Cancelled" &&
        //                       Convert.ToDecimal(x.NoofLeaves) != 0.5m &&
        //                       (x.FromDate.Date == prevDate.Date || x.ToDate.Date == prevDate.Date));

        //    if(isEmployeeLeave)
        //        prevDate = prevDate.AddDays(-1);

        //    bool isSunday = prevDate.DayOfWeek == DayOfWeek.Sunday;

        //    bool isOfficeLeave = await _leaveSchedulesRepository.TableNoTracking
        //        .AnyAsync(x => !x.Deleted && x.SiteId == siteId && x.Date.HasValue && x.Date.Value.Date == prevDate.Date);

        //    return isSunday || isOfficeLeave;
        //}

        public async Task<bool> CheckPreviousHoliday(string siteId, string employeeId, DateTime startDate)
        {
            var prevDate = startDate.AddDays(-1);

            // Get previous day's leave
            var prevLeave = await _employeeLeaveRepository.TableNoTracking
                .Where(x => !x.Deleted &&
                            x.EmployeeId == employeeId &&
                            x.LeaveStatuses != null &&
                            x.LeaveStatuses.DropDownValue != "Decline" &&
                            x.LeaveStatuses.DropDownValue != "Cancelled" &&
                            Convert.ToDecimal(x.NoofLeaves) != 0.5m &&
                            (x.FromDate.Date == prevDate.Date || x.ToDate.Date == prevDate.Date))
                .FirstOrDefaultAsync();

            bool isOfficeLeave = false;

            // Check if previous day is employee leave
            if (prevLeave != null)
            {
                // If the previous leave was a sandwich leave, treat it as office leave
                if (prevLeave.IsSandwich)
                    isOfficeLeave = true;

                // Move one more day back
                prevDate = prevDate.AddDays(-1);
            }

            // Check if the previous day is a Sunday
            bool isSunday = prevDate.DayOfWeek == DayOfWeek.Sunday;

            // Check if it's an official office leave/holiday
            bool isScheduledOfficeLeave = await _leaveSchedulesRepository.TableNoTracking
                .AnyAsync(x => !x.Deleted &&
                               x.SiteId == siteId &&
                               x.Date.HasValue &&
                               x.Date.Value.Date == prevDate.Date);

            // Return true if previous day is Sunday or office leave
            return isSunday || isOfficeLeave || isScheduledOfficeLeave;
        }

        //public async Task<bool> CheckNextHoliday(string siteId, string employeeId, DateTime endDate)
        //{
        //    var nextDate = endDate.AddDays(1);

        //    bool isEmployeeLeave = await _employeeLeaveRepository.TableNoTracking
        //        .AnyAsync(x => !x.Deleted &&
        //                       x.EmployeeId == employeeId &&
        //                       x.LeaveStatuses != null &&
        //                       x.LeaveStatuses.DropDownValue != "Decline" &&
        //                       x.LeaveStatuses.DropDownValue != "Cancelled" &&
        //                       Convert.ToDecimal(x.NoofLeaves) != 0.5m &&
        //                       (x.FromDate.Date == nextDate.Date || x.ToDate.Date == nextDate.Date));

        //    if (isEmployeeLeave)
        //        nextDate = nextDate.AddDays(1);

        //    bool isSunday = nextDate.DayOfWeek == DayOfWeek.Sunday;

        //    bool isOfficeLeave = await _leaveSchedulesRepository.TableNoTracking
        //        .AnyAsync(x => !x.Deleted && x.SiteId == siteId && x.Date.HasValue && x.Date.Value.Date == nextDate.Date);

        //    return isSunday || isOfficeLeave;
        //}

        //public async Task<SandwichLeaveResult> IsSandwichLeave(string siteId, string employeeId, DateTime startDate, DateTime endDate)
        //{
        //    // Check previous and next holidays
        //    bool hasPrevHoliday = await CheckPreviousHoliday(siteId, employeeId, startDate);
        //    bool hasNextHoliday = await CheckNextHoliday(siteId, employeeId, endDate);

        //    // Build complete holiday list (office + Sundays)
        //    var officeLeaves = await _leaveSchedulesRepository.TableNoTracking
        //        .Where(x => !x.Deleted && x.SiteId == siteId && x.Date.HasValue)
        //        .Select(x => x.Date.Value.Date)
        //        .ToListAsync();

        //    var holidays = officeLeaves.ToHashSet();

        //    // add sundays's in holidays
        //    DateTime minCheck = startDate.AddDays(-30);
        //    DateTime maxCheck = endDate.AddDays(30);
        //    for (var d = minCheck; d <= maxCheck; d = d.AddDays(1))
        //        if (d.DayOfWeek == DayOfWeek.Sunday)
        //            holidays.Add(d);

        //    // employee leaves (non-declined, non-cancelled, non-half day)
        //    var employeeLeaves = await _employeeLeaveRepository.TableNoTracking
        //        .Where(x => !x.Deleted &&
        //                    x.EmployeeId == employeeId &&
        //                    x.LeaveStatuses != null &&
        //                    x.LeaveStatuses.DropDownValue != "Decline" &&
        //                    x.LeaveStatuses.DropDownValue != "Cancelled" &&
        //                    Convert.ToDecimal(x.NoofLeaves) != 0.5m)
        //        .Select(x => new { x.FromDate, x.ToDate })
        //        .ToListAsync();

        //    var leaveDays = new HashSet<DateTime>();
        //    foreach (var l in employeeLeaves)
        //        for (var d = l.FromDate.Date; d <= l.ToDate.Date; d = d.AddDays(1))
        //            leaveDays.Add(d);

        //    var nonWorkingDays = new HashSet<DateTime>(holidays.Union(leaveDays));

        //    // Determine sandwich condition
        //    bool isSandwich = hasPrevHoliday && hasNextHoliday;

        //    DateTime seriesStart = startDate;
        //    DateTime seriesEnd = endDate;

        //    // If sandwich, expand the continuous non-working series
        //    if (isSandwich)
        //    {
        //        var prev = startDate.AddDays(-1);
        //        while (nonWorkingDays.Contains(prev))
        //        {
        //            seriesStart = prev;
        //            prev = prev.AddDays(-1);
        //        }

        //        var next = endDate.AddDays(1);
        //        while (nonWorkingDays.Contains(next))
        //        {
        //            seriesEnd = next;
        //            next = next.AddDays(1);
        //        }

        //        // If leave doesn't actually fall within that continuous non-working period, reset to false
        //        if (!(startDate >= seriesStart && endDate <= seriesEnd))
        //            isSandwich = false;
        //    }

        //    // Count total days in the continuous series (if sandwich)
        //    int totalDays = 0;
        //    if (isSandwich)
        //    {
        //        for (var d = seriesStart; d <= seriesEnd; d = d.AddDays(1))
        //            if (!leaveDays.Contains(d))
        //                totalDays++;
        //    }

        //    // Return result
        //    return new SandwichLeaveResult
        //    {
        //        IsSandwich = isSandwich,
        //        SeriesStart = isSandwich ? seriesStart : (DateTime?)null,
        //        SeriesEnd = isSandwich ? seriesEnd : (DateTime?)null,
        //        TotalDays = isSandwich ? totalDays : 0
        //    };
        //}

        public async Task<SandwichLeaveResult> IsSandwichLeave(string siteId, string employeeId, DateTime startDate, DateTime endDate)
        {
            // Get office leaves and holidays
            var officeLeaves = await _leaveSchedulesRepository.TableNoTracking
                .Where(x => !x.Deleted && x.SiteId == siteId && x.Date.HasValue)
                .Select(x => x.Date.Value.Date)
                .ToListAsync();

            var holidays = officeLeaves.ToHashSet();

            // Add Sundays
            DateTime minCheck = startDate.AddDays(-30);
            DateTime maxCheck = endDate.AddDays(30);
            for (var d = minCheck; d <= maxCheck; d = d.AddDays(1))
                if (d.DayOfWeek == DayOfWeek.Sunday)
                    holidays.Add(d);

            // Get existing employee leaves
            var employeeLeaves = await _employeeLeaveRepository.TableNoTracking
                .Where(x => !x.Deleted &&
                            x.EmployeeId == employeeId &&
                            x.LeaveStatuses != null &&
                            x.LeaveStatuses.DropDownValue != "Decline" &&
                            x.LeaveStatuses.DropDownValue != "Cancelled" && Convert.ToDecimal(x.NoofLeaves) != 0.5m)
                .Select(x => new { x.FromDate, x.ToDate })
                .ToListAsync();

            var leaveDays = new HashSet<DateTime>();
            foreach (var l in employeeLeaves)
                for (var d = l.FromDate.Date; d <= l.ToDate.Date; d = d.AddDays(1))
                    leaveDays.Add(d);

            // Combine all non-working days
            var nonWorkingDays = new HashSet<DateTime>(holidays.Union(leaveDays));

            // Check if there is any non-working day before applying leave
            bool hasPrev = false;
            DateTime prev = startDate.AddDays(-1);

            while (nonWorkingDays.Contains(prev))
            {
                hasPrev = true;
                prev = prev.AddDays(-1);
            }

            // Check if there is any non-working day after applying leave
            bool hasNext = false;
            DateTime next = endDate.AddDays(1);
            while (nonWorkingDays.Contains(next))
            {
                hasNext = true;
                next = next.AddDays(1);
            }

            var isHolidayBetween = IsAnyHolidayBetween(startDate, endDate, holidays);

            //bool hasPrevOfficeLeave = holidays.Contains(startDate.AddDays(-1));
            //bool hasNextOfficeLeave = holidays.Contains(endDate.AddDays(1));
            bool hasPrevOfficeLeave = CheckPreviousNextOfficeLeave(-1, startDate, holidays, leaveDays);
            bool hasNextOfficeLeave = CheckPreviousNextOfficeLeave(1, endDate, holidays, leaveDays);

            //bool isSandwich = (hasPrev && hasNext) || (hasPrev && leaveDays.Contains(startDate.AddDays(-2))) || isHolidayBetween;
            //bool isBridging = leaveDays.Any(ld => ld < startDate && nonWorkingDays.Contains(ld.AddDays(1)) && (startDate - ld).TotalDays <= 4) && leaveDays.Any(ld => ld > endDate && nonWorkingDays.Contains(ld.AddDays(-1)));

            bool hasLeftBridge = leaveDays.Any(ld =>
            {
                if (ld >= startDate) return false;

                var daysBetween = Enumerable.Range(1, (int)(startDate - ld).TotalDays - 1)
                                            .Select(d => ld.AddDays(d))
                                            .ToList();

                return daysBetween.Count > 0
                       && daysBetween.All(nonWorkingDays.Contains)
                       && !daysBetween.Any(leaveDays.Contains);
            });

            bool hasRightBridge = leaveDays.Any(ld =>
            {
                if (ld <= endDate) return false;

                var daysBetween = Enumerable.Range(1, (int)(ld - endDate).TotalDays - 1)
                                            .Select(d => endDate.AddDays(d))
                                            .ToList();

                return daysBetween.Count > 0
                       && daysBetween.All(nonWorkingDays.Contains)
                       && !daysBetween.Any(leaveDays.Contains);
            });

            bool isBridging = hasLeftBridge || hasRightBridge;

            bool isSandwich = ((hasPrev && hasNext) && (hasPrevOfficeLeave && hasNextOfficeLeave)) || isBridging || isHolidayBetween;

            DateTime seriesStart = startDate;
            DateTime seriesEnd = endDate;

            if (isSandwich)
            {
                prev = startDate.AddDays(-1);
                while (nonWorkingDays.Contains(prev))
                {
                    seriesStart = prev;
                    prev = prev.AddDays(-1);
                }

                next = endDate.AddDays(1);
                while (nonWorkingDays.Contains(next))
                {
                    seriesEnd = next;
                    next = next.AddDays(1);
                }
            }

            // Count total leave days in the series
            int totalDays = 0;
            for (var d = seriesStart; d <= seriesEnd; d = d.AddDays(1))
                if (!leaveDays.Contains(d)) // || (d >= startDate && d <= endDate)
                    totalDays++;

            return new SandwichLeaveResult
            {
                IsSandwich = isSandwich,
                SeriesStart = isSandwich ? seriesStart : (DateTime?)null,
                SeriesEnd = isSandwich ? seriesEnd : (DateTime?)null,
                TotalDays = totalDays
            };
        }
        public bool IsAnyHolidayBetween(DateTime startDate, DateTime endDate, HashSet<DateTime> holidays)
        {
            // Check any holiday between start date and end date
            for (var date = startDate.AddDays(1); date < endDate; date = date.AddDays(1))
            {
                if (holidays.Contains(date.Date))
                    return true;
            }
            return false;
        }
        #endregion

        #region InsertEmployeeLeave
        // Title: InsertEmployeeLeave
        // Description: This method inserts a new EmployeeLeave entity into the repository. It takes a EmployeeLeave object as input and uses the _employeeLeaveRepository to handle the insertion operation.
        public void InsertEmployeeLeave(EmployeeLeave entity)
        {
            _employeeLeaveRepository.Insert(entity);
        }
        #endregion

        #region UpdateEmployeeLeave
        // Title: UpdateEmployeeLeave
        // Description: This method updates the specified EmployeeLeave entity in the repository. It takes a EmployeeLeave object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateEmployeeLeave(EmployeeLeave entity)
        {
            _employeeLeaveRepository.Update(entity);
        }
        #endregion

        #region DeleteEmployeeLeave
        // Title: DeleteEmployeeLeave
        // Description: Marks the specified EmployeeLeave entity as deleted by setting its `Deleted` property to true. 
        public void DeleteEmployeeLeave(EmployeeLeave entity)
        {
            entity.Deleted = true;

            _employeeLeaveRepository.Update(entity);
        }
        #endregion

        bool CheckPreviousNextOfficeLeave(int prevousOrNextday, DateTime date, HashSet<DateTime> holidays, HashSet<DateTime> leaveDays)
        {
            DateTime day = date.AddDays(prevousOrNextday);

            // If previous day is holiday → true
            if (holidays.Contains(day))
                return true;

            // If previous day is working day → false
            if (!leaveDays.Contains(day))
                return false;

            // Previous day is leave → keep checking backwards
            while (leaveDays.Contains(day))
            {
                day = day.AddDays(prevousOrNextday);

                // Found holiday after sequence of leave days → true
                if (holidays.Contains(day))
                    return true;

                // Found working day → false
                if (!leaveDays.Contains(day))
                    return false;
            }
            return false;
        }
    }
}
