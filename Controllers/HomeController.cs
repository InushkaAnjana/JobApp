using DocumentFormat.OpenXml.Spreadsheet;
using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Data;

namespace JobAppHR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IDBOperations dbOperations, IUtilityFn utilityFn, IConfiguration configuration)
        {
            _logger = logger;
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            _configuration = configuration;
        }

        [Authorize(Policy = "NormalUserPolicy")]
        public IActionResult Index()
        {
            ViewBag.Message = StaticData.BaseUrl;
            return View("Home");
        }

        [Authorize(Policy = "NormalUserPolicy")]
        public IActionResult Settings()
        {
            ViewData["Title"] = "Appearance Settings";
            return View();
        }

        public RedirectResult AzureLogin()
        {
            string tenantID = _configuration["AzureAd:TenantId"] ?? "534253fc-dfb6-462f-b5ca-cbe81939f5ee";
            string redirectUri = StaticData.BaseUrl + "/Home/UAzure";
            string clientID = _configuration["AzureAd:ClientId"] ?? "57ab62d2-0f80-4eb9-9015-0c5b15a23330";
            string scope = "openid profile offline_access user.read";
            string responseMode = "query";

            string redirectUrl = "https://login.microsoftonline.com/" + tenantID + "/oauth2/v2.0/authorize?response_type=code&client_id=" + clientID + "&redirect_uri=" + redirectUri + "&scope=" + scope + "&response_mode=" + responseMode + "&state=987qaz";
            return Redirect(redirectUrl);
        }

        //public async Task<ActionResult> UAzure(AuthModel urlDetails)
        public async Task<ActionResult> UAzure(string code)
        {
            //string code = urlDetails.code;
            //string code = "";
            try
            {
                string clientId = _configuration["AzureAd:ClientId"] ?? "57ab62d2-0f80-4eb9-9015-0c5b15a23330";
                string clientSecret = _configuration["AzureAd:ClientSecret"] ?? "";
                string tenantId = _configuration["AzureAd:TenantId"] ?? "534253fc-dfb6-462f-b5ca-cbe81939f5ee";
                string redirectUri = StaticData.BaseUrl + "/Home/UAzure"; //"https://localhost:7161/Home/UAzure";
                string baseUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(baseUrl);

                // Construct the request parameters
                var postData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("code", code), // Assuming the 'Code' property contains the authorization code
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                };

                // Request access token
                HttpResponseMessage tokenResponse = await httpClient.PostAsync("", new FormUrlEncodedContent(postData));

                if (tokenResponse.IsSuccessStatusCode)
                {
                    // Read and parse token response
                    string tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
                    dynamic tokenJson = JObject.Parse(tokenResponseContent);
                    string accessToken = tokenJson.access_token;

                    // Use access token to retrieve user details
                    string graphApiEndpoint = "https://graph.microsoft.com/v1.0/me";
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    HttpResponseMessage userResponse = await httpClient.GetAsync(graphApiEndpoint);

                    if (userResponse.IsSuccessStatusCode)
                    {
                        // Read and parse user details
                        string userResponseContent = await userResponse.Content.ReadAsStringAsync();
                        dynamic userJson = JObject.Parse(userResponseContent);

                        // Access user details here
                        string userEmail = userJson.mail;
                        string userName = userJson.displayName;
                        string userId = userJson.userPrincipalName;
                        string userGroup = "";
                        string userRole = "";

                        userId = userId.Substring(0, 6);
                        HttpContext.Session.SetString("UserName", userName);
                        HttpContext.Session.SetString("UserId", userId);
                        
                        //get the user group
                        string sql = "SELECT UserGroup, UserEmail FROM Users WHERE UserId = '" + userId + "' AND ActiveStatus = 'ACTIVE'";
                        DataTable dataTable = _DBOperations.SelectRows(sql);

                        if (dataTable.Rows.Count > 0)
                        {
                            userGroup = dataTable.Rows[0]["UserGroup"].ToString();

                            if (userGroup == "0" || userGroup == "1")
                            {
                                userRole = "Admin";
                            }
                            else
                            {
                                userRole = "Normal";
                            }
                        }

                        var claims = new List<Claim>
                        {
                            new Claim("UserRole", userRole),
                            new Claim("UserId", userId),
                            new Claim("UserName", userName),
                            new Claim("UserEmail", userEmail),
                            new Claim("UserGroup", userGroup),
                        };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity));

                        return View("Home");
                    }
                    else
                    {
                        // Handle user details retrieval failure
                        // Log the error or return an appropriate response
                        return StatusCode((int)userResponse.StatusCode);
                    }
                }
                else
                {
                    // Handle access token retrieval failure
                    // Log the error or return an appropriate response
                    return StatusCode((int)tokenResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {

                // Handle any exceptions
                // Log the exception or return an appropriate response
                return StatusCode(500);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Dev login page — only available when EnableDevUserFallback is true.
        /// Allows testers to login as different users to test multi-user collaborative screening.
        /// </summary>
        [AllowAnonymous]
        public IActionResult DevLogin(string? returnUrl)
        {
            // Only allow dev login when fallback is enabled
            var enableDevFallback = _configuration.GetValue<bool>("Authentication:EnableDevUserFallback");

            if (!enableDevFallback)
                return RedirectToAction("AzureLogin");

            // Get list of active users from DB
            string sql = "SELECT UserId, UserGroup FROM Users WHERE ActiveStatus = 'ACTIVE' ORDER BY UserId";
            DataTable userTable = _DBOperations.SelectRows(sql);
            ViewBag.UserList = userTable;
            ViewBag.ReturnUrl = returnUrl ?? "/";
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> DevLogin(string userId, string? returnUrl)
        {
            var enableDevFallback = _configuration.GetValue<bool>("Authentication:EnableDevUserFallback");

            if (!enableDevFallback)
                return RedirectToAction("AzureLogin");

            // Look up user in DB
            string sql = "SELECT UserId, UserGroup FROM Users WHERE UserId = '" + userId + "' AND ActiveStatus = 'ACTIVE'";
            DataTable dataTable = _DBOperations.SelectRows(sql);

            if (dataTable.Rows.Count == 0)
            {
                ViewBag.Error = "User not found or inactive.";
                return DevLogin(returnUrl);
            }

            string userGroup = dataTable.Rows[0]["UserGroup"].ToString() ?? "";
            string userRole = (userGroup == "0" || userGroup == "1") ? "Admin" : "Normal";
            string userName = "Dev-" + userId; // Display name for dev

            var claims = new List<Claim>
            {
                new Claim("UserRole", userRole),
                new Claim("UserId", userId),
                new Claim("UserName", userName),
                new Claim("UserEmail", userId + "@dev.local"),
                new Claim("UserGroup", userGroup),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // Store in session too
            HttpContext.Session.SetString("UserId", userId);
            HttpContext.Session.SetString("UserName", userName);
            HttpContext.Session.SetString("UserEmail", userId + "@dev.local");
            HttpContext.Session.SetString("UserGroup", userGroup);
            HttpContext.Session.SetString("UserRole", userRole);

            return Redirect(returnUrl ?? "/");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            // Clear the authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Clear the session
            HttpContext.Session.Clear();

            // Redirect back to home (which will force a login)
            return RedirectToAction("Index");
        }
    }
}