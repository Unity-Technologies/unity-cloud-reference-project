using System;
using System.Text;

namespace Unity.ReferenceProject.SearchSortFilter
{
    public interface IHighlightModule
    {
        public (bool, string) IsHighlighted(string target);
        public (bool, string) IsHighlighted(string bindPathName, string target);
    }
    
    public class HighlightModule: IHighlightModule
    {
        readonly ISearchModule m_Module;

        public HighlightModule(ISearchModule module)
        {
            m_Module = module;
        }

        public (bool, string) IsHighlighted(string bindPathName, string target)
        {
            if (!m_Module.ContainsBindPathName(bindPathName))
                return (false, target);

            return IsHighlighted(target);
        }
        
        public (bool, string) IsHighlighted(string target)
        {
            var highlightString = m_Module.searchString;

            if (string.IsNullOrEmpty(highlightString) || string.IsNullOrWhiteSpace(highlightString))
                return (false, target);

            var indexOf = target.IndexOf(highlightString, StringComparison.OrdinalIgnoreCase);
            var sb = new StringBuilder();
            var currentIndex = 0;

            while (indexOf != -1)
            {
                sb.Append(target.Substring(currentIndex, indexOf - currentIndex));
                sb.Append("<color=#2096F3>");
                sb.Append(target.Substring(indexOf, highlightString.Length));
                sb.Append("</color>");
                currentIndex = indexOf + highlightString.Length;

                indexOf = target.IndexOf(highlightString, currentIndex, StringComparison.OrdinalIgnoreCase);
            }

            sb.Append(target.Substring(currentIndex));
            return (true, sb.ToString());
        }
    }
    
    public class HighlightModuleNode: IHighlightModule
    {
        public IHighlightModule HighlightModule { private get; set; }

        public (bool, string) IsHighlighted(string target)
        {
            if (HighlightModule != null)
                return HighlightModule.IsHighlighted(target);
            return (false, target);
        }

        public (bool, string) IsHighlighted(string bindPathName, string target)
        {
            if (HighlightModule != null)
                return HighlightModule.IsHighlighted(bindPathName, target);
            return (false, target);
        }
    }
}
