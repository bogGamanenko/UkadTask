namespace UkadTask.Infrastructure
{
    using System.Collections.Generic;
    using System.Net;
    using System.Xml;

    public static class SiteMapParser
    {
        const string siteMapPath = @"/sitemap.xml";
        const string urlset = "urlset";

        public static List<string> GetSiteMapUrls(string siteUrl)
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
            catch (WebException)
            {
                
            }

            return result;
        }
    }
}