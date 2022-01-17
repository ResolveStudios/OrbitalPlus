using System.Collections.Generic;
using System.Text;

namespace Orbital
{
    public class YtResults
    {
        public string extractor;
        public string _type;
        public string extractor_key;
        public string webpage_url;
        public string webpage_url_basename;
        public List<YtResultsEntry> entries;
        public string id;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("id: " + id);
            sb.AppendLine("extractor: " + extractor);
            sb.AppendLine("type: " + _type);
            sb.AppendLine("extractor key: " + extractor_key);
            sb.AppendLine("webpage url: " + webpage_url);
            sb.AppendLine("webpage url basename: " + webpage_url_basename);
            sb.AppendLine(entries.ToString());
            return sb.ToString();
        }
    }
}