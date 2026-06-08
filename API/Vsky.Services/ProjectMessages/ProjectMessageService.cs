using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Services.ProjectMessage
{
    public class ProjectMessageService : IProjectMessageService
    {
        #region Define Services
        private readonly IRepository<ProjectsMessages> _ProjectMessageRepository;
        private readonly ApplicationDbContext _db;
        #endregion

        #region Services Initializations
        public ProjectMessageService(IRepository<ProjectsMessages> ProjectMessagesrepository, ApplicationDbContext db)
        {
            _ProjectMessageRepository = ProjectMessagesrepository;
            _db = db;
        }
        #endregion

        #region GetProjectMessagesById
        // Title: GetProjectMessagesById
        // Description: This method retrieves a ProjectMessages from the database by its unique identifier (`id`). 
        public async Task<ProjectsMessages> GetProjectMessagesById(string id)
        {
            return await _ProjectMessageRepository.TableNoTracking.Where(x => !x.Deleted && x.Id == id).FirstOrDefaultAsync();
        }
        #endregion

        #region GetProjectMessagesByProjectId
        // Title: GetProjectMessagesByProjectId
        public async Task<List<ProjectsMessages>> GetProjectMessagesByProjectId(string siteId, string projectId, string LoggedUserId)
        {
            return await _ProjectMessageRepository.TableNoTracking.Where(x => x.SiteId == siteId && x.ProjectId == projectId && !x.Deleted).Select(m => new ProjectsMessages
            {
                Id = m.Id,
                CreatedById = m.CreatedById,
                CreatedOnUtc = m.CreatedOnUtc,
                ProjectId = projectId,
                SiteId = siteId,
                Message = m.Message,
                Reaction = m.Reaction,
                SentDate = m.SentDate,
                IsSent = m.SentBy == LoggedUserId ? true : false,
                MessageTime = GetTimeAgo((DateTime)m.CreatedOnUtc),
                SentByUser = new ApplicationUser
                {
                    Person = new Person
                    {
                        FullName = m.SentByUser.Person.FirstName + " " + m.SentByUser.Person.LastName,
                    }
                },
            }).OrderBy(m=>m.CreatedOnUtc).ToListAsync(); 
        }
        #endregion

        #region InsertMessage
        // Title: InsertMessage
        public void InsertProjectMessages(ProjectsMessages entity)
        {
            _ProjectMessageRepository.Insert(entity);
        }
        #endregion

        #region UpdateProjectMessages
        // Title: UpdateProjectMessages
        public void UpdateProjectMessages(ProjectsMessages entity)
        {
            _ProjectMessageRepository.Update(entity);
        }
        #endregion

        #region DeleteProjectMessages
        // Title: DeleteProjectMessages
        // Description: Marks the specified ProjectMessages entity as deleted by setting its `Deleted` property to true. 
        public void DeleteProjectMessages(ProjectsMessages entity)
        {
            entity.Deleted = true;
            _ProjectMessageRepository.Update(entity);
        }
        #endregion

        //public static string GetTimeAgo(DateTime dateTime)
        //{
        //    var timeSpan = DateTime.UtcNow - dateTime;

        //    if (timeSpan.TotalSeconds < 60)
        //        return $"{timeSpan.Seconds} second{(timeSpan.Seconds == 1 ? "" : "s")} ago";
        //    if (timeSpan.TotalMinutes < 60)
        //        return $"{timeSpan.Minutes} minute{(timeSpan.Minutes == 1 ? "" : "s")} ago";
        //    if (timeSpan.TotalHours < 24)
        //        return $"{timeSpan.Hours} hour{(timeSpan.Hours == 1 ? "" : "s")} ago";
        //    if (timeSpan.TotalDays < 7)
        //        return $"{timeSpan.Days} day{(timeSpan.Days == 1 ? "" : "s")} ago";
        //    if (timeSpan.TotalDays < 30)
        //        return $"{timeSpan.Days / 7} week{(timeSpan.Days / 7 == 1 ? "" : "s")} ago";
        //    if (timeSpan.TotalDays < 365)
        //        return $"{timeSpan.Days / 30} month{(timeSpan.Days / 30 == 1 ? "" : "s")} ago";

        //    return $"{timeSpan.Days / 365} year{(timeSpan.Days / 365 == 1 ? "" : "s")} ago";
        //}
        public static string GetTimeAgo(DateTime dateTime)
        {
            var now = DateTime.UtcNow;
            var timeSpan = now - dateTime;

            if (timeSpan.TotalSeconds < 0)
                return "Just now";

            if (timeSpan.TotalSeconds < 60)
                return $"{timeSpan.Seconds} second{(timeSpan.Seconds == 1 ? "" : "s")} ago";

            if (timeSpan.TotalMinutes < 60)
                return $"{timeSpan.Minutes} minute{(timeSpan.Minutes == 1 ? "" : "s")} ago";

            if (timeSpan.TotalHours < 24)
                return $"{timeSpan.Hours} hour{(timeSpan.Hours == 1 ? "" : "s")} ago";

            if (timeSpan.TotalDays < 7)
                return $"{timeSpan.Days} day{(timeSpan.Days == 1 ? "" : "s")} ago";

            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) == 1 ? "" : "s")} ago";

            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) == 1 ? "" : "s")} ago";

            return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) == 1 ? "" : "s")} ago";
        }

    }
}
