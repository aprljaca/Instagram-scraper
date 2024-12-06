namespace Instagram.Models
{
    public class Group
    {
        public string? group { get; set; }
        public string? title { get; set; }
        public string? context { get; set; }
        public List<Facepile>? facepile { get; set; }
        public string? subtitle { get; set; }
        public string? subtitle_button_text { get; set; }
        public string? category { get; set; }
        public List<string>? actions { get; set; }
        public bool? show_hashtag_icon { get; set; }
    }
}
