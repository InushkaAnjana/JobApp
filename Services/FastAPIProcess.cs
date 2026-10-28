using DocumentFormat.OpenXml.EMMA;
using JobAppHR.Models;
using System.Net.Http.Headers;

namespace JobAppHR.Services
{
    public class FastAPIProcess : IFastAPIProcess
    {
        public async Task<ApiResponse> UploadApplicationData(string fileNameAppData, string fileNameResultData)
        {
            ApiResponse apiResponse = new();

            try
            {
                string filePathAppData = Path.Combine(StaticData.UploadPath, fileNameAppData);
                string filePathResultData = Path.Combine(StaticData.UploadPath, fileNameResultData);

                // Check if files exist
                if (!File.Exists(filePathAppData))
                {
                    apiResponse.isSuccess = false;
                    apiResponse.message = $"Application data file not found: {filePathAppData}";
                    return apiResponse;
                }

                if (!File.Exists(filePathResultData))
                {
                    apiResponse.isSuccess = false;
                    apiResponse.message = $"Result data file not found: {filePathResultData}";
                    return apiResponse;
                }

                using var form = new MultipartFormDataContent();
                
                //get application data
                var fileContent = new ByteArrayContent(File.ReadAllBytes(filePathAppData));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                // here it is important that second parameter matches with name given in API.
                form.Add(fileContent, "application_file", Path.GetFileName(filePathAppData));

                //get result data
                fileContent = new ByteArrayContent(File.ReadAllBytes(filePathResultData));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                // here it is important that second parameter matches with name given in API.
                form.Add(fileContent, "exam_results_file", Path.GetFileName(filePathResultData));

                using var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(StaticData.FastAPIUrl),
                    Timeout = TimeSpan.FromSeconds(60) // Shorter timeout for faster fallback
                };

                var response = await httpClient.PostAsync($"/upload_bulk", form);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    apiResponse.isSuccess = true;
                    apiResponse.result = responseContent;
                    apiResponse.message = "Upload successful";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    apiResponse.isSuccess = false;
                    apiResponse.message = $"HTTP {response.StatusCode}: {errorContent}";
                }
            }
            catch (HttpRequestException ex)
            {
                apiResponse.isSuccess = false;
                apiResponse.message = $"Network error: {ex.Message}";
            }
            catch (TaskCanceledException ex)
            {
                apiResponse.isSuccess = false;
                apiResponse.message = $"Request timeout: {ex.Message}";
            }
            catch (Exception ex)
            {
                apiResponse.isSuccess = false;
                apiResponse.message = $"Unexpected error: {ex.Message}";
            }

            return apiResponse;
        }

        public async Task<ApiResponse> DeleteAllData()
        {
            ApiResponse apiResponse = new();

            try
            {
                using var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(StaticData.FastAPIUrl),
                    Timeout = TimeSpan.FromSeconds(30) // Shorter timeout for faster fallback
                };

                var response = await httpClient.DeleteAsync($"/metadata/delete_all/");

                if (response.IsSuccessStatusCode)
                {                 
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    apiResponse.isSuccess = true;
                    apiResponse.result = responseContent;
                    apiResponse.message = "Delete successful";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    apiResponse.isSuccess = false;
                    apiResponse.message = $"HTTP {response.StatusCode}: {errorContent}";
                }
            }
            catch (HttpRequestException ex)
            {
                apiResponse.isSuccess = false;
                apiResponse.message = $"Network error: {ex.Message}";
            }
            catch (TaskCanceledException ex)
            {
                apiResponse.isSuccess = false;
                apiResponse.message = $"Request timeout: {ex.Message}";
            }
            catch (Exception ex)
            {
                apiResponse.isSuccess = false;
                apiResponse.message = $"Unexpected error: {ex.Message}";
            }

            return apiResponse;
        }

        public async Task<ApiResponse> FilterByPosition(string positionCode, string intakeCode)
        {
            ApiResponse apiResponse = new();

            try
            {
                using var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(StaticData.FastAPIUrl),
                    Timeout = TimeSpan.FromSeconds(60) // Shorter timeout for faster fallback
                };

                string queryString = $"?position={positionCode}&max_age={60}&intake_code={intakeCode}";

                var response = await httpClient.GetAsync($"/filter_by_position/" + queryString);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    apiResponse.isSuccess = true;
                    apiResponse.result = responseContent;
                    apiResponse.message = "Filter successful";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    apiResponse.isSuccess = false;
                    apiResponse.message = $"HTTP {response.StatusCode}: {errorContent}";
                }
            }
            catch (HttpRequestException ex)
            {
                apiResponse.isSuccess = false;
                apiResponse.message = $"Network error: {ex.Message}";
            }
            catch (TaskCanceledException ex)
            {
                apiResponse.isSuccess = false;
                apiResponse.message = $"Request timeout: {ex.Message}";
            }
            catch (Exception ex)
            {
                apiResponse.isSuccess = false;
                apiResponse.message = $"Unexpected error: {ex.Message}";
            }

            return apiResponse;
        }
    }
}
