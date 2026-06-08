using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailKit.Search;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Common;
using Vsky.Services.Sites;

namespace Vsky.Services.HelpDesks
{
    public class HelpDeskService : IHelpDeskService
    {
        #region Define Services
        private readonly IRepository<HelpDesk> _helpDeskRepository;
        private readonly IRepository<HelpDeskStatusLog> _helpDeskStatusLogRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IRepository<EmailReplies> _emailRepliesRepository;
        private readonly IRepository<Notes> _notesRepository;
        private readonly IRepository<HelpDeskEmailRepliesMapping> _helpDeskEmailRepliesMapping;
        private readonly IRepository<Site> _siteRepository;
        private readonly IRepository<SitesModifiedLogs> _sitesModifiedLogRepository;
        private readonly ICommonService _commonService;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations
        public HelpDeskService(
            IRepository<HelpDesk> helpDeskRepository,
            IRepository<HelpDeskStatusLog> helpDeskStatusLogRepository,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            IRepository<EmailReplies> emailRepliesRepository,
            IRepository<Notes> notesRepository,
            IRepository<HelpDeskEmailRepliesMapping> helpDeskEmailRepliesMapping,
            IRepository<Site> siteRepository,
            IRepository<SitesModifiedLogs> sitesModifiedLogRepository,
            ICommonService commonService,
            IApplicationUserRoleService applicationUserRoleService
            )
        {
            _helpDeskRepository = helpDeskRepository;
            _helpDeskStatusLogRepository = helpDeskStatusLogRepository;
            _userManager = userManager;
            _db = db;
            _emailRepliesRepository = emailRepliesRepository;
            _notesRepository = notesRepository;
            _helpDeskEmailRepliesMapping = helpDeskEmailRepliesMapping;
            _siteRepository = siteRepository;
            _sitesModifiedLogRepository = sitesModifiedLogRepository;
            _commonService = commonService;
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

        #region GetAllHelpDesks
        // Title: GetAllHelpDesks
        // Description: This method retrieves a paginated list of HelpDesk based on various search criteria such as title
        public IPagedList<HelpDesk> GetAllHelpDesks(
            string siteId, 
            string searchText,
            string LoggedUserId,
            string assignedToId,
            List<string> employeeEmails,
            List<string> statusIds, 
            List<string> priorityIds, 
            List<string> topicIds,
            List<string> questionIds, 
            string createdBy, 
            string title,
            int ticketNo,
            List<string> companyIds, 
            List<string> categoryIds,
            DateTime? ticketFromDate, 
            DateTime? ticketToDate,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending, 
            int page = 1, 
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _helpDeskRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);

            //Get user by userid
            var userdata = _userManager.FindByIdAsync(LoggedUserId).GetAwaiter().GetResult();
            var user = _userManager.FindByNameAsync(userdata.UserName).GetAwaiter().GetResult();
            if (user != null && !user.Deleted && user.Active)
            {
                //Get user roles
                var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();

                // Fetch the NormalizedName of each role
                //var normalizedRoles = _db.Roles.Where(r => roles.Contains(r.Name)).Select(r => r.NormalizedName).ToArray();
                var normalizedRoles = _applicationUserRoleService
                    .GetNormalizedRoleNamesByUserAndSite(user.Id, siteId)
                    .GetAwaiter()
                    .GetResult()
                    .ToArray();

                if (!normalizedRoles.Any(r => r == "admin" || r == "support team"))
                {
                    query = query.Where(x => x.RequesterId == createdBy);
                }
                //if (!normalizedRoles.Any(r => r == "admin") && normalizedRoles.Any(r => r == "support team"))
                //{
                //    query = query.Where(x => x.AssignedToId == createdBy || x.CreatedById == LoggedUserId || x.AssignedToId == null);
                //}
            }

            if(!string.IsNullOrEmpty(assignedToId))
            {
                if(assignedToId == "Me")
                {
                    // show only tickets assignedTo looged-in user

                    query = query.Where(x => x.AssignedToId == createdBy);
                }
                else if (assignedToId == "View All")
                {
                    // show all tickets which are assigned to someone
                    query = query.Where(x => x.AssignedToId != null);
                }
            }

            if (employeeEmails != null && employeeEmails.Any())
            {
                query = query.Where(x =>
                    // RequesterId exists and matches
                    (x.RequesterId != null && employeeEmails.Contains(x.Employee.Person.PrimaryEmailAddress))

                    // RequesterId not found → match by email
                    || (x.RequesterId == null && employeeEmails.Contains(x.RequesterEmail))
                );
            }

            if (statusIds != null && statusIds.Any())
            {
                query = query.Where(x =>
                    statusIds.Contains(
                        x.HelpDeskStatusLog
                         .OrderByDescending(p => p.CreatedOnUtc)
                         .Select(p => p.Status.Id)
                         .FirstOrDefault()
                    )
                );
            }

            if (priorityIds != null && priorityIds.Any())
                query = query.Where(x => priorityIds.Contains(x.PriorityId));

            if (topicIds != null && topicIds.Any())
                query = query.Where(x => topicIds.Contains(x.TopicId));

            if (questionIds != null && questionIds.Any())
                query = query.Where(x => questionIds.Contains(x.QuestionId));

            if (companyIds != null && companyIds.Any())
                query = query.Where(x => companyIds.Contains(x.CompanyId));

            if (categoryIds != null && categoryIds.Any())
                query = query.Where(x => categoryIds.Contains(x.CategoryId));

            if (!string.IsNullOrWhiteSpace(title))
            {
                title = title.Trim().ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(title));
            }

