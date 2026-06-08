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

namespace Vsky.Services.ProjectModules
{
    public class ProjectModuleFilesService : IProjectModuleFilesService
    {
        #region Define Services
        private readonly IRepository<ProjectModuleFiles> _projectModuleFilesRepository;
        #endregion

        #region Services Initializations

        public ProjectModuleFilesService(IRepository<ProjectModuleFiles> projectModuleFilesRepository)
        {
            _projectModuleFilesRepository = projectModuleFilesRepository;
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

        #region GetProjectModuleFileById
        // Title: GetProjectModuleFileById
        // Description: This method retrieves a ProjectModuleFiles from the database by its unique identifier (`id`). 
        public async Task<ProjectModuleFiles> GetProjectModuleFileById(string id)
        {
            var query = _projectModuleFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectModuleFileByFileId
        // Title: GetProjectModuleFileByFileId
        // Description: This method retrieves a ProjectFiles from the database by fileId. 
        public async Task<ProjectModuleFiles> GetProjectModuleFileByFileId(string fileId)
        {
            var query = _projectModuleFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.FileId == fileId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllProjectModuleFilesByProjectModuleId
        public async Task<List<ProjectModuleFiles>> GetAllProjectModuleFilesByProjectModuleId(string projectModuleId)
        {
            var query = _projectModuleFilesRepository.TableNoTracking.Where(x => x.ProjectModuleId == projectModuleId);
            query = query.Select(x => new ProjectModuleFiles
            {
                Id = x.Id,
                FileId = x.FileId,
                ProjectModuleId = x.ProjectModuleId,
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

        #region InsertProjectModuleFile
        // Title: InsertProjectModuleFile
        // Description: This method inserts a new ProjectModuleFiles entity into the repository. It takes a ProjectModuleFiles object as input and uses the _projectModuleFilesRepository to handle the insertion operation.
        public void InsertProjectModuleFile(ProjectModuleFiles entity)
        {
            _projectModuleFilesRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectModuleFile
        // Title: UpdateProjectModuleFile
        // Description: This method updates the specified ProjectModuleFiles entity in the repository. It takes a ProjectModuleFiles object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateProjectModuleFile(ProjectModuleFiles entity)
        {
            _projectModuleFilesRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectModuleFile
        // Title: DeleteProjectModuleFile
        // Description: Marks the specified ProjectModuleFiles entity as deleted by setting its `Deleted` property to true. 
        public void DeleteProjectModuleFile(ProjectModuleFiles entity)
        {
            entity.Deleted = true;
            _projectModuleFilesRepository.Update(entity);
        }
        #endregion
    }
}


