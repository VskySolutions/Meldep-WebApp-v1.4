using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Core;
using Vsky.Models;

namespace Vsky.Services.HelpDesks
{
    public interface IHelpDeskTopicService
    {
        #region GetAllHelpDeskTopicList
        Task<List<HelpDeskTopic>> GetAllHelpDeskTopicList(string siteId);
        #endregion

        #region GetAllHelpDeskTopicQuestionList
        Task<List<HelpDeskTopicQuestions>> GetAllHelpDeskTopicQuestionList(string siteId, string topicId = null);
        #endregion

        #region GetAllHelpDeskTopicListForDropdown
        Task<List<CommonDropDown>> GetAllHelpDeskTopicListForDropdown(string siteId);
        #endregion

        #region GetAllHelpDeskTopicQuestionsListForDropdown
        Task<List<CommonDropDown>> GetAllHelpDeskTopicQuestionsListForDropdown(string siteId, string topicId = null);
        #endregion

        #region GetAllHelpDeskTopicAndQuestionList
        IPagedList<HelpDeskTopicQuestions> GetAllHelpDeskTopicAndQuestionList(
            string siteId,
            string searchText,
            string topic,
            string question,
            string sortBy,
            bool descending,
            int page = 1,
            int pageSize = int.MaxValue,
            bool lookup = false
        );
        #endregion

        #region GetHelpDeskTopicById
        Task<HelpDeskTopic> GetHelpDeskTopicById(string id);
        #endregion

        #region GetHelpDeskQuestionById
        Task<HelpDeskTopicQuestions> GetHelpDeskQuestionById(string id);
        #endregion

        #region GetHelpDeskTopicByTitle
        Task<HelpDeskTopic> GetHelpDeskTopicByTitle(string SiteId, string title, string id = null);
        #endregion

        #region GetHelpDeskQuestionByTopicAndQuestion
        Task<HelpDeskTopicQuestions> GetHelpDeskQuestionByTopicAndQuestion(string SiteId, string topicId, string question, string id = null);
        #endregion

        #region InsertHelpDeskTopic
        void InsertHelpDeskTopic(HelpDeskTopic entity);
        #endregion

        #region UpdateHelpDeskTopic
        void UpdateHelpDeskTopic(HelpDeskTopic entity);
        #endregion

        #region DeleteHelpDeskTopic
        void DeleteHelpDeskTopic(HelpDeskTopic entity);
        #endregion

        #region InsertHelpDeskQuestion
        void InsertHelpDeskQuestion(HelpDeskTopicQuestions entity);
        #endregion

        #region UpdateHelpDeskQuestion
        void UpdateHelpDeskQuestion(HelpDeskTopicQuestions entity);
        #endregion

        #region DeleteHelpDeskQuestion
        void DeleteHelpDeskQuestion(HelpDeskTopicQuestions entity);
        #endregion
    }
}
