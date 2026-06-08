using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.Employees;
using Vsky.Services.Projects;
using Vsky.Services.Sites;

namespace Vsky.Services.Common
{
    public class CommonService : ICommonService
    {
        #region Define repositories
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeService _employeeService;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        private readonly IProjectFilesService _projectFilesService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;

        #endregion

        #region Repositories initialization
        public CommonService(IRepository<Country> countryRepository,
            IRepository<StateProvince> stateProvinceRepository,
            IRepository<Picture> pictureRepository,
            IRepository<Address> addressRepository,
             ApplicationDbContext db,
            UserManager<ApplicationUser> userManager, 
            IEmployeeService employeeService, 
            IDropDownService dropDownService, 
            IDropDownTypeService dropDownTypeService,
            IProjectFilesService projectFilesService,
            IAzureBlobImageServices azureBlobImageServices)
        {
            _countryRepository = countryRepository;
            _stateProvinceRepository = stateProvinceRepository;
            _pictureRepository = pictureRepository;
            _addressRepository = addressRepository;
            _db = db;
            _userManager = userManager;
            _employeeService = employeeService;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
            _projectFilesService = projectFilesService;
            _azureBlobImageServices = azureBlobImageServices;
        }
        #endregion

        #region Get all countries
        public async Task<IList<Country>> GetAllCountries()
        {
            var query = _countryRepository.TableNoTracking;
            query = query.Where(x => x.Active).OrderBy(x => x.Name);
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region Get all states by countryId
        public async Task<IList<StateProvince>> GetAllStateProvinces(string countryId)
        {
            var query = _stateProvinceRepository.TableNoTracking;
            query = query.Where(x => x.CountryId == countryId);
            query = query.Where(x => x.Active).OrderBy(x => x.Name);
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region Find employee id By LoggedUserId
        public string GetEmployeeIdByUserId(string SiteId, string LoggedUserId)
        {
            string employeeId = null;
            if (employeeId == null)
            {
                var LoggedUser = _db.Users.Where(m => m.Id == LoggedUserId).FirstOrDefault();
                if (LoggedUser != null)
                {
                    //var employee = _employeeService.GetEmployeeByEmail(SiteId, LoggedUser.Email).GetAwaiter().GetResult();
                    //var employee = _employeeService.GetEmployeeByPersonId(LoggedUser.PersonId).GetAwaiter().GetResult();
                    var employee = _employeeService.GetEmployeeByPersonIdBySiteId(LoggedUser.PersonId, SiteId).GetAwaiter().GetResult();
                    employeeId = employee?.Id;
                }
            }
            return employeeId;
        }
        #endregion

        #region Find employee id By LoggedUserId and Email
        public string GetEmployeeIdByUserIdAndEmail(string SiteId, string LoggedUserId)
        {
            string employeeId = null;
            if (employeeId == null)
            {
                var LoggedUser = _db.Users.Where(m => m.Id == LoggedUserId).FirstOrDefault();
                if (LoggedUser != null)
                {
                    var employee = _employeeService.GetEmployeeByEmailAndBySiteId(LoggedUser.Email, SiteId).GetAwaiter().GetResult();
                    employeeId = employee?.Id;
                }
            }
            return employeeId;
        }
        #endregion

        #region Find LoggedUserId by employee id
        public string GetLoggeduserIdByEmployeeId(string SiteId, string employeeId)
        {
            string LoggedUserId = null;
            if (LoggedUserId == null)
            {
                var Employee = _db.Employee.Where(m => m.Id == employeeId && m.SiteId == SiteId).FirstOrDefault();
                if (Employee != null)
                {
                    var user = _db.Users.Where(m => m.PersonId == Employee.PersonId && m.Person.PersonSitesMapping.Any(p => p.SiteId == SiteId)).FirstOrDefault();
                    LoggedUserId = user?.Id;
                }
            }
            return LoggedUserId;
        }
        #endregion

        //#region Find LoggedUserId by person id
        //public string GetLoggeduserIdByPersonId(string SiteId, string personId)
        //{
        //    string LoggedUserId = null;
        //    if (LoggedUserId == null)
        //    {
        //        var Person = _db.Person.Where(m => m.Id == personId && m.PersonSitesMapping.Any(p => p.SiteId == SiteId)).FirstOrDefault();
        //        if (Person != null)
        //        {
        //            var user = _db.Users.Where(m => m.Email == Person.PrimaryEmailAddress && m.Person.PersonSitesMapping.Any(p => p.SiteId == SiteId)).FirstOrDefault();
        //            LoggedUserId = user?.Id;
        //        }
        //    }
        //    return LoggedUserId;
        //}
        //#endregion

        #region Find DrownValueId By TypeandValue
        public string GetDrownValueIdByTypeandValue(string SiteId, string dropdowntype, string dropdownvalue)
        {
            string dropdownId = null;
            if (dropdowntype != null)
            {
                var type = _dropDownTypeService.GetDropDownTypeByType(SiteId, dropdowntype).GetAwaiter().GetResult();
                if (type != null)
                {
                    var dropdownvalueid = _dropDownService.GetDropDownByTypeAndValue(SiteId, type.Id, dropdownvalue).GetAwaiter().GetResult();
                    dropdownId = dropdownvalueid?.Id;
                }
            }
            return dropdownId;
        }
        #endregion

        #region Find state by id
        public async Task<StateProvince> GetByStateId(string id)
        {
            var query = _stateProvinceRepository.Table;
            query = query.Where(x => x.Active);
            query = query.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region Find country by id
        public async Task<Country> GetByCountryId(string id)
        {
            var query = _countryRepository.TableNoTracking;
            query = query.Where(x => x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region Get Validation Details By Country Id
        public async Task<CustomCountry> GetValidationDetailsByCountryId(string countryId)
        {
            return await _countryRepository.TableNoTracking
                .Where(x => x.Id == countryId && x.Active)
                .Select(x => new CustomCountry
                {
                    CountryCode = x.CountryCode,
                    PhoneNumberPattern = !string.IsNullOrWhiteSpace(x.PhoneNumberPattern) ? x.PhoneNumberPattern : "^\\d{10}$",
                    PhoneNumberMaxLength = x.PhoneNumberMaxLength > 0 ? x.PhoneNumberMaxLength: 10,
                    PhoneNumberPlaceHolder = x.PhoneNumberPlaceHolder,
                    ZipCodePattern = x.ZipCodePattern,
                    ZipCodeLabel = x.ZipCodeLabel,
                    ZipCodeMaxLength = x.ZipCodeMaxLength,
                    ZipCodePlaceHolder = x.ZipCodePlaceHolder,
                })
                .FirstOrDefaultAsync();
        }
        #endregion

        #region Find picture by id
        public async Task<Picture> GetByPictureId(string id)
        {
            var query = _pictureRepository.Table;
            query = query.Where(x => x.Id == id).AsNoTracking();
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region Find binary picture by picture id
        public string GetSetGlobleSiteId(string siteId, string LoggedUserId)
        {
            if(siteId == null)
            {
                var LoggedUser = _db.Users.Where(m => m.Id == LoggedUserId).FirstOrDefault();
                if(LoggedUser != null)
                {
                    var person = _db.Person.Where(m => m.Id == LoggedUser.PersonId).FirstOrDefault();
                    //Find People Assigned Site
                    var assignedSite = _db.PersonSitesMapping.Where(x => x.PersonId == person.Id).OrderBy(x => x.CreatedOnUtc).FirstOrDefault();
                    siteId = assignedSite == null ? "" : assignedSite.SiteId;
                }
            }
            return siteId;
        }
        #endregion

        public IPagedList<Picture> GetAllFilesByProjectId(string SiteId, string projectId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _pictureRepository.TableNoTracking.Where(x => x.SiteId == SiteId && x.ModuleId == projectId && !x.Deleted /*&& x.Type.Contains("Project")*/);

            query = query.OrderByDescending(x => x.CreatedOnUtc).Select(x => new Picture
            {
                Id = x.Id,
                SubModuleId = x.SubModuleId,
                Type = x.Type,
                Module = x.Module,
                ModuleId = x.ModuleId,
                Sub_Module = x.Sub_Module,
                VirtualPath = x.VirtualPath,
                SeoFilename = x.SeoFilename,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                //CreatedDateStr = x.CreatedOnUtc.HasValue? x.CreatedOnUtc.Value.ToString("MM/dd/yyyy hh:mm:ss tt"): string.Empty,
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    Person = new Person
                    {
                        Id = x.CreatedBy.Person.Id,
                        FirstName = x.CreatedBy.Person.FirstName,
                        LastName = x.CreatedBy.Person.LastName,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                    }
                },
                //CreatedDateStr = x.CreatedOnUtc.ToString("MM/dd/yyyy hh:mm:ss tt"),
            });

            var list = new PagedList<Picture>(query, page, pageSize);
            return list;
        }

        #region GetPicturesBySubModuleId
        public async Task<int> GetPicturesCountBySubModuleId(string subModuleId, string type)
        {
            return _pictureRepository.TableNoTracking.Where(x => !x.Deleted && x.SubModuleId == subModuleId && x.Type == type).Count();
        }
        #endregion

        #region Insert picture
        public void InsertPicture(Picture entity)
        {
            _pictureRepository.Insert(entity);
        }
        #endregion

        #region Update picture 
        public void UpdatePicture(Picture entity)
        {
            _pictureRepository.Update(entity);
        }
        #endregion

        #region DeletePicture
        // Title: DeletePicture
        // Description: Marks the specified picture entity as deleted by setting its `Deleted` property to true. 
        public void DeletePicture(Picture entity)
        {
            entity.Deleted = true;
            _pictureRepository.Update(entity);
        }
        #endregion       

        #region Find Address By Id
        public async Task<Address> GetAddressById(string id)
        {
            var query = _addressRepository.TableNoTracking.Where(x => x.Id == id).AsNoTracking();
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region Add/Update Address
        public string AddUpdateAddress(string AddressId = null, string City = "", string StateProvinceId = null, string CountryId = null, string AddressLine1 = "", string AddressLine2 = "", string ZipCode = "", string LoggedUserId = "")
        {
            string Id = null;
            if (!string.IsNullOrEmpty(AddressId))
            {
                var AddressData = _addressRepository.TableNoTracking.Where(x => x.Id == AddressId).FirstOrDefault();
                AddressData.StateProvinceId = StateProvinceId;
                AddressData.AddressLine1 = AddressLine1;
                AddressData.AddressLine2 = AddressLine2;
                AddressData.City = City;
                AddressData.CountryId = CountryId;
                AddressData.ZipCode = ZipCode;
                AddressData.UpdatedOnUtc = DateTime.UtcNow;
                AddressData.UpdatedById = LoggedUserId;
                _addressRepository.Update(AddressData);
                Id = AddressData.Id;
            }
            else
            {
                Address address = new Address();
                address.Id = Guid.NewGuid().ToString();
                address.City = City;
                address.CountryId = CountryId;
                address.StateProvinceId = StateProvinceId;
                address.AddressLine1 = AddressLine1;
                address.AddressLine2 = AddressLine2;
                address.ZipCode = ZipCode;
                address.CreatedOnUtc = DateTime.UtcNow;
                address.CreatedById = LoggedUserId;
                _addressRepository.Insert(address);
                Id = address.Id;
            }
            return Id;
        }
        #endregion

        public IQueryable<T> ApplySorting<T>(IQueryable<T> query, Dictionary<string, string> sorts)
        {
            if (sorts == null || !sorts.Any())
                return query;

            IOrderedQueryable<T> orderedQuery = null;
            int index = 0;

            foreach (var sort in sorts)
            {
                string[] parts = sort.Key.Split('.'); // e.g. ["address", "city"]
                bool isDescending = sort.Value.Equals("desc", StringComparison.OrdinalIgnoreCase);

                var parameter = Expression.Parameter(typeof(T), "x");
                Expression property = parameter;

                foreach (var part in parts)
                {
                    property = Expression.PropertyOrField(property, part.First().ToString().ToUpper() + part.Substring(1));
                }

                var lambda = Expression.Lambda(property, parameter);

                string methodName = (index == 0 ? "OrderBy" : "ThenBy") + (isDescending ? "Descending" : "");
                var resultExp = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new Type[] { typeof(T), property.Type },
                    (index == 0 ? query.Expression : orderedQuery.Expression),
                    Expression.Quote(lambda)
                );

                orderedQuery = (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(resultExp);
                index++;
            }

            return orderedQuery ?? query;
        }

        #region Delete Existing files from DB and Azure
        public async Task DeleteProjectRemovedFilesAsync(string siteId, string projectId, List<string> existingIds)
        {
            var existingFiles = await _projectFilesService.GetAllProjectFileByProjectId(siteId, projectId);

            var dbIds = existingFiles.Select(x => x.Id).ToList();

            var removedIds = dbIds
                                .Except(existingIds ?? new List<string>())
                                .ToList();

            if (!removedIds.Any())
                return;

            var deleteUrls = new List<string>();

            foreach (var id in removedIds)
            {
                var mapping = existingFiles.FirstOrDefault(x => x.Id == id);
                if (mapping == null) continue;

                var picture = await GetByPictureId(mapping.FileId);

                if (picture != null)
                {
                    deleteUrls.Add(picture.VirtualPath);

                    DeletePicture(picture);
                }

                _projectFilesService.DeleteProjectFiles(mapping);
            }

            await _azureBlobImageServices.DeleteMultipleImages(deleteUrls);
        }
        #endregion
    }
}