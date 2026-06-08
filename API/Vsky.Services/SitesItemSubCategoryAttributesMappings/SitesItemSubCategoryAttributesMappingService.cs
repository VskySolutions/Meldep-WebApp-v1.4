using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Services.SitesItemSubCategoryAttributesMappings
{
    public class SitesItemSubCategoryAttributesMappingService : ISitesItemSubCategoryAttributesMappingService
    {
        #region Define Services
        private readonly IRepository<SitesItemSubCategoryAttributesMapping> _sitesItemSubCategoryAttributesMappingRepository;
        #endregion

        #region Services Initializations
        public SitesItemSubCategoryAttributesMappingService(
             IRepository<SitesItemSubCategoryAttributesMapping> sitesItemSubCategoryAttributesMappingRepository)
        {
            _sitesItemSubCategoryAttributesMappingRepository = sitesItemSubCategoryAttributesMappingRepository;
        }
        #endregion

        #region GetAllSitesItemSubCategoryAttributesList
        public async Task<List<SitesItemSubCategoryAttributesMapping>> GetAllSitesItemSubCategoryAttributesListByItemSubCategoryId(string SiteId, string itemSubCategoryId)
        {
            var query = _sitesItemSubCategoryAttributesMappingRepository.TableNoTracking.Where(x => !x.Deleted && x.ItemSubCategoryId == itemSubCategoryId && x.SiteId == SiteId && !x.ItemSubcategory.Deleted &&!x.ItemSubCategoryAttributes.Deleted);

            query = query.OrderByDescending(x => x.CreatedOnUtc).Select(x => new SitesItemSubCategoryAttributesMapping
            {
                Id = x.Id,
                ItemSubCategoryAttributeId = x.ItemSubCategoryAttributeId,
                ItemSubCategoryAttributes = new ItemSubCategoryAttributes
                {
                    Name = x.ItemSubCategoryAttributes.Name,
                    FieldType = x.ItemSubCategoryAttributes.FieldType,
                    ItemSubCategoryAttributesValues = x.ItemSubCategoryAttributes.ItemSubCategoryAttributesValues
                    .Where(v => !v.Deleted &&
                                (v.ItemSubCategoryId == x.ItemSubCategoryId || v.ItemSubCategoryId == null))
                    .Select(v => new ItemSubCategoryAttributesValues
                    {
                        Id = v.Id,
                        Text = v.Text
                    }).ToList()
                },
            });
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        public async Task<List<SitesItemSubCategoryAttributesMapping>>GetAttributeMappingByItemSubCategoryId(string siteId, string itemSubCategoryId)
        {
            return await _sitesItemSubCategoryAttributesMappingRepository.TableNoTracking
                .Where(x => !x.Deleted && x.SiteId == siteId && x.ItemSubCategoryId == itemSubCategoryId)
                .OrderByDescending(x => x.CreatedOnUtc)
                .ToListAsync();
        }

        #region GetSitesItemSubCategoryAttributeById
        public async Task<SitesItemSubCategoryAttributesMapping> GetSitesItemSubCategoryAttributeById(string id)
        {
            var query = _sitesItemSubCategoryAttributesMappingRepository.TableNoTracking
                .Where(x => x.Id == id && !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertSitesItemSubCategoryAttributesMappingList
        // Title: InsertSitesItemSubCategoryAttributesMappingList
        public void InsertSitesItemSubCategoryAttributesMappingList(IList<SitesItemSubCategoryAttributesMapping> entities)
        {
            _sitesItemSubCategoryAttributesMappingRepository.Insert(entities);
        }
        #endregion

        #region DeleteSitesItemSubCategoryAttributesMappingList
        // Title: DeleteSitesItemSubCategoryAttributesMappingList
        public void DeleteSitesItemSubCategoryAttributesMappingList(IList<SitesItemSubCategoryAttributesMapping> sitesItemSubCategoryAttributeEntities)
        {
            var sitesItemSubCategoryAttributes = new List<SitesItemSubCategoryAttributesMapping>();
            foreach (var sitesItemSubCategoryAttribute in sitesItemSubCategoryAttributeEntities)
            {
                sitesItemSubCategoryAttribute.Deleted = true;

                sitesItemSubCategoryAttributes.Add(sitesItemSubCategoryAttribute);
            }
            _sitesItemSubCategoryAttributesMappingRepository.Update(sitesItemSubCategoryAttributeEntities);
        }
        #endregion
    }
}
