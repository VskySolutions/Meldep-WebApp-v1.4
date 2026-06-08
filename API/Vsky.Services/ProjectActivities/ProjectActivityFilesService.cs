using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using System.Threading.Tasks;

namespace Vsky.Services.ProjectActivities
{
    public class ProjectActivityFilesService : IProjectActivityFilesService
    {
        #region Define Services
        private readonly IRepository<ProjectActivityFiles> _projectActivityFilesRepository;
        #endregion

        #region Services Initializations

        public ProjectActivityFilesService(IRepository<ProjectActivityFiles> projectActivityFilesRepository)
        {
            _projectActivityFilesRepository = projectActivityFilesRepository;
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

        #region GetProjectActivityFileById
        // Title: GetProjectActivityFileById
        // Description: This method retrieves a ProjectActivityFiles from the database by its unique identifier (`id`). 
        public async Task<ProjectActivityFiles> GetProjectActivityFileById(string id)
        {
            var query = _projectActivityFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetProjectActivityFileByFileId
        // Title: GetProjectActivityFileByFileId
        // Description: This method retrieves a ProjectActivityFiles from the database by fileId. 
        public async Task<ProjectActivityFiles> GetProjectActivityFileByFileId(string fileId)
        {
            var query = _projectActivityFilesRepository.TableNoTracking.Where(x => !x.Deleted && x.FileId == fileId);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetAllProjectActivityFilesByProjectActivityId
        public async Task<List<ProjectActivityFiles>> GetAllProjectActivityFilesByProjectActivityId(string projectActivityId)
        {
            var query = _projectActivityFilesRepository.TableNoTracking.Where(x => x.ProjectActivityId == projectActivityId);
            query = query.Select(x => new ProjectActivityFiles
            {
                Id = x.Id,
                FileId = x.FileId,
                ProjectActivityId = x.ProjectActivityId,
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

        #region InsertProjectActivityFile
        // Title: InsertProjectActivityFile
        // Description: This method inserts a new ProjectActivityFiles entity into the repository. It takes a ProjectActivityFiles object as input and uses the _projectActivityFilesRepository to handle the insertion operation.
        public void InsertProjectActivityFile(ProjectActivityFiles entity)
        {
            _projectActivityFilesRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectActivityFile
        // Title: UpdateProjectActivityFile
        // Description: This method updates the specified ProjectActivityFiles entity in the repository. It takes a ProjectActivityFiles object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateProjectActivityFile(ProjectActivityFiles entity)
        {
            _projectActivityFilesRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectActivityFile
        // Title: DeleteProjectActivityFile
        // Description: Marks the specified ProjectActivityFiles entity as deleted by setting its `Deleted` property to true. 
        public void DeleteProjectActivityFile(ProjectActivityFiles entity)
        {
            entity.Deleted = true;
            _projectActivityFilesRepository.Update(entity);
        }
        #endregion
    }
}



