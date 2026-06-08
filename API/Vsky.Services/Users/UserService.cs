using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Vsky.Services.Sites;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.Mail;
using SendGrid.Helpers.Mail;
using Microsoft.PowerBI.Api.Models;

namespace Vsky.Services.Users
{
    public class UserService : IUserService
    {
        #region Services Initializations
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<Person> _personRepository;
        private readonly IRepository<SitesRoles> _SitesRolesRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<HelpDesk> _helpDeskRepository;
        public UserService(
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor,
            IRepository<Person> personRepository,
            IRepository<SitesRoles> sitesRolesRepository,
            IRepository<Employee> employeeRepository,
            IRepository<HelpDesk> helpDeskRepository)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _personRepository = personRepository;
            _SitesRolesRepository = sitesRolesRepository;
            _employeeRepository = employeeRepository;
            _helpDeskRepository = helpDeskRepository;
        }
        #endregion

        #region Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        public IPagedList<ApplicationUser> GetAllUsersList(string SiteId, string SearchText, string userStatus, string userName, string fullName, string email, List<string> siteRoleIds, string UserId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _userManager.Users.Where(x => !x.Deleted && x.UserRoles.Any(m => m.SiteId == SiteId && m.Role.Name != "SuperAdmin") && x.Person.PersonSitesMapping.Any(psm => psm.SiteId == SiteId && !psm.Deleted));
            var activeSiteRoleIds = _SitesRolesRepository.TableNoTracking.Where(x => x.SiteId == SiteId && !x.Deleted).Select(x => x.RoleId).ToList();

            if (!string.IsNullOrWhiteSpace(userStatus))
                query = query.Where(x => userStatus == "Active" ? x.Active : !x.Active);

