using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.Design;
using Vsky.Services.Sites;

namespace Vsky.Services.Leads
{
    public class LeadService : ILeadService
    {
        #region Services Initializations
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Lead> _leadRepository;
        private readonly IRepository<LeadStages> _leadStagesRepository;
        private readonly IRepository<LeadActivities> _leadActivitiesRepository;
        private readonly IRepository<Notes> _notesRepository;

        public LeadService(
            UserManager<ApplicationUser> userManager,
            IRepository<Lead> leadRepository,
            IRepository<LeadStages> leadStagesRepository,
            IRepository<LeadActivities> leadActivitiesRepository, IRepository<Notes> notesRepository)
        {
            _userManager = userManager;
            _leadRepository = leadRepository;
            _leadStagesRepository = leadStagesRepository;
            _leadActivitiesRepository = leadActivitiesRepository;
            _notesRepository = notesRepository;
        }
        #endregion

        #region Private Methods

        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region GetAllLeads
        public async Task<IPagedList<Lead>> GetAllLeads(
            string SiteId,
            string userId,
            List<string> leadGroupIdsForUser,
            string SearchText,
            string personId,
            string companyId,
            List<string> leadGroupIds,
            string leadSourceId,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _leadRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            bool isAdmin = await IsCurrentUserAdmin(userId);
            if (!isAdmin)
            {
                query = query.Where(p => leadGroupIdsForUser.Contains(p.LeadGroupId));
            }

            if (!string.IsNullOrWhiteSpace(personId))
                query = query.Where(x => x.PersonId == personId);

            if (!string.IsNullOrWhiteSpace(companyId))
                query = query.Where(x => x.CompanyId == companyId);

            if (leadGroupIds?.Any() == true) query = query.Where(x => leadGroupIds.Contains(x.LeadGroupId));

            if (!string.IsNullOrWhiteSpace(leadSourceId))
                query = query.Where(x => x.LeadSourceId == leadSourceId);

            query = query.Where(x => !x.Deleted);

            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.Company.Name.Contains(SearchText.ToLower()) ||
                    (m.Person.FirstName.Contains(SearchText.ToLower()) || m.Person.LastName.Contains(SearchText.ToLower())) ||
                    m.LeadSources.DropDownValue.Contains(SearchText.ToLower()) ||
                    m.LeadGroup.DropDownValue.Contains(SearchText.ToLower()) ||
                    m.LeadArrivalDate.Value.Date == parsedDate.Date
                );
            }

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }

            // projection
            query = query.Select(x => new Lead
            {
                Id = x.Id,
                LeadArrivalDate = x.LeadArrivalDate,
                LeadNote = x.LeadNote,
                Person = new Person
                {
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName,
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    PrimaryPhoneNumber = x.Person.PrimaryPhoneNumber,
                    PrimaryEmailAddress = x.Person.PrimaryEmailAddress,
                    Id = x.Person.Id
                },
                Client = new CompanyClients
                {
                    Id = x.Client.Id,
                    Company = new Company
                    {
                        Id = x.Client.Company.Id,
                        Name = x.Client.Company.Name,
                    }
                },
                Company = new Company
                {
                    Id = x.Company.Id,
                    Name = x.Company.Name,
                },
                LeadSources = new DropDown
                {
                    DropDownValue = x.LeadSources.DropDownValue,
                    Id = x.LeadSources.Id
                },
                LeadGroup = new DropDown
                {
                    DropDownValue = x.LeadGroup.DropDownValue,
                    Id = x.LeadGroup.Id
                },
                LeadActivityLogs = x.LeadActivityLogs.Where(x => !x.Deleted).Select(m => new LeadActivityLogs
                {
                    Id = m.Id,
                }).ToList(),
                LeadNotesCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "Lead").Count(),
            });

            var list = new PagedList<Lead>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetAllLeadsCount
        public int GetAllLeadsCount(string SiteId)
        {
            return _leadRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId).Count();
        }
        #endregion

        #region GetById
        public async Task<Lead> GetById(string id)
        {
            var query = _leadRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetLeadDetailsById
        public async Task<Lead> GetLeadDetailsById(string id)
        {
            var query = _leadRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new Lead
            {
                Id = x.Id,
                LeadArrivalDate = x.LeadArrivalDate,
                LeadNote = x.LeadNote,
                LeadReference = x.LeadReference,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc,
                Person = new Person
                {
                    FirstName = x.Person.FirstName,
                    LastName = x.Person.LastName,
                    Id = x.Person.Id
                },
                Client = new CompanyClients
                {
                    Id = x.Client.Id,
                    Company = new Company
                    {
                        Id = x.Client.Company.Id,
                        Name = x.Client.Company.Name,
                    }
                },
                Company = new Company
                {
                    Id = x.Company.Id,
                    Name = x.Company.Name,
                },
                LeadSources = new DropDown
                {
                    DropDownValue = x.LeadSources.DropDownValue,
                    Id = x.LeadSources.Id
                },
                LeadGroup = new DropDown
                {
                    DropDownValue = x.LeadGroup.DropDownValue,
                    Id = x.LeadGroup.Id
                },
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
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
                LeadActivityLogs = x.LeadActivityLogs.Where(x => !x.Deleted).Select(m => new LeadActivityLogs
                {
                    Id = m.Id,
                    ActivityDate = m.ActivityDate,
                    ActivityNote = m.ActivityNote,
                    LeadActivity = new LeadActivities
                    {
                        Id = m.LeadActivity.Id,
                        ActivityName = m.LeadActivity.ActivityName,
                    },
                    LeadStage = new LeadStages
                    {
                        Id = m.LeadActivity.Id,
                        StageName = m.LeadStage.StageName,
                    }
                }).ToList(),
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region Get Lead Stages
        public async Task<IList<LeadStages>> GetAllLeadStages()
        {
            var query = _leadStagesRepository.TableNoTracking;
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region Get Lead Stages
        public async Task<IList<LeadActivities>> GetAllActivities()
        {
            var query = _leadActivitiesRepository.TableNoTracking;
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllLeadActivityListForDropdown
        public async Task<List<CommonDropDown>> GetAllLeadActivityListForDropdown()
        {
            var query = _leadActivitiesRepository.TableNoTracking.Where(x => !x.Deleted);
            var list = await query
                .OrderBy(x => x.ActivityName)
                .Select(x => new CommonDropDown
                {
                    Value = x.Id,
                    Text = x.ActivityName
                }).ToListAsync();

            return list;
        }
        #endregion

        #region GetAllLeadListForDropdown
        public async Task<List<Lead>> GetAllLeadListForDropdown(string SiteId)
        {
            var query = _leadRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
            query = query.Select(x => new Lead
            {
                Id = x.Id,
                Person = new Person
                {
                    FullName = x.Person.FirstName + " " + x.Person.LastName,
                    Id = x.Person.Id,
                }
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion 

        #region InsertLead
        public void InsertLead(Lead entity)
        {
            _leadRepository.Insert(entity);
        }
        #endregion

        #region UpdateLead
        public void UpdateLead(Lead entity)
        {
            _leadRepository.Update(entity);
        }
        #endregion

        #region DeleteLead
        public void DeleteLead(Lead entity)
        {
            entity.Deleted = true;

            _leadRepository.Update(entity);
        }
        #endregion

        #region private method
        private async Task<bool> IsCurrentUserAdmin(string CId)
        {
            var userdata = await _userManager.FindByIdAsync(CId);
            var user = await _userManager.FindByNameAsync(userdata.UserName);
            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains("Admin") || roles.Contains("Site Super Admin") || roles.Contains("System Super Admin");

            return isAdmin;
        }
        #endregion
    }
}
