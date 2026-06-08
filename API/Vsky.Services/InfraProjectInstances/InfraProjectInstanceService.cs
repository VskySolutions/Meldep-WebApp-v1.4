using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.InfraProjectInstances
{
    public class InfraProjectInstanceService : IInfraProjectInstanceService
    {
        #region Define Services
        private readonly IRepository<InfraProjectInstance> _infraProjectInstanceRepository;
        #endregion

        #region Services Initializations
        public InfraProjectInstanceService(IRepository<InfraProjectInstance> infraProjectInstanceRepository)
        {
            _infraProjectInstanceRepository = infraProjectInstanceRepository;
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

        #region GetAllInfraProjectInstanceForList
        public IPagedList<InfraProjectInstance> GetAllInfraProjectInstanceForList(
            string siteId,
            List<string> infraProjectIds,
            List<string> platformIds,
            List<string> instanceTypeIds,
            string searchText,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {
            var query = _infraProjectInstanceRepository.TableNoTracking.Where(x => !x.Deleted);

            if (infraProjectIds?.Any() == true) query = query.Where(x => infraProjectIds.Contains(x.InfraProjectId));
            if (platformIds?.Any() == true) query = query.Where(x => platformIds.Contains(x.PlatformId));
            if (instanceTypeIds?.Any() == true) query = query.Where(x => instanceTypeIds.Contains(x.InstanceTypeId));

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();
                query = query.Where(m =>
                    m.URL.ToLower().Contains(searchText) ||
                    m.Platform.DropDownValue.ToLower().Contains(searchText) ||
                    m.InstanceType.DropDownValue.ToLower().Contains(searchText) ||
                    m.InfraProject.Name.ToLower().Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            query = query.Select(x => new InfraProjectInstance
            {
                Id = x.Id,
                URL = x.URL,
                Instructions = x.Instructions,
                InfraProject = new Project
                {
                    Id = x.InfraProject.Id,
                    Name = x.InfraProject.Name
                },
                InstanceType = new DropDown
                {
                    Id = x.InstanceType.Id,
                    DropDownValue = x.InstanceType.DropDownValue
                },
                Platform = new DropDown
                {
                    Id = x.Platform.Id,
                    DropDownValue = x.Platform.DropDownValue
                }
            });

            var list = new PagedList<InfraProjectInstance>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetAllInfraProjectInstanceListForDropdown
        public async Task<List<InfraProjectInstance>> GetAllInfraProjectInstanceListForDropdown(string SiteId, string projectId = null)
        {
            var query = _infraProjectInstanceRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrEmpty(projectId) && projectId != "undefined")
                query = query.Where(x => x.InfraProjectId == projectId);

            query = query.Select(x => new InfraProjectInstance
            {
                Id = x.Id,
                URL = x.URL,
                Platform = new DropDown
                {
                    Id = x.Platform.Id,
                    DropDownValue = x.Platform.DropDownValue
                },
                InstanceType = new DropDown
                {
                    DropDownValue = x.InstanceType.DropDownValue
                }
            });

            var list = await query.OrderBy(m => m.Platform.DropDownValue).ToListAsync();
            return list;
        }
        #endregion

        #region GetInfraProjectInstanceById
        // Title: GetInfraProjectInstanceById
        // Description: This method retrieves a Infra Project Instance from the database by its unique identifier (`id`). 
        public async Task<InfraProjectInstance> GetInfraProjectInstanceById(string id)
        {
            var query = _infraProjectInstanceRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetInfraProjectInstanceInDetailById
        // Title: GetInfraProjectInstanceInDetailById
        // Description: The method selects relevant fields from the Infra Project Instance entity
        public async Task<InfraProjectInstance> GetInfraProjectInstanceInDetailById(string id)
        {
            var query = _infraProjectInstanceRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id)
                .Select(x => new InfraProjectInstance
                {
                    Id = x.Id,
                    URL = x.URL,
                    Instructions = x.Instructions,
                    CreatedOnUtc = x.CreatedOnUtc,
                    UpdatedOnUtc = x.UpdatedOnUtc,
                    InfraProject = new Project
                    {
                        Id = x.InfraProject.Id,
                        Name = x.InfraProject.Name
                    },
                    InstanceType = new DropDown
                    {
                        Id = x.InstanceType.Id,
                        DropDownValue = x.InstanceType.DropDownValue
                    },
                    Platform = new DropDown
                    {
                        Id = x.Platform.Id,
                        DropDownValue = x.Platform.DropDownValue
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
                    InfraProjectInstanceRole = x.InfraProjectInstanceRole.Where(m => !m.Deleted).OrderBy(x => x.RoleName).Select(m => new InfraProjectInstanceRole
                    {
                        Id = m.Id,
                        RoleName = m.RoleName,
                        InfraProjectInstanceRoleUsers = m.InfraProjectInstanceRoleUsers.Where(x => !x.Deleted).Select(r => new InfraProjectInstanceRoleUsers
                        {
                            Id = r.Id,
                            UserName = r.UserName,
                            Password = r.Password
                        }).ToList()
                    }).ToList()
                });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertInfraProjectInstance
        public void InsertInfraProjectInstance(Models.InfraProjectInstance entity)
        {
            _infraProjectInstanceRepository.Insert(entity);
        }
        #endregion

        #region UpdateInfraProjectInstance
        public void UpdateInfraProjectInstance(Models.InfraProjectInstance entity)
        {
            _infraProjectInstanceRepository.Update(entity);
        }
        #endregion

        #region DeleteInfraProjectInstance
        public void DeleteInfraProjectInstance(Models.InfraProjectInstance entity)
        {
            entity.Deleted = true;
            _infraProjectInstanceRepository.Update(entity);
        }
        #endregion
    }
}
