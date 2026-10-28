using Fingers10.ExcelExport.ActionResults;
using JobAppHR.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobAppHR.Repository
{
    public interface IUtilityFn
    {
        List<T> ConvertToList<T>(DataTable dataTable);
        SelectList ConvertToSelectList(DataTable table, string valueField, string textField);
        DataTable ConvertToDataTable<T>(IEnumerable<T> self);
        string ConvertToTitleCase(string text);
        User GetCurrentUser();
        string ConvertToCSV(DataTable dataTable, string fileName);
        DataTable ConvertJsonToDataTable(string jsonString);

        DataTable GetEligibleApplicationCodes(string jsonString);

        //List<string> GetApplicationCodes(string jsonString);
    }
}
