using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Employees
{
    public class EmployeeService : IEmployeeService
    {
        #region Define Services
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<EmployeeType> _employeeTypeRepository;
        private readonly IRepository<EmployeeDesignation> _employeeDesignationRepository;
        private readonly ApplicationDbContext _db;
        //private readonly ICommonService _commonService;
        #endregion

        #region Services Initializations

        public EmployeeService(
            IRepository<Employee> employeeRepository, 
            IRepository<EmployeeType> employeeTypeRepository, 
            IRepository<EmployeeDesignation> employeeDesignationRepository, 
            ApplicationDbContext db
            //ICommonService commonService
            )
        {
            _employeeRepository = employeeRepository;
            _employeeTypeRepository = employeeTypeRepository;
            _employeeDesignationRepository = employeeDesignationRepository;
            _db = db;
            //_commonService = commonService;
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

        #region GetAllEmployees
        // Title: GetAllEmployees
        // Description: This method retrieves a paginated list of employees based on various search criteria such as employee name, 
        // It also supports sorting and includes related data .The method allows for both full and lookup (limited) data retrieval modes.
        public IPagedList<Employee> GetAllEmployees(
            string SiteId,
            string SearchText,
            string employeeCode,
            List<string> EmployeeIds,
            string primaryEmailAddress,
            List<string> employeeTypeIds,
            List<string> employeeDepartmentIds,
            List<string> employeeDesignationIds,
            List<string> orgLocationIds,
            string employeeStatus,
            string AllStatusId,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _employeeRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(employeeStatus) && employeeStatus != "All")
            {
                query = query.Where(x =>
                    employeeStatus == "Active"
                        ? x.Active
                        : !x.Active
                );
            }

            if (EmployeeIds != null && EmployeeIds.Any())
                query = query.Where(x => EmployeeIds.Contains(x.PersonId));

            if (!string.IsNullOrWhiteSpace(employeeCode))
            {
                employeeCode = employeeCode.Trim();
                query = query.Where(x => x.EmployeeCode.Contains(employeeCode));
            }

            if (!string.IsNullOrWhiteSpace(primaryEmailAddress))
            {
                primaryEmailAddress = primaryEmailAddress.Trim().ToLower();
                query = query.Where(x => x.Person.PrimaryEmailAddress.ToLower().Contains(primaryEmailAddress));
            }

            if (employeeDepartmentIds != null && employeeDepartmentIds.Any())
                query = query.Where(x => x.EmployeeDepartment.Any(m => employeeDepartmentIds.Contains(m.EmployeeDepartmentId)));

            if (employeeTypeIds != null && employeeTypeIds.Any())
                query = query.Where(x => x.EmployeeType.Any(m => employeeTypeIds.Contains(m.EmployeeTypeId)));

            if (employeeDesignationIds != null && employeeDesignationIds.Any())
                query = query.Where(x => x.EmployeeDesignation.Any(m => employeeDesignationIds.Contains(m.EmployeeDesignationId)));

            if (orgLocationIds != null && orgLocationIds.Any())
                query = query.Where(x => x.EmployeeOrgLocation.Any(m => orgLocationIds.Contains(m.OrgLocationId)));



            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                string orderBy;
                if (sortBy == "person.fullName")
                {
                    orderBy = $"{GetOrderBy("Person.FirstName")} {(descending ? "desc" : "asc")}, {GetOrderBy("Person.LastName")} {(descending ? "desc" : "asc")}";
                }
                else
                {
                    orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                }
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.UpdatedOnUtc);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(e =>
                    e.EmployeeCode.Contains(SearchText.ToLower()) ||
                    (e.Person.FirstName + " " + e.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                    e.Person.PrimaryEmailAddress.ToLower().Contains(SearchText.ToLower()) ||
                    e.Person.PrimaryPhoneNumber.ToLower().Contains(SearchText.ToLower()) ||
                    e.EmployeeType.Any(et => et.EmployeeTypeDropdown.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    e.EmployeeDepartment.Any(dep => dep.Department.Name.ToLower().Contains(SearchText.ToLower())) ||
                    e.EmployeeDesignation.Any(des => des.Designation.DropDownValue.ToLower().Contains(SearchText.ToLower())) ||
                    e.EmployeeOrgLocation.Any(org => org.OrgLocation.DropDownValue.ToLower().Contains(SearchText.ToLower()))
                    )
                );
            }

            query = query.Select(x => new Employee
            {
                Id = x.Id,
                //SiteId = x.SiteId,
                PersonId = x.PersonId,
                EmployeeCode = x.EmployeeCode,
                OfficialEmail = x.OfficialEmail,
                EmergencyContactName = x.EmergencyContactName,
                EmergencyPhoneNo = x.EmergencyPhoneNo,
                SameASPermanentAddress = x.SameASPermanentAddress,
                AadhaarCardNo = x.AadhaarCardNo,
                PanCardNo = x.PanCardNo,
                EPFUANNo = x.EPFUANNo,
                JoiningDate = x.JoiningDate,
                ReleaseDate = x.ReleaseDate,
                EducationDetail = x.EducationDetail,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress,
                    PrimaryPhoneNumber = x.Person.PrimaryPhoneNumber,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    Address = new Address
                    {
                        Id = x.Address.Id,
                        AddressCountry = new Country
                        {
                            Id = x.Address.AddressCountry.Id,
                            TwoLetterIsoCode = x.Address.AddressCountry.TwoLetterIsoCode,
                        }
                    },
                },
                EmployeeDepartment = x.EmployeeDepartment.Where(p => !p.Deleted).Select(p => new EmployeeDepartment
                {
                    Id = p.Id,
                    EmployeeDepartmentId = p.EmployeeDepartmentId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Department = new Department
                    {
                        Id = p.Department.Id,
                        Name = p.Department.Name
                    }
                }).ToList(),
                EmployeeType = x.EmployeeType.Where(p => !p.Deleted).Select(p => new EmployeeType
                {
                    Id = p.Id,
                    EmployeeTypeId = p.EmployeeTypeId,
                    EmployeeTypeDropdown = new DropDown
                    {
                        Id = p.EmployeeTypeDropdown.Id,
                        DropDownValue = p.EmployeeTypeDropdown.DropDownValue
                    },
                }).ToList(),
                EmployeeDesignation = x.EmployeeDesignation.Where(p => !p.Deleted).Select(p => new EmployeeDesignation
                {
                    Id = p.Id,
                    EmployeeDesignationId = p.EmployeeDesignationId,
                    ShiftId = p.ShiftId,
                    LeaveApproverId = p.LeaveApproverId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Designation = new DropDown
                    {
                        Id = p.Designation.Id,
                        DropDownValue = p.Designation.DropDownValue
                    },
                    Shift = new DropDown
                    {
                        Id = p.Shift.Id,
                        DropDownValue = p.Shift.DropDownValue
                    },
                    LeaveApprover = new Employee
                    {
                        Person = new Person
                        {
                            Id = p.LeaveApprover.Person.Id,
                            FullName = p.LeaveApprover.Person.FirstName + " " + p.LeaveApprover.Person.LastName
                        }
                    }
                }).ToList(),
                EmployeeOrgLocation = x.EmployeeOrgLocation.Where(p => !p.Deleted).Select(p => new EmployeeOrgLocation
                {
                    Id = p.Id,
                    OrgLocationId = p.OrgLocationId,
                    OrgLocation = new DropDown
                    {
                        Id = p.OrgLocation.Id,
                        DropDownValue = p.OrgLocation.DropDownValue
                    },
                }).ToList(),
                EmployeeStatuses = x.EmployeeStatuses.Where(p => !p.Deleted).Select(p => new EmployeeStatus
                {
                    Id = p.Id,
                    EmployeeId = p.EmployeeId,
                    EmployeeStatusId = p.EmployeeStatusId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = new DropDown
                    {
                        Id = p.Status.Id,
                        DropDownValue = p.Status.DropDownValue
                    }
                }).ToList()
            });

            var list = new PagedList<Employee>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetEmployeeById
        // Title: GetEmployeeById
        // Description: This method retrieves a employee from the database by its unique identifier (`id`). 
        public async Task<Employee> GetById(string id)
        {
            var query = _employeeRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllEmployeeListForDropdown
        public async Task<List<Employee>> GetAllEmployeeListForDropdown(string SiteId)
        {
            var query = _employeeRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId);
            query = query.Select(x => new Employee
            {
                Id = x.Id,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName
                },
            });
            query = query.OrderBy(m => m.Person.FirstName);
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllActivityOwnerListForDropdown
        public async Task<List<Employee>> GetAllActivityOwnerListForDropdown(string SiteId, string activeEmployeeStatus, string exEmployeeStatus)
        {
            // Default to the current month if TargetMonthStr is not provided
            var targetMonthDates = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var query = _employeeRepository.TableNoTracking.Where(m => !m.Deleted && !m.Person.Deleted && m.SiteId == SiteId);
            
            query = query.Where(x => x.EmployeeStatuses.Where(s => !s.Deleted).OrderByDescending(s => s.StartDate).FirstOrDefault().EmployeeStatusId == activeEmployeeStatus);
            
            query = query.Where(x => !x.EmployeeStatuses.Where(m => !m.Deleted).Any(m => m.EmployeeStatusId == exEmployeeStatus && m.StartDate > x.EmployeeStatuses.Where(s => !s.Deleted).OrderByDescending(s => s.StartDate).FirstOrDefault().StartDate));

            query = query.Select(x => new Employee
            {
                Id = x.Id,
                EstimateHrs = x.ProjectActivities.Where(c => !c.Deleted && c.TargetMonth.Value.Month == targetMonthDates.Month && c.TargetMonth.Value.Year == targetMonthDates.Year).Sum(c => c.EstimateHours),
                Person = new Person
                {
                    Id = x.Person.Id,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName
                },
            });
            query = query.OrderBy(m => m.Person.FirstName);

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllActiveEmployeeListForDropdown
        public async Task<List<Employee>> GetAllActiveEmployeeListForDropdown(string SiteId, DateTime? TargetMonth = null)
        {
            // Default to the current month if TargetMonthStr is not provided
            var query = _employeeRepository.TableNoTracking.Where(m => !m.Deleted && !m.Person.Deleted && m.SiteId == SiteId && m.Active);

            DateTime targetMonthDate = TargetMonth?.Date ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            query = query.Select(x => new Employee
            {
                Id = x.Id,
                EstimateHrs = x.ProjectActivities.Where(c => !c.Deleted && c.TargetMonth.Value.Month == targetMonthDate.Month && c.TargetMonth.Value.Year == targetMonthDate.Year).Sum(c => c.EstimateHours),
                Person = new Person
                {
                    Id = x.Person.Id,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName,
                    MiddleName = x.Person.MiddleName,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress
                },
            });
            query = query.OrderBy(m => m.Person.FirstName);
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetEmployeesByStatus
        public async Task<List<Employee>> GetEmployeesByStatus(string SiteId, string statusId)
        {
            var query = _employeeRepository.TableNoTracking.Where(e => !e.Deleted && !e.Person.Deleted && e.SiteId == SiteId);
            // Filter by the latest status matching the given statusId
            query = query.Where(e => e.EmployeeStatuses.Where(es => !es.Deleted).OrderByDescending(es => es.StartDate).FirstOrDefault().EmployeeStatusId == statusId);
            // Select relevant fields
            query = query.Select(e => new Employee
            {
                Id = e.Id,
                Person = new Person
                {
                    Id = e.Person.Id,
                    FullName = e.Person.FirstName + " " + e.Person.LastName,
                    FirstName = e.Person.FirstName,
                    LastName = e.Person.LastName
                }
            });
            query = query.OrderBy(e => e.Person.FirstName);
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllEmployeeByStatusId
        // Title: GetAllEmployeeByStatusId
        // Description: This method retrieves a employee from the database by statusId.
        public List<Employee> GetAllEmployeeByStatusId(string SiteId, string statusId)
        {
            var query = _employeeRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId && m.EmployeeStatuses.Any(es => es.EmployeeStatusId != null && es.EmployeeStatusId == statusId));
            query = query.Select(x => new Employee
            {
                Id = x.Id
            });

            return query.ToList();
        }
        #endregion

        #region Employee Methods
        public async Task<Employee> GetActiveEmployeeByStatusId(string SiteId, string statusId, string assignedToId)
        {
            return await _employeeRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId && m.EmployeeStatuses.Any(es => es.EmployeeStatusId == statusId) && m.Id == assignedToId).FirstOrDefaultAsync();
        }
        #endregion

        #region GetAllEmployeesByEmployementType
        // Title: GetAllEmployeesByEmployementType
        // Description: This method retrieves a employee from the database by employementTypeId. 
        public List<Employee> GetAllEmployeesByEmployementType(string SiteId, string employementTypeId, string activeEmployeeStatus, string exEmployeeStatus)
        {
            var query = _employeeRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId && m.EmployeeType.Any(es => es.EmployeeTypeId != null && es.EmployeeTypeId == employementTypeId && es.StartDate != null && es.EndDate == null));
            query = query.Where(x => x.EmployeeStatuses.Where(s => !s.Deleted).OrderByDescending(s => s.StartDate).FirstOrDefault().EmployeeStatusId == activeEmployeeStatus);
            query = query.Where(x => !x.EmployeeStatuses.Where(m => !m.Deleted).Any(m => m.EmployeeStatusId == exEmployeeStatus && m.StartDate > x.EmployeeStatuses.Where(s => !s.Deleted).OrderByDescending(s => s.StartDate).FirstOrDefault().StartDate));

            query = query.Select(x => new Employee
            {
                Id = x.Id
            });
            return query.ToList();
        }
        #endregion

        #region GetEmployeeDetailsById
        // Title: GetEmployeeDetailsById
        // Description: The method selects relevant fields from the empployee entity.
        public async Task<Employee> GetEmployeeDetailsById(string id)
        {
            var query = _employeeRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            query = query.Select(x => new Employee
            {
                Id = x.Id,
                SiteId = x.SiteId,
                PersonId = x.PersonId,
                EmployeeCode = x.EmployeeCode,
                OfficialEmail = x.OfficialEmail,
                EmergencyContactName = x.EmergencyContactName,
                EmergencyPhoneNo = x.EmergencyPhoneNo,
                SameASPermanentAddress = x.SameASPermanentAddress,
                AadhaarCardNo = x.AadhaarCardNo,
                PanCardNo = x.PanCardNo,
                EPFUANNo = x.EPFUANNo,
                JoiningDate = x.JoiningDate,
                ReleaseDate = x.ReleaseDate,
                EducationDetail = x.EducationDetail,

                Active = x.Active,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FirstName = x.Person.FirstName,
                    MiddleName = x.Person.MiddleName,
                    LastName = x.Person.LastName,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    PrimaryPhoneNumber = x.Person.PrimaryPhoneNumber,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress,
                    AddressId = x.Person.AddressId,
                    GenderId = x.Person.GenderId,
                    AddressTypeId = x.Person.AddressTypeId,
                    PictureId = x.Person.PictureId,
                    Address = new Address
                    {
                        Id = x.Person.Address.Id,
                        AddressLine1 = x.Person.Address.AddressLine1,
                        AddressLine2 = x.Person.Address.AddressLine2,
                        StateProvinceId = x.Person.Address.StateProvinceId,
                        CountryId = x.Person.Address.CountryId,
                        City = x.Person.Address.City,
                        ZipCode = x.Person.Address.ZipCode,
                        AddressCountry = new Country
                        {
                            Id = x.Person.Address.AddressCountry.Id,
                            Name = x.Person.Address.AddressCountry.Name,
                        },
                        AddressStateProvince = new StateProvince
                        {
                            Id = x.Person.Address.AddressStateProvince.Id,
                            Name = x.Person.Address.AddressStateProvince.Name,
                        },
                    },
                    Gender = new DropDown
                    {
                        Id = x.Person.Gender.Id,
                        DropDownValue = x.Person.Gender.DropDownValue,
                    },
                    AddressType = new DropDown
                    {
                        Id = x.Person.AddressType.Id,
                        DropDownValue = x.Person.AddressType.DropDownValue,
                    },
                    Picture = new Picture
                    {
                        Id = x.Person.Picture.Id,
                        VirtualPath = x.Person.Picture.VirtualPath,
                        SeoFilename = x.Person.Picture.SeoFilename
                    }
                },
                Address = new Address
                {
                    Id = x.Address.Id,
                    AddressLine1 = x.Address.AddressLine1,
                    AddressLine2 = x.Address.AddressLine2,
                    City = x.Address.City,
                    CountryId = x.Address.CountryId,
                    StateProvinceId = x.Address.StateProvinceId,
                    ZipCode = x.Address.ZipCode,
                    AddressCountry = new Country
                    {
                        Id = x.Address.AddressCountry.Id,
                        Name = x.Address.AddressCountry.Name,
                    },
                    AddressStateProvince = new StateProvince
                    {
                        Id = x.Address.AddressStateProvince.Id,
                        Name = x.Address.AddressStateProvince.Name,
                    },
                },
                EmployeeDepartment = x.EmployeeDepartment.Where(p => !p.Deleted).OrderByDescending(p => p.CreatedOnUtc).Select(p => new EmployeeDepartment
                {
                    Id = p.Id,
                    EmployeeDepartmentId = p.EmployeeDepartmentId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Duration = p.Duration,
                    Note = p.Note,
                    Department = new Department
                    {
                        Id = p.Department.Id,
                        Name = p.Department.Name
                    }
                }).ToList(),
                EmployeeDesignation = x.EmployeeDesignation.Where(p => !p.Deleted).OrderByDescending(p => p.CreatedOnUtc).Select(p => new EmployeeDesignation
                {
                    Id = p.Id,
                    EmployeeDesignationId = p.EmployeeDesignationId,
                    ShiftId = p.ShiftId,
                    LeaveApproverId = p.LeaveApproverId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Duration = p.Duration,
                    Note = p.Note,
                    Designation = new DropDown
                    {
                        Id = p.Designation.Id,
                        DropDownValue = p.Designation.DropDownValue
                    },
                    Shift = new DropDown
                    {
                        Id = p.Shift.Id,
                        DropDownValue = p.Shift.DropDownValue
                    },
                    LeaveApprover = new Employee
                    {
                        Person = new Person
                        {
                            Id = p.LeaveApprover.Person.Id,
                            FullName = p.LeaveApprover.Person.FirstName + " " + p.LeaveApprover.Person.LastName
                        }
                    }
                }).ToList(),
                EmployeeStatuses = x.EmployeeStatuses.Where(p => !p.Deleted).OrderByDescending(p => p.CreatedOnUtc).Select(p => new EmployeeStatus
                {
                    Id = p.Id,
                    EmployeeStatusId = p.EmployeeStatusId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Duration = p.Duration,
                    Note = p.Note,
                    Status = new DropDown
                    {
                        Id = p.Status.Id,
                        DropDownValue = p.Status.DropDownValue
                    }
                }).ToList(),
                EmployeeType = x.EmployeeType.Where(p => !p.Deleted).OrderByDescending(p => p.CreatedOnUtc).Select(p => new EmployeeType
                {
                    Id = p.Id,
                    EmployeeTypeId = p.EmployeeTypeId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Duration = p.Duration,
                    Note = p.Note,
                    EmployeeTypeDropdown = new DropDown
                    {
                        Id = p.EmployeeTypeDropdown.Id,
                        DropDownValue = p.EmployeeTypeDropdown.DropDownValue
                    }
                }).ToList(),
                EmployeeOrgLocation = x.EmployeeOrgLocation.Where(p => !p.Deleted).Select(p => new EmployeeOrgLocation
                {
                    Id = p.Id,
                    OrgLocationId = p.OrgLocationId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Duration = p.Duration,
                    Note= p.Note,
                    OrgLocation = new DropDown
                    {
                        Id = p.OrgLocation.Id,
                        DropDownValue = p.OrgLocation.DropDownValue
                    }
                }).ToList()
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetEmployeeByPersonId
        // Title: GetEmployeeByPersonId
        // Description: This method retrieves a employee based on its name.The method returns the first matching employee or null if no match is found.
        public async Task<Employee> GetEmployeeByPersonId(string PersonId, string id = null)
        {
            var query = _employeeRepository.TableNoTracking.Where(x => !x.Deleted && x.PersonId == PersonId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        public async Task<Employee> GetEmployeeByPersonIdBySiteId(string PersonId, string siteId, string id = null)
        {
            var query = _employeeRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.PersonId == PersonId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<Employee> GetEmployeeByEmailAndBySiteId(string Email, string siteId, string id = null)
        {
            var query = _employeeRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.OfficialEmail == Email);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<Employee> GetEmployeeDetailsByPersonId(string PersonId)
        {
            var query = _employeeRepository.TableNoTracking.Where(x => !x.Deleted && x.PersonId == PersonId);
            query = query.Select(x => new Employee
            {
                Id = x.Id,
                //SiteId = x.SiteId,
                PersonId = x.PersonId,
                EmployeeCode = x.EmployeeCode,
                OfficialEmail = x.OfficialEmail,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FirstName = x.Person.FirstName,
                    MiddleName = x.Person.MiddleName,
                    LastName = x.Person.LastName,
                    PrimaryPhoneNumber = x.Person.PrimaryPhoneNumber,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress
                }
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<Employee> GetEmployeeByEmail(string SiteId, string Email)
        {
            //return await _employeeRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId).FirstOrDefaultAsync(m => m.OfficialEmail == Email);
            var query = _employeeRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.OfficialEmail == Email);

            query = query.Select(x => new Employee
            {
                Id = x.Id,
                PersonId = x.PersonId,
                Person = new Person
                {
                    Id = x.Person.Id,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress
                }
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetEmployeeByEmployeeCode
        public async Task<Employee> GetEmployeeByEmployeeCode(string SiteId, string employeeCode, string id = null)
        {
            var query = _employeeRepository.TableNoTracking.Where(x => x.SiteId == SiteId && x.EmployeeCode == employeeCode);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        public async Task<List<Employee>> GetEmployeesForAnniversary(string SiteId, int daysBefore)
        {
            var targetDate = DateTime.Today.AddDays(daysBefore);

            return await _employeeRepository.TableNoTracking
                .Where(e => !e.Deleted && e.SiteId == SiteId && e.Active && e.JoiningDate != null && e.JoiningDate.Value.Year < targetDate.Year)
                .Where(e =>
                    // Anniversary exact match
                    (e.JoiningDate.Value.Month == targetDate.Month &&
                     e.JoiningDate.Value.Day == targetDate.Day)
                    ||
                    // Feb 29 → Feb 28 fallback if non-leap year
                    (e.JoiningDate.Value.Month == 2 &&
                     e.JoiningDate.Value.Day == 29 &&
                     !DateTime.IsLeapYear(targetDate.Year) &&
                     targetDate.Month == 2 && targetDate.Day == 28)
                )
                .Select(x => new Employee
                {
                    Id = x.Id,
                    SiteId = x.SiteId,
                    Person = new Person
                    {
                        Id = x.Person.Id,
                        FullName = x.Person.FirstName + " " + x.Person.LastName
                    },
                    JoiningDate = x.JoiningDate,
                    YearsCompleted = targetDate.Year - x.JoiningDate.Value.Year
                })
                .ToListAsync();
        }

        #region Auto Generate employee code
        public async Task<Employee> GetEmployeeCode()
        {
            return await _employeeRepository.TableNoTracking.OrderByDescending(e => e.EmployeeCode).FirstOrDefaultAsync();
        }

        #endregion

        #region InsertEmployee
        // Title: InsertEmployee
        // Description: This method inserts a new Employee entity into the repository. It takes a Employee object as input and uses the _employeeRepository to handle the insertion operation.
        public void InsertEmployee(Employee entity)
        {
            _employeeRepository.Insert(entity);
        }
        #endregion

        #region UpdateEmployee
        // Title: UpdateEmployee
        // Description: This method updates the specified Employee entity in the repository. It takes a Employee object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateEmployee(Employee entity)
        {
            _employeeRepository.Update(entity);
        }
        #endregion

        #region DeleteEmplyee
        // Title: DeleteEmplyee
        // Description: Marks the specified employee entity as deleted by setting its `Deleted` property to true. 
        public void DeleteEmployee(Employee entity)
        {
            entity.Deleted = true;

            _employeeRepository.Update(entity);
        }
        #endregion
    }
}
