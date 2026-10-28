using AutoMapper;
//using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.EMMA;
using Fingers10.ExcelExport.ActionResults;
using JobAppHR.Models;
using JobAppHR.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting.Internal;
using NuGet.Packaging.Signing;
using System.Data;
using System.Reflection;

namespace JobAppHR.Controllers
{
    [Authorize(Policy = "NormalUserPolicy")]
    public class IntakeApplicant : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private readonly IManualProcess _ManualProcess;
        private readonly IFilterProcess _FilterProcess;
        private readonly IWebHostEnvironment _hostEnvironment;
        private string loginMsg = "";

        public IntakeApplicant(IDBOperations dbOperations, IUtilityFn utilityFn, IWebHostEnvironment hostEnvironment, IManualProcess manualProcess, IFilterProcess filterProcess)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            _hostEnvironment = hostEnvironment;
            _ManualProcess = manualProcess;
            _FilterProcess = filterProcess;
            loginMsg = "Your session is expired. Please re-login.";          
        }


        // GET: IntakeApplication
        public ActionResult Index()
        {
            List<string> IntakeCodeList = new List<string>();
            List<string> TPCodeList = new List<string>();

            //dataList = _DBOperations.AnyDataList("INTAKE", "INTAKECODE", "INTAKECODE", "", "INTAKEID DESC");
            //ViewBag.IntakeCode = new SelectList(dataList, "Value", "Text");

            DataTable dataTable = _DBOperations.SelectRows("INTAKE", "IntakeCode", "", "", "");

            string intakeCode = "";

            foreach (DataRow dr in dataTable.Rows)
            {
                intakeCode = dr["IntakeCode"].ToString();

                if (intakeCode.EndsWith("/TP"))
                {
                    TPCodeList.Add(intakeCode);
                }
                else
                {
                    IntakeCodeList.Add(intakeCode);
                }
            }

            ViewBag.IntakeCode = IntakeCodeList;
            ViewBag.TPCode = TPCodeList;

            return View();
        }

        // GET: IntakeApplication/Details/5
        [HttpPost]
        public ActionResult ViewApplicantList(string intakeCode, bool showAll = false)
        {
            List<ApplicantViewModel> list = new List<ApplicantViewModel>();
            if (intakeCode != null) {
                string fieldList = "IntakeCode,ApplicationCode,Concat(Initials,' ',Surname, ' ') as NameWithInitials,FullName,NIC,Overage,concat(AgeYears,'Y ',AgeMonths, 'M ', AgeDays, 'D') as Age";
                
                string whereClause = "IntakeCode = '" + intakeCode + "'";
                if (!showAll)
                {
                    whereClause += " AND SaveStatus = 'OK'";
                }
                string sql = "SELECT " + fieldList + " FROM Application WHERE " + whereClause;

                
                //DataTable tmpTable = _DBOperations.SelectRows("Application", fieldList, "IntakeCode", intakeCode, "");
                DataTable tmpTable = _DBOperations.SelectRows(sql);

                list = _UtilityFn.ConvertToList<ApplicantViewModel>(tmpTable);
            }

            ViewBag.IntakeCode = intakeCode;
            ViewBag.ShowAll = showAll;
            return PartialView("_ApplicantListPartial", list);
        }

        public ActionResult ViewApplicantData(string applicationCode)
        {
            ApplicationData applicationData;
            applicationData = new ApplicationData();
            applicationData.DocumentNames = new DocumentName();

            //personal data
            string fieldList = "*";
            DataTable tmpTable = _DBOperations.SelectRows("Application", fieldList, "ApplicationCode", applicationCode, "");

            List<PersonalData> list = _UtilityFn.ConvertToList<PersonalData>(tmpTable);

            applicationData.PersonalData = list[0];

            //O/L exam results

            //----------O/L exam headers-------------
            DataTable mainTable = _DBOperations.SelectRows("SEExam", fieldList, "ApplicationCode", applicationCode, "");

            //1st attempt
            var rows =  mainTable.AsEnumerable().Where(r => r.Field<string>("ExamCode") == "O/L" && r.Field<Int16>("Attempt") == 1);

            if (rows.Any())
            {
                tmpTable = rows.CopyToDataTable();

                List<SEExam> listexam = _UtilityFn.ConvertToList<SEExam>(tmpTable);
                if (listexam.Count > 0)
                    applicationData.OLExam1 = listexam[0];
                applicationData.DocumentNames.OLExam_Attempt1 = tmpTable.Rows[0]["AttachmentName"].ToString();
            }

            //2nd attempt
            rows = mainTable.AsEnumerable().Where(r => r.Field<string>("ExamCode") == "O/L" && r.Field<Int16>("Attempt") == 2);
            if (rows.Any())
            {
                tmpTable = rows.CopyToDataTable();
                List<SEExam> listexam = _UtilityFn.ConvertToList<SEExam>(tmpTable);
                if (listexam.Count > 0)
                    applicationData.OLExam2 = listexam[0];
                applicationData.DocumentNames.OLExam_Attempt2 = tmpTable.Rows[0]["AttachmentName"].ToString();
            }

            //3rd attempt
            rows = mainTable.AsEnumerable().Where(r => r.Field<string>("ExamCode") == "O/L" && r.Field<Int16>("Attempt") == 3);
            if (rows.Any())
            {
                tmpTable = rows.CopyToDataTable();
                List<SEExam> listexam = _UtilityFn.ConvertToList<SEExam>(tmpTable);
                if (listexam.Count > 0)
                    applicationData.OLExam3 = listexam[0];
                applicationData.DocumentNames.OLExam_Attempt3 = tmpTable.Rows[0]["AttachmentName"].ToString();
            }

            //----------A/L exam headers-------------
            rows = mainTable.AsEnumerable().Where(r => r.Field<string>("ExamCode") == "A/L" && r.Field<Int16>("Attempt") == 1);
            if (rows.Any())
            {
                tmpTable = rows.CopyToDataTable();
                List<SEExam> listexam = _UtilityFn.ConvertToList<SEExam>(tmpTable);
                if (listexam.Count > 0)
                    applicationData.ALExam = listexam[0];
                applicationData.DocumentNames.ALExam_Attempt1 = tmpTable.Rows[0]["AttachmentName"].ToString();
            }

            //----------O/L exam results-----------------
            mainTable = _DBOperations.SelectRows("SEResult", fieldList, "ApplicationCode", applicationCode, "");
            
            //1st attempt
            rows = mainTable.AsEnumerable().Where(r => r.Field<string>("ExamCode") == "O/L" && r.Field<Int16>("Attempt") == 1);

            if (rows.Any())
            {
                tmpTable = rows.CopyToDataTable();
                tmpTable = MarkMandatorySubjects(tmpTable);
                List<SEResult> listresult = _UtilityFn.ConvertToList<SEResult>(tmpTable);
                if (listresult.Count > 0)
                    applicationData.OLResults1 = listresult;
            }

            //2nd attempt
            rows = mainTable.AsEnumerable().Where(r => r.Field<string>("ExamCode") == "O/L" && r.Field<Int16>("Attempt") == 2);
            if (rows.Any())
            {
                tmpTable = rows.CopyToDataTable();
                List<SEResult> listresult = _UtilityFn.ConvertToList<SEResult>(tmpTable);
                if (listresult.Count > 0)
                    applicationData.OLResults2 = listresult;
            }

            //3rd attempt
            rows = mainTable.AsEnumerable().Where(r => r.Field<string>("ExamCode") == "O/L" && r.Field<Int16>("Attempt") == 3);
            if (rows.Any())
            {
                tmpTable = rows.CopyToDataTable();
                List<SEResult> listresult = _UtilityFn.ConvertToList<SEResult>(tmpTable);
                if (listresult.Count > 0)
                    applicationData.OLResults3 = listresult;
            }

            //----------A/L exam results-----------------
            rows = mainTable.AsEnumerable().Where(r => r.Field<string>("ExamCode") == "A/L" && r.Field<Int16>("Attempt") == 1);
            if (rows.Any())
            {
                tmpTable = rows.CopyToDataTable();
                List<SEResult> listresult = _UtilityFn.ConvertToList<SEResult>(tmpTable);
                if (listresult.Count > 0)
                    applicationData.ALResults = listresult;
            }

            //---------Higher Education--------------------
            tmpTable = _DBOperations.SelectRows("HEQualification", fieldList, "ApplicationCode", applicationCode, "");
            int rowCount = tmpTable.Rows.Count;
            if (rowCount > 0)
            {
                List<HEQualification> listqual = _UtilityFn.ConvertToList<HEQualification>(tmpTable);
                
                applicationData.HEQualifications = listqual;

                applicationData.DocumentNames.HEQualification = new string[rowCount];

                for (int i = 0; i < rowCount; i++)
                {
                    applicationData.DocumentNames.HEQualification[i] = tmpTable.Rows[i]["AttachmentName"].ToString();
                }
            }

            //---------Prof. Qualifications--------------------
            tmpTable = _DBOperations.SelectRows("ProfQualification", fieldList, "ApplicationCode", applicationCode, "");
            rowCount = tmpTable.Rows.Count;
            if (rowCount > 0)
            {
                List<ProfQualification> listqual = _UtilityFn.ConvertToList<ProfQualification>(tmpTable);
                
                applicationData.ProfQualifications = listqual;

                applicationData.DocumentNames.ProfQualification = new string[rowCount];

                for (int i = 0; i < rowCount; i++)
                {
                    applicationData.DocumentNames.ProfQualification[i] = tmpTable.Rows[i]["AttachmentName"].ToString();
                }
            }

            //---------Work Experience--------------------
            tmpTable = _DBOperations.SelectRows("WorkExperience", fieldList, "ApplicationCode", applicationCode, "");
            rowCount = tmpTable.Rows.Count;
            if (rowCount > 0)
            {
                List<WorkExperience> listqual = _UtilityFn.ConvertToList<WorkExperience>(tmpTable);

                applicationData.WorkExperiences = listqual;

                applicationData.DocumentNames.ServiceLetter = new string[rowCount];

                for (int i = 0; i < rowCount; i++)
                {
                    applicationData.DocumentNames.ServiceLetter[i] = tmpTable.Rows[i]["AttachmentName"].ToString();
                }
            }

            //---------Other Documents--------------------
            tmpTable = _DBOperations.SelectRows("OtherDocument", fieldList, "ApplicationCode", applicationCode, "");
            rowCount = tmpTable.Rows.Count;
            if (rowCount > 0)
            {
                DataRow dr = tmpTable.Rows[0];
                foreach(DataColumn col in tmpTable.Columns)
                {
                    Type t = typeof(DocumentName);
                    PropertyInfo prop = t.GetProperty(col.ColumnName);
                    if (prop != null)
                        prop.SetValue(applicationData.DocumentNames, dr[col.ColumnName].ToString());                    
                }
            }

            //---------get filter progress if available
            List<FilterProgress> progressList = _FilterProcess.ShowProgress(applicationCode);
            applicationData.FilterProgress = progressList;

            return View(applicationData);
        }

        public FileResult? ViewDocument(string documentName, string applicationCode)
        {
            string folderName = applicationCode.Replace('/', '_');
            string path = Path.Combine(StaticData.UploadPath, folderName);
            string docType = "";

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists)
            {
                FileInfo[] fileInfo = directoryInfo.GetFiles(documentName + ".*");

                if (fileInfo.Length > 0 && fileInfo[0] != null)
                {
                    string fileExtension = Path.GetExtension(fileInfo[0].FullName);
                    fileExtension = fileExtension.ToLower();

                    if (fileExtension.EndsWith(".pdf"))
                    {
                        docType = "application/pdf";
                    }
                    else if (fileExtension.EndsWith(".jpg") || fileExtension.EndsWith(".jpeg"))
                    {
                        docType = "image/jpeg";
                    }
                    else if (fileExtension.EndsWith(".png"))
                    {
                        docType = "image/png";
                    }
                    else
                    {
                        docType = "application/octet-stream";
                    }

                    path = Path.Combine(path, fileInfo[0].Name);
                    byte[] bytes;
                    using (var stream = new MemoryStream())
                    {
                        bytes = System.IO.File.ReadAllBytes(path);
                    }

                    return File(bytes, docType);
                }
                else
                    return null;
            }
            else
                return null;
        }

        public IActionResult ExportToExcel(string intakeCode, string currentStage, int? freezeNo = 0, bool showAll = false, string currentStatus = "PASS")
        {
            List<FullReportModel> list = new();
            string fileName = intakeCode + "_Applicants_" + (string.IsNullOrEmpty(currentStage) ? "All" : currentStage);
            string reportTitle = (string.IsNullOrEmpty(currentStage) ? "All" : currentStage) + "_Applicants";

            if (string.IsNullOrEmpty(currentStage))
                currentStage = "";

            if (!string.IsNullOrWhiteSpace(intakeCode))
            {
                //var tempList = _ManualProcess.FilterByAll(intakeCode, currentStage);
                list = _ManualProcess.GetFullReportData(intakeCode, currentStage, freezeNo, showAll, currentStatus);

                //var config = new MapperConfiguration(cfg =>
                //    cfg.CreateMap<ManualFilter, FullReportModel>()
                //);
                //var mapper = new Mapper(config);

                //foreach (var tempItem in tempList)
                //{
                //    var item = mapper.Map<ManualFilter, FullReportModel>(tempItem);
                //    list.Add(item);
                //}
            }

            //refer https://github.com/fingers10/ExcelExport
            //[IncludeInReport] data annotation in the data model is required if that particular field to appear in excel
            return new ExcelResult<FullReportModel>(list, reportTitle, fileName);
        }

        private DataTable MarkMandatorySubjects(DataTable dataTable)
        {
            DataTable subjectTable = _DBOperations.SelectRows("Subject", "*", "Mandatory", "YES", "");
            string subjectName = "";
            string examCode = "";

            foreach(DataRow row in dataTable.Rows)
            {
                subjectName = row["SubjectName"].ToString();
                examCode = row["ExamCode"].ToString();

                DataRow[] drs = subjectTable.Select("SubjectName = '" + subjectName + "'");

                if (drs.Length > 0)
                {
                    row["SubjectName"] = subjectName + "*";
                }
            }

            return dataTable;
        }


        [HttpGet]
        public JsonResult GetJobPositionName(string intakeCode)
        {
            var jobPositionName = _DBOperations.GetJobPositionName(intakeCode);
            return Json(new { jobPositionName });
        }


    }
}
