// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileWrapper.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the FileWrapper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EmailScraper.Wrappers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public static class FileWrapper
    {
        public static void WriteResultsToFile(Stopwatch stopwatch, string domain, IReadOnlyCollection<string> siteEmails)
        {
            File.WriteAllText(Environment.CurrentDirectory + $@"\{domain}.results", $"{stopwatch.Elapsed.Minutes}mins {stopwatch.Elapsed.Seconds}secs elapsed" + Environment.NewLine);
            File.AppendAllLines(Environment.CurrentDirectory + $@"\{domain}.results", siteEmails);
        }

      public static void WriteResultsToSpreadSheet(string domain, IReadOnlyCollection<string> emails, IReadOnlyCollection<string> urls)
      {
        SpreadSheetWrapper.CreateFile();
        SpreadSheetWrapper.AddEmailsToWorkbook(emails);
        //SpreadSheetWrapper.AddUrlsToWorkbook(urls);
        SpreadSheetWrapper.SaveFile(Environment.CurrentDirectory + $@"\{domain}.xlsx");
      }
    }
}
