using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;


namespace Vsky.Services.ProjectTasks
{
    public class ProjectTaskTagService : IProjectTaskTagService
    {
        #region Define services
        private readonly IRepository<ProjectTask_Tags> _projectTaskTagRepository;

        public ProjectTaskTagService(IRepository<ProjectTask_Tags> projectTaskTagRepository)
        {
            _projectTaskTagRepository = projectTaskTagRepository;
        }
        #endregion

        #region GetProjectTagByUserId
        public async Task<List<CommonDropDown>> GetProjectTaskTagByUserId(string LoggedUserId)
        {
            var query = await _projectTaskTagRepository.TableNoTracking
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

        #region GetByNameTaskIdAndUserId
        public async Task<ProjectTask_Tags> GetByNameTaskIdAndUserId(string SiteId, string Name, string TaskId, string LoggedUserId)
        {
            var query = _projectTaskTagRepository.TableNoTracking.Where(x => !x.Deleted && x.Tags.Name == Name && x.Tags.SiteId == SiteId && x.TaskId == TaskId && x.AspNetUserId == LoggedUserId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectTaskTagsByTaskIdAndUserId
        // Title: GetProjectTaskTagsByTaskIdAndUserId
        public List<ProjectTask_Tags> GetProjectTaskTagsByTaskIdAndUserId(string siteId, string taskId, string LoggedUserId)
        {

            var query = _projectTaskTagRepository.TableNoTracking.Where(x => !x.Deleted && x.Tags.SiteId == siteId && x.TaskId == taskId && x.AspNetUserId == LoggedUserId);
            query = query.Select(x => new ProjectTask_Tags
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

        #region InsertProjectTaskTag
        public void InsertProjectTaskTag(ProjectTask_Tags entity)
        {
            _projectTaskTagRepository.Insert(entity);
        }
        #endregion

        #region DeleteTaskTag
        // Title : DeleteTaskTag
        public void DeleteTaskTag(ProjectTask_Tags entity)
        {
            entity.Deleted = true;
            _projectTaskTagRepository.Update(entity);
        }
        #endregion
    }
}