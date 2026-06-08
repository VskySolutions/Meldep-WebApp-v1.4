using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.Projects
{
    public class ProjectFilesService : IProjectFilesService
    {
        #region Define Services
        private readonly IRepository<ProjectFiles> _projectFilesRepository;
        #endregion

        #region Services Initializations

        public ProjectFilesService(IRepository<ProjectFiles> projectFilesRepository)
        {
            _projectFilesRepository = projectFilesRepository;
        }

        #endregion

        #region Private Methods
        // Title: GetOrderBy
        // Description: This method returns the input string as it is, which can be used as the `ORDER BY` clause in a SQL query.
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetProjectFileById
        // Title: GetProjectFileById
        // Description: This method retrieves a ProjectFiles from the database by its unique identifier (`id`). 
        public async Task<ProjectFiles> GetProjectFileById(string id)
        {
            var query = _projectFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectFileByFileId
        // Title: GetProjectFileByFileId
        // Description: This method retrieves a ProjectFiles from the database by fileId. 
        public async Task<ProjectFiles> GetProjectFileByFileId(string fileId)
        {
            var query = _projectFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.FileId == fileId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllProjectFileByProjectId
        public async Task<List<ProjectFiles>> GetAllProjectFileByProjectId(string siteId, string projectId)
        {
            var query = _projectFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.ProjectId == projectId && x.Project.SiteId == siteId);
            query = query.Select(x => new ProjectFiles
            {
                Id = x.Id,
                FileId = x.FileId,
                ProjectId = x.ProjectId,
                File = new Picture
                {
                    Id = x.File.Id,
                    VirtualPath = x.File.VirtualPath,
                    MimeType = x.File.MimeType,
                    SeoFilename = x.File.SeoFilename
                }
            });

            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        #region InsertProjectFile
        // Title: InsertProjectFile
        // Description: This method inserts a new ProjectFiles entity into the repository. It takes a ProjectFiles object as input and uses the _projectFilesRepository to handle the insertion operation.
        public void InsertProjectFile(ProjectFiles entity)
        {
            _projectFilesRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectFile
        // Title: UpdateProjectFile
        // Description: This method updates the specified ProjectFiles entity in the repository. It takes a ProjectFiles object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateProjectFile(ProjectFiles entity)
        {
            _projectFilesRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectFiles
        // Title: DeleteProjectFiles
        // Description: Marks the specified ProjectFiles entity as deleted by setting its `Deleted` property to true. 
        public void DeleteProjectFiles(ProjectFiles entity)
        {
            entity.Deleted = true;
            _projectFilesRepository.Update(entity);
        }
        #endregion
    }
}

