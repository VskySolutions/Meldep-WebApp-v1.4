using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vsky.Models;

namespace Vsky.Services.SOPTemplates
{
    public interface ISOPTemplateSectionService
    {
        SOPTemplateSection GetSOPTemplateSectionById(string Id);
        void InsertSOPTemplateSection(SOPTemplateSection entity);
        void UpdateSOPTemplateSection(SOPTemplateSection entity);
        void DeleteSOPTemplateSection(SOPTemplateSection entity);
    }
}
