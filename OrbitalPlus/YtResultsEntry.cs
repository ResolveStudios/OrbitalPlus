using System.Text;
using System.Text.Json.Serialization;

namespace Orbital
{
    public class YtResultsEntry
    {
        public string uploader;
        public int view_count;
        public string title;
        public object description;
        public double duration;
        public string ie_key;
        public string id;
        public string url;
        public string _type;
        [JsonIgnore] internal YtResult more;
        [JsonIgnore] internal bool expanded;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("uploader: " + uploader);
            sb.AppendLine("view count" + view_count);
            sb.AppendLine("title: " + title);
            sb.AppendLine("description: " + description);
            sb.AppendLine("duration: " + duration);
            sb.AppendLine("ie key: " + ie_key);
            sb.AppendLine("id: " + id);
            sb.AppendLine("url: " + url);
            sb.AppendLine("type: " + _type);
            return sb.ToString();
        }
    }
}