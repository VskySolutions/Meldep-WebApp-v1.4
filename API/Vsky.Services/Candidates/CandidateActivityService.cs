using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.Candidates
{
    public class CandidateActivityService : ICandidateActivityService
    {
        #region Fields
        private readonly IRepository<CandidateActivities> _candidateActivityLogsRepository;
        #endregion

        #region Ctor
        public CandidateActivityService(IRepository<CandidateActivities> candidateActivityLogsRepository)
        {
            _candidateActivityLogsRepository = candidateActivityLogsRepository;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region Public Methods
        public IPagedList<CandidateActivities> GetAllCandidateActivityLogs(string sortBy,
           bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _candidateActivityLogsRepository.Table;
            // global filter
            query = query.Where(x => !x.Deleted);
            //sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";

                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderBy(x => x.CreatedById);
            }

            if (lookup)
            {
                query = query.Select(x => new CandidateActivities
                {
                    Id = x.Id,
                });
            }
            else
            {
                // projection
                query = query.Select(x => new CandidateActivities
                {
                    Id = x.Id,
                    ActivityName = x.ActivityName,
                    DueDate = x.DueDate,
                    CreatedOnUtc = x.CreatedOnUtc,
                    Priority = new DropDown
                    {
                        Id = x.Priority.Id,
                        DropDownValue = x.Priority.DropDownValue
                    },
                    Employee = new Employee
                    {
                        Id = x.Employee.Id,
                        Person = new Person
                        {
                            Id = x.Employee.Person.Id,
                            FirstName = x.Employee.Person.FirstName,
                            MiddleName = x.Employee.Person.MiddleName,
                            LastName = x.Employee.Person.LastName,
                            FullName = x.Employee.Person.FirstName + "" + x.Employee.Person.LastName
                        }
                    },
                });
            }
            var list = new PagedList<CandidateActivities>(query, page, pageSize);

            return list;
        }
        #endregion

        #region Get activities
        public async Task<IList<CandidateActivities>> GetAllActivities(string SiteId)
        {
            var query = _candidateActivityLogsRepository.TableNoTracking.Where(x => x.Candidates.SiteId == SiteId);
            var list = await query.ToListAsync();
            return list;
        }
        #endregion

        public async Task<CandidateActivities> GetById(string id)
        {
            var query = _candidateActivityLogsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        public List<CandidateActivities> GetCandidateDetailsById(string SiteId, string id)
        {
            var query = _candidateActivityLogsRepository.TableNoTracking.Where(x => !x.Deleted && x.Candidates.SiteId == SiteId && x.CandidateId == id);
            query = query.OrderByDescending(x => x.UpdatedOnUtc).Select(x => new CandidateActivities
            {
                Id = x.Id,
                ActivityName = x.ActivityName,
                DueDate = x.DueDate,
                PriorityId = x.PriorityId,
                EmployeeOwnerId = x.EmployeeOwnerId,
                Candidates = new Candidate
                {
                    Id = x.Candidates.Id,
                    Person = new Person
                    {
                        Id = x.Candidates.Person.Id,
                        FullName = x.Candidates.Person.FirstName + " " + x.Candidates.Person.LastName,
                    }
                },
                Priority = new DropDown
                {
                    Id = x.Priority.Id,
                    DropDownValue = x.Priority.DropDownValue
                },
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FirstName = x.Employee.Person.FirstName,
                        MiddleName = x.Employee.Person.MiddleName,
                        LastName = x.Employee.Person.LastName,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName
                    }
                },
                User = new ApplicationUser
                {
                    Id = x.User.Id,
                    UserName = x.User.UserName
                }
            });

            var item = query.ToList();
            return item;
        }

        public void InsertCandidateActivityLogs(CandidateActivities entity)
        {
            _candidateActivityLogsRepository.Insert(entity);
        }

        public void UpdateCandidateActivityLogs(CandidateActivities entity)
        {
            _candidateActivityLogsRepository.Update(entity);
        }

        public void DeleteCandidateActivityLogs(CandidateActivities entity)
        {
            entity.Deleted = true;

            _candidateActivityLogsRepository.Update(entity);
        }

    }
}
