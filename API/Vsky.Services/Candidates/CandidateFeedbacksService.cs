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

namespace Vsky.Services.Candidates
{
    public class CandidateFeedbacksService : ICandidateFeedbacksService
    {
        #region Fields
        private readonly IRepository<CandidateFeedback> _candidateFeedbackRepository;

        public CandidateFeedbacksService(IRepository<CandidateFeedback> candidateFeedbackRepository)
        {
            _candidateFeedbackRepository = candidateFeedbackRepository;
        }
        #endregion

        #region Private Methods
        private static string GetOrderBy(string orderBy)
        {
            return orderBy;
        }
        #endregion

        #region GetAll
        public IPagedList<CandidateFeedback> GetAllCandidateFeedbackLogs(string sortBy,
           bool descending, int page = 1, int pageSize = int.MaxValue, bool lookup = false)
        {
            var query = _candidateFeedbackRepository.TableNoTracking.Where(x => !x.Deleted);

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
                query = query.Select(x => new CandidateFeedback
                {
                    Id = x.Id
                });
            }
            else
            {
                query = query.OrderByDescending(x => x.UpdatedOnUtc).Select(x => new CandidateFeedback
                {
                    Id = x.Id,
                    DueDate = x.DueDate,
                    Answer = x.Answer,
                    CreatedOnUtc = x.CreatedOnUtc,
                    Candidates = new Candidate
                    {
                        Id = x.Candidates.Id,
                        Person = new Person
                        {
                            Id = x.Candidates.Person.Id,
                            FullName = x.Candidates.Person.FirstName + "" + x.Employee.Person.LastName
                        }
                    },
                    Employee = new Employee
                    {
                        Id = x.Employee.Id,
                        Person = new Person
                        {
                            Id = x.Employee.Person.Id,
                            FullName = x.Employee.Person.FirstName + "" + x.Employee.Person.LastName
                        }
                    },
                    CandidateQuestions = new DropDown
                    {
                        Id = x.CandidateQuestions.Id,
                        DropDownValue = x.CandidateQuestions.DropDownValue
                    }
                });
            }
            var list = new PagedList<CandidateFeedback>(query, page, pageSize);

            return list;
        }
        #endregion

        #region GetById
        public async Task<CandidateFeedback> GetCandidateFeedbackById(string id)
        {
            var query = _candidateFeedbackRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }

        public List<CandidateFeedback> GetCandidateFeedbackDetailsById(string SiteId, string id)
        {
            var query = _candidateFeedbackRepository.TableNoTracking.Where(x => !x.Deleted && x.Candidates.SiteId == SiteId && x.CandidateId == id);

            query = query.OrderByDescending(x => x.UpdatedOnUtc).Select(x => new CandidateFeedback
            {
                Id = x.Id,
                DueDate = x.DueDate,
                EmployeeOwnerId = x.EmployeeOwnerId,
                QuestionId = x.QuestionId,
                Answer = x.Answer,
                CreatedOnUtc = x.CreatedOnUtc,
                Candidates = new Candidate
                {
                    Id = x.Candidates.Id,
                    Person = new Person
                    {
                        Id = x.Candidates.Person.Id,
                        FullName = x.Candidates.Person.FirstName + " " + x.Candidates.Person.LastName,
                    }
                },
                Employee = new Employee
                {
                    Id = x.Employee.Id,
                    Person = new Person
                    {
                        Id = x.Employee.Person.Id,
                        FullName = x.Employee.Person.FirstName + " " + x.Employee.Person.LastName
                    }
                },
                User = new ApplicationUser
                {
                    Id = x.User.Id,
                    UserName = x.User.UserName
                },
                CandidateQuestions = new DropDown
                {
                    Id = x.CandidateQuestions.Id,
                    DropDownValue = x.CandidateQuestions.DropDownValue
                }
            });

            var item = query.ToList();
            return item;
        }
        #endregion

        #region InsertCandidateFeedbacks
        public void InsertCandidateFeedbacks(CandidateFeedback entity)
        {
            _candidateFeedbackRepository.Insert(entity);
        }
        #endregion

        #region UpdateCandidateFeedbacks
        public void UpdateCandidateFeedbacks(CandidateFeedback entity)
        {
            _candidateFeedbackRepository.Update(entity);
        }
        #endregion

        #region DeleteCandidateFeedbacks
        public void DeleteCandidateFeedbacks(CandidateFeedback entity)
        {
            entity.Deleted = true;

            _candidateFeedbackRepository.Update(entity);
        }
        #endregion
    }
}
