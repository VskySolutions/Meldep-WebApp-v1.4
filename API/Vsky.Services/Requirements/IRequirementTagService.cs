using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.Requirements
{
    public interface IRequirementTagService
    {
        #region GetRequirementTagByUserId
        Task<List<CommonDropDown>> GetRequirementTagByUserId(string LoggedUserId);
        #endregion

        #region GetByNameRequirementIdAndUserId
        Task<RequirementTags> GetByNameRequirementIdAndUserId(string siteId, string name, string requirementId, string LoggedUserId);
        #endregion

        #region GetRequirementTagsByRequirementIdAndUserId
        List<RequirementTags> GetRequirementTagsByRequirementIdAndUserId(string siteId, string requirementId, string LoggedUserId);
        #endregion

        #region InsertRequirementTags
        void InsertRequirementTags(RequirementTags entity);
        #endregion

        #region DeleteRequirementTags
        void DeleteRequirementTags(RequirementTags entity);
        #endregion
    }
}
