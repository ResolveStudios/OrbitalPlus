using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Orbital
{
    public class YtResult
    {
        public string thumbnail;
        public int duration;
        public string uploader_id;
        public int like_count;
        public int view_count;
        public string uploader;
        public int n_entries;
        public string vcodec;
        public string channel;
        public string album;
        public string alt_title;
        public double average_rating;
        public List<string> categories;
        public string uploader_url;
        public double tbr;
        public int height;
        public string description;
        public string format_note;
        public int playlist_index;
        public int filesize;
        public string webpage_url_basename;
        public string channel_id;
        public string url;
        public object playlist_uploader;
        public object is_live;
        public object playlist_uploader_id;
        public object requested_subtitles;
        public int asr;
        public string id;
        public int width;
        public List<Format> formats;
        public string playlist_id;
        public object playlist_title;
        public int fps;
        public string channel_url;
        public HttpHeaders http_headers;
        public string title;
        public string upload_date;
        public string extractor_key;
        public string format_id;
        public string webpage_url;
        public List<Thumbnail> thumbnails;
        public string artist;
        public string playlist;
        public List<string> tags;
        public string display_id;
        public string acodec;
        public string protocol;
        public string ext;
        public string creator;
        public int age_limit;
        public string fulltitle;
        public string _filename;
        public int quality;
        public string format;
        public string track;
        public string extractor;
        [JsonIgnore] internal string json;
        [JsonIgnore] internal bool isReady;
        [JsonIgnore] internal bool thumbnailSpriteLoading;
    }
}