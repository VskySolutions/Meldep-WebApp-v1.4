using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Vsky.Services.Sites;
namespace Vsky.Services.Servers
{
    public class ServerService : IServerService
    {
        #region Services Intialization

        private readonly IRepository<Server> _serverRepository;
        private readonly IRepository<Notes> _notesRepository;

        public ServerService(IRepository<Server> serverRepository, IRepository<Notes> notesRepository)
        {
            _serverRepository = serverRepository;
            _notesRepository = notesRepository;
        }
        #endregion

        #region GetOrderBy

        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region GetAllServers

        public IPagedList<Server> GetAllServers(string SiteId, string SearchText, string ProviderId, string CustomerId, string ContractId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _serverRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(ProviderId))
            {
                query = query.Where(m => m.ProviderId == ProviderId);
            }

            if (!string.IsNullOrWhiteSpace(CustomerId))
            {
                query = query.Where(m => m.CustomerId == CustomerId);
            }

            if (!string.IsNullOrWhiteSpace(ContractId))
            {
                query = query.Where(m => m.ContractId == ContractId);
            }
            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                DateTime.TryParse(SearchText, out var parsedDate);
                query = query.Where(m =>
                    m.Provider.DropDownValue.ToLower().Contains(SearchText.ToLower()) ||
                    m.CustomerId.ToLower().Contains(SearchText.ToLower()) ||
                    m.ContractId.ToLower().Contains(SearchText.ToLower()) ||
                    m.StartDate.Value.Date == parsedDate.Date
                );
            }
             query = query.Select(x => new Server
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                ContractId = x.ContractId,
                ProviderId = x.ProviderId,
                Username = x.Username,
                Password = x.Password,
                Instructions = x.Instructions,
                Notes = x.Notes,
                FtpHostname = x.FtpHostname,
                FtpPassword = x.FtpPassword,
                FtpUsername = x.FtpUsername,
                FtpPort = x.FtpPort,
                StartDate = x.StartDate,
                 SiteId = x.SiteId,
                 CardDigit = x.CardDigit,
                Provider = new DropDown
                {
                    Id = x.ProviderId,
                    DropDownValue = x.Provider.DropDownValue,
                },
                ServerPayments = x.ServerPayments.Where(m => !m.Deleted).Select(m => new ServerPayments
                {
                    Id = m.Id,
                    CreatedById = m.CreatedById,
                    CreatedOnUtc = m.CreatedOnUtc,
                    UpdatedById = m.UpdatedById,
                    UpdatedOnUtc = m.UpdatedOnUtc,
                    RenewDate = m.RenewDate,
                    Amount = m.Amount,
                    Notes = m.Notes,
                }).ToList(),
                ServerNotesCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "server").Count(),
            });
            var list = new PagedList<Server>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetFTPList

        public IPagedList<Server> GetFTPList(string SiteId, string SearchText, string ContractId, string CustomerId, string FTPUsername, string FTPHostname, string Port, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _serverRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && !string.IsNullOrWhiteSpace(x.FtpUsername));

            if (!string.IsNullOrWhiteSpace(CustomerId))
            {
                query = query.Where(m => m.CustomerId == CustomerId);
            }

            if (!string.IsNullOrWhiteSpace(ContractId))
            {
                query = query.Where(m => m.ContractId == ContractId);
            }

            if (!string.IsNullOrWhiteSpace(FTPUsername))
            {
                query = query.Where(m => m.FtpUsername == FTPUsername);
            }

            if (!string.IsNullOrWhiteSpace(FTPHostname))
            {
                query = query.Where(m => m.FtpHostname == FTPHostname);
            }

            if (!string.IsNullOrWhiteSpace(Port))
            {
                query = query.Where(m => m.FtpPort == Port);
            }
            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                    m.ContractId.ToLower().Contains(SearchText.ToLower()) ||
                    m.CustomerId.ToLower().Contains(SearchText.ToLower()) ||
                    m.FtpUsername.ToLower().Contains(SearchText.ToLower()) ||
                    m.FtpPassword.ToLower().Contains(SearchText.ToLower()) ||
                    m.FtpHostname.ToLower().Contains(SearchText.ToLower())
                );
            }
               query = query.Select(x => new Server
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                ContractId = x.ContractId,
                ProviderId = x.ProviderId,
                Username = x.Username,
                Password = x.Password,
                Instructions = x.Instructions,
                Notes = x.Notes,
                FtpHostname = x.FtpHostname,
                FtpPassword = x.FtpPassword,
                FtpUsername = x.FtpUsername,
                FtpPort = x.FtpPort,
                StartDate = x.StartDate,
                   SiteId = x.SiteId,
                   CardDigit = x.CardDigit,
                Provider = new DropDown
                {
                    Id = x.ProviderId,
                    DropDownValue = x.Provider.DropDownValue,
                },
                ServerPayments = x.ServerPayments.Where(m => !m.Deleted).Select(m => new ServerPayments
                {
                    Id = m.Id,
                    CreatedById = m.CreatedById,
                    CreatedOnUtc = m.CreatedOnUtc,
                    UpdatedById = m.UpdatedById,
                    UpdatedOnUtc = m.UpdatedOnUtc,
                    RenewDate = m.RenewDate,
                    Amount = m.Amount,
                    Notes = m.Notes,
                }).ToList(),
                FtpNotesCount = _notesRepository.TableNoTracking.Where(m => !m.Deleted && m.SubModuleId == x.Id && m.Type == "ftplist").Count(),
            });
            var list = new PagedList<Server>(query, page, pageSize);
            return list;
        }
        #endregion

        #region GetById
        public async Task<Server> GetById(string id)
        {
            var query = _serverRepository.TableNoTracking.Where(x => x.Id == id && !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<Server> GetByCustomerId(string customerId)
        {
            var query = _serverRepository.TableNoTracking.Where(x => x.CustomerId == customerId && !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetServerDetailsById
        public async Task<Server> GetServerDetailsById(string id)
        {
            var query = _serverRepository.TableNoTracking.Where(x => x.Id == id && !x.Deleted);
            query = query.Select(x => new Server
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                ContractId = x.ContractId,
                ProviderId = x.ProviderId,
                Username = x.Username,
                Password = x.Password,
                Instructions = x.Instructions,
                Notes = x.Notes,
                PIN = x.PIN,
                FtpHostname = x.FtpHostname,
                FtpPassword = x.FtpPassword,
                FtpUsername = x.FtpUsername,
                FtpPort = x.FtpPort,
                StartDate = x.StartDate,
                SiteId = x.SiteId,
                CardDigit = x.CardDigit,
                Provider = new DropDown
                {
                    Id = x.ProviderId,
                    DropDownValue = x.Provider.DropDownValue,
                },
                ServerPayments = x.ServerPayments.Where(m => !m.Deleted).Select(m => new ServerPayments
                {
                    Id = m.Id,
                    CreatedById = m.CreatedById,
                    CreatedOnUtc = m.CreatedOnUtc,
                    UpdatedById = m.UpdatedById,
                    UpdatedOnUtc = m.UpdatedOnUtc,
                    RenewDate = m.RenewDate,
                    Amount = m.Amount,
                    Notes = m.Notes,
                }).ToList()
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllServerListForDropdown
        public async Task<List<Server>> GetAllServerListForDropdown(string SiteId)
        {
            var query = _serverRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId);
            query = query.Select(x => new Server
            {
                Id = x.Id,
                Provider = new DropDown
                {
                    Id = x.Provider.Id,
                    DropDownValue = x.Provider.DropDownValue
                },
                CustomerId = x.CustomerId,
                ContractId = x.ContractId,
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetAllFTPListForDropdown
        public async Task<List<Server>> GetAllFTPListForDropdown(string SiteId, string type)
        {
            var query = _serverRepository.TableNoTracking.Where(m => !m.Deleted && m.SiteId == SiteId && !string.IsNullOrWhiteSpace(m.FtpUsername));
            if (type == "Contract")
            {
                query = query.Select(x => new Server
                {
                    Id = x.Id,
                    ContractId = x.ContractId,
                });
            }
            else if (type == "Customer")
            {
                query = query.Select(x => new Server
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                });
            }
            else if (type == "Username")
            {
                query = query.Select(x => new Server
                {
                    Id = x.Id,
                    FtpUsername = x.FtpUsername,
                });
            }
            else if (type == "Hostname")
            {
                query = query.Select(x => new Server
                {
                    Id = x.Id,
                    FtpHostname = x.FtpHostname,
                });
            }
            else if (type == "Port")
            {
                query = query.Select(x => new Server
                {
                    Id = x.Id,
                    FtpPort = x.FtpPort,
                });
            }

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region InsertServer
        public void InsertServer(Server entity)
        {
            _serverRepository.Insert(entity);
        }
        #endregion

        #region UpdateServer
        public void UpdateServer(Server entity)
        {
            _serverRepository.Update(entity);
        }
        #endregion

        #region DeleteServer
        public void DeleteServer(Server entity)
        {
            entity.Deleted = true;

            _serverRepository.Update(entity);
        }
        #endregion
    }
}
