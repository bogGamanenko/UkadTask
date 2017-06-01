namespace UkadTask.Models
{
    using System;

    public class TestedUrl
    {
        public TestedUrl(string url, TimeSpan requestTime)
        {
            this.Url = url;
            this.RequestTimeMin = requestTime;
            this.RequestTimeMax = requestTime;
        }

        public string Url { get; set; }

        public TimeSpan RequestTimeMin { get; set; }

        public TimeSpan RequestTimeMax { get; set; }

        public DateTime Date { get; set; }
    }
}