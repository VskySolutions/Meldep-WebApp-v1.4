using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Core;
using Vsky.Models;
using Vsky.Services.Sites;

namespace Vsky.Services.HelpDesks
{
    public class HelpDeskTopicService : IHelpDeskTopicService
    {
        #region Define Services
        public readonly IRepository<HelpDeskTopic> _helpDeskTopicRepository;
        public readonly IRepository<HelpDeskTopicQuestions> _helpDeskTopicQuestionsRepository;
        #endregion

        #region Services Initializations
        public HelpDeskTopicService(IRepository<HelpDeskTopic> helpDeskTopicRepository, IRepository<HelpDeskTopicQuestions> helpDeskTopicQuestionsRepository)
        {
            _helpDeskTopicRepository = helpDeskTopicRepository;
            _helpDeskTopicQuestionsRepository = helpDeskTopicQuestionsRepository;
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

        #region GetAllHelpDeskTopicList
        // Title: GetAllHelpDeskTopicList
        // Description: Retrieves the list of Help Desk Topics for the specified site.
        public async Task<List<HelpDeskTopic>> GetAllHelpDeskTopicList(string siteId)
        {
            var query = _helpDeskTopicRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId);

            query = query
                .OrderBy(x => x.Title)
                .Select(x => new HelpDeskTopic
            {
                Id = x.Id,
                Title = x.Title,
                IsActive = x.IsActive,
            });

            var list = await query.ToListAsync();

            return list;
        }
        #endregion

        #region GetAllHelpDeskTopicQuestionList
        // Title: GetAllHelpDeskTopicQuestionList
        // Description: Retrieves the list of Help Desk Topic Questions for the specified site and optional topic.
        public async Task<List<HelpDeskTopicQuestions>> GetAllHelpDeskTopicQuestionList(string siteId, string topicId = null)
        {
            var query = _helpDeskTopicQuestionsRepository.TableNoTracking.Where(x => !x.Deleted && x.HelpDeskTopic.SiteId == siteId);

            if (!string.IsNullOrWhiteSpace(topicId))
            {
                query = query.Where(x => x.TopicId == topicId && !x.HelpDeskTopic.Deleted);
            }
            query = query
                .OrderBy(x => x.Question)
                .Select(x => new HelpDeskTopicQuestions
            {
                Id = x.Id,
                Question = x.Question,
                Description = x.Description,
                IsActive = x.IsActive,
            });

            var list = await query.ToListAsync();

            return list;
        }
        #endregion

        #region GetAllHelpDeskTopicListForDropdown
        // Title: GetAllHelpDeskTopicListForDropdown
        // Description: Retrieves the list of active Help Desk Topics for the dropdown based on the specified site.
        public async Task<List<CommonDropDown>> GetAllHelpDeskTopicListForDropdown(string siteId)
        {
            var query = _helpDeskTopicRepository.TableNoTracking.Where(x => !x.Deleted && x.SiteId == siteId && x.IsActive);

            var list = await query
                 .OrderBy(x => x.Title)
                 .Select(x => new CommonDropDown
                 {
                     Value = x.Id,
                     Text = x.Title,
                 }).ToListAsync();

            return list;
        }
        #endregion

        #region GetAllHelpDeskTopicQuestionsListForDropdown
        // Title: GetAllHelpDeskTopicQuestionsListForDropdown
        // Description: Retrieves the list of active Help Desk Topic Questions for the dropdown based on the specified site and optional topic.
        public async Task<List<CommonDropDown>> GetAllHelpDeskTopicQuestionsListForDropdown(string siteId, string topicId = null)
        {
            var query = _helpDeskTopicQuestionsRepository.TableNoTracking.Where(x => !x.Deleted && x.HelpDeskTopic.SiteId == siteId && x.IsActive);

            if (!string.IsNullOrWhiteSpace(topicId))
            {
                query = query.Where(x => x.TopicId == topicId && !x.HelpDeskTopic.Deleted);
            }
            var list = await query
                  .OrderBy(x => x.Question)
                  .Select(x => new CommonDropDown
                  {
                      Value = x.Id,
                      Text = x.Question,
                  }).ToListAsync();

            return list;
        }
        #endregion

        #region GetAllHelpDeskTopicAndQuestionList
        public IPagedList<HelpDeskTopicQuestions> GetAllHelpDeskTopicAndQuestionList(
            string siteId,
            string searchText,
            string topic,
            string question,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        )
        {
            var query = _helpDeskTopicQuestionsRepository.TableNoTracking.Where(x => !x.Deleted && !x.HelpDeskTopic.Deleted && x.IsActive && x.HelpDeskTopic.SiteId == siteId);

            if (!string.IsNullOrWhiteSpace(topic))
            {
                topic = topic.Trim().ToLower();
                query = query.Where(x => x.HelpDeskTopic.Title.ToLower().Contains(topic));
            }
            if (!string.IsNullOrWhiteSpace(question))
            {
                question = question.Trim().ToLower();
                query = query.Where(x => x.Question.ToLower().Contains(question));
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();

                query = query.Where(m =>
                    m.Question.ToLower().Contains(searchText.Trim().ToLower()) ||
                    (m.HelpDeskTopic != null && m.HelpDeskTopic.Title != null && m.HelpDeskTopic.Title.ToLower().Contains(searchText))
                   );
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var orderBy = $"{GetOrderBy(sortBy)} {(descending ? "desc" : "asc")}";
                query = query.OrderBy(orderBy);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc);
            }

