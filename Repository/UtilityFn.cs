using AutoMapper;
using Fingers10.ExcelExport.ActionResults;
using JobAppHR.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;
using System.Security.Claims;

namespace JobAppHR.Repository
{
    public class UtilityFn : IUtilityFn
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISession _session;

        public UtilityFn(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _session = _httpContextAccessor.HttpContext.Session;
        }

        public List<T> ConvertToList<T>(DataTable dataTable)
        {
            var columnNames = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
            var properties = typeof(T).GetProperties();
            return dataTable.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        try
                        {
                            if (pro.PropertyType.FullName == "System.Int32")
                                pro.SetValue(objT, Convert.ToInt32(row[pro.Name]));
                            else
                                pro.SetValue(objT, row[pro.Name]);
                        }
                        catch (Exception ex) { }
                    }
                }
                return objT;
            }).ToList();
        }

        public SelectList ConvertToSelectList(DataTable table, string valueField, string textField)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            foreach (DataRow row in table.Rows)
            {
                list.Add(new SelectListItem()
                {
                    Text = row[textField].ToString(),
                    Value = row[valueField].ToString()
                });
            }

            return new SelectList(list, "Value", "Text");
        }

        public DataTable ConvertToDataTable<T>(IEnumerable<T> self)
        {
            var properties = typeof(T).GetProperties();

            var dataTable = new DataTable();
            foreach (var info in properties)
                dataTable.Columns.Add(info.Name, Nullable.GetUnderlyingType(info.PropertyType)
                   ?? info.PropertyType);

            foreach (var entity in self)
                dataTable.Rows.Add(properties.Select(p => p.GetValue(entity)).ToArray());

            return dataTable;
        }

        public string ConvertToTitleCase(string text)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        }

        public User GetCurrentUser()
        {
            User user = new();

            var httpContext = _httpContextAccessor.HttpContext;
            var claims = httpContext?.User?.Claims?.ToList() ?? new List<Claim>();

            string ReadClaimOrSession(string claimType, string sessionKey)
            {
                var claimValue = claims.FirstOrDefault(c => c.Type == claimType)?.Value;
                if (!string.IsNullOrEmpty(claimValue))
                    return claimValue;

                var sessionValue = _session.GetString(sessionKey);
                return sessionValue ?? string.Empty;
            }

            user.UserId = ReadClaimOrSession("UserId", "UserId");
            user.UserName = ReadClaimOrSession("UserName", "UserName");
            user.UserEmail = ReadClaimOrSession("UserEmail", "UserEmail");
            user.UserGroup = ReadClaimOrSession("UserGroup", "UserGroup");
            user.UserRole = ReadClaimOrSession("UserRole", "UserRole");

            if (string.IsNullOrEmpty(user.UserId))
                throw new InvalidOperationException("User context is missing. Please sign in again.");

            _session.SetString("UserId", user.UserId);
            _session.SetString("UserName", user.UserName);

            return user;
        }

        public string ConvertToCSV(DataTable dataTable, string fileName)
        {
            string msg = "SUCCESS";
            string filePath = Path.Combine(StaticData.UploadPath, fileName);
            StreamWriter sw = new StreamWriter(filePath, false);
            //headers
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                sw.Write(dataTable.Columns[i]);
                if (i < dataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dataTable.Rows)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();

                        if (value.Contains(",") || value.Contains("\""))
                        {
                            value = '"' + value.Replace("\"", "\"\"") + '"';
                            sw.Write(value);
                        }
                        else
                        {
                            //sw.Write(dr[i].ToString());
                            sw.Write(value);
                        }
                    }
                    if (i < dataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();

            return msg;
        }

        public DataTable ConvertJsonToDataTable(string jsonString)
        {
            DataTable dataTable = new DataTable();

            try
            {
                // Parse the JSON string into a JArray
                JArray jsonArray = JArray.Parse(jsonString);

                // Loop through each item in the array
                foreach (JObject jsonObject in jsonArray.Children<JObject>())
                {
                    // Create a DataRow
                    DataRow dataRow = dataTable.NewRow();

                    // Loop through each property in the JSON object
                    foreach (JProperty property in jsonObject.Properties())
                    {
                        // Add columns to the DataTable if they don't exist
                        if (!dataTable.Columns.Contains(property.Name))
                        {
                            dataTable.Columns.Add(property.Name);
                        }

                        // Set the value for the DataRow
                        dataRow[property.Name] = property.Value;
                    }

                    // Add the DataRow to the DataTable
                    dataTable.Rows.Add(dataRow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error converting JSON to DataTable: " + ex.Message);
            }

            return dataTable;
        }

        public DataTable GetEligibleApplicationCodes(string jsonString)
        {
            //List<string> applicationCodes = new List<string>();
            
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("ApplicationCode");

            try
            {
                // Parse the JSON string into a JToken
                JToken jsonToken = JToken.Parse(jsonString);

                // Check if the JSON is an array or an object
                if (jsonToken is JArray jsonArray)
                {
                    // Loop through each item in the array
                    foreach (JObject jsonObject in jsonArray.Children<JObject>())
                    {
                        // Extract the ApplicationCode and add it to the list
                        string applicationCode = jsonObject["ApplicationCode"]?.ToString();
                        if (!string.IsNullOrEmpty(applicationCode))
                        {
                            //applicationCodes.Add(applicationCode);
                            dataTable.Rows.Add(applicationCode);
                        }
                    }
                }
                //else if (jsonToken is JObject jsonObject)
                //{
                //    // Extract the ApplicationCode and add it to the list
                //    string applicationCode = jsonObject["ApplicationCode"]?.ToString();
                //    if (!string.IsNullOrEmpty(applicationCode))
                //    {
                //        //applicationCodes.Add(applicationCode);
                //        dataTable.Rows.Add(applicationCode);
                //    }
                //}
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine("Error parsing JSON: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            //return applicationCodes;
            return dataTable;
        }
    }
}
