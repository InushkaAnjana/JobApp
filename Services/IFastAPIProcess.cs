using JobAppHR.Models;

namespace JobAppHR.Services
{
    public interface IFastAPIProcess
    {
        Task<ApiResponse> UploadApplicationData(string fileNameAppData, string fileNameResultData);
        Task<ApiResponse> DeleteAllData();
        Task<ApiResponse> FilterByPosition(string positionCode, string intakeCode);
    }
}
