using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.InfraAccounts
{
    public class InfraAccountServicesService : IInfraAccountServicesService
    {
        #region Define Services
        private readonly IRepository<InfraAccountServices> _infraAccountServicesRepository;
        private readonly IInfraAccountServiceCalculationService _calculationService;
        #endregion

        #region Services Initializations
        public InfraAccountServicesService(
            IRepository<InfraAccountServices>infraAccountServicesRepository,
            IInfraAccountServiceCalculationService calculationService
        )
        {
            _infraAccountServicesRepository = infraAccountServicesRepository;
            _calculationService = calculationService;
        }
        # endregion

        #region Private Methods
        // Title: GetOrderBy
        // Description: This method returns the input string as it is, which can be used as the `ORDER BY` clause in a SQL query.
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllInfraAccountServicesList
        public IPagedList<InfraAccountServices> GetAllInfraAccountServicesList(
            string siteId,
            List<string> projectIds,
            List<string> itemTypeIds,
            List<string> infraAccountIds,
            List<string> ownerShipTypeIds,
            List<string> paymentTermIds,
            string searchText,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {
            var query = _infraAccountServicesRepository.TableNoTracking.Where(x => !x.Deleted && x.InfraAccount.SiteId == siteId && !x.InfraAccount.Deleted);

            if (projectIds?.Any() == true)
            {
                query = query.Where(x =>
                    x.InfraProjectServices.Any(s => projectIds.Contains(s.InfraProjectId))
                );
            }
            if (itemTypeIds?.Any() == true) query = query.Where(x => itemTypeIds.Contains(x.ItemTypeId));
            if (infraAccountIds?.Any() == true) query = query.Where(x => infraAccountIds.Contains(x.InfraAccountId));
            if (ownerShipTypeIds?.Any() == true) query = query.Where(x => ownerShipTypeIds.Contains(x.OwnerShipTypeId));
            if (paymentTermIds?.Any() == true) query = query.Where(x => paymentTermIds.Contains(x.PaymentTermId));

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();
                query = query.Where(m =>
                    m.URL.ToLower().Contains(searchText) ||
                    m.Name.ToLower().Contains(searchText) ||
                    m.InfraAccount.Name.ToLower().Contains(searchText) ||
                    m.ItemType.DropDownValue.ToLower().Contains(searchText) ||
                    m.OwnerShipType.DropDownValue.ToLower().Contains(searchText) ||
                    m.PaymentTerm.DropDownValue.ToLower().Contains(searchText) ||
                    m.WalletNumber.ToLower().Contains(searchText) ||
                    m.WalletType.DropDownValue.ToLower().Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            query = query.Select(m => new InfraAccountServices
            {
                Id = m.Id,
                Name = m.Name,
                URL = m.URL,
                StartDate = m.StartDate,
                PriceInDollar = m.PriceInDollar,
                WalletNumber = m.WalletNumber,
                Instructions = m.Instructions,
                PriceHistories = m.PriceHistories.Where(ph => !ph.Deleted).OrderBy(ph => ph.StartDate).ToList(),
                InfraAccount = new InfraAccount
                {
                    Id = m.InfraAccount.Id,
                    Name = m.InfraAccount.Name,
                    CustomerId = m.InfraAccount.CustomerId,
                    Provider = new DropDown
                    {
                        DropDownValue = m.InfraAccount.Provider.DropDownValue
                    }
                },
                InfraAccountService = new InfraAccountServices
                {
                    Id = m.InfraAccountService.Id,
                    Name = m.InfraAccountService.Name
                },
                ItemType = new DropDown
                {
                    Id = m.ItemType.Id,
                    DropDownValue = m.ItemType.DropDownValue
                },
                OwnerShipType = new DropDown
                {
                    Id = m.OwnerShipType.Id,
                    DropDownValue = m.OwnerShipType.DropDownValue
                },
                PaymentTerm = new DropDown
                {
                    Id = m.PaymentTerm.Id,
                    DropDownValue = m.PaymentTerm.DropDownValue
                },
                WalletType = new DropDown
                {
                    Id = m.WalletType.Id,
                    DropDownValue = m.WalletType.DropDownValue
                },
                InfraProjectServices = m.InfraProjectServices.Where(m => !m.Deleted).Select(x => new InfraProjectServices
                {
                    Id = x.Id,
                    Project = new Project
                    {
                        Id = x.Project.Id,
                        Name = x.Project.Name
                    }
                }).ToList()
            });

            var list = new PagedList<InfraAccountServices>(query, page, pageSize);
            foreach (var item in list)
            {
                item.YTD = _calculationService.CalculateYTD(item.PriceHistories);
                item.PriceInDollar = _calculationService.GetCurrentCyclePrice(item.PriceHistories);
            }
            return list;
        }
        #endregion

        #region GetInfraAccountServicesById
        // Title: GetInfraAccountServicesById
        // Description: This method retrieves a InfraAccountServices from the database by its unique identifier (`id`). 
        public async Task<InfraAccountServices> GetInfraAccountServicesById(string id)
        {
            var query = _infraAccountServicesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllInfraServiceListForDropdown
        public async Task<List<CommonDropDown>> GetAllInfraServiceListForDropdown(string SiteId, string accountId = null)
        {
            var query = _infraAccountServicesRepository.TableNoTracking.Where(x => !x.Deleted && x.InfraAccount.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(accountId) && accountId != "undefined")
                query = query.Where(x => x.InfraAccountId == accountId);

            var list = await query
                .OrderBy(x => x.Name)
                .Select(x => new CommonDropDown
            {
                Value = x.Id,
                Text = x.Name
            }).ToListAsync();

            return list;
        }
        #endregion

        #region GetInfraAccountServicesByName
        // Title: GetInfraAccountServicesByName
        // Description: This method retrieves a Infra Account based on its name. It allows an optional exclusion of a Infra Account by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific Infra Account. The method returns the first matching Infra Account or null if no match is found.
        public async Task<InfraAccountServices> GetInfraAccountServicesByName(string SiteId, string name, string accountId, string id = null)
        {
            var query = _infraAccountServicesRepository.TableNoTracking.Where(x => !x.Deleted && x.InfraAccount.SiteId == SiteId && x.Name.ToLower() == name.ToLower() && x.InfraAccountId == accountId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetInfraAccountServicesInDetailById
        // Title: GetInfraAccountServicesInDetailById
        // Description: The method selects relevant fields from the Infra Account service entity
        public async Task<InfraAccountServices> GetInfraAccountServicesInDetailById(string id)
        {
            var query = _infraAccountServicesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id)
                .Select(m => new InfraAccountServices
                {
                    Id = m.Id,
                    Name = m.Name,
                    URL = m.URL,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    PriceInDollar = m.PriceInDollar,
                    ActualPriceInDollar = Math.Round(
                        (decimal)m.PriceInDollar /
                        Math.Max(
                            m.InfraProjectServices.Count(x => !x.Deleted),
                            1
                        ),
                        2
                    ),
                    WalletNumber = m.WalletNumber,
                    CreatedOnUtc = m.CreatedOnUtc,
                    UpdatedOnUtc = m.UpdatedOnUtc,
                    Instructions = m.Instructions,
                    PriceHistories = m.PriceHistories.Where(ph => !ph.Deleted).OrderBy(ph => ph.StartDate).ToList(),
                    InfraAccount = new InfraAccount
                    {
                        Id = m.InfraAccount.Id,
                        Name = m.InfraAccount.Name,
                        CustomerId = m.InfraAccount.CustomerId
                    },
                    InfraAccountService = new InfraAccountServices
                    {
                        Name = m.InfraAccountService.Name
                    },
                    ItemType = new DropDown
                    {
                        Id = m.ItemType.Id,
                        DropDownValue = m.ItemType.DropDownValue
                    },
                    OwnerShipType = new DropDown
                    {
                        Id = m.OwnerShipType.Id,
                        DropDownValue = m.OwnerShipType.DropDownValue
                    },
                    PaymentTerm = new DropDown
                    {
                        Id = m.PaymentTerm.Id,
                        DropDownValue = m.PaymentTerm.DropDownValue
                    },
                    WalletType = new DropDown
                    {
                        Id = m.WalletType.Id,
                        DropDownValue = m.WalletType.DropDownValue
                    },
                    CreatedBy = new ApplicationUser
                    {
                        Id = m.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = m.CreatedBy.PersonId,
                            FullName = m.CreatedBy.Person.FirstName + " " + m.CreatedBy.Person.LastName
                        }
                    },
                    UpdatedBy = new ApplicationUser
                    {
                        Id = m.UpdatedBy.Id,
                        Person = new Person
                        {
                            Id = m.UpdatedBy.PersonId,
                            FullName = m.UpdatedBy.Person.FirstName + " " + m.UpdatedBy.Person.LastName
                        }
                    },
                    InfraProjectServices = m.InfraProjectServices.Where(m => !m.Deleted).Select(x => new InfraProjectServices
                    {
                        Id = x.Id,
                        Project = new Project
                        {
                            Id = x.Project.Id,
                            Name = x.Project.Name
                        }
                    }).ToList()
                });
                var item = await query.FirstOrDefaultAsync();
                //if (item != null)
                //{
                //    item.YTD = _calculationService.CalculateYTD(item.PriceHistories);
                //    item.PriceInDollar = _calculationService.GetCurrentCyclePrice(item.PriceHistories);

                //}
                return item;
        }
        #endregion

        #region HasActiveServices
        public async Task<bool> HasActiveServices(string accountId)
        {
            return await _infraAccountServicesRepository.TableNoTracking
                .AnyAsync(x =>
                    !x.Deleted &&
                    !x.InfraAccount.Deleted &&
                    x.InfraAccountId == accountId
                );
        }
        #endregion

        #region UpdateInfraAccountServices
        public void UpdateInfraAccountServices(Models.InfraAccountServices entity)
        {
            _infraAccountServicesRepository.Update(entity);
        }
        #endregion

        #region DeleteInfraAccountServices
        public void DeleteInfraAccountServices(Models.InfraAccountServices entity)
        {
            entity.Deleted = true;
            _infraAccountServicesRepository.Update(entity);
        }
        #endregion

        #region InsertInfraAccountServicesList
        public void InsertInfraAccountServicesList(IList<Models.InfraAccountServices> entities)
        {
            _infraAccountServicesRepository.Insert(entities);
        }
        #endregion

        #region UpdateInfraAccountServicesList
        public void UpdateInfraAccountServicesList(IList<Models.InfraAccountServices> entities)
        {
            _infraAccountServicesRepository.Update(entities);
        }
        #endregion

        #region DeleteInfraAccountServicesList
        public void DeleteInfraAccountServicesList(List<Models.InfraAccountServices> entities)
        {
            var infraAccountServices = new List<Models.InfraAccountServices>();
            foreach (var items in entities)
            {
                items.Deleted = true;
                infraAccountServices.Add(items);
            }
            _infraAccountServicesRepository.Update(infraAccountServices);
        }
        #endregion
    }
}
