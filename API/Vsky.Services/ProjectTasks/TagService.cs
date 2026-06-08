using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectTasks
{
    public class TagService : ITagService
    {
        #region Define services
        private readonly IRepository<Tags> _tagsRepository;

        public TagService(IRepository<Tags> tagsRepository)
        {
            _tagsRepository = tagsRepository; 
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region tags list

        public IPagedList<Tags> GetAllTags(string SiteId, string SearchText, string name, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue)
        {
            var query = _tagsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);
           
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(name));
            }

            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.CreatedOnUtc);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(m =>
                   m.Name.ToLower().Contains(SearchText.ToLower()) ||
                   m.BgColor.ToLower().Contains(SearchText.ToLower()) ||
                   m.Color.ToLower().Contains(SearchText.ToLower())
                );
            }
            query = query.Where(x => !x.Deleted).Select(x => new Tags
            {
                Id = x.Id,
                Name = x.Name,
                Color = x.Color,
                BgColor = x.BgColor,
                SortOrder = x.SortOrder,
            });
            var list = new PagedList<Tags>(query, page, pageSize);
            return list;
        }

        public async Task<List<Tags>> GetAllTagList(string SiteId)
        {
            var query = _tagsRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == SiteId);

            query = query.OrderByDescending(x => x.CreatedOnUtc).Select(x => new Tags
            {
                Id = x.Id,
                Name = x.Name,
                Color = x.Color,
                BgColor= x.BgColor,
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region GetTagByName
        public async Task<Tags> GetTagByName(string SiteId, string Name)
        {
            var query = _tagsRepository.TableNoTracking.Where(x => x.Name.Trim().ToLower() == Name.Trim().ToLower() && x.SiteId == SiteId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertTags
        public void InsertTags(Tags entity)
        {
            _tagsRepository.Insert(entity); 
        }
        #endregion

        public async Task<Tags> GetById(string id)
        {
            var query = _tagsRepository.TableNoTracking.Where(x => x.Id == id);
            query = query.Where(x => !x.Deleted);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public async Task<Tags> GetTagDetailsById(string id)
        {
            var query = _tagsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new Tags
            {
                Id = x.Id,
                Name = x.Name,
                Color = x.Color,
                BgColor = x.BgColor
            });

            var list = await query.FirstOrDefaultAsync();
            return list;
        }

        #region UpdateTags
        public void UpdateTags(Tags entity)
        {
            _tagsRepository.Update(entity);
        }
        #endregion

        #region DeleteTags
        public void DeleteTags(Tags entity)
        {
            entity.Deleted = true;
            //entity.Active = false;
            _tagsRepository.Update(entity);
        }
        #endregion
    }
}
