using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.ProjectMessage
{
    public interface IProjectMessageService
    {

        #region GetProjectMessagesById
        // Title: GetProjectMessagesById
        // Description: This method retrieves a ProjectMessages from the database by its unique identifier (`id`). 
        Task<ProjectsMessages> GetProjectMessagesById(string id);
        #endregion

        #region GetProjectMessagesByProjectId
        // Title: GetProjectMessagesByProjectId
        Task<List<ProjectsMessages>> GetProjectMessagesByProjectId(string siteId, string projectId, string LoggedUserId);
        #endregion

        #region InsertMessage
        // Title: InsertMessage
        void InsertProjectMessages(ProjectsMessages entity);
        #endregion

        #region UpdateProjectMessages
        // Title: UpdateProjectMessages
        void UpdateProjectMessages(ProjectsMessages entity);
        #endregion

        #region DeleteProjectMessages
        // Title: DeleteProjectMessages
        // Description: Marks the specified ProjectMessages entity as deleted by setting its `Deleted` property to true. 
        void DeleteProjectMessages(ProjectsMessages entity);
        #endregion
    }
}
