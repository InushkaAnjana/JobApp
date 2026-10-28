using JobAppHR.Models;
using System.Data;

namespace JobAppHR.Repository
{
    public interface IFilterProcess
    {
        DataTable FilterSummary(string intakeCode);
        List<AgeFilter> FilterByAge(string intakeCode);
        //Task<List<ALFilter>> FilterByAL(string intakeCode, string currentStage);
        Task<List<ALFilter>> FilterByAL(string intakeCode, string currentStage);
        Task<List<OLFilter>> FilterByOL(string intakeCode, string currentStage);
        List<HEPQFilter> FilterByHEPQ(string intakeCode, string currentStage, string currentStatus);
        List<FinalFilter> FilterFinal(string intakeCode, string currentStage);
        List<FinalFilter> ShowFinal(string intakeCode, string currentStage, int? freezeNo = 0);
        List<FailedApplicants> ShowFail(string intakeCode, string currentStage, int? freezeNo = 0);
        List<FilterProgress> ShowProgress(string applicationCode);
        List<ShortListed> ShowShortListed(string intakeCode, string currentStage, int? freezeNo);
        ApiResponse ConfirmFinal(string intakeCode, string intakeType);
        string[] CountShortListed(string intakeCode, int? freezeNo);
    }
}
