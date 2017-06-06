namespace UkadTask.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using Infrastructure;
    using Models;
    using System.Threading.Tasks;

    public class HomeController : Controller
    {
        const string testedUrlsKey = "testedUrlsKey";
        const string historyKey = "historyKey";
        const string erroMessage = " [Bad Request!]";

        private static Dictionary<string, List<string>> cachedUrls = new Dictionary<string, List<string>>();

        private List<TestedUrl> GetTestedUrls()
        {
            var urls = (List<TestedUrl>)this.HttpContext.Session[testedUrlsKey];

            if(urls == null)
            {
                urls = new List<TestedUrl>();
                this.HttpContext.Session[testedUrlsKey] = urls;
            }

            return urls;
        }

        private List<TestedUrl> GetHistory()
        {
            var history = (List<TestedUrl>)this.HttpContext.Session[historyKey];

            if (history == null)
            {
                history = new List<TestedUrl>();
                this.HttpContext.Session[historyKey] = history;
            }

            return history;
        }

        private void AddHistory(string url, TimeSpan requestTime)
        {
            var testedUrl = new TestedUrl(url, requestTime);
            testedUrl.Date = DateTime.Now;

            var history = GetHistory();
            history.Add(testedUrl);                        
        }

        public ActionResult Index()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<ActionResult> Site(SiteUrl siteUrl)
        {            
            if (this.ModelState.IsValid)
            {
                List<string> urls;

                if (cachedUrls.Keys.Contains(siteUrl.Url))
                {
                    urls = cachedUrls[siteUrl.Url];
                }
                else
                {
                    urls = SiteMapParser.GetUrlsFromXml(siteUrl.Url);
                    if (urls.Count == 0)
                    {
                        urls = await SiteMapParser.GetUrlsFromHtml(siteUrl.Url);
                    }
                    cachedUrls[siteUrl.Url] = urls;
                }

                var testedUrls = this.GetTestedUrls();
                var viewModel = new SiteViewModel()
                {
                    TestedUrls = testedUrls,
                    Urls = urls
                };

                return this.View(viewModel);
            }
            else
            {
                return this.View("Index", siteUrl);
            }
        }

        public ActionResult History()
        {
            var history = this.GetHistory();
            history = history.OrderByDescending(u => u.Date).ToList();

            return this.View(history);
        }
        
        public ActionResult TestUrl(string url)
        {
            var timer = new Stopwatch();
            var request = WebRequest.Create(url);
            TimeSpan responseTime;

            try
            {
                timer.Start();
                request.GetResponse();
                timer.Stop();
                responseTime = timer.Elapsed;
            }
            catch(Exception)
            {
                url += erroMessage;
                responseTime = TimeSpan.FromMilliseconds(0);
            }

            this.AddHistory(url, responseTime);
            var urls = this.GetTestedUrls();
            var testedUrl = urls.FirstOrDefault(u => u.Url == url);

            if(testedUrl != null)
            {
                if(testedUrl.RequestTimeMin > responseTime)
                {               
                    testedUrl.RequestTimeMin = responseTime;
                }
                else if(testedUrl.RequestTimeMax < responseTime)
                {
                    testedUrl.RequestTimeMax = responseTime;
                }
            }
            else
            {
                testedUrl = new TestedUrl(url, responseTime);
                urls.Add(testedUrl);
            }

            urls = urls.OrderByDescending(u => u.RequestTimeMax).ToList();
            return this.PartialView("_TestedUrls", urls);
        }
        
        public JsonResult TestedUrls()
        {          
            var results = this.GetTestedUrls()
                              .Select(u => u.RequestTimeMax.TotalMilliseconds)
                              .OrderByDescending(t => t)
                              .ToList();            
   
            return this.Json(results, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckUrl(string url)
        {
            var result = Uri.IsWellFormedUriString(url, UriKind.Absolute);
            
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}