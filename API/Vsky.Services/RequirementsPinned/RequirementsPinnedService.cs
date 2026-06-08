using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.RequirementsPinned
{
    public class RequirementsPinnedService : IRequirementsPinnedService
    {
        #region Define services
        private readonly IRepository<RequirementPinned> _requirementPinnedRepository;

        public RequirementsPinnedService(IRepository<RequirementPinned> requirementPinnedRepository)
        {
            _requirementPinnedRepository = requirementPinnedRepository;
        }
        #endregion

        #region
        // Title: GetRequirementPinnedByUser
        // Description: This method retrieves a RequirementPinned record for a specific requirement and user.
        // It queries the RequirementPinned table and returns the first matching record if the user has pinned the requirement, or null if no record exists.
        public async Task<RequirementPinned> GetRequirementPinnedByUser(string requirementId, string LoggedUserId)
        {
            var query = _requirementPinnedRepository.TableNoTracking.Where(x => x.RequirementId == requirementId && x.AspNetUserId == LoggedUserId);


            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertRequirementPin
        public void InsertRequirementPin(RequirementPinned entity)
        {
            _requirementPinnedRepository.Insert(entity);
        }
        #endregion

        #region UpdateRequirementPin
        public void UpdateRequirementPin(RequirementPinned entity)
        {
            _requirementPinnedRepository.Update(entity);
        }
        #endregion
    }
}
