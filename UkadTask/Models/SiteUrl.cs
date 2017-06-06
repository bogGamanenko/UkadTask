namespace UkadTask.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class SiteUrl
    {
        [Required(ErrorMessage = "Enter the URL")]
        [Remote("CheckUrl", "Home", ErrorMessage = "URL is not valid")]
        public string Url { get; set; }
    }
}