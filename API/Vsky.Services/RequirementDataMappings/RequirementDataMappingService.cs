using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Common;

namespace Vsky.Services.RequirementDataMappings
{
    public class RequirementDataMappingService : IRequirementDataMappingService
    {
        #region Define Services
        private readonly IRepository<RequirementDataMapping> _requirementDataMappingRepository;
        private readonly IRepository<Notes> _notesRepository;
        private readonly ICommonService _commonService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations

        public RequirementDataMappingService(
            IRepository<RequirementDataMapping> requirementDataMappingRepository,
            IRepository<Notes> notesRepository,
            ICommonService commonService,
            UserManager<ApplicationUser> userManager,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _requirementDataMappingRepository = requirementDataMappingRepository;
            _notesRepository = notesRepository;
            _commonService = commonService;
            _userManager = userManager;
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

        #region GetAllRequirementDataMappings
        public async Task<IPagedList<RequirementDataMapping>> GetAllRequirementDataMappings(
            string SiteId,
            string loggedUserId,
            string SearchText,
            List<string> requirementIds,
            string source,
            string target,
            string sortBy,
            Dictionary<string, string> sorts,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _requirementDataMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Requirement.SiteId == SiteId);

            if (requirementIds != null && requirementIds.Any())
                query = query.Where(x => requirementIds.Contains(x.RequirementId));

            if (!string.IsNullOrWhiteSpace(source))
            {
                source = source.Trim().ToLower();
                query = query.Where(x => x.Source.ToLower().Contains(source));
            }

            if (!string.IsNullOrWhiteSpace(target))
            {
                target = target.Trim().ToLower();
                query = query.Where(x => x.Target.ToLower().Contains(target));
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
                       m.Requirement.Title.ToLower().Contains(SearchText.ToLower()) ||
                       m.Source.ToLower().Contains(SearchText.ToLower()) ||
                       m.Target.ToLower().Contains(SearchText.ToLower())
                );
            }

            query = query.Select(x => new RequirementDataMapping
            {
                Id = x.Id,
                RequirementId = x.RequirementId,
                Source = x.Source,
                Target = x.Target,
                Requirement = new Requirement
                {
                    Id = x.Requirement.Id,
                    Title = x.Requirement.Title
                }
            });

            var list = new PagedList<RequirementDataMapping>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetRequirementDataMappingsByRequirementId
        public async Task<List<RequirementDataMapping>> GetRequirementDataMappingsByRequirementId(string requirementId)
        {
            var query = _requirementDataMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.RequirementId == requirementId);

            query = query.Select(x => new RequirementDataMapping
            {
                Id = x.Id,
                RequirementId = x.RequirementId,
                Source = x.Source,
                Target = x.Target,
                Requirement = new Requirement
                {
                    Id = x.Requirement.Id,
                    Title = x.Requirement.Title
                },
                RequirementDataMappingNotes = x.RequirementDataMappingNotes.Where(t => !t.Deleted).OrderBy(t => t.CreatedOnUtc).Select(t => new RequirementDataMappingNotes
                {
                    Id = t.Id,
                    Note = t.Note
                }).ToList(),
            });

            var item = await query.ToListAsync();
            return item;
        }
        #endregion

        #region GetById
        public async Task<RequirementDataMapping> GetRequirementDataMappingById(string id)
        {
            var query = _requirementDataMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);

            var item = await query.FirstOrDefaultAsync();

            return item;
        }
        #endregion

        #region InsertRequirementDataMapping
        public void InsertRequirementDataMapping(RequirementDataMapping entity)
        {
            _requirementDataMappingRepository.Insert(entity);
        }
        #endregion

        #region UpdateRequirementDataMapping
        public void UpdateRequirementDataMapping(RequirementDataMapping entity)
        {
            _requirementDataMappingRepository.Update(entity);
        }
        #endregion

        #region DeleteRequirementDataMapping
        public void DeleteRequirementDataMapping(RequirementDataMapping entity)
        {
            entity.Deleted = true;

            _requirementDataMappingRepository.Update(entity);
        }
        #endregion
    }
}
