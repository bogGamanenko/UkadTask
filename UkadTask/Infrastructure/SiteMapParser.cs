namespace UkadTask.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;   
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml;

    public static class SiteMapParser
    {
        const string siteMapPath = @"/sitemap.xml";
        const string urlset = "urlset";
        const string anchorPattern = @"(<a.*?>.*?</a>)";
        const string hrefPattern = "href=\"(.*?)\"";

        public static List<string> GetUrlsFromXml(string siteUrl)
        {
            var xmlDoc = new XmlDocument();
            var result = new List<string>();

            try
            {
                xmlDoc.Load(siteUrl + siteMapPath);

                foreach (XmlNode topNode in xmlDoc.ChildNodes)
                {
                    if (topNode.Name.ToLower() == urlset)
                    {
                        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                        nsmgr.AddNamespace("ns", topNode.NamespaceURI);

                        foreach (XmlNode urlNode in topNode.ChildNodes)
                        {
                            var locNode = urlNode.SelectSingleNode("ns:loc", nsmgr);
                            if (locNode != null)
                            {
                                result.Add(locNode.InnerText);
                            }
                        }

                        break;
                    }
                }
            }
            catch (Exception) { }

            return result;
        }

        public static async Task<List<string>> GetUrlsFromHtml(string siteUrl)
        {
            var result = new List<string>();
            var htmlContent = await GetSiteContent(siteUrl);

            if(htmlContent != null)
            {
                var matches = Regex.Matches(htmlContent, anchorPattern, RegexOptions.Singleline);

                foreach (Match match in matches)
                {
                    string htmlAnchor = match.Groups[1].Value;
                    var matchLink = Regex.Match(htmlAnchor, hrefPattern, RegexOptions.Singleline);
                    
                    if (matchLink.Success)
                    {
                        var link = matchLink.Groups[1].Value;
                        if (link[0] == '/' && link.Length > 1)
                        {
                            if (link[1] == '/')
                            {
                                link = "http:" + link;
                            }
                            else
                            {
                                link = siteUrl + link;
                            }

                            result.Add(link);
                        }
                        else if (link.Contains("http"))
                        {
                            result.Add(link);
                        }                                                                                                  
                    }
                }
            }

            return result.Distinct().ToList();            
        }

        private static async Task<string> GetSiteContent(string url)
        {
            string content = null;

            try
            {
                using (var client = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        using (var response = await client.SendAsync(request))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                content = await response.Content.ReadAsStringAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception) { }            

            return content;
        }
    }
}