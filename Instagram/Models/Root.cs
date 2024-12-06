using System.Text.RegularExpressions;

namespace Instagram.Models
{
    public class Root
    {
        public List<User>? users { get; set; }
        public bool? big_list { get; set; }
        public int? page_size { get; set; }
        public string? next_max_id { get; set; }
        public List<Group>? groups { get; set; }
        public bool? more_groups_available { get; set; }
        public bool? has_more { get; set; }
        public bool? should_limit_list_of_followers { get; set; }
        public bool? use_clickable_see_more { get; set; }
        public bool? show_spam_follow_request_tab { get; set; }
        public string? status { get; set; }
    }
}
