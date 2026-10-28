using JobAppHR.Models;
using System.Data;

namespace JobAppHR.Repository
{
    public interface IManualProcess
    {
        DataTable FilterSummary(string intakeCode);
        List<ManualFilter> FilterByAll(string intakeCode, string currentStage, string currentStatus);
        List<ManualFilter> GetAllData(string intakeCode, string currentStage, DataTable maintbl);
        List<FullReportModel> GetFullReportData(string intakeCode, string currentStage, int? freezeNo = 0, bool showAll = false, string currentStatus = "PASS");

    }
}
