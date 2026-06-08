using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ImageMigration
{
    public class ImageMigrationService : IImageMigrationService
    {
        private readonly IRepository<Models.ImageMigration> _imageMigrationRepository;
        public ImageMigrationService(IRepository<Models.ImageMigration> imageMigrationRepository)
        {
            _imageMigrationRepository = imageMigrationRepository;
        }

        public async Task<List<Models.ImageMigration>> GetAllImageMigrations(string TableName, string TableId, bool Deleted, int Take, int IsProcessed = 0)
        {
            var query = _imageMigrationRepository.TableNoTracking.Where(m => m.Deleted == Deleted && m.IsProcessed == IsProcessed).Take(Take);

            if (!string.IsNullOrWhiteSpace(TableName))
                query = query.Where(x => x.TableName == TableName);

            if (!string.IsNullOrWhiteSpace(TableId))
                query = query.Where(x => x.TableId == TableId);

            query = query.OrderBy(m => m.TableNumber);

            var list = query.ToList();
            return list;
        }

        public void InsertImageMigration(Models.ImageMigration entity)
        {
            _imageMigrationRepository.Insert(entity);
        }

        public void UpdateImageMigration(Models.ImageMigration entity)
        {
            _imageMigrationRepository.Update(entity);
        }

        public void DeleteImageMigration(Models.ImageMigration entity)
        {
            entity.Deleted = true;
            _imageMigrationRepository.Update(entity);
        }
    }
}