            if (ticketNo > 0)
            {
                query = query.Where(x => x.TicketNo == ticketNo);
            }

            if (ticketFromDate != null)
                query = query.Where(x => x.CreatedOnUtc.Value.Date >= ticketFromDate);

            if (ticketToDate != null)
                query = query.Where(a => a.CreatedOnUtc.Value.Date <= ticketToDate);

            //if (date != null)
            //    query = query.Where(x => x.CreatedOnUtc.Date == date);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                DateTime.TryParse(searchText, out var parsedDate);
                searchText = searchText.ToLower();

                query = query.Where(m =>
                    m.Title.ToLower().Contains(searchText.Trim().ToLower()) ||
                    m.CreatedOnUtc.HasValue && m.CreatedOnUtc.Value.Date == parsedDate.Date ||
                    m.UpdatedOnUtc.HasValue && m.UpdatedOnUtc.Value.Date == parsedDate.Date ||
                    (m.UpdatedBy != null && m.UpdatedBy.Person != null && (m.UpdatedBy.Person.FirstName + " " + m.UpdatedBy.Person.LastName).Contains(searchText)) ||
                    (m.Employee != null && m.Employee.Person != null && m.Employee.Person.PrimaryEmailAddress != null && m.Employee.Person.PrimaryEmailAddress.ToLower().Contains(searchText)) ||
                    (m.HelpDeskTopic != null && m.HelpDeskTopic.Title != null && m.HelpDeskTopic.Title.ToLower().Contains(searchText)) ||
                    (m.HelpDeskTopicQuestions != null && m.HelpDeskTopicQuestions.Question != null && m.HelpDeskTopicQuestions.Question.ToLower().Contains(searchText)) ||
                    (m.Category != null && m.Category.DropDownValue != null && m.Category.DropDownValue.ToLower().Contains(searchText)) ||
                    (m.Priority != null && m.Priority.DropDownValue != null && m.Priority.DropDownValue.ToLower().Contains(searchText)) ||
                    (m.AssignedTo != null && (m.AssignedTo.Person.FirstName + " " + m.AssignedTo.Person.LastName).Contains(searchText)) ||
                    (m.HelpDeskStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.DropDownValue).FirstOrDefault() ?? "").ToLower().Contains(searchText) ||
                    m.RequesterEmail.ToLower().Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "statusId")
                {
                    query = descending
                        ? query.OrderByDescending(x =>
                            x.HelpDeskStatusLog
                             .OrderByDescending(p => p.CreatedOnUtc)
                             .Select(p => p.Status.Id)
                             .FirstOrDefault())
                        : query.OrderBy(x =>
                            x.HelpDeskStatusLog
                             .OrderByDescending(p => p.CreatedOnUtc)
                             .Select(p => p.Status.Id)
                             .FirstOrDefault());
                }
                else
                {
                    var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                    query = query.OrderBy(orderBy);
                }
            }
            //else
            //{
            //    query = query.OrderByDescending(x => x.UpdatedOnUtc);
            //}

            // Apply multi-level dictionary sorting
            if (sorts != null && sorts.Count > 0)
            {
                // Separate status sorting
                if (sorts.TryGetValue("statusId", out var statusDirection))
                {
                    bool statusDesc = statusDirection == "desc";

                    query = statusDesc
                        ? query.OrderByDescending(x =>
                            x.HelpDeskStatusLog
                                .OrderByDescending(p => p.CreatedOnUtc)
                                .Select(p => p.Status.Id)
                                .FirstOrDefault())
                        : query.OrderBy(x =>
                            x.HelpDeskStatusLog
                                .OrderByDescending(p => p.CreatedOnUtc)
                                .Select(p => p.Status.Id)
                                .FirstOrDefault());

                    // Remove from generic sorting
                    sorts.Remove("statusId");
                }

                if (sorts.Any())
                {
                    query = _commonService.ApplySorting(query, sorts);
                }
            }

            query = query.Select(x => new HelpDesk
            {
                Id = x.Id,
                Title = x.Title,
                TicketNo = x.TicketNo,
                PriorityId = x.PriorityId,
                CategoryId = x.CategoryId,
                TwilioEmailId = x.TwilioEmailId,
                AssignedToId = x.AssignedToId,
                RequesterEmail = x.RequesterEmail,
                RequesterId = x.RequesterId,
                QuestionId = x.QuestionId,
                TopicId = x.TopicId,
                ClosingComment = x.ClosingComment,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                UpdatedById = x.UpdatedById,
                SiteId = x.SiteId,
                //CreatedDateStr = x.CreatedOnUtc.ToString("MM/dd/yyyy hh:mm:ss"),
                //UpdatedDateStr = x.UpdatedOnUtc.ToString("MM/dd/yyyy hh:mm tt"),
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                        PrimaryEmailAddress = x.Employee.Person.PrimaryEmailAddress
                    }
                },
                HelpDeskTopic = new HelpDeskTopic
                {
                    Id = x.HelpDeskTopic.Id,
                    Title = x.HelpDeskTopic.Title,
                },
                HelpDeskTopicQuestions = new HelpDeskTopicQuestions
                {
                    Id = x.HelpDeskTopicQuestions.Id,
                    Question = x.HelpDeskTopicQuestions.Question,
                },
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person
                    {
                        Id = x.AssignedTo.Person.Id,
                        FirstName = x.AssignedTo.Person.FirstName,
                        LastName = x.AssignedTo.Person.LastName,
                        FullName = x.AssignedTo.Person.FirstName + " " + x.AssignedTo.Person.LastName,
                        PrimaryEmailAddress = x.AssignedTo.Person.PrimaryEmailAddress
                    }
                },
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.Person.Id,
                        FirstName = x.CreatedBy.Person.FirstName,
                        LastName = x.CreatedBy.Person.LastName,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                    }
                },
                UpdatedBy = new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = new Person
                    {
                        Id = x.UpdatedBy.Person.Id,
                        FirstName = x.UpdatedBy.Person.FirstName,
                        LastName = x.UpdatedBy.Person.LastName,
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName
                    }
                },
                Priority = new DropDown { Id = x.Priority.Id, DropDownValue = x.Priority.DropDownValue },
                Category = new DropDown { Id = x.Category.Id, DropDownValue = x.Category.DropDownValue },
                //EmailReplies = 
                StatusText = x.HelpDeskStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.DropDownValue).FirstOrDefault(),
                PreviousStatusText = x.HelpDeskStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.DropDownValue).Skip(1).FirstOrDefault(), // Take the second record
                StatusId = x.HelpDeskStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.Id).FirstOrDefault(),
                DateStr = x.HelpDeskStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.CreatedOnUtc).FirstOrDefault().ToString("MM/dd/yyyy hh:mm:ss"),
                EmailRepliesCount = _emailRepliesRepository.TableNoTracking
                    .Where(er => !er.Deleted &&
                        _helpDeskEmailRepliesMapping.TableNoTracking
                            .Any(m => m.EmailRepliesId == er.Id && m.HelpDeskId == x.Id && m.EmailReplies.IsSystemEmail == false)
                    )
                    .Select(er => er.TwilioEmailId)
                    .Count(),

                HelpDeskNotesCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Help Desk Notes").Count(),
                AssignedToCount = _sitesModifiedLogRepository.TableNoTracking.Where(h => !h.Deleted && h.SiteId == siteId && h.SubModuleId == x.Id && h.ColumnName == "Assigned To").Count()
                //TwilioEmailId = _emailRepliesRepository.TableNoTracking.Where(h => !h.Deleted && h.HelpDeskId == x.Id).Select(h => h.TwilioEmailId).FirstOrDefault()
            });

            var list = new PagedList<HelpDesk>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetAllHelpDeskListForDropdown
        public async Task<List<RequesterDropdownDto>> GetRequesterDropdown(string siteId)
        {
            return await _helpDeskRepository.TableNoTracking
                .Where(x => !x.Deleted && x.SiteId == siteId)
                .Select(x => new
                {
                    x.Id,
                    Email = x.RequesterId != null
                        && x.Employee != null
                        && x.Employee.Person != null
                            ? x.Employee.Person.PrimaryEmailAddress
                            : x.RequesterEmail
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .GroupBy(x => x.Email.ToLower())
                .Select(g => new RequesterDropdownDto
                {
                    Email = g.First().Email
                })
                .OrderBy(x => x.Email)
                .ToListAsync();
        }

        public async Task<List<CompanyDropdownDto>> GetCompanyDropdown(string siteId)
        {
            return await _helpDeskRepository.TableNoTracking
                .Where(x => !x.Deleted && x.SiteId == siteId && x.Company != null)
                .GroupBy(x => new
                {
                    x.Company.Id,
                    x.Company.Company.Name
                })
                .Select(g => new CompanyDropdownDto
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name
                })
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
        #endregion

        #region GetHelpDeskById
        // Title: GetHelpDeskById
        // Description: This method retrieves a HelpDesk from the database by its unique identifier (`id`). 
        public async Task<HelpDesk> GetHelpDeskById(string id)
        {
            var query = _helpDeskRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetHelpDeskByStatusId
        public async Task<HelpDesk> GetHelpDeskByStatusId(string statusId)
        {
            var query = _helpDeskRepository.TableNoTracking.Where(x => !x.Deleted && x.StatusId == statusId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<IList<HelpDesk>> GetAllHelpDesksByStatusId(string statusId)
        {
            //var query = _helpDeskRepository.TableNoTracking.Where(x => !x.Deleted && x.StatusId == statusId);

            //return await query.ToListAsync();

            var helpDeskIds = await _helpDeskStatusLogRepository.TableNoTracking
                        .Where(x => x.StatusId == statusId)
                        .Select(x => x.HelpDeskId)
                        .ToListAsync();

            var helpDesks = await _helpDeskRepository.TableNoTracking
                                .Where(x => !x.Deleted && helpDeskIds.Contains(x.Id))
                                .Include(x => x.Priority)
                                .Include(x => x.CreatedBy).ThenInclude(x => x.Person)
                                .ToListAsync();

            return helpDesks;
        }
        #endregion

        #region GetHelpDeskDetailsById
        // Title: GetHelpDeskDetailsById
        // Description: The method selects relevant fields from the HelpDesk entity, including related entities such as nd employee mappings, and returns a `HelpDesk` object with these details. 
        public async Task<HelpDesk> GetHelpDeskDetailsById(string id)
        {
            var query = _helpDeskRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).Select(x => new HelpDesk
            {
                Id = x.Id,
                Title = x.Title,
                TicketNo = x.TicketNo,
                Description = x.Description,
                SiteId = x.SiteId,
                TwilioEmailId = x.TwilioEmailId,
                PriorityId = x.PriorityId,
                CategoryId = x.CategoryId,
                RequesterEmail = x.RequesterEmail,
                RequesterId = x.RequesterId,
                CompanyId = x.CompanyId,
                AssignedToId = x.AssignedToId,
                AverageDurationInMinutes = x.AverageDurationInMinutes,
                ClosingComment = x.ClosingComment,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                //CreatedDateStr = x.CreatedOnUtc.ToString("MM/dd/yyyy hh:mm tt"),
                //UpdatedDateStr = x.UpdatedOnUtc.ToString("MM/dd/yyyy hh:mm tt"),
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                        PrimaryEmailAddress = x.Employee.Person.PrimaryEmailAddress
                    }

                },
                HelpDeskTopic = new HelpDeskTopic
                {
                    Id = x.HelpDeskTopic.Id,
                    Title = x.HelpDeskTopic.Title,
                },
                HelpDeskTopicQuestions = new HelpDeskTopicQuestions
                {
                    Id = x.HelpDeskTopicQuestions.Id,
                    Question = x.HelpDeskTopicQuestions.Question,
                },
                Priority = new DropDown { Id = x.Priority.Id, DropDownValue = x.Priority.DropDownValue },
                Category = new DropDown { Id = x.Category.Id, DropDownValue = x.Category.DropDownValue },
                Company = new CompanyClients { Id = x.Company.Id, Name = x.Company.Company.Name },
                SitePrefix = _siteRepository.TableNoTracking.Where(s => s.Id == x.SiteId).Select(s => s.TicketNoPrefix).FirstOrDefault(),
                //OwnerId = x.HelpDeskEmailRepliesMapping.Where(p => !p.EmailReplies.Deleted && x.Id == p.HelpDeskId).Select(p => p.EmailReplies.OwnerId).FirstOrDefault(),
                AssignedTo = new Employee
                {
                    Id = x.AssignedTo.Id,
                    Person = new Person
                    {
                        FullName = x.AssignedTo.Person.FirstName + " " + x.AssignedTo.Person.LastName,
                        PrimaryEmailAddress = x.AssignedTo.Person.PrimaryEmailAddress
                    }
                },
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.UpdatedBy.Person.Id,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                    }
                },
              
                UpdatedBy = new ApplicationUser
                {
                    Id = x.UpdatedBy.Id,
                    Person = new Person
                    {
                        Id = x.UpdatedBy.PersonId,
                        FullName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName
                    }
                },
                StatusText = x.HelpDeskStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.DropDownValue).FirstOrDefault(),
                PreviousStatusText = x.HelpDeskStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.DropDownValue).Skip(1).FirstOrDefault(), // Take the second record
                StatusId = x.HelpDeskStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.Id).FirstOrDefault(),
                AssignedToCount = _sitesModifiedLogRepository.TableNoTracking.Where(h => !h.Deleted && h.SiteId == x.SiteId && h.SubModuleId == x.Id && h.ColumnName == "Assigned To").Count(),
                HelpDeskStatusLog = x.HelpDeskStatusLog.OrderByDescending(d => d.CreatedOnUtc).Select(p => new HelpDeskStatusLog
                {
                    Id = p.Id,
                    DurationInMinutes = p.DurationInMinutes,
                    CreatedOnUtc = p.CreatedOnUtc,
                    //CreatedDate = p.CreatedOnUtc.ToString("MM/dd/yyyy hh:mm:ss"),
                    CreatedDate = p.CreatedOnUtc.ToString("MM/dd/yyyy hh:mm tt"),
                    Status = new DropDown
                    {
                        Id = p.Status.Id,
                        DropDownValue = p.Status.DropDownValue
                    },
                    CreatedBy = new ApplicationUser
                    {
                        Id = p.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = p.CreatedBy.PersonId,
                            FullName = p.CreatedBy.Person.FirstName + " " + p.CreatedBy.Person.LastName
                        }
                    },
                }).ToList(),
            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetHelpDeskByTitle
        // Title: GetHelpDeskByTitle
        // Description: This method retrieves a HelpDesk based on its title.It allows an optional exclusion of a HelpDesk by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific HelpDesk. The method returns the first matching HelpDesk or null if no match is found.
        public async Task<HelpDesk> GetHelpDeskByTitle(string siteId, string title, string id = null)
        {
            var query = _helpDeskRepository.TableNoTracking.Where(x => !x.Deleted && x.Title.ToLower() == title.ToLower() && x.SiteId == siteId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion

        #region GetTwilioEmailReplies
        public async Task<HelpDesk> GetTwilioEmailReplies(string TwilioEmailId)
        {
            return await _helpDeskRepository.Table
            .Include(x => x.HelpDeskStatusLog)
                .ThenInclude(x => x.Status)
            .FirstOrDefaultAsync(x => x.TwilioEmailId == TwilioEmailId);
        }
        #endregion

        #region GetLastTicketId
        // Title: GetLastTicketId
        // Description: This method retrieves the highest TicketId from the database or returns 1 if none are found. 
        public async Task<int> GetLastTicketId(string siteId)
        {
            var lastTicketId = await _helpDeskRepository.TableNoTracking.Where(x => x.SiteId == siteId).OrderByDescending(x => x.TicketNo).Select(x => x.TicketNo).FirstOrDefaultAsync();

            //if (!string.IsNullOrEmpty(lastTicketId))
            //    nextNumber = int.Parse(lastTicketId.Split('-')[1]) + 1;

            //return $"HR-{nextNumber:D2}";
            return lastTicketId > 0 ? lastTicketId + 1 : 1;
        }

        //public async Task<string> GetLastTicketId()
        //{
        //    var prefix = "HR";
        //    var query = await _helpDeskRepository.TableNoTracking
        //        .Where(x => x.TicketNo.StartsWith($"{prefix}-"))
        //        .OrderByDescending(x => x.TicketNo) // string order is fine for now
        //        .FirstOrDefaultAsync();

        //    int nextNumber = 1;

        //    if (query != null)
        //    {
        //        var parts = query.TicketNo.Split('-');
        //        if (parts.Length == 2 && int.TryParse(parts[1], out int lastNumber))
        //        {
        //            nextNumber = lastNumber + 1;
        //        }
        //    }

        //    return $"{prefix}-{nextNumber:D2}";
        //}
        #endregion

        #region GetLatestStatusLogByTicketId
        //public async Task<HelpDeskStatusLog> GetLatestStatusLogByTicketId(string siteId, string ticketId)
        //{
        //    var query = _helpDeskStatusLogRepository.TableNoTracking.Where(x => !x.HelpDesk.Deleted && x.HelpDesk.SiteId == siteId && x.HelpDeskId == ticketId);

        //    var item = await query.OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
        //    return item;
        //}

        public async Task<(HelpDeskStatusLog latestLog, int logCount)> GetLatestStatusLogByTicketId(string siteId, string ticketId)
        {
            //var query = _helpDeskStatusLogRepository.TableNoTracking
            //    .Where(x => !x.HelpDesk.Deleted && x.HelpDesk.SiteId == siteId && x.HelpDeskId == ticketId)
            //    .OrderBy(x => x.CreatedDate);

            //var logList = await query.ToListAsync();

            //var latestLog = logList.LastOrDefault();
            //int logCount = logList.Count;

            //return (latestLog, logCount);
            var logList = await _helpDeskStatusLogRepository.TableNoTracking.Where(x => !x.HelpDesk.Deleted && x.HelpDesk.SiteId == siteId && x.HelpDeskId == ticketId).ToListAsync();

            var orderedLogs = logList
                .OrderBy(x => string.IsNullOrEmpty(x.CreatedDate)
                    ? DateTime.MinValue
                    : Convert.ToDateTime(x.CreatedDate))
                .ToList();

            var latestLog = orderedLogs.LastOrDefault();
            int logCount = orderedLogs.Count;

            return (latestLog, logCount);
        }
        #endregion

        public void AddHelpDeskStatusLog(string helpDeskId, string statusId, string userId, DateTime GetDateTime)
        {
            //Get last status log
            var lastCreatedOnUtc = _helpDeskStatusLogRepository.Table
                                .Where(x => x.HelpDeskId == helpDeskId)
                                .OrderByDescending(x => x.CreatedOnUtc)
                                .Select(x => (DateTime?)x.CreatedOnUtc)
                                .FirstOrDefault();

            int durationInMinutes = 0;

            // Calculate duration
            if (lastCreatedOnUtc.HasValue && GetDateTime > lastCreatedOnUtc.Value)
            {
                durationInMinutes = (int)Math.Round((GetDateTime - lastCreatedOnUtc.Value).TotalMinutes, MidpointRounding.AwayFromZero);
            }

            // Insert status log
            var newLog = new HelpDeskStatusLog
            {
                HelpDeskId = helpDeskId,
                StatusId = statusId,
                CreatedById = userId,
                CreatedOnUtc = GetDateTime,
                DurationInMinutes = durationInMinutes
            };

            _helpDeskStatusLogRepository.Insert(newLog);

            // Calculate average duration
            var avgDuration = _helpDeskStatusLogRepository.Table
                                .Where(x => x.HelpDeskId == helpDeskId && x.DurationInMinutes > 0)
                                .Select(x => (int?)x.DurationInMinutes)
                                .Average() ?? 0;

            // Update HelpDesk table
            if (avgDuration > 0)
            {
                var helpDesk = _helpDeskRepository.GetById(helpDeskId);

                if (helpDesk != null)
                {
                    helpDesk.AverageDurationInMinutes = (int)avgDuration;
                    helpDesk.UpdatedById = userId;
                    helpDesk.UpdatedOnUtc = GetDateTime;
                    _helpDeskRepository.Update(helpDesk);
                }
            }
        }

        #region InsertHelpDesk
        // Title: InsertHelpDesk
        // Description: This method inserts a new HelpDesk entity into the repository. It takes a HelpDesk object as input and uses the _helpDeskRepository to handle the insertion operation.
        public void InsertHelpDesk(HelpDesk entity)
        {
            _helpDeskRepository.Insert(entity);
        }
        #endregion

        #region InsertHelpDeskStatusLog
        // Title: InsertHelpDeskStatusLog
        // Description: This method inserts a new HelpDeskStatusLog entity into the repository. It takes a HelpDeskStatusLog object as input and uses the _helpDeskStatusLogRepository to handle the insertion operation.
        public void InsertHelpDeskStatusLog(HelpDeskStatusLog entity)
        {
            _helpDeskStatusLogRepository.Insert(entity);
        }
        #endregion

        #region UpdateHelpDesk
        // Title: UpdateHelpDesk
        // Description: This method updates the specified HelpDesk entity in the repository. It takes a HelpDesk object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateHelpDesk(HelpDesk entity)
        {
            _helpDeskRepository.Update(entity);
        }
        #endregion

        #region DeleteHelpDesk
        // Title: DeleteHelpDesk
        // Description: Marks the specified HelpDesk entity as deleted by setting its `Deleted` property to true. 
        public void DeleteHelpDesk(HelpDesk entity)
        {
            entity.Deleted = true;
            _helpDeskRepository.Update(entity);
        }
        #endregion

    }
}
