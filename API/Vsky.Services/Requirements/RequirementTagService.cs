using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Requirements
{
    public class RequirementTagService : IRequirementTagService
    {
        #region Define services

        private readonly IRepository<RequirementTags> _requirementTagRepository;
        public RequirementTagService(IRepository<RequirementTags> requirementTagRepository)
        {
            _requirementTagRepository = requirementTagRepository;
        }
        #endregion

        #region GetRequirementTagByUserId
        public async Task<List<CommonDropDown>> GetRequirementTagByUserId(string LoggedUserId)
        {
            var query = await _requirementTagRepository.TableNoTracking
                        .Where(x => !x.Deleted && x.AspNetUserId == LoggedUserId)
                        .Select(x => new CommonDropDown
                        {
                            Text = x.Tags.Name,
                            Value = x.Tags.Id,
                            BgColor = x.Tags.BgColor,
                            Color = x.Tags.Color
                        })
                       .ToListAsync();
            var list = query
                  .GroupBy(x => x.Text)
                  .Select(g => g.First())
                  .OrderBy(x => x.Text)
                  .ToList();

            return list;
        }
        #endregion

        #region GetByNameRequirementIdAndUserId
        public async Task<RequirementTags> GetByNameRequirementIdAndUserId(string siteId, string name, string requirementId, string LoggedUserId)
        {
            var query = _requirementTagRepository.TableNoTracking.Where(x => !x.Deleted && x.Tags.SiteId == siteId && x.Tags.Name == name && x.RequirementId == requirementId && x.AspNetUserId == LoggedUserId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetRequirementTagsByRequirementIdAndUserId
        public List<RequirementTags> GetRequirementTagsByRequirementIdAndUserId(string siteId, string requirementId, string LoggedUserId)
        {

            var query = _requirementTagRepository.TableNoTracking.Where(x => !x.Deleted && x.Tags.SiteId == siteId && x.RequirementId == requirementId && x.AspNetUserId == LoggedUserId);
            query = query.Select(x => new RequirementTags
            {
                Id = x.Id,
                Tags = new Tags
                {
                    Id = x.Tags.Id,
                    Name = x.Tags.Name,
                    BgColor = x.Tags.BgColor,
                    Color = x.Tags.Color
                }
            });
            return query.ToList();
        }
        #endregion

        #region InsertRequirementTags
        public void InsertRequirementTags(RequirementTags entity)
        {
            _requirementTagRepository.Insert(entity);
        }
        #endregion

        #region DeleteRequirementTags
        public void DeleteRequirementTags(RequirementTags entity)
        {
            entity.Deleted = true;
            _requirementTagRepository.Update(entity);
        }
        #endregion
    }
}
