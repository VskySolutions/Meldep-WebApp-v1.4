using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.AdPosts
{
    public class AdPostingStatusService : IAdPostingStatusService
    {
        #region Define Services
        private readonly IRepository<AdPostingStatus> _adPostingStatusRepository;
        #endregion

        #region Services Initializations
        public AdPostingStatusService(IRepository<AdPostingStatus> adPostingStatusRepository)
        {
            _adPostingStatusRepository = adPostingStatusRepository;
        }
        #endregion

        #region GetAdPostingStatusById
        // Title: GetAdPostingStatusById
        // Description: This method retrieves a AdPostingStatus from the database by its unique identifier (`id`). 
        public async Task<AdPostingStatus> GetAdPostingStatusById(string id)
        {
            var query = _adPostingStatusRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAdPostingStatusesByAdId
        // Title: GetAdPostingStatusesByAdId
        // Description: The method selects relevant fields from the AdPostingStatus entity, including related entities such as project module and project task, and returns a `AdPostingStatus` object with these details. 
        public async Task<List<AdPostingStatus>> GetAdPostingStatusesByAdId(string adId)
        {
            var query = _adPostingStatusRepository.TableNoTracking.Where(x => !x.Deleted && x.AdId == adId).Select(x => new AdPostingStatus
            {
                Id = x.Id,
                AdPostChannelId = x.AdPostChannelId,
                Likes = x.Likes,
                Comments = x.Comments,
                Shares = x.Shares,
                Date = x.Date,
                AdPostChannel = new AdPostChannel
                {
                    Id = x.AdPostChannel.Id,
                    Name = x.AdPostChannel.Name
                }
            });
            var item = await query.ToListAsync();
            return item;
        }
        #endregion

        #region GetAdPostingStatusByAdId
        // Title: GetAdPostingStatusByAdId
        // Description: This method retrieves a AdPostingStatus based on its name and Id. It allows an optional exclusion of a AdPostingStatus by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific AdPostingStatus. The method returns the first matching AdPostingStatus or null if no match is found.
        public async Task<AdPostingStatus> GetAdPostingStatusByAdId(string channelId, string adId)
        {
            var query = _adPostingStatusRepository.TableNoTracking.Where(x => !x.Deleted && x.AdPostChannelId == channelId && x.AdId == adId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertAdPostingStatus
        public void InsertAdPostingStatus(AdPostingStatus entity)
        {
            _adPostingStatusRepository.Insert(entity);
        }
        #endregion

        #region UpdateAdPostingStatus
        // Title: UpdateAdPostingStatus
        // Description: This method updates the specified AdPostingStatus entity in the repository. It takes a AdPostingStatus object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateAdPostingStatus(AdPostingStatus entity)
        {
            _adPostingStatusRepository.Update(entity);
        }
        #endregion

        #region DeleteAdPostingStatus
        // Title: DeleteAdPostingStatus
        // Description: Marks the specified AdPostingStatus entity as deleted by setting its `Deleted` property to true. 
        public void DeleteAdPostingStatus(AdPostingStatus entity)
        {
            entity.Deleted = true;
            _adPostingStatusRepository.Update(entity);
        }
        #endregion
    }
}
