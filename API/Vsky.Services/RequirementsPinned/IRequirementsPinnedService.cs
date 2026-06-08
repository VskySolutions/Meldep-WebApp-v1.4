using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.RequirementsPinned
{
    public interface IRequirementsPinnedService
    {
        #region GetRequirementPinnedByUser
        Task<RequirementPinned> GetRequirementPinnedByUser(string requirementId, string LoggedUserId);
        #endregion

        #region InsertRequirementPin
        void InsertRequirementPin(RequirementPinned entity);
        #endregion

        #region UpdateRequirementPin
        void UpdateRequirementPin(RequirementPinned entity);
        #endregion
    }
}
