using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.RequirementsColor
{
    public class RequirementsColorService : IRequirementsColorService
    {
        #region Define services
        private readonly IRepository<RequirementColor> _requirementColorRepository;

        public RequirementsColorService(IRepository<RequirementColor> requirementColorRepository)
        {
            _requirementColorRepository = requirementColorRepository;
        }
        #endregion

        #region GetRequirementColorByUser
        // Title: GetRequirementColorByUser
        // Description: This method retrieves a RequirementtColor record for a specific requirement and user.
        // It queries the RequirementColor table and returns the first matching record if the user has color the requirement, or null if no record exists
        public async Task<RequirementColor> GetRequirementColorByUser(string requirementId, string LoggedUserId)
        {
            var query = _requirementColorRepository.TableNoTracking.Where(x => !x.Deleted && x.RequirementId == requirementId && x.AspNetUserId == LoggedUserId);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertRequirementColor
        public void InsertRequirementColor(RequirementColor entity)
        {
            _requirementColorRepository.Insert(entity);
        }
        #endregion

        #region UpdateRequirementColor
        public void UpdateRequirementColor(RequirementColor entity)
        {
            _requirementColorRepository.Update(entity);
        }
        #endregion
    }
}
