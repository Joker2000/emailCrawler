// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentParser.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DocumentParser type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EmailScraper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using HtmlAgilityPack;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public static class DocumentParser
    {
        public static HtmlDocument Parse(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document;
        }

        public static IEnumerable<string> FindEmails(HtmlDocument document)
        {
            var anchors = document.DocumentNode.SelectNodes("//a[@href]");
            if (anchors == null)
            {
                return new List<string>();
            }

            var emails =
                anchors.Where(x => x.Attributes["href"].Value.Contains("mailto:"))
                    .Select(x => x.Attributes["href"].Value.Replace("mailto:", string.Empty));

            return emails;
        }

        public static IEnumerable<string> FindNewUrls(HtmlDocument document, string baseUrl, HashSet<string> siteUrls)
        {
            var anchors = document.DocumentNode.SelectNodes("//a[@href]");
            if (anchors == null)
            {
                return new List<string>();
            }

            var urls =
                anchors.Where(x => !x.Attributes["href"].Value.Contains("mailto:"))
                    .Select(x => x.Attributes["href"].Value)
                    .ToList();

            urls = AddBaseUrlToRelativeUrl(baseUrl, urls).ToList();
            urls = RemoveInvalidUrls(urls);
            urls = RemoveVisitedUrls(urls, siteUrls);
            urls = RemoveUrlsFromDifferentDomains(baseUrl, urls);

            return urls;
        }

        public static string GetDomain(string url)
        {
            Uri baseUri;
            Uri.TryCreate(url, UriKind.Absolute, out baseUri);
            return baseUri.Host;
        }

        private static List<string> RemoveVisitedUrls(List<string> urls, HashSet<string> siteUrls)
        {
            var formattedUrls = urls.Select(url => url.TrimEnd('/')).ToList();

            foreach (var siteUrl in siteUrls)
            {
                if (formattedUrls.Contains(siteUrl))
                {
                    formattedUrls.RemoveAll(x => x.Equals(siteUrl));
                }
            }

            return formattedUrls;
        }

        private static List<string> RemoveUrlsFromDifferentDomains(string baseUrl, IEnumerable<string> urls)
        {
            var domain = GetDomain(baseUrl);
            var currentDomainUrls = new List<string>();
            foreach (var url in urls)
            {
                Uri temporaryUri;
                Uri.TryCreate(url, UriKind.Absolute, out temporaryUri);
                if (string.Equals(domain, temporaryUri.Host))
                {
                    currentDomainUrls.Add(url);
                }
            }

            return currentDomainUrls;
        }

        private static List<string> RemoveInvalidUrls(List<string> urls)
        {
            var uniqueUrls = urls.Select(item => (string)item.Clone()).ToList();

            foreach (var url in urls.Where(url => (!(url.StartsWith("http") || url.StartsWith("https")) || url.EndsWith(".pdf"))))
            {
                uniqueUrls.Remove(url);
            }

            return uniqueUrls;
        }

        private static IEnumerable<string> AddBaseUrlToRelativeUrl(string baseUrl, IEnumerable<string> urls)
        {
            var fixedUrls = new HashSet<string>();

            foreach (var url in urls)
            {
                if (url.StartsWith("/"))
                {
                    fixedUrls.Add(baseUrl + url);
                }
                else
                {
                    fixedUrls.Add(url);
                }
            }

            return fixedUrls;
        }
    }
}