            // firstname
            if (!string.IsNullOrEmpty(userName))
                query = query.Where(x => x.UserName == userName);

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                fullName = fullName.Trim().ToLower();
                query = query.Where(x => (x.Person.FirstName.ToLower() + " " + x.Person.LastName.ToLower()).Contains(fullName));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                email = email.Trim().ToLower();
                query = query.Where(x => x.Email.ToLower().Contains(email));

            }
            if (siteRoleIds != null && siteRoleIds.Any())
            {
                var roleIds = _SitesRolesRepository.TableNoTracking.Where(x => siteRoleIds.Contains(x.Id) && !x.Deleted).Select(x => x.RoleId).ToList();
                query = query.Where(x => x.UserRoles.Any(m => roleIds.Contains(m.RoleId) && m.SiteId == SiteId));
            }
            //sorting
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

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                   (m.UserName.ToLower().Contains(SearchText.ToLower()) ||
                   (m.Person.FirstName + " " + m.Person.LastName).ToLower().Contains(SearchText.ToLower()) ||
                   m.Email.ToLower().Contains(SearchText.ToLower()) ||
                   m.PhoneNumber.ToLower().Contains(SearchText.ToLower())) ||
                   m.UserRoles.Any(r => r.SiteId == SiteId && r.Role.Name.ToLower().Contains(SearchText.ToLower()))
                );
            }

            query = query.Select(x => new ApplicationUser
            {
                Id = x.Id,
                UserName = x.UserName,
                Active = x.Active,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                UpdatedById = x.UpdatedById,
                Type = x.Type,
                Person = new Person
                {
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    //IsSharedUser = x.Person.PersonSitesMapping.Where(psm => psm.IsSharedUser)
                    IsSharedUser = x.Person.PersonSitesMapping.Any(psm => psm.SiteId == SiteId && psm.IsSharedUser)
                },
                UserRoles = x.UserRoles
                 .Where(m => activeSiteRoleIds.Contains(m.RoleId) && m.SiteId == SiteId) // <-- only non-deleted site roles
                 .Select(mapping => new ApplicationUserRole
                 {
                     Role = mapping.Role,
                     RoleId = mapping.RoleId
                 })
                 .ToList()
            });

            var list = new PagedList<ApplicationUser>(query, page, pageSize);
            return list;
        }

        public async Task<string> GetUserInitialAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            //Find People
            var person = _personRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == user.PersonId).FirstOrDefault();
            var name = $"{person.FirstName} {person.LastName}";
            var list = name.Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
            var initials = "";

            foreach (var item in list)
            {
                initials += item[..1].ToUpper();
            }

            return initials;
        }

        public async Task<IList<ApplicationUser>> GetAdminUsersAsync()
        {
            return (await _userManager.GetUsersInRoleAsync(Roles.Administrator)).Where(x => !x.Deleted).ToList();
        }

        public async Task<IList<ApplicationUser>> GetUsersAsync(string SiteId)
        {
            return (await _userManager.GetUsersInRoleAsync(Roles.Employee)).Where(x => !x.Deleted && x.Person.PersonSitesMapping.Any(m => m.SiteId == SiteId)).ToList();
        }

        public async Task<ApplicationUser> GetById(string SiteId, string Id)
        {
            return await _userManager.Users.Where(x => !x.Deleted && x.Id == Id && x.Person.PersonSitesMapping.Any(psm => psm.SiteId == SiteId && !psm.Deleted)).Include(x => x.Person).FirstOrDefaultAsync();
        }
        public async Task<ApplicationUser> GetUserByEmail(string SiteId, string Email)
        {
            return await _userManager.Users.Where(x => !x.Deleted && x.Email == Email && x.Person.PersonSitesMapping.Any(psm => psm.SiteId == SiteId && !psm.Deleted)).Include(x => x.Person).FirstOrDefaultAsync();
        }

        //public async Task<ApplicationUser> GetUserByEmployeeId(
        //    string siteId,
        //    string employeeId)
        //{
        //    // Get Employee's PersonId
        //    var personId = await _employeeRepository.TableNoTracking
        //        .Where(e =>
        //            e.Id == employeeId &&
        //            e.SiteId == siteId &&
        //            e.Active &&
        //            !e.Deleted
        //        )
        //        .Select(e => e.PersonId)
        //        .FirstOrDefaultAsync();

        //    if (string.IsNullOrEmpty(personId))
        //        return null;

        //    // Get User using PersonId
        //    return await _userManager.Users
        //        .Where(u =>
        //            u.PersonId == personId &&
        //            u.Active &&
        //            !u.Deleted
        //        )
        //        .FirstOrDefaultAsync();
        //}

        public async Task<ApplicationUser> GetUserByEmployeeId(
            string siteId,
            string employeeId
        )
        {
            var employee = await _employeeRepository.TableNoTracking
                .Where(e =>
                    e.Id == employeeId &&
                    e.SiteId == siteId &&
                    e.Active &&
                    !e.Deleted)
                .Select(e => new
                {
                    Email = e.OfficialEmail
                })
                .FirstOrDefaultAsync();

            if (employee == null || string.IsNullOrEmpty(employee.Email))
                return null;

            return await _userManager.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == employee.Email &&
                    u.Active &&
                    !u.Deleted);
        }

        public async Task<string> GetUserIdByRole(string SiteId, string role)
        {
            var user = await _userManager.Users
                .Where(x => !x.Deleted && x.UserRoles.Any(m => m.Role.Name == role) && x.Person.PersonSitesMapping.Any(psm => psm.SiteId == SiteId && !psm.Deleted))
                .Include(x => x.Person)
                .FirstOrDefaultAsync();

            return user?.Id;
        }

        public async Task<string> GetUserIdByPersonId(string SiteId, string personId)
        {
            var user = await _userManager.Users
                .Where(x => !x.Deleted && x.PersonId == personId && x.Person.PersonSitesMapping.Any(psm => psm.SiteId == SiteId && !psm.Deleted))
                .Include(x => x.Person)
                .FirstOrDefaultAsync();

            return user?.Id;
        }

        public async Task<List<ApplicationUser>> GetAllUserFirstNameListForDropdown(string SiteId)
        {
            var query = _userManager.Users.Where(x => !x.Deleted && x.UserRoles.Any(m => m.Role.Name != "SuperAdmin") && x.Person.FirstName != null && x.Person.PersonSitesMapping.Any(p => p.SiteId == SiteId));
            query = query.Select(x => new ApplicationUser
            {
                Person = new Person
                {
                    FirstName = x.Person.FirstName
                }
            });
            return query.ToList();
        }

        public async Task<List<ApplicationUser>> GetAllUserLastNameListForDropdown(string SiteId)
        {
            var query = _userManager.Users.Where(x => !x.Deleted && x.UserRoles.Any(m => m.Role.Name != "SuperAdmin") && x.Person.LastName != null && x.Person.PersonSitesMapping.Any(p => p.SiteId == SiteId));
            query = query.Select(x => new ApplicationUser
            {
                Person = new Person
                {
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName
                }
            });
            return query.ToList();
        }

        public async Task<List<ApplicationUser>> GetAllUserListForDropdown(string SiteId, string flag = null)
        {
            var query = _userManager.Users.Where(x => !x.Deleted && x.UserRoles.Any(m => m.Role.Name != "SuperAdmin") && x.Person.FirstName != null && x.Person.PersonSitesMapping.Any(p => p.SiteId == SiteId && !p.Deleted));
            if (!string.IsNullOrWhiteSpace(flag) && flag != "undefined")
            {
                query = query.Where(x => x.Active);
            }
            query = query.Select(x => new ApplicationUser
            {
                Id = x.Id,
                UserName = x.UserName,
                Person = new Person
                {
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName
                }
            });
            return query.ToList();
        }

        public Task<string> GetUserFullNameAsync(ApplicationUser user, string SiteId)
        {
            var fullName = string.Empty;
            //Find People
            var person = _personRepository.TableNoTracking.Where(x => !x.Deleted && x.PersonSitesMapping.Any(m => m.SiteId == SiteId) && x.Id == user.PersonId).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(person.FirstName) && !string.IsNullOrWhiteSpace(person.LastName))
            {
                fullName = $"{person.FirstName} {person.LastName}";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(person.FirstName))
                {
                    fullName = person.FirstName;
                }

                if (!string.IsNullOrWhiteSpace(person.LastName))
                {
                    fullName = person.LastName;
                }
            }
            return Task.FromResult(fullName);
        }

        public string GeneratePassword()
        {
            var options = _userManager.Options.Password;
            var length = options.RequiredLength;
            //var nonAlphanumeric = options.RequireNonAlphanumeric;
            var digit = options.RequireDigit;
            var lowercase = options.RequireLowercase;
            var uppercase = options.RequireUppercase;
            var password = new StringBuilder();
            var random = new Random();

            while (password.Length < length)
            {
                var c = (char)random.Next(32, 126);

                if (char.IsLetterOrDigit(c))
                {
                    password.Append(c);

                    if (char.IsDigit(c))
                    {
                        password.Append(c);
                    }

                    if (char.IsDigit(c))
                    {
                        digit = false;
                    }
                    else if (char.IsLower(c))
                    {
                        lowercase = false;
                    }
                    else if (char.IsUpper(c))
                    {
                        uppercase = false;
                    }
                }
            }

            if (digit)
            {
                password.Append((char)random.Next(48, 58));
            }

            if (lowercase)
            {
                password.Append((char)random.Next(97, 123));
            }

            if (uppercase)
            {
                password.Append((char)random.Next(65, 91));
            }

            return password.ToString();
        }

        //public async Task<List<ApplicationUser>> GetUsersByRole(string SiteId, string role)
        //{
        //    var query = _userManager.Users.Where(x => !x.Deleted && x.Active && x.UserRoles.Any(m => m.Role.Name == role) && x.Person.PersonSitesMapping.Any(psm => psm.SiteId == SiteId));
        //    query = query.Select(async x => new ApplicationUser
        //    {
        //        Id = x.Id,
        //        Email = x.Email,
        //        Person = x.Person,
        //        EmployeeId = _employeeRepository.TableNoTracking.Where(e => !e.Deleted && e.PersonId == x.PersonId).Select(e => e.Id).FirstOrDefault()
        //    });
        //    return query.ToList();
        //}

        public async Task<List<ApplicationUser>> GetUsersByRole(string SiteId, string role)
        {
            var query = _userManager.Users
                .Where(x =>
                    !x.Deleted &&
                    x.Active &&
                    x.UserRoles.Any(m => m.Role.Name == role && m.SiteId == SiteId) &&
                    x.Person.PersonSitesMapping.Any(psm => psm.SiteId == SiteId)
                )
                .Select(x => new ApplicationUser
                {
                    Id = x.Id,
                    Email = x.Email,
                    Person = x.Person,
                    EmployeeId = _employeeRepository.TableNoTracking
                        .Where(e => !e.Deleted && e.Active && e.SiteId == SiteId && e.OfficialEmail == x.Email)
                        .Select(e => e.Id)
                        .FirstOrDefault()
                });

            return await query.ToListAsync();
        }
        public async Task<List<ApplicationUser>> GetSupportTeamUsersData(string siteId, string role)
        {
            var activeStatuses = new[]
            {
                "Open",
                "Assigned",
                "In Progress",
                "Reopen"
            };

            var query =
                from u in _userManager.Users
                where
                    !u.Deleted &&
                    u.Active &&
                    u.UserRoles.Any(r => r.Role.Name == role && r.SiteId == siteId) &&
                    u.Person.PersonSitesMapping.Any(psm => psm.SiteId == siteId)

                let employeeId =
                    _employeeRepository.TableNoTracking
                        .Where(e =>
                            !e.Deleted &&
                            e.Active &&
                            e.SiteId == siteId &&
                            e.OfficialEmail == u.Email
                        )
                        .Select(e => e.Id)
                        .FirstOrDefault()

                let totalTickets =
                    _helpDeskRepository.TableNoTracking.Count(hd =>
                        !hd.Deleted &&
                        hd.AssignedToId == employeeId &&
                        activeStatuses.Contains(
                            hd.HelpDeskStatusLog
                                .OrderByDescending(sl => sl.CreatedOnUtc)
                                .Select(sl => sl.Status.DropDownValue)
                                .FirstOrDefault()
                        )
                    )

                select new ApplicationUser
                {
                    Id = u.Id,
                    Email = u.Email,
                    Person = u.Person,
                    EmployeeId = employeeId,
                    SupportTeamUserName = u.Person.FirstName + " " + u.Person.LastName + (totalTickets > 0 ? " (" + totalTickets + ")" : ""),
                    TicketCounts = new EmployeeTicketCount
                    {
                        Total = totalTickets
                    }

                    //TicketCounts = new EmployeeTicketCount
                    //{
                    //    Total =
                    //        _helpDeskRepository.TableNoTracking.Count(hd =>
                    //            !hd.Deleted &&
                    //            hd.AssignedToId == employeeId &&
                    //            activeStatuses.Contains(
                    //                hd.HelpDeskStatusLog
                    //                    .OrderByDescending(sl => sl.CreatedOnUtc)
                    //                    .Select(sl => sl.Status.DropDownValue)
                    //                    .FirstOrDefault()
                    //            )
                    //        )
                    //}
                };

            return await query.ToListAsync();
        }
        public string GetIdByMigrateUser(string userName)
        {
            string migrateId = null;
            if (migrateId == null)
            {
                var migrateUser = _userManager.Users.Where(x => !x.Deleted && x.UserName == "migrate").FirstOrDefault();
                if (migrateUser != null)
                {
                    migrateId = migrateUser.Id;
                }
            }
            return migrateId;
        }

        public string GetAdminUserId()
        {
            var adminUser = _userManager.Users.FirstOrDefault(x => !x.Deleted && x.UserName == "admin");
            return adminUser?.Id;
        }
        #endregion

    }
}