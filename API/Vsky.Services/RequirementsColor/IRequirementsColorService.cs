using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.RequirementsColor
{
    public interface IRequirementsColorService
    {
        #region GetRequirementColorByUser
        Task<RequirementColor> GetRequirementColorByUser(string requirementId, string LoggedUserId);
        #endregion

        #region InsertRequirementColor
        void InsertRequirementColor(RequirementColor entity);
        #endregion

        #region UpdateRequirementColor
        void UpdateRequirementColor(RequirementColor entity);
        #endregion
    }
}
