using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.FilePathDetail
{
    public class FilePathDetailsService : IFilePathDetailsService
    {
        #region Define Services
        private readonly IRepository<FilePathDetails> _filePathDetailsRepository;
        #endregion

        #region Services Initializations

        public FilePathDetailsService(IRepository<FilePathDetails> filePathDetailsRepository)
        {
            _filePathDetailsRepository = filePathDetailsRepository;
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

        #region GetFilePathById
        // Title: GetFilePathById
        // Description: This method retrieves a FilePathDetails from the database by its unique identifier (`id`). 
        public async Task<FilePathDetails> GetFilePathById(string id)
        {
            var query = _filePathDetailsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetFilePathDetailsById
        // Title: GetFilePathDetailsById
        // Description: The method selects relevant fields from the FilePathDetails entity.
        public async Task<FilePathDetails> GetFilePathDetailsById(string id)
        {
            var query = _filePathDetailsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            query = query.Select(x => new FilePathDetails
            {
                Id = x.Id,
                ModuleName = x.ModuleName,
                FilePath = x.FilePath,
                FileName = x.FileName,
                Requirement = new Requirement
                {
                    Id = x.Requirement.Id,
                    Title = x.Requirement.Title
                },

            });
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertFilePathDetails
        // Title: InsertFilePathDetails
        // Description: This method inserts a new FilePathDetails entity into the repository. It takes a FilePathDetails object as input and uses the _filePathDetailsRepository to handle the insertion operation.
        public void InsertFilePathDetails(IList<FilePathDetails> entities)
        {
            _filePathDetailsRepository.Insert(entities);
        }
        #endregion

        #region UpdateFilePathDetails
        // Title: UpdateFilePathDetails
        // Description: This method updates the specified FilePathDetails entity in the repository. It takes a FilePathDetails object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateFilePathDetails(IList<FilePathDetails> entities)
        {
            _filePathDetailsRepository.Update(entities);
        }
        #endregion

        #region DeleteFilePathDetails
        // Title: DeleteFilePathDetails
        // Description: Marks the specified FilePathDetails entity as deleted by setting its `Deleted` property to true. 
        public void DeleteFilePathDetails(List<FilePathDetails> entities)
        {
            var list = new List<FilePathDetails>();
            foreach (var item in entities)
            {
                item.Deleted = true;
                list.Add(item);
            }
            _filePathDetailsRepository.Update(list);
        }
        #endregion
    }
}
