using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.RequirementDataMappings
{
    public class RequirementDataMappingNotesService : IRequirementDataMappingNotesService
    {
        #region Service Initialization
        private readonly IRepository<RequirementDataMappingNotes> _requirementNotesRepository;

        public RequirementDataMappingNotesService(
            IRepository<RequirementDataMappingNotes> requirementNotesRepository
        )
        {
            _requirementNotesRepository = requirementNotesRepository;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAllRequirementDataMappingNotesByRequirementMappingId
        public async Task<List<RequirementDataMappingNotes>> GetAllRequirementDataMappingNotesByRequirementMappingId(string SiteId, string requirementMappingId, bool latestOnTop)
        {
            var query = _requirementNotesRepository.TableNoTracking.Where(x => !x.Deleted && x.RequirementDataMapping.Requirement.SiteId == SiteId && x.RequirementDataMappingId == requirementMappingId);

            query = latestOnTop
                    ? query.OrderByDescending(x => x.CreatedOnUtc) // latest first
                    : query.OrderBy(x => x.CreatedOnUtc); // oldest first

            query = query.Select(x => new RequirementDataMappingNotes
            {
                Id = x.Id,
                RequirementDataMappingId = x.RequirementDataMappingId,
                Note = x.Note,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                CreatedBy = new ApplicationUser
                {
                    Id = x.CreatedBy.Id,
                    UserName = x.CreatedBy.UserName,
                    Person = new Person
                    {
                        Id = x.CreatedBy.PersonId,
                        FullName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName
                    }
                }
            });

            return await query.ToListAsync();
        }
        #endregion

        #region GetRequirementDataMappingNotesById
        public async Task<RequirementDataMappingNotes> GetRequirementDataMappingNotesById(string id)
        {
            return await _requirementNotesRepository.TableNoTracking.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);
        }
        #endregion

        #region InsertRequirementDataMappingNotes
        public void InsertRequirementDataMappingNotes(RequirementDataMappingNotes entity)
        {
            _requirementNotesRepository.Insert(entity);
        }
        #endregion

        #region UpdateRequirementDataMappingNotes
        public void UpdateRequirementDataMappingNotes(RequirementDataMappingNotes entity)
        {
            _requirementNotesRepository.Update(entity);
        }
        #endregion

        #region DeleteRequirementDataMappingNotes
        public void DeleteRequirementDataMappingNotes(RequirementDataMappingNotes entity)
        {
            entity.Deleted = true;

            _requirementNotesRepository.Update(entity);
        }

        #endregion
    }
}
