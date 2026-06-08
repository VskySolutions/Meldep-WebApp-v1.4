using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.EmailReply
{
    public class HelpDeskEmailRepliesMappingService : IHelpDeskEmailRepliesMappingService
    {
        #region Define Services
        private readonly IRepository<HelpDeskEmailRepliesMapping> _helpDeskEmailRepliesMappingRepository;
        #endregion

        #region Services Initializations
        public HelpDeskEmailRepliesMappingService(IRepository<HelpDeskEmailRepliesMapping> helpDeskEmailRepliesMappingRepository)
        {
            _helpDeskEmailRepliesMappingRepository = helpDeskEmailRepliesMappingRepository;
        }
        #endregion

        #region GetAllHelpDeskEmailRepliesMappingList
        public async Task<List<HelpDeskEmailRepliesMapping>> GetAllHelpDeskEmailRepliesMappingList(string siteId, string helpDeskId, int skipIndex = 0, int takeCount = 10, bool isSystemEmail = false)
        {
            var query = _helpDeskEmailRepliesMappingRepository.TableNoTracking.Where(x => !x.HelpDesk.Deleted && !x.EmailReplies.Deleted && x.HelpDesk.SiteId == siteId && x.HelpDeskId == helpDeskId && (
                isSystemEmail ||
                x.EmailReplies.IsSystemEmail == false
            ))
            .OrderByDescending(p => p.EmailReplies.CreatedOnUtc)
            .Skip(skipIndex * takeCount)
            .Take(takeCount)
            .Select(x => new HelpDeskEmailRepliesMapping
            {
                Id = x.Id,
                HelpDeskId = x.HelpDeskId,
                EmailRepliesId = x.EmailRepliesId,
                EmailReplies = new EmailReplies
                {
                    Id = x.EmailReplies.Id,
                    OwnerId = x.EmailReplies.OwnerId,
                    FromEmail = x.EmailReplies.FromEmail,
                    Subject = x.EmailReplies.Subject,
                    Body = x.EmailReplies.Body,
                    ToEmail = x.EmailReplies.ToEmail,
                    ExternalToEmail = x.EmailReplies.ExternalToEmail,
                    CCEmail = x.EmailReplies.CCEmail,
                    IsRead = x.EmailReplies.IsRead,
                    IsSystemEmail = x.EmailReplies.IsSystemEmail,
                    TwilioEmailId = x.EmailReplies.TwilioEmailId,
                    CreatedOnUtc = x.EmailReplies.CreatedOnUtc
                },
                HelpDesk = new HelpDesk
                {
                    CreatedBy = new ApplicationUser
                    {
                        Id = x.HelpDesk.CreatedBy.Id,
                        Person = new Person
                        {
                            Id = x.HelpDesk.CreatedBy.PersonId,
                            FullName = x.HelpDesk.CreatedBy.Person.FirstName + " " + x.HelpDesk.CreatedBy.Person.LastName
                        }
                    },
                    StatusText = x.HelpDesk.HelpDeskStatusLog.OrderByDescending(p => p.CreatedOnUtc).Select(p => p.Status.DropDownValue).FirstOrDefault(),
                }
            });

            var list = await query.ToListAsync();

            return list;
        }
        #endregion

        #region Insert HelpDeskEmailRepliesMapping
        public void InsertHelpDeskEmailRepliesMapping(HelpDeskEmailRepliesMapping entity)
        {
            _helpDeskEmailRepliesMappingRepository.Insert(entity);
        }
        #endregion

        #region Update UpdateHelpDeskEmailRepliesMapping
        public void UpdateHelpDeskEmailRepliesMapping(HelpDeskEmailRepliesMapping entity)
        {
            _helpDeskEmailRepliesMappingRepository.Update(entity);
        }
        #endregion
    }
}
