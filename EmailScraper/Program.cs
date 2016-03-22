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
  using System.Linq;
  using System.Net;

  using EmailScraper.Wrappers;

  [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented",
    Justification = "Reviewed. Suppression is OK here.")]
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

    private static void WriteResults()
    {
      var domain = DocumentParser.GetDomain(Url);
      ConsoleWrapper.WriteResultsToConsole(SiteEmails, Stopwatch);
      FileWrapper.WriteResultsToFile(Stopwatch, domain, SiteEmails);
      FileWrapper.WriteResultsToSpreadSheet(domain, SiteEmails, SiteUrls);
    }

    private static void FindAllEmailsInDomain(string url)
    {
      try
      {
        var html = new WebClient().DownloadString(url);
        Console.WriteLine($"Parsing: {url}");
        var document = DocumentParser.Parse(html);
        var emails = DocumentParser.FindEmails(document);
        var urls = DocumentParser.FindNewUrls(document, Url, SiteUrls);
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
        var webResponse = (HttpWebResponse) ex.Response;

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
  }
}
