// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EmailScraper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;

    using HtmlAgilityPack;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public class Program
    {
        private const string Url = "https://www.churchill.com";
        private static readonly HashSet<string> SiteUrls = new HashSet<string>();
        private static readonly HashSet<string> SiteEmails = new HashSet<string>();
        private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();

        public static void Main(string[] args)
        {
            Stopwatch.Start();
            FindAllEmailsInDomain(Url);
            Stopwatch.Stop();
            WriteResults();
            Console.Read();
        }

        private static void WriteResultsToConsole()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Emails:");
            SiteEmails.ToList().ForEach(Console.WriteLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Found {SiteEmails.Count} emails");
            Console.WriteLine($"{Stopwatch.Elapsed.Minutes}mins {Stopwatch.Elapsed.Seconds}secs elapsed");
        }

        private static void WriteResults()
        {
            WriteResultsToConsole();
            WriteResultsToFile();
        }

        private static void WriteResultsToFile()
        {
            File.WriteAllText(Environment.CurrentDirectory + $@"\{GetDomain(Url)}.results", $"{Stopwatch.Elapsed.Minutes}mins {Stopwatch.Elapsed.Seconds}secs elapsed" + Environment.NewLine);
            File.AppendAllLines(Environment.CurrentDirectory + $@"\{GetDomain(Url)}.results", SiteEmails);
        }

        private static void FindAllEmailsInDomain(string url)
        {
            try
            {
                var html = new WebClient().DownloadString(url);
                Console.WriteLine($"Parsing: {url}");
                var document = Parse(html);
                var emails = FindEmails(document);
                var urls = FindNewUrls(document, Url);
                var domainUrls = urls as string[] ?? urls.ToArray();
                AddUrls(domainUrls);
                AddEmails(emails);

                foreach (var domainUrl in domainUrls)
                {
                    FindAllEmailsInDomain(domainUrl);
                }
            }
            catch (WebException ex)
            {
                var webResponse = (HttpWebResponse)ex.Response;

                switch (webResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"HTTP 404: {url}");
                        return;
                    case HttpStatusCode.InternalServerError:
                        Console.WriteLine($"Internal Server Error 500: {url}");
                        return;
                    case HttpStatusCode.BadRequest:
                        Console.WriteLine($"Bad Request 400: {url}");
                        return;
                    case HttpStatusCode.Redirect:
                        Console.WriteLine($"Redirect 302: {url}");
                        return;
                    default:
                        Console.WriteLine(ex);
                        return;
                }
            }
        }

        private static void AddEmails(IEnumerable<string> emails)
        {
            foreach (var email in emails)
            {
                SiteEmails.Add(email.Trim().ToLower());
            }
        }

        private static void AddUrls(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                SiteUrls.Add(url.Trim());
            }
        }

        private static IEnumerable<string> FindEmails(HtmlDocument document)
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
        
        private static IEnumerable<string> FindNewUrls(HtmlDocument document, string baseUrl)
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
            urls = RemoveVisitedUrls(urls);
            urls = RemoveUrlsFromDifferentDomains(baseUrl, urls);

            return urls;
        }

        private static List<string> RemoveVisitedUrls(List<string> urls)
        {
            var formattedUrls = urls.Select(url => url.TrimEnd('/')).ToList();

            foreach (var siteUrl in SiteUrls)
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

        private static string GetDomain(string url)
        {
            Uri baseUri;
            Uri.TryCreate(url, UriKind.Absolute, out baseUri);
            return baseUri.Host;
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

        private static HtmlDocument Parse(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document;
        }
    }
}
