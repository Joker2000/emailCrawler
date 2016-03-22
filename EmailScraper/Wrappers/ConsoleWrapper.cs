// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleWrapper.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ConsoleWrapper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EmailScraper.Wrappers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public static class ConsoleWrapper
    {
        public static void WriteResultsToConsole(IReadOnlyCollection<string> siteEmails, Stopwatch stopwatch)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Emails:");
            siteEmails.ToList().ForEach(Console.WriteLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Found {siteEmails.Count} emails");
            Console.WriteLine($"{stopwatch.Elapsed.Minutes}mins {stopwatch.Elapsed.Seconds}secs elapsed");
        }
    }
}
