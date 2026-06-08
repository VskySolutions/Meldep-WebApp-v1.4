using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.InfraFTPs
{
    public class InfraFTPService : IInfraFTPService
    {
        #region Define Services
        private readonly IRepository<InfraFTP> _infraFTPRepository;
        #endregion

        #region Services Initializations
        public InfraFTPService(IRepository<InfraFTP> infraFTPRepository)
        {
            _infraFTPRepository = infraFTPRepository;
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

        #region GetAllInfraFTPForList
        public IPagedList<InfraFTP> GetAllInfraFTPForList(
            string siteId, 
            List<string> infraServiceIds,
            List<string> protocolTypeIds,
            List<string> encryptionTypeIds,
            string Name,
            string searchText,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false)
        {
            var query = _infraFTPRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);

            if (infraServiceIds?.Any() == true) query = query.Where(x => infraServiceIds.Contains(x.InfraServiceId));
            if (protocolTypeIds?.Any() == true) query = query.Where(x => protocolTypeIds.Contains(x.ProtocolTypeId));
            if (encryptionTypeIds?.Any() == true) query = query.Where(x => encryptionTypeIds.Contains(x.EncryptionTypeId));
            if (!string.IsNullOrWhiteSpace(Name)) query = query.Where(x => x.Name.ToLower() == Name.ToLower());

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();
                query = query.Where(m =>
                    m.InfraService.Name.ToLower().Contains(searchText) ||
                    m.ProtocolType.DropDownValue.ToLower().Contains(searchText) ||
                    m.EncryptionType.DropDownValue.ToLower().Contains(searchText) ||
                    m.Host.ToLower().Contains(searchText) ||
                    m.Port.ToString().Contains(searchText) ||
                    m.Instructions.ToLower().Contains(searchText) ||
                    m.WalletNumber.ToLower().Contains(searchText) ||
                    m.WalletType.DropDownValue.ToLower().Contains(searchText) ||
                    m.Name.ToLower().Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
                query = query.OrderByDescending(x => x.CreatedOnUtc);

            query = query.Select(x => new InfraFTP
            {
                Id = x.Id,
                Name = x.Name,
                WalletNumber = x.WalletNumber,
                Host = x.Host,
                Port = x.Port,
                Instructions = x.Instructions,
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
                },
                InfraFTPsProjectInstanceMapping = x.InfraFTPsProjectInstanceMapping.Where(m => !m.Deleted).Select(x => new InfraFTPsProjectInstanceMapping
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

            var list = new PagedList<InfraFTP>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetInfraFTPById
        // Title: GetInfraFTPById
        // Description: This method retrieves a InfraFTP from the database by its unique identifier (`id`). 
        public async Task<InfraFTP> GetInfraFTPById(string id)
        {
            var query = _infraFTPRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetInfraFTPInDetailById
        // Title: GetInfraFTPInDetailById
        // Description: The method selects relevant fields from the Infra FTP entity
        public async Task<InfraFTP> GetInfraFTPInDetailById(string id)
        {
            var query = _infraFTPRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id)
                .Select(x => new InfraFTP
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
                    InfraFTPsProjectInstanceMapping = x.InfraFTPsProjectInstanceMapping.Where(m => !m.Deleted).Select(x => new InfraFTPsProjectInstanceMapping
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

        #region InsertInfraFTP
        public void InsertInfraFTP(Models.InfraFTP entity)
        {
            _infraFTPRepository.Insert(entity);
        }
        #endregion

        #region UpdateInfraFTP
        public void UpdateInfraFTP(Models.InfraFTP entity)
        {
            _infraFTPRepository.Update(entity);
        }
        #endregion

        #region DeleteInfraFTP
        public void DeleteInfraFTP(Models.InfraFTP entity)
        {
            entity.Deleted = true;
            _infraFTPRepository.Update(entity);
        }
        #endregion

    }
}
