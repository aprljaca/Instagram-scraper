using System.Text.Json.Serialization;

namespace Instagram.Models
{
    public class Facepile
    {
        public string pk { get; set; }
        public string pk_id { get; set; }
        public string username { get; set; }
        public string full_name { get; set; }
        public bool is_private { get; set; }
        public string fbid_v2 { get; set; }
        public int third_party_downloads_enabled { get; set; }
        public string strong_id__ { get; set; }
        public string id { get; set; }
        public string profile_pic_id { get; set; }
        public string profile_pic_url { get; set; }
        public bool is_verified { get; set; }
        public bool has_anonymous_profile_picture { get; set; }
        public List<object> account_badges { get; set; }
    }
}