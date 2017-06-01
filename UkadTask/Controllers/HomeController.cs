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

    public class HomeController : Controller
    {
        const string testedUrlsKey = "testedUrlsKey";
        const string historyKey = "historyKey";

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
        public ActionResult Site(SiteUrl siteUrl)
        {
            if(this.ModelState.IsValid)
            {
                List<string> urls;

                if (cachedUrls.Keys.Contains(siteUrl.Url))
                {
                    urls = cachedUrls[siteUrl.Url];
                }
                else
                {
                    urls = SiteMapParser.GetSiteMapUrls(siteUrl.Url);
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

            timer.Start();
            request.GetResponse();
            timer.Stop();

            this.AddHistory(url, timer.Elapsed);

            var urls = this.GetTestedUrls();
            var testedUrl = urls.FirstOrDefault(u => u.Url == url);

            if(testedUrl != null)
            {
                if(testedUrl.RequestTimeMin > timer.Elapsed)
                {               
                    testedUrl.RequestTimeMin = timer.Elapsed;
                }
                else if(testedUrl.RequestTimeMax < timer.Elapsed)
                {
                    testedUrl.RequestTimeMax = timer.Elapsed;
                }
            }
            else
            {
                testedUrl = new TestedUrl(url, timer.Elapsed);
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
    }
}