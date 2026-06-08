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

namespace Vsky.Services.ProjectTasks
{
    public class ProjectTaskFilesService : IProjectTaskFilesService
    {
        #region Define Services
        private readonly IRepository<ProjectTaskFiles> _projectTaskFilesRepository;
        #endregion

        #region Services Initializations

        public ProjectTaskFilesService(IRepository<ProjectTaskFiles> projectTaskFilesRepository)
        {
            _projectTaskFilesRepository = projectTaskFilesRepository;
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

        #region GetProjectTaskFileById
        // Title: GetProjectTaskFileById
        // Description: This method retrieves a ProjectTaskFiles from the database by its unique identifier (`id`). 
        public async Task<ProjectTaskFiles> GetProjectTaskFileById(string id)
        {
            var query = _projectTaskFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectTaskFileByFileId
        // Title: GetProjectTaskFileByFileId
        // Description: This method retrieves a ProjectTaskFiles from the database by fileId. 
        public async Task<ProjectTaskFiles> GetProjectTaskFileByFileId(string fileId)
        {
            var query = _projectTaskFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.FileId == fileId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllProjectTaskFilesByProjectTaskId
        public async Task<List<ProjectTaskFiles>> GetAllProjectTaskFilesByProjectTaskId(string projectTaskId)
        {
            var query = _projectTaskFilesRepository.TableNoTracking.Where(x => x.ProjectTaskId == projectTaskId);
            query = query.Select(x => new ProjectTaskFiles
            {
                Id = x.Id,
                FileId = x.FileId,
                ProjectTaskId = x.ProjectTaskId,
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

        #region InsertProjectTaskFile
        // Title: InsertProjectTaskFile
        // Description: This method inserts a new ProjectTaskFiles entity into the repository. It takes a ProjectTaskFiles object as input and uses the _projectTaskFilesRepository to handle the insertion operation.
        public void InsertProjectTaskFile(ProjectTaskFiles entity)
        {
            _projectTaskFilesRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectTaskFile
        // Title: UpdateProjectTaskFile
        // Description: This method updates the specified ProjectTaskFiles entity in the repository. It takes a ProjectTaskFiles object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateProjectTaskFile(ProjectTaskFiles entity)
        {
            _projectTaskFilesRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectTaskFile
        // Title: DeleteProjectTaskFile
        // Description: Marks the specified ProjectTaskFiles entity as deleted by setting its `Deleted` property to true. 
        public void DeleteProjectTaskFile(ProjectTaskFiles entity)
        {
            entity.Deleted = true;
            _projectTaskFilesRepository.Update(entity);
        }
        #endregion
    }
}



