using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.ImageMigration
{
    public interface IImageMigrationService
    {
        Task<List<Models.ImageMigration>> GetAllImageMigrations(string TableName, string TableId, bool Deletedm, int Take, int IsProcessed = 0);
        void InsertImageMigration(Models.ImageMigration entity);
        void UpdateImageMigration(Models.ImageMigration entity);
        void DeleteImageMigration(Models.ImageMigration entity);
    }
}
