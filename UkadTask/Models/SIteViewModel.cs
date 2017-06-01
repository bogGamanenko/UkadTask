namespace UkadTask.Models
{
    using System.Collections.Generic;

    public class SiteViewModel
    {
        public List<TestedUrl> TestedUrls { get; set; }

        public List<string> Urls { get; set; }
    }
}