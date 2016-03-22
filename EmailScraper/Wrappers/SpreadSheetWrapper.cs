using System.Collections.Generic;
using System.Linq;
using GemBox.Spreadsheet;

namespace EmailScraper.Wrappers
{
  public static class SpreadSheetWrapper
  {
    private static ExcelFile workbook;
    private const string Workbook = "Results";
    private const string EmailColumn = "Email Addresses";
    private const string UrlColumn = "URLs";

    public static void CreateFile()
    {
      if (workbook != null) return;
      SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
      workbook = new ExcelFile();
    }

    public static void AddEmailsToWorkbook(IEnumerable<string> emails)
    {
      workbook.Worksheets.Add(Workbook).Cells["A1"].Value = EmailColumn;
      var i = 3;
      foreach (var email in emails)
      {
        workbook.Worksheets[Workbook].Cells[$"A{i}"].Value = email;
        i++;
      }
    }

    public static void AddUrlsToWorkbook(IEnumerable<string> urls)
    {
      workbook.Worksheets[Workbook].Cells["C1"].Value = UrlColumn;
      var i = 3;
      foreach (var url in urls)
      {
        workbook.Worksheets[Workbook].Cells[$"C{i}"].Value = url;
        i++;
      }
    }

    public static void SaveFile(string filename)
    {
      workbook.Save(filename);
    }
  }
}