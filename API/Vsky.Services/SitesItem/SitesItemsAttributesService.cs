using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.SitesItem
{
    public class SitesItemsAttributesService : ISitesItemsAttributesService
    {
        #region Define Services
        private readonly IRepository<SitesItemsAttributes> _sitesItemsAttributesRepository;
        #endregion

        #region Services Initializations
        public SitesItemsAttributesService(IRepository<SitesItemsAttributes> sitesItemsAttributesRepository)
        {
            _sitesItemsAttributesRepository = sitesItemsAttributesRepository;
        }
        #endregion

        #region GetSitesItemsAttributeById
        // Title: GetSitesItemsAttributeById
        // Description: This method retrieves a sites items attribute from the database by its unique identifier (`id`). 
        public async Task<SitesItemsAttributes> GetSitesItemsAttributeById(string id)
        {
            var query = _sitesItemsAttributesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetSitesItemsAttributeByValueAndSubCategoryId
        public async Task<SitesItemsAttributes> GetSitesItemsAttributeByValueAndSubCategoryId(string itemSubCategoryId, string value, string id = null)
        {
            var query = _sitesItemsAttributesRepository.TableNoTracking.Where(x => !x.Deleted && !x.SitesItems.Deleted && x.SitesItems.ItemSubCategoryId == itemSubCategoryId && x.Value.ToLower() == value.ToLower());

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        #endregion

        #region InsertSitesItemsAttributeList
        // Title: InsertSitesItemsAttribute
        // Description: This method inserts a sites items attribute entity into the repository. It takes a sites items attribute object as input and uses the _sitesItemsAttributesRepository to handle the insertion operation.
        public void InsertSitesItemsAttributeList(IList<SitesItemsAttributes> entity)
        {
            _sitesItemsAttributesRepository.Insert(entity);
        }
        #endregion

        #region UpdateSitesItemsAttributeList
        // Title: UpdateSitesItemsAttributeList
        // Description: This method updates the specified sites items attribute entity in the repository. It takes a sites items attribute object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateSitesItemsAttributeList(IList<SitesItemsAttributes> entities)
        {
            _sitesItemsAttributesRepository.Update(entities);
        }
        #endregion

        #region DeleteSitesItemsAttributeList
        // Title: DeleteSitesItemsAttributeList
        // Description: Marks the specified sites items attribute entity as deleted by setting its `Deleted` property to true. 
        public void DeleteSitesItemsAttributeList(List<SitesItemsAttributes> entities)
        {
            var list = new List<SitesItemsAttributes>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _sitesItemsAttributesRepository.Update(list);
        }
        #endregion
    }
}
