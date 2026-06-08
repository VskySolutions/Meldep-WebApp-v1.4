using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.Common
{
    public interface ICommonService
    {
        #region Get all countries
        Task<IList<Country>> GetAllCountries();
        #endregion

        #region Get all states by countryId
        Task<IList<StateProvince>> GetAllStateProvinces(string countryId);
        #endregion

        #region Get EmployeeId By LoggedUserId
        string GetEmployeeIdByUserId(string SiteId, string LoggedUserId);
        #endregion

        #region Find employee id By LoggedUserId and Email
        public string GetEmployeeIdByUserIdAndEmail(string SiteId, string LoggedUserId);
        #endregion

        #region Get LoggedUserId by employee id
        string GetLoggeduserIdByEmployeeId(string SiteId, string employeeId);
        #endregion

        //#region Get LoggedUserId by person id
        //string GetLoggeduserIdByPersonId(string SiteId, string personId);
        //#endregion

        #region Get DrownValueId By TypeandValue
        string GetDrownValueIdByTypeandValue(string SiteId, string dropdowntype, string dropdownvalue);
        #endregion

        #region Get state by id
        Task<StateProvince> GetByStateId(string id);
        #endregion

        #region Get country by id
        Task<Country> GetByCountryId(string id);
        #endregion

        #region Get Validation Details By Country Id
        Task<CustomCountry> GetValidationDetailsByCountryId(string id);
        #endregion

        #region Get address by id
        Task<Address> GetAddressById(string id);
        #endregion

        #region GetSetGlobleSiteId
        string GetSetGlobleSiteId(string siteId, string LoggedUserId);
        #endregion

        #region Find picture by id
        Task<Picture> GetByPictureId(string pitureId);
        #endregion

        IPagedList<Picture> GetAllFilesByProjectId(string SiteId, string projectId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false);

        #region GetPicturesCountBySubModuleId
        Task<int> GetPicturesCountBySubModuleId(string subModuleId, string type);
        #endregion

        #region Insert picture
        void InsertPicture(Picture entity);
        #endregion

        #region Update picture 
        void UpdatePicture(Picture entity);
        #endregion

        #region DeletePicture
        void DeletePicture(Picture entity);
        #endregion

        #region Add/Update Address 
        string AddUpdateAddress(string AddressId = null, string City = "", string StateProvinceId = null, string CountryId = null, string AddressLine1 = "", string AddressLine2 = "", string ZipCode = "",string LoggedUserId = "");
        #endregion

        #region Multi Level sorting
        IQueryable<T> ApplySorting<T>(IQueryable<T> query, Dictionary<string, string> sorts);
        #endregion

        #region Delete Existing files from DB and Azure
        Task DeleteProjectRemovedFilesAsync(string siteId, string projectId, List<string> existingIds);
        #endregion
    }
}