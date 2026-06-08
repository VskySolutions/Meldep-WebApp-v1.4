using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.SOPTemplates
{
    public interface ISOPTemplateSectionItemsService
    {
        SOPTemplateSectionItems GetSOPTemplateSectionItemById(string Id);
        void InsertSOPTemplateSectionItem(SOPTemplateSectionItems entity);
        void UpdateSOPTemplateSectionItem(SOPTemplateSectionItems entity);
        void DeleteSOPTemplateSectionItem(SOPTemplateSectionItems entity);
    }
}
