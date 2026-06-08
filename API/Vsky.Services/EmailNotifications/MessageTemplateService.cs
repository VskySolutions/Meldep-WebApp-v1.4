using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Linq.Dynamic.Core;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Microsoft.EntityFrameworkCore;

namespace Vsky.Services.EmailNotifications
{
    public class MessageTemplateService : IMessageTemplateService
    {
        #region Define Services
        private readonly IRepository<MessageTemplate> _messageTemplateRepository;
        #endregion

        #region Services Initializations
        public MessageTemplateService(
            IRepository<MessageTemplate> messageTemplateRepository
            )
        {
            _messageTemplateRepository = messageTemplateRepository;
        }

        #endregion

        #region GetAllMessageTemplates
        public IPagedList<MessageTemplate> GetAllMessageTemplates(
            string siteId,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _messageTemplateRepository.TableNoTracking.Where(x => x.Active);

            query = query.Select(x => new MessageTemplate
            {
                Id = x.Id,
                Name = x.Name,
                Subject = x.Subject,
                Body = x.Body,
                EmailAccountId = x.EmailAccountId,
                EmailAccount = new EmailAccount
                {
                    Id = x.EmailAccount.Id
                }
            });

            var list = new PagedList<MessageTemplate>(query, page, pageSize);
            return list;
        }
        public async Task<List<MessageTemplate>> GetAllMasterMessageTemplates()
        {
            return await _messageTemplateRepository.TableNoTracking
                .Where(x => x.Active)
                .Select(x => new MessageTemplate
                {
                    Id = x.Id,
                    Name = x.Name,
                    Subject = x.Subject,
                    Body = x.Body,
                    EmailAccountId = x.EmailAccountId,
                    EmailAccount = new EmailAccount
                    {
                        Id = x.EmailAccount.Id
                    }
                })
                .ToListAsync();
        }
        #endregion
    }
}
