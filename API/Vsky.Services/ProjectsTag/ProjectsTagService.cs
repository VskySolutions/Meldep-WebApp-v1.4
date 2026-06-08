using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectsTag
{
    public class ProjectsTagService : IProjectsTagService
    {
        #region Define services
        private readonly IRepository<ProjectTags> _projectTagRepository;

        public ProjectsTagService(IRepository<ProjectTags> projectTagRepository)
        {
            _projectTagRepository = projectTagRepository;
        }
        #endregion


        #region GetProjectTagByUserId
        public async Task<List<CommonDropDown>> GetProjectTagByUserId(string LoggedUserId)
        {
            var query = await _projectTagRepository.TableNoTracking
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

        #region GetByNameProjectIdAndUserId
        public async Task<ProjectTags> GetByNameProjectIdAndUserId(string SiteId, string Name, string projectId, string LoggedUserId)
        {
            var query = _projectTagRepository.TableNoTracking.Where(x => !x.Deleted && x.Tags.Name == Name && x.Tags.SiteId == SiteId && x.ProjectId == projectId && x.AspNetUserId == LoggedUserId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectTagsByProjectIdAndUserId
        // Title: GetProjectTagsByProjectIdAndUserId
        public List<ProjectTags> GetProjectTagsByProjectIdAndUserId(string siteId, string projectId, string LoggedUserId)
        {

            var query = _projectTagRepository.TableNoTracking.Where(x => !x.Deleted && x.Tags.SiteId == siteId && x.ProjectId == projectId && x.AspNetUserId == LoggedUserId);
            query = query.Select(x => new ProjectTags
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

        #region InsertProjectTag
        public void InsertProjectTag(ProjectTags entity)
        {
            _projectTagRepository.Insert(entity);
        }
        #endregion

        #region DeleteProjectTag
        public void DeleteProjectTag(ProjectTags entity)
        {
            entity.Deleted = true;
            _projectTagRepository.Update(entity);
        }
        #endregion
    }
}
