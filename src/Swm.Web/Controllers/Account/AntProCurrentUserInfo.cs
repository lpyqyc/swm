namespace Swm.Web.Controllers
{
    public class AntProCurrentUserInfo
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string Userid { get; set; }
        public string Email { get; set; }
        public string Signature { get; set; }
        public string Title { get; set; }
        public string Group { get; set; }
        public KeyLabelPair[] Tags { get; set; }
        public int NotifyCount { get; set; }
        public int UnreadCount { get; set; }
        public string Country { get; set; }
        public Geographic Geographic { get; set; }

        public string Address { get; set; }
        public string Phone { get; set; }
    }

    public class KeyLabelPair
    {
        public string Label { get; set; }
        public string Key { get; set; }
    }

    public class Geographic
    {
        public KeyLabelPair Province { get; set; }
        public KeyLabelPair City { get; set; }
    }
}
