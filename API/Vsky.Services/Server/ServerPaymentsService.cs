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
using Vsky.Services.Sites;
namespace Vsky.Services.Servers
{
    public class ServerPaymentsService : IServerPaymentsService
    {
        #region Services Intialization

        private readonly IRepository<ServerPayments> _serverPaymentsRepository;

        public ServerPaymentsService(IRepository<ServerPayments> serverPaymentsRepository)
        {
            _serverPaymentsRepository = serverPaymentsRepository;
        }
        #endregion

        #region GetOrderBy

        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }

        #endregion

        #region GetAllServerPayments
        public IPagedList<ServerPayments> GetAllServerPayments(string SiteId, string name, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _serverPaymentsRepository.TableNoTracking.Where(x => !x.Deleted && x.Server.SiteId == SiteId);
            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }

            query = query.Select(x => new ServerPayments
            {
                Id = x.Id,
                ServerId = x.ServerId,
                RenewDate = x.RenewDate,
                Notes = x.Notes,
                Server = new Server
                {
                    Id = x.Id,
                    CustomerId = x.Server.CustomerId,
                    ContractId = x.Server.ContractId,
                    ProviderId = x.Server.ProviderId,
                    Username = x.Server.Username,
                    Password = x.Server.Password,
                    Instructions = x.Server.Instructions,
                    Notes = x.Server.Notes,
                    FtpHostname = x.Server.FtpHostname,
                    FtpPassword = x.Server.FtpPassword,
                    FtpUsername = x.Server.FtpUsername,
                    FtpPort = x.Server.FtpPort,
                    StartDate = x.Server.StartDate,
                    SiteId = x.Server.SiteId,
                    CardDigit = x.Server.CardDigit,
                    Provider = new DropDown
                    {
                        Id = x.Server.Provider.Id,
                        DropDownValue = x.Server.Provider.DropDownValue,
                    }
                }                
            });
            var list = new PagedList<ServerPayments>(query, page, pageSize);
            return list;

        }
        #endregion

        #region GetById
        public async Task<ServerPayments> GetById(string id)
        {
            var query = _serverPaymentsRepository.TableNoTracking.Where(x => x.Id == id && !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertServerPayments
        public void InsertServerPayments(ServerPayments entity)
        {
            _serverPaymentsRepository.Insert(entity);
        }
        #endregion

        #region UpdateServerPayments
        public void UpdateServerPayments(ServerPayments entity)
        {
            _serverPaymentsRepository.Update(entity);
        }
        #endregion

        #region DeleteServerPayments
        public void DeleteServerPayments(ServerPayments entity)
        {
            entity.Deleted = true;

            _serverPaymentsRepository.Update(entity);
        }
        #endregion
    }
}
