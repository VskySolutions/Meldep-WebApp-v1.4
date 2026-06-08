using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Vsky.Services.Sites;

namespace Vsky.Services.TrainingPortals
{
    public class TrainingPortalService : ITrainingPortalService
    {
        #region Define Services
        private readonly IRepository<TrainingPortal> _TrainingPortalRepository;
        private readonly IRepository<Picture> _pictureRepository;
        #endregion

        #region Services Initializations
        public TrainingPortalService
        (
            IRepository<TrainingPortal> TrainingPortalRepository,
            IRepository<Picture> Picturerepository
        )
        {
            _TrainingPortalRepository = TrainingPortalRepository;
            _pictureRepository = Picturerepository;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllTrainingList
        public IPagedList<TrainingPortal> GetAllTrainingList(string SiteId, string SearchText, string name, List<string> EmployeeDesignationIds, string sortBy, bool descending, int pageIndex = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _TrainingPortalRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(name));
            }

            if (EmployeeDesignationIds != null && EmployeeDesignationIds.Any())
                query = query.Where(x => x.TrainingPortalMappings.Any(m => EmployeeDesignationIds.Contains(m.EmployeeDesignationId)));

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.Name);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                  //m.TrainingPortalNumber.ToString().Contains(SearchText.ToLower()) ||
                  m.Name.ToLower().Contains(SearchText.ToLower()) ||
                  m.Url.ToLower().Contains(SearchText.ToLower()) ||
                  m.Description.ToLower().Contains(SearchText.ToLower()) ||
                  m.TrainingPortalMappings.Any(tp =>
                    tp.EmployeeDesignationType.DropDownValue.ToLower().Contains(SearchText.ToLower()))
                );
            }
             query = query.Select(x => new TrainingPortal
            {
                Id = x.Id,
                TrainingFileId = x.TrainingFileId,
                Name = x.Name,
                Url = x.Url,
                Description = x.Description,
                TrainingPortalNumber = x.TrainingPortalNumber,
                 File = new Picture
                 {
                     Id = x.File.Id,
                     VirtualPath = x.File.VirtualPath,
                     SeoFilename = x.File.SeoFilename
                 },
                 TrainingPortalMappings = x.TrainingPortalMappings.Select(mapping => new Training_Portal_Mapping
                {
                    Id = mapping.Id,
                    TrainingId = mapping.TrainingId,
                    EmployeeDesignationId = mapping.EmployeeDesignationId,
                    EmployeeDesignationType = new DropDown
                    {
                        Id = mapping.EmployeeDesignationType.Id,
                        DropDownValue = mapping.EmployeeDesignationType.DropDownValue
                    }
                }).ToList()
            });

            var list = new PagedList<TrainingPortal>(query, pageIndex, pageSize);
            return list;
        }
        #endregion

        #region GetById
        public async Task<TrainingPortal> GetById(string id, string SiteId)
        {
            var query = _TrainingPortalRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertTraining
        public void InsertTraining(TrainingPortal entity)
        {
            _TrainingPortalRepository.Insert(entity);
        }
        #endregion

        #region UpdateTraining
        public void UpdateTraining(TrainingPortal entity)
        {
            _TrainingPortalRepository.Update(entity);
        }
        #endregion

        #region DeleteTraining
        public void DeleteTraining(TrainingPortal entity)
        {
            entity.Deleted = true;
            _TrainingPortalRepository.Update(entity);
        }
        #endregion

        #region InsertPicture
        public void InsertPicture(Picture entity)
        {
            _pictureRepository.Insert(entity);
        }
        #endregion

        #region UpdatePicture
        public void UpdatePicture(Picture entity)
        {
            _pictureRepository.Update(entity);
        }
        #endregion

        #region GetTrainingByName
        public async Task<TrainingPortal> GetTrainingByName(string SiteId, string name, string id = null)
        {
            var query = _TrainingPortalRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId && x.Name.ToLower() == name.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region Get training details by id
        public async Task<TrainingPortal> GetTrainingDetailsById(string id)
        {
            var query = _TrainingPortalRepository.TableNoTracking.Where(x => !x.Deleted && x.Id ==  id);

            query = query.Select(x => new TrainingPortal
            {
                Id = x.Id,
                Name = x.Name,
                Url = x.Url,
                Description = x.Description,
                TrainingFileId = x.TrainingFileId,
                File = new Picture
                {
                    Id = x.File.Id,
                    VirtualPath = x.File.VirtualPath,
                    SeoFilename = x.File.SeoFilename
                },
                TrainingPortalMappings = x.TrainingPortalMappings.Select(mapping => new Training_Portal_Mapping
                {
                    Id = mapping.Id,
                    TrainingId = mapping.TrainingId,
                    EmployeeDesignationId = mapping.EmployeeDesignationId,
                    EmployeeDesignationType = new DropDown
                    {
                        Id = mapping.EmployeeDesignationType.Id,
                        DropDownValue = mapping.EmployeeDesignationType.DropDownValue
                    }
                }).ToList()
            });

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

    }
}
