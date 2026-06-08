using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.InfraDatabases
{
    public class InfraDatabaseService : IInfraDatabaseService
    {
        #region Define Services
        private readonly IRepository<InfraDatabase> _infraDatabaseRepository;
        #endregion

        #region Services Initializations
        public InfraDatabaseService(IRepository<InfraDatabase> InfraDatabaseRepository)
        {
            _infraDatabaseRepository = InfraDatabaseRepository;
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

        #region GetAllInfraDatabaseForList
        public IPagedList<InfraDatabase> GetAllInfraDatabaseForList(
            string siteId,
            List<string> infraServiceIds,
            string searchText,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {
            var query = _infraDatabaseRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);

            if (infraServiceIds?.Any() == true) query = query.Where(x => infraServiceIds.Contains(x.InfraServiceId));

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();
                query = query.Where(m =>
                    m.WalletNumber.ToLower().Contains(searchText) ||
                    m.InfraService.Name.ToLower().Contains(searchText) ||
                    m.WalletType.DropDownValue.ToLower().Contains(searchText) ||
                    m.ServerName.ToLower().Contains(searchText) ||
                    m.Name.ToLower().Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            query = query.Select(x => new InfraDatabase
            {
                Id = x.Id,
                Name = x.Name,
                ServerName = x.ServerName,
                IsReadOrWrite = x.IsReadOrWrite,
                InfraServiceId = x.InfraServiceId,
                WalletTypeId = x.WalletTypeId,
                WalletNumber = x.WalletNumber,
                Instructions = x.Instructions,
                WalletType = new DropDown
                {
                    Id = x.WalletType.Id,
                    DropDownValue = x.WalletType.DropDownValue
                },
                InfraService = new InfraAccountServices
                {
                    Id = x.InfraService.Id,
                    Name = x.InfraService.Name
                },
                InfraDatabaseProjectInstanceMapping = x.InfraDatabaseProjectInstanceMapping.Where(m => !m.Deleted).Select(x => new InfraDatabaseProjectInstanceMapping
                {
                    Id = x.Id,
                    InfraProjectInstance = new InfraProjectInstance
                    {
                        Id = x.InfraProjectInstance.Id,
                        URL = x.InfraProjectInstance.URL,
                        Platform = new DropDown
                        {
                            DropDownValue = x.InfraProjectInstance.Platform.DropDownValue
                        }
                    }
                }).ToList()
            });

            var list = new PagedList<InfraDatabase>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetInfraDatabaseById
        // Title: GetInfraDatabaseById
        // Description: This method retrieves a InfraDatabase from the database by its unique identifier (`id`). 
        public async Task<InfraDatabase> GetInfraDatabaseById(string id)
        {
            var query = _infraDatabaseRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetInfraDatabaseInDetailById
        // Title: GetInfraDatabaseInDetailById
        // Description: The method selects relevant fields from the Infra FTP entity
        public async Task<InfraDatabase> GetInfraDatabaseInDetailById(string id)
        {
            var query = _infraDatabaseRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id)
                .Select(x => new InfraDatabase
                {
                    Id = x.Id,
                    Name = x.Name,
                    ServerName = x.ServerName,
                    IsReadOrWrite = x.IsReadOrWrite,
                    InfraServiceId = x.InfraServiceId,
                    WalletTypeId = x.WalletTypeId,
                    WalletNumber = x.WalletNumber,
                    Instructions = x.Instructions,
                    CreatedOnUtc = x.CreatedOnUtc,
                    UpdatedOnUtc = x.UpdatedOnUtc,
                    WalletType = new DropDown
                    {
                        Id = x.WalletType.Id,
                        DropDownValue = x.WalletType.DropDownValue
                    },
                    InfraService = new InfraAccountServices
                    {
                        Id = x.InfraService.Id,
                        Name = x.InfraService.Name
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
                    InfraDatabaseProjectInstanceMapping = x.InfraDatabaseProjectInstanceMapping.Where(m => !m.Deleted).Select(x => new InfraDatabaseProjectInstanceMapping
                    {
                        Id = x.Id,
                        InfraProjectInstance = new InfraProjectInstance
                        {
                            Id = x.InfraProjectInstance.Id,
                            URL = x.InfraProjectInstance.URL,
                            Platform = new DropDown
                            {
                                Id = x.InfraProjectInstance.Platform.Id,
                                DropDownValue = x.InfraProjectInstance.Platform.DropDownValue
                            }
                        }
                    }).ToList()
                });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertInfraDatabase
        public void InsertInfraDatabase(Models.InfraDatabase entity)
        {
            _infraDatabaseRepository.Insert(entity);
        }
        #endregion

        #region UpdateInfraDatabase
        public void UpdateInfraDatabase(Models.InfraDatabase entity)
        {
            _infraDatabaseRepository.Update(entity);
        }
        #endregion

        #region DeleteInfraDatabase
        public void DeleteInfraDatabase(Models.InfraDatabase entity)
        {
            entity.Deleted = true;
            _infraDatabaseRepository.Update(entity);
        }
        #endregion
    }
}

