using System.Collections.Generic;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.DropDowns
{
    public interface IDropDownService
    {
        #region GetAllDropDowns
        IPagedList<DropDown> GetAllDropDowns(string SiteId, string SearchText, List<string> dropdownTypeIds, string sortBy,
            bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);
        #endregion

        #region GetDropDowns
        Task<IList<DropDown>> GetDropDowns(string typeId);
        #endregion

        #region GetDropDowns
        Task<IList<DropDown>> GetAllDropDowns();
        #endregion

        #region GetByName
        Task<DropDown> GetByName(string SiteId, string name);
        Task<DropDown> GetDropdownValueByName(string SiteId, string name);
        #endregion

        #region GetDropDownById
        Task<DropDown> GetDropDownById(string id);
        Task<string> GetDropDownByTypeNameAndName(string SiteId, string TypeName, string Name);
        #endregion

        #region GetDropDownByTypeAndValue
        Task<DropDown> GetDropDownByTypeAndValue(string SiteId, string dropDownTypeId, string value, string id = null);
        #endregion

        #region GetDropdownByButtonAsync
        Task<IList<DropDown>> GetDropdownByButton(string buttonType, string dropDownTypeId);
        #endregion

        #region InsertDropDown
        void InsertDropDown(DropDown entity);
        #endregion

        #region UpdateDropDown
        void UpdateDropDown(DropDown entity);
        #endregion

        #region DeleteDropDown
        void DeleteDropDown(DropDown entity);
        #endregion
    }
}