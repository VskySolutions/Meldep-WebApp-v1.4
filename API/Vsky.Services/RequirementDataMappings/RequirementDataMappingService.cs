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

        #region GetAllRequirementDataMappingGroups
        //public async Task<List<RequirementDataMappingGroup>> GetAllRequirementDataMappingGroups(string siteId)
        //{
        //    var query = _requirementDataMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Requirement.SiteId == siteId)
        //        .Select(x => new
        //        {
        //            RequirementId = x.RequirementId,
        //            RequirementTitle = x.Requirement.Title,

        //            Mapping = new RequirementDataMapping
        //            {
        //                Id = x.Id,
        //                RequirementId = x.RequirementId,
        //                Source = x.Source,
        //                Target = x.Target,
        //                CreatedOnUtc = x.CreatedOnUtc,
        //                CreatedBy = new ApplicationUser
        //                {
        //                    Id = x.CreatedBy.Id,
        //                    UserName = x.CreatedBy.UserName,
        //                    Person = new Person
        //                    {
        //                        Id = x.CreatedBy.Person.Id,
        //                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
        //                    }
        //                },
        //                RequirementDataMappingNotes =
        //                    x.RequirementDataMappingNotes
        //                        .Where(n => !n.Deleted)
        //                        .OrderBy(n => n.CreatedOnUtc)
        //                        .Select(n => new RequirementDataMappingNotes
        //                        {
        //                            Id = n.Id,
        //                            RequirementDataMappingId =
        //                                n.RequirementDataMappingId,
        //                            Note = n.Note,
        //                            CreatedOnUtc = n.CreatedOnUtc
        //                        })
        //                        .ToList()
        //            }
        //        });

        //    var result = await query.ToListAsync();

        //    return result
        //        .GroupBy(x => new
        //        {
        //            x.RequirementId,
        //            x.RequirementTitle
        //        })
        //        .Select(x => new RequirementDataMappingGroup
        //        {
        //            RequirementId = x.Key.RequirementId,
        //            RequirementTitle = x.Key.RequirementTitle,
        //            DataMappings = x
        //                .Select(m => m.Mapping)
        //                .OrderByDescending(m => m.CreatedOnUtc)
        //                .ToList()
        //        })
        //        .ToList();
        //}

        public async Task<IPagedList<RequirementDataMappingGroup>> GetAllRequirementDataMappingGroups(
            string SiteId,
            string loggedUserId,
            string SearchText,
            List<string> projectIds,
            List<string> projectModuleIds,
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
            {
                query = query.Where(x => requirementIds.Contains(x.RequirementId));
            }

            if (projectIds != null && projectIds.Any())
            {
                query = query.Where(x => projectIds.Contains(x.Requirement.Project.Id));
            }

            if (projectModuleIds != null && projectModuleIds.Any())
            {
                query = query.Where(x => projectModuleIds.Contains(x.Requirement.ProjectModule.Id));
            }

            if (!string.IsNullOrWhiteSpace(source))
            {
                source = source.Trim().ToLower();

                query = query.Where(x => x.Source.ToLower().Contains(source));
            }

            if (!string.IsNullOrWhiteSpace(target))
            {
                target = target.Trim().ToLower();

                query = query.Where(x =>
                    x.Target.ToLower().Contains(target));
            }

            // Search
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                SearchText = SearchText.Trim().ToLower();

                query = query.Where(x =>
                    x.Requirement.Title.ToLower().Contains(SearchText) ||
                    x.Source.ToLower().Contains(SearchText) ||
                    x.Target.ToLower().Contains(SearchText));
            }

            // Get the Requirement IDs after applying all filters
            var requirementQuery = query
                .Select(x => new
                {
                    RequirementId = x.RequirementId,
                    RequirementTitle = x.Requirement.Title,
                    CreatedOnUtc = x.CreatedOnUtc
                })
                .GroupBy(x => new
                {
                    x.RequirementId,
                    x.RequirementTitle
                })
                .Select(x => new RequirementDataMappingGroup
                {
                    RequirementId = x.Key.RequirementId,
                    RequirementTitle = x.Key.RequirementTitle
                });

            // Sorting Requirement groups
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "requirementTitle")
                {
                    requirementQuery = descending
                        ? requirementQuery.OrderByDescending(x => x.RequirementTitle)
                        : requirementQuery.OrderBy(x => x.RequirementTitle);
                }
                else
                {
                    requirementQuery = requirementQuery.OrderBy(x => x.RequirementTitle);
                }
            }
            else
            {
                requirementQuery = requirementQuery.OrderBy(x => x.RequirementTitle);
            }

            // Pagination happens at Requirement level
            var pagedRequirements = new PagedList<RequirementDataMappingGroup>(
                requirementQuery,
                page,
                pageSize
            );

            // Get only the Requirement IDs for the current page
            var pagedRequirementIds = pagedRequirements
                .Select(x => x.RequirementId)
                .ToList();

            // Get mappings for the current page Requirements
            var mappingQuery = _requirementDataMappingRepository.TableNoTracking
                .Where(x =>
                    !x.Deleted &&
                    pagedRequirementIds.Contains(x.RequirementId))
                .Select(x => new RequirementDataMapping
                {
                    Id = x.Id,
                    RequirementId = x.RequirementId,
                    Source = x.Source,
                    Target = x.Target,
                    CreatedOnUtc = x.CreatedOnUtc,
                    UpdatedOnUtc = x.UpdatedOnUtc,

                    CreatedBy = new ApplicationUser
                    {
                        Id = x.CreatedBy.Id,
                        UserName = x.CreatedBy.UserName,
                        Person = new Person
                        {
                            Id = x.CreatedBy.Person.Id,
                            FullName =
                                x.CreatedBy.Person.FirstName +
                                " " +
                                x.CreatedBy.Person.LastName
                        }
                    },
                    UpdatedBy = new ApplicationUser
                    {
                        Id = x.UpdatedBy.Id,
                        Person = new Person
                        {
                            Id = x.UpdatedBy.Person.Id,
                            FullName =
                                x.UpdatedBy.Person.FirstName +
                                " " +
                                x.UpdatedBy.Person.LastName
                        }
                    },

                    RequirementDataMappingNotes =
                        x.RequirementDataMappingNotes
                            .Where(n => !n.Deleted)
                            .OrderBy(n => n.CreatedOnUtc)
                            .Select(n => new RequirementDataMappingNotes
                            {
                                Id = n.Id,
                                RequirementDataMappingId =
                                    n.RequirementDataMappingId,
                                Note = n.Note,
                                CreatedOnUtc = n.CreatedOnUtc
                            })
                            .ToList()
                });

            var mappings = await mappingQuery.ToListAsync();

            // Put mappings under their Requirement
            foreach (var requirement in pagedRequirements)
            {
                requirement.DataMappings = mappings
                    .Where(x =>
                        x.RequirementId == requirement.RequirementId)
                    .OrderByDescending(x => x.CreatedOnUtc)
                    .ToList();
            }

            return pagedRequirements;
        }
        #endregion

        #region GetAllDataMappingByRequirementId
        public IPagedList<RequirementDataMapping> GetAllDataMappingByRequirementId(string SiteId, string requirementId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _requirementDataMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.Requirement.SiteId == SiteId && x.RequirementId == requirementId);

            query = query.OrderByDescending(x => x.CreatedOnUtc).Select(x => new RequirementDataMapping
            {
                Id = x.Id,
                Source = x.Source,
                Target = x.Target,
                CreatedOnUtc = x.CreatedOnUtc,
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
                CreatedOnUtc = x.CreatedOnUtc,
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                    }
                },
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
