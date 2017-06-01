namespace UkadTask.Models
{
    using System.ComponentModel.DataAnnotations;

    public class SiteUrl
    {
        [Required(ErrorMessage = "Enter the URL")]        
        public string Url { get; set; }
    }
}