            query = query.Select(x => new HelpDeskTopicQuestions
            {
                Id = x.Id,
                TopicId = x.TopicId,
                Question = x.Question,
                HelpDeskTopic = new HelpDeskTopic
                {
                    Id = x.HelpDeskTopic.Id,
                    Title = x.HelpDeskTopic.Title,
                }
            });

            var list = new PagedList<HelpDeskTopicQuestions>(query, page, pageSize);
            return list;

            //var list = await _helpDeskTopicQuestionsRepository.TableNoTracking
            //    .Where(q => !q.Deleted && !q.HelpDeskTopic.Deleted && q.HelpDeskTopic.SiteId == siteId)
            //    .Select(q => new HelpDeskTopicQuestionList
            //    {
            //        TopicId = q.HelpDeskTopic.Id,
            //        TopicTitle = q.HelpDeskTopic.Title,
            //        QuestionId = q.Id,
            //        Question = q.Question,
            //        Description = q.Description
            //    })
            //    .ToListAsync();

            //return list;
        }
        #endregion

        #region GetHelpDeskTopicById
        // Title: GetHelpDeskTopicById
        // Description: This method retrieves a help desk topic from the database by its unique identifier (`id`). 
        public async Task<HelpDeskTopic> GetHelpDeskTopicById(string id)
        {
            var query = _helpDeskTopicRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetHelpDeskQuestionById
        // Title: GetHelpDeskQuestionById
        // Description: This method retrieves a help desk question from the database by its unique identifier (`id`). 
        public async Task<HelpDeskTopicQuestions> GetHelpDeskQuestionById(string id)
        {
            var query = _helpDeskTopicQuestionsRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id);
            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetHelpDeskTopicByTitle
        // Title: GetHelpDeskTopicByTitle
        // Description: This method retrieves a help desk topic based on its title. It allows an optional exclusion of a  help desk topic by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific help desk topic. The method returns the first matching help desk topic or null if no match is found.
        public async Task<HelpDeskTopic> GetHelpDeskTopicByTitle(string SiteId, string title, string id = null)
        {
            var query = _helpDeskTopicRepository.TableNoTracking.Where(x => x.Title == title && !x.Deleted && x.SiteId == SiteId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region GetHelpDeskQuestionByTopicAndQuestion
        // Title: GetHelpDeskQuestionByTopicAndQuestion
        // Description: This method retrieves a help desk question based on its question. It allows an optional exclusion of a help desk question by its ID, which can be useful for scenarios like checking for duplicates. while excluding a specific help desk question. The method returns the first matching help desk question or null if no match is found.
        public async Task<HelpDeskTopicQuestions> GetHelpDeskQuestionByTopicAndQuestion(string SiteId, string topicId, string question, string id = null)
        {
            var query = _helpDeskTopicQuestionsRepository.TableNoTracking.Where(x => x.TopicId == topicId && x.Question == question && !x.Deleted && x.HelpDeskTopic.SiteId == SiteId);

            if (!string.IsNullOrEmpty(id))
                query = query.Where(x => x.Id != id);

            var item = await query.FirstOrDefaultAsync();
            return item;
        }
        #endregion

        #region InsertHelpDeskTopic
        // Title: InsertHelpDeskTopic
        // Description: This method inserts a new help desk topic entity into the repository. It takes a help desk topic object as input and uses the _helpDeskTopicRepository to handle the insertion operation.
        public void InsertHelpDeskTopic(HelpDeskTopic entity)
        {
            _helpDeskTopicRepository.Insert(entity);
        }
        #endregion

        #region InsertHelpDeskQuestion
        // Title: InsertHelpDeskQuestion
        // Description: This method inserts a new help desk question entity into the repository. It takes a help desk question object as input and uses the _helpDeskTopicQuestionsRepository to handle the insertion operation.
        public void InsertHelpDeskQuestion(HelpDeskTopicQuestions entity)
        {
            _helpDeskTopicQuestionsRepository.Insert(entity);
        }
        #endregion


        #region UpdateHelpDeskTopic
        // Title: UpdateHelpDeskTopic
        // Description: This method updates the specified help desk topic entity in the repository. It takes a help desk topic object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateHelpDeskTopic(HelpDeskTopic entity)
        {
            _helpDeskTopicRepository.Update(entity);
        }
        #endregion

        #region UpdateHelpDeskQuestion
        // Title: UpdateHelpDeskQuestion
        // Description: This method updates the specified help desk question entity in the repository. It takes a help desk question object as input and uses the repository's Update method to persist changes to the data source.
        public void UpdateHelpDeskQuestion(HelpDeskTopicQuestions entity)
        {
            _helpDeskTopicQuestionsRepository.Update(entity);
        }
        #endregion


        #region DeleteHelpDeskTopic
        // Title: DeleteHelpDeskTopic
        // Description: Marks the specified help desk topic entity as deleted by setting its `Deleted` property to true.
        public void DeleteHelpDeskTopic(HelpDeskTopic entity)
        {
            entity.Deleted = true;
            _helpDeskTopicRepository.Update(entity);
        }
        #endregion

        #region DeleteHelpDeskQuestion
        // Title: DeleteHelpDeskQuestion
        // Description: Marks the specified help desk question entity as deleted by setting its `Deleted` property to true.
        public void DeleteHelpDeskQuestion(HelpDeskTopicQuestions entity)
        {
            entity.Deleted = true;
            _helpDeskTopicQuestionsRepository.Update(entity);
        }
        #endregion
    }
}
