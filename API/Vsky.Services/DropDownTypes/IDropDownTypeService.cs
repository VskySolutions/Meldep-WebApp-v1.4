using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.DropDownTypes
{
    public interface IDropDownTypeService
    {
        #region GetAllDropDownTypes
        IPagedList<DropDownType> GetAllDropDownTypes(string SiteId,
            string SearchText,
            string moduleName,
            string groupName,
            List<string> dropdownTypeIds,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetAllDropDownTypeListBySiteId
        Task<List<DropDownType>> GetAllDropDownTypeListBySiteId(string SiteId);
        #endregion

        #region GetFirstCreatedDropDownType
        Task<DropDownType> GetFirstCreatedDropDownType();
        #endregion

        #region GetAllDropDownTypeListForDropdown
        Task<List<DropDownType>> GetAllDropDownTypeListForDropdown(string SiteId);
        #endregion

        #region GetDropDownTypeById
        Task<DropDownType> GetDropDownTypeById(string id);
        #endregion

        #region GetDropDownTypeDetailsById
        Task<DropDownType> GetDropDownTypeDetailsById(string id);
        #endregion

        #region GetDropDownType
        Task<DropDownType> GetDropDownType(string SiteId, string type);
        #endregion

        #region GetDropDownTypeForSite
        Task<DropDownType> GetDropDownTypeBySite(string SiteId, string type);
        #endregion

        #region GetDropDownTypeByType
        Task<DropDownType> GetDropDownTypeByType(string SiteId, string type, string id = null);
        #endregion

        #region GetDropDownTypeByGroupName
        Task<DropDownType> GetDropDownTypeByGroupName(string SiteId, string groupName);
        #endregion

        #region GetDropDownTypeListByGroupName
        Task<List<DropDownType>> GetDropDownTypeListByGroupName(string SiteId, string groupName);
        #endregion

        #region GetDropdownTypeListByModuleName
        Task<List<DropDownType>> GetDropdownTypeListByModuleName(string SiteId, string moduleName);
        #endregion

        #region InsertDropDownType
        void InsertDropDownType(DropDownType entity);
        #endregion

        #region InsertDropDownTypeList
        void InsertDropDownTypeList(IList<DropDownType> entities);
        #endregion

        #region UpdateDropDownType
        void UpdateDropDownType(DropDownType entity);
        #endregion

        #region UpdateDropDownTypeList
        void UpdateDropDownTypeList(IList<DropDownType> entities);
        #endregion

        #region DeleteDropDownType
        void DeleteDropDownType(DropDownType entity);
        #endregion

        #region DeleteDropDownTypeList
        void DeleteDropDownTypeList(List<DropDownType> entities);
        #endregion
    }
}