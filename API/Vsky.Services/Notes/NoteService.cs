using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Vsky.Services.Note
{
    public class NoteService : INoteService
    {
        #region Service Initialization
        private readonly IRepository<Notes> _noteRepository;
        public NoteService(IRepository<Notes> noteRepository)
        {
            _noteRepository = noteRepository;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetById
        public async Task<Notes> GetById(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var query = await _noteRepository.TableNoTracking.Where(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
                return query;
            }
            return null;
        }
        #endregion

        #region GetById
        public List<Notes> GetAllNoteByTypeAndRecord(string SiteId, string subModuleId, string Type, bool latestOnTop)
        {
            var query = _noteRepository.TableNoTracking.Where(x => x.SiteId == SiteId && x.SubModuleId == subModuleId && x.Type == Type && !x.Deleted);

            query = latestOnTop
                    ? query.OrderByDescending(x => x.CreatedOnUtc) // latest first
                    : query.OrderBy(x => x.CreatedOnUtc); // oldest first

            return query.Select(x => new Notes
            {
            Id = x.Id,
            ModuleId = x.ModuleId,
            Module = x.Module,
            SubModuleId = x.SubModuleId,
            Sub_Module = x.Sub_Module,
            Type = x.Type,
            Note = x.Note,
            CreatedById = x.CreatedById,
            CreatedOnUtc = x.CreatedOnUtc,
            //CreatedDateStr = x.CreatedOnUtc.ToString("MM/dd/yyyy hh:mm:ss tt"),
            UpdatedById = x.UpdatedById,
            UpdatedOnUtc = x.UpdatedOnUtc,
            User = new ApplicationUser
            {
                Id = x.User.Id,
                UserName = x.User.UserName,
                Person = new Person
                {
                    Id = x.User.PersonId,
                    FirstName = x.User.Person.FirstName,
                    LastName = x.User.Person.LastName,
                    FullName = x.User.Person.FirstName + " " + x.User.Person.LastName
                }
            },
            }).ToList();
            //return query;
        }
        public List<Notes> GetAllNoteByTypeAndRecordIdForCandidate(string SiteId, string subModuleId, string Type)
        {
            var query = _noteRepository.TableNoTracking.Where(x => x.SubModuleId == subModuleId && x.Type == Type && x.SiteId == SiteId && !x.Deleted).Select(x => new Notes
            {
                Id = x.Id,
                SubModuleId = x.SubModuleId,
                Note = x.Note,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc
            }).ToList();
            return query;
        }

        public IPagedList<Notes> GetAllNotesByProjectId(string SiteId, string projectId, string sortBy, bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            //var query = _noteRepository.TableNoTracking.Where(x => x.SiteId == SiteId && x.RecordId == projectId && !x.Deleted);
            var query = _noteRepository.TableNoTracking.Where(x => x.SiteId == SiteId && x.ModuleId == projectId && !x.Deleted);

            query = query.OrderByDescending(x => x.UpdatedOnUtc).Select(x => new Notes
            {
                Id = x.Id,
                SubModuleId = x.SubModuleId,
                Note = x.Note,
                Type = x.Type,
                Module = x.Module,
                ModuleId = x.ModuleId,
                Sub_Module = x.Sub_Module,
                CreatedById = x.CreatedById,
                CreatedOnUtc = x.CreatedOnUtc,
                //CreatedDateStr = x.CreatedOnUtc.ToString("MM/dd/yyyy hh:mm:ss tt"),
                UpdatedById = x.UpdatedById,
                UpdatedOnUtc = x.UpdatedOnUtc,
                User = new ApplicationUser
                {
                    Id = x.User.Id,
                    UserName = x.User.UserName,
                    Person = new Person
                    {
                        Id = x.User.PersonId,
                        FirstName = x.User.Person.FirstName,
                        LastName = x.User.Person.LastName,
                        FullName = x.User.Person.FirstName + " " + x.User.Person.LastName
                    }
                },
            });

            var list = new PagedList<Notes>(query, page, pageSize);
            return list;
        }
        #endregion

        #region InsertNote
        public void InsertNote(Notes entity)
        {
            _noteRepository.Insert(entity);
        }
        #endregion

        #region UpdateNote
        public void UpdateNote(Notes entity)
        {
            _noteRepository.Update(entity);
        }
        #endregion

        #region DeleteNotes
        public void DeleteNotes(Notes entity)
        {
            entity.Deleted = true;

            _noteRepository.Update(entity);
        }

        #endregion
    }
}
