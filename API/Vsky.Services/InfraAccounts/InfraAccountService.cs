using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.InfraAccounts
{
    public class InfraAccountService : IInfraAccountService
    {
        #region Define Services
        private readonly IRepository<InfraAccount> _infraAccountRepository;
        #endregion

        #region Services Initializations
        public InfraAccountService(IRepository<InfraAccount> infraAccountRepository)
        {
            _infraAccountRepository = infraAccountRepository;
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

        #region GetAllInfraAccountList
        public IPagedList<InfraAccount> GetAllInfraAccountList(
            string siteId,
            string searchText,
            List<string> providerIds,
            List<string> infraAccountIds,
            string CCLast4Digits,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {
            var query = _infraAccountRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);

            if (providerIds?.Any() == true) query = query.Where(x => providerIds.Contains(x.ProviderId));
            if (infraAccountIds?.Any() == true) query = query.Where(x => infraAccountIds.Contains(x.Id));

            if (!string.IsNullOrWhiteSpace(CCLast4Digits))
            {
                CCLast4Digits = CCLast4Digits.Trim().ToLower();
                query = query.Where(x => x.CCLast4Digits.ToLower().Contains(CCLast4Digits));
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();
                query = query.Where(m =>
                    m.Provider.DropDownValue.ToLower().Contains(searchText) ||
                    m.URL.ToLower().Contains(searchText) ||
                    m.CCLast4Digits.ToLower().Contains(searchText) ||
                    m.WalletNumber.ToLower().Contains(searchText) ||
                    m.WalletType.DropDownValue.ToLower().Contains(searchText) ||
                    m.CustomerId.ToLower().Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy == "totalServicesCost")
                {
                    query = descending
                        ? query.OrderByDescending(x => x.InfraAccountServices
                            .Where(s => !s.Deleted)
                            .Sum(s => (decimal?)s.PriceInDollar) ?? 0)
                        : query.OrderBy(x => x.InfraAccountServices
                            .Where(s => !s.Deleted)
                            .Sum(s => (decimal?)s.PriceInDollar) ?? 0);
                }
                else
                {
                    var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                    query = query.OrderBy(orderBy);
                }
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            query = query.Select(x => new InfraAccount
            {
                Id = x.Id,
                URL = x.URL,
                Name = x.Name,
                CCLast4Digits = x.CCLast4Digits,
                WalletNumber = x.WalletNumber,
                CustomerId = x.CustomerId,
                WalletType = new DropDown
                {
                    Id = x.WalletType.Id,
                    DropDownValue = x.WalletType.DropDownValue
                },
                Provider = new DropDown
                {
                    Id = x.Provider.Id,
                    DropDownValue = x.Provider.DropDownValue
                },
                TotalServicesCost = x.InfraAccountServices.Where(s => !s.Deleted).Sum(s => (decimal?)s.PriceInDollar) ?? 0
            });

            var list = new PagedList<InfraAccount>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetAllInfraAccountForDropdown
        public async Task<List<InfraAccount>> GetAllInfraAccountListForDropdown(string SiteId)
        {
            var query = _infraAccountRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            query = query.Select(x => new InfraAccount
            {
                Id = x.Id,
                Name = x.Name,
                CustomerId = x.CustomerId,
                Provider = new DropDown
                {
                    Id = x.Provider.Id,
                    DropDownValue = x.Provider.DropDownValue
                }
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetInfraAccountById
        // Title: GetInfraAccountById
        // Description: This method retrieves a InfraAccount from the database by its unique identifier (`id`). 
        public async Task<InfraAccount> GetInfraAccountById(string id)
        {
            var query = _infraAccountRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetInfraAccountDetailsById
        // Title: GetInfraAccountDetailsById
        // Description: The method selects relevant fields from the Infra Account entity
        public async Task<InfraAccount> GetInfraAccountDetailsById(string id)
        {
            var query = _infraAccountRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id)
                .Select(x => new InfraAccount
                {
                    Id = x.Id,
                    Name = x.Name,
                    URL = x.URL,
                    CCLast4Digits = x.CCLast4Digits,
                    WalletNumber = x.WalletNumber,
                    CustomerId = x.CustomerId,
                    Instructions = x.Instructions,
                    CreatedOnUtc = x.CreatedOnUtc,
                    UpdatedOnUtc = x.UpdatedOnUtc,
                    WalletType = new DropDown
                    {
                        Id = x.WalletType.Id,
                        DropDownValue = x.WalletType.DropDownValue
                    },
                    Provider = new DropDown
                    {
                        Id = x.Provider.Id,
                        DropDownValue = x.Provider.DropDownValue
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
                    InfraAccountServices = x.InfraAccountServices.Where(m => !m.Deleted).OrderByDescending(x => x.CreatedOnUtc).Select(m => new InfraAccountServices
                    {
                        Id = m.Id,
                        Name = m.Name,
                        URL = m.URL,
                        StartDate = m.StartDate,
                        EndDate = m.EndDate,
                        PriceInDollar = m.PriceInDollar,
                        WalletNumber = m.WalletNumber,
                        Instructions = m.Instructions,
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
                        InfraFTPList = m.InfraFTPList.Where(m => !m.Deleted).Select(x => new InfraFTP
                        {
                            Id = x.Id,
                            Name = x.Name,
                            InfraServiceId = x.InfraServiceId,
                            ProtocolTypeId = x.ProtocolTypeId,
                            EncryptionTypeId = x.EncryptionTypeId,
                            WalletTypeId = x.WalletTypeId,
                            WalletNumber = x.WalletNumber,
                            Host = x.Host,
                            Port = x.Port,
                            Instructions = x.Instructions,
                            CreatedOnUtc = x.CreatedOnUtc,
                            UpdatedOnUtc = x.UpdatedOnUtc,
                            WalletType = new DropDown
                            {
                                Id = x.WalletType.Id,
                                DropDownValue = x.WalletType.DropDownValue
                            },
                            ProtocolType = new DropDown
                            {
                                Id = x.ProtocolType.Id,
                                DropDownValue = x.ProtocolType.DropDownValue
                            },
                            EncryptionType = new DropDown
                            {
                                Id = x.EncryptionType.Id,
                                DropDownValue = x.EncryptionType.DropDownValue
                            },
                            InfraService = new InfraAccountServices
                            {
                                Id = x.InfraService.Id,
                                Name = x.InfraService.Name
                            }
                        }).ToList(),
                        InfraDatabaseList = m.InfraDatabaseList.Where(m => !m.Deleted).Select(x => new InfraDatabase
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
                            }
                        }).ToList()
                    }).ToList()
                });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        //#region GetInfraAccountByCustomerId
        //// Title: GetInfraAccountByCustomerId
        //// Description: This method retrieves a Infra Account based on its customerId. It allows an optional exclusion of a Infra Account by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific Infra Account. The method returns the first matching Infra Account or null if no match is found.
        //public async Task<InfraAccount> GetInfraAccountByCustomerId(string SiteId, string customerId, string id = null)
        //{
        //    var query = _infraAccountRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.CustomerId.ToLower() == customerId.ToLower());

        //    if (!string.IsNullOrEmpty(id))
        //        query = query.Where(x => x.Id != id);

        //    var item = await query.FirstOrDefaultAsync();
        //    return item;
        //}
        //#endregion

        #region InsertInfraAccount
        public void InsertInfraAccount(Models.InfraAccount entity)
        {
            _infraAccountRepository.Insert(entity);
        }
        #endregion

        #region UpdateInfraAccount
        public void UpdateInfraAccount(Models.InfraAccount entity)
        {
            _infraAccountRepository.Update(entity);
        }
        #endregion

        #region DeleteInfraAccount
        public void DeleteInfraAccount(Models.InfraAccount entity)
        {
            entity.Deleted = true;
            _infraAccountRepository.Update(entity);
        }
        #endregion

    }
}
