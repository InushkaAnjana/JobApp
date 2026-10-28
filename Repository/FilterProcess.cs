using DocumentFormat.OpenXml.Spreadsheet;
using JobAppHR.Models;
using JobAppHR.Services;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Data;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;

[assembly: InternalsVisibleTo("JobAppHR.Tests")]

namespace JobAppHR.Repository
{
    public class FilterProcess : IFilterProcess
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private readonly IFastAPIProcess _fastAPIProcess;
        private readonly string fileNameAppData = "AppData.csv";
        private readonly string fileNameResultData = "ResultData.csv";
        public FilterProcess(IDBOperations dbOperations, IUtilityFn utilityFn, IFastAPIProcess fastAPIProcess)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            _fastAPIProcess = fastAPIProcess;
        }

        public DataTable FilterSummary(string intakeCode)
        {
            DataTable temptbl = new DataTable();
            DataTable finaltbl = new DataTable();
            string sql = "";
            int stage = 0;
            string updatedDateField = "";
            string[] stages = new string[] { "NOT-PROCESSED", "FILTERED-BY-AGE", "FILTERED-BY-A/L", "FILTERED-BY-O/L", "FILTERED-BY-HIGHER.EDU.& PROF.QUAL." };

            finaltbl.Columns.Add("IntakeCode");
            finaltbl.Columns.Add("CurrentStage");
            finaltbl.Columns.Add("StageName");
            //finaltbl.Columns.Add("StageCount", typeof(Int32));
            finaltbl.Columns.Add("StageCount");
            finaltbl.Columns.Add("LastProcessedDate", typeof(DateTime));

            //get not processed rows from the application table
            stage = 0;
            sql = "SELECT Count(*) FROM Application WHERE (IntakeCode = '" + intakeCode + "') AND (Processed IS NULL OR Processed = '') AND (SaveStatus = 'OK') GROUP BY IntakeCode";
            temptbl = _DBOperations.SelectRows(sql);

            if (temptbl.Rows.Count > 0)
            {
                DataRow dr = finaltbl.NewRow();
                dr[0] = intakeCode;
                dr[1] = stage.ToString();
                dr[2] = stages[stage];
                dr[3] = temptbl.Rows[0][0].ToString();
                dr[4] = DateTime.Today.ToShortDateString();

                finaltbl.Rows.Add(dr);
            }

            //get stage 1 (age) passed records from FilteredData Table 
            stage = 1;
            updatedDateField = "Max(Stage1UpdatedOn)";
            sql = "SELECT Count(*) As subcount," + updatedDateField + " FROM FilteredData WHERE (IntakeCode = '" + intakeCode + "') AND (CurrentStage = '" + stage + "' AND CurrentStatus = 'PASS') AND (FreezeNo IS NULL) GROUP BY IntakeCode";
            temptbl = _DBOperations.SelectRows(sql);

            if (temptbl.Rows.Count > 0)
            {
                DataRow dr = finaltbl.NewRow();
                dr[0] = intakeCode;
                dr[1] = stage.ToString();
                dr[2] = stages[stage];
                dr[3] = temptbl.Rows[0][0].ToString();
                dr[4] = temptbl.Rows[0][1] != DBNull.Value ? Convert.ToDateTime(temptbl.Rows[0][1]) : DBNull.Value;

                finaltbl.Rows.Add(dr);
            }

            //get stage 2 (A/L) passed records from FilteredData Table 
            stage = 2;
            updatedDateField = "Max(Stage2UpdatedOn)";
            sql = "SELECT Count(*) As subcount," + updatedDateField + " FROM FilteredData WHERE (IntakeCode = '" + intakeCode + "') AND (CurrentStage = '" + stage + "' AND CurrentStatus = 'PASS') AND (FreezeNo IS NULL) GROUP BY IntakeCode";
            temptbl = _DBOperations.SelectRows(sql);

            if (temptbl.Rows.Count > 0)
            {
                DataRow dr = finaltbl.NewRow();
                dr[0] = intakeCode;
                dr[1] = stage.ToString();
                dr[2] = stages[stage];
                dr[3] = temptbl.Rows[0][0].ToString();
                dr[4] = temptbl.Rows[0][1] != DBNull.Value ? Convert.ToDateTime(temptbl.Rows[0][1]) : DBNull.Value;

                finaltbl.Rows.Add(dr);
            }

            //get stage 3 (O/L) passed records from FilteredData Table 
            stage = 3;
            updatedDateField = "Max(Stage3UpdatedOn)";
            sql = "SELECT Count(*) As subcount," + updatedDateField + " FROM FilteredData WHERE (IntakeCode = '" + intakeCode + "') AND (CurrentStage = '" + stage + "' AND CurrentStatus = 'PASS') AND (FreezeNo IS NULL) GROUP BY IntakeCode";
            temptbl = _DBOperations.SelectRows(sql);

            if (temptbl.Rows.Count > 0)
            {
                DataRow dr = finaltbl.NewRow();
                dr[0] = intakeCode;
                dr[1] = stage.ToString();
                dr[2] = stages[stage];
                dr[3] = temptbl.Rows[0][0].ToString();
                dr[4] = temptbl.Rows[0][1] != DBNull.Value ? Convert.ToDateTime(temptbl.Rows[0][1]) : DBNull.Value;

                finaltbl.Rows.Add(dr);
            }

            //get stage 4 (Higher/Prof Qual) passed records from FilteredData Table 
            stage = 4;
            updatedDateField = "Max(Stage4UpdatedOn)";
            //sql = "SELECT Count(*) As subcount," + updatedDateField + " FROM FilteredData WHERE (IntakeCode = '" + intakeCode + "') AND (CurrentStage = '" + stage + "') AND (CurrentStatus = 'PASS' OR CurrentStatus = 'TO-CHECK') GROUP BY IntakeCode";
            sql = "SELECT SUM(CASE WHEN CurrentStatus = 'TO-CHECK' THEN 1 ELSE 0 END) AS ToCheck_Count,SUM(CASE WHEN CurrentStatus = 'PASS' THEN 1 ELSE 0 END) AS Pass_Count," + updatedDateField + " FROM FilteredData WHERE (IntakeCode = '" + intakeCode + "') AND (CurrentStage = '" + stage + "') AND (FreezeNo IS NULL)";
            temptbl = _DBOperations.SelectRows(sql);

            if (temptbl.Rows.Count > 0 && temptbl.Rows[0][0] != DBNull.Value)
            {
                DataRow dr = finaltbl.NewRow();
                dr[0] = intakeCode;
                dr[1] = stage.ToString();
                dr[2] = stages[stage];
                dr[3] = temptbl.Rows[0][0].ToString() + " | " + temptbl.Rows[0][1].ToString();
                dr[4] = temptbl.Rows[0][2] != DBNull.Value ? Convert.ToDateTime(temptbl.Rows[0][2]) : DBNull.Value;

                finaltbl.Rows.Add(dr);
            }

            //get stage FINAL passed records from FilteredData Table
            updatedDateField = "Max(FinalUpdatedOn)";
            sql = "SELECT Count(*) As subcount, " + updatedDateField + " FROM FilteredData WHERE (IntakeCode = '" + intakeCode + "') AND (CurrentStage = 'FINAL' AND CurrentStatus = 'PASS') AND (FreezeNo IS NULL) GROUP BY IntakeCode";
            temptbl = _DBOperations.SelectRows(sql);

            if (temptbl.Rows.Count > 0)
            {
                DataRow dr = finaltbl.NewRow();
                dr[0] = intakeCode;
                dr[1] = "FINAL";
                dr[2] = "FINAL";
                dr[3] = temptbl.Rows[0][0].ToString();
                dr[4] = temptbl.Rows[0][1] != DBNull.Value ? Convert.ToDateTime(temptbl.Rows[0][1]) : DBNull.Value;

                finaltbl.Rows.Add(dr);
            }

            //get FAIL records at any stage from FilteredData Table
            updatedDateField = "Max(FinalUpdatedOn)";
            sql = "SELECT Count(*) As subcount," + updatedDateField + " FROM FilteredData WHERE (IntakeCode = '" + intakeCode + "') AND (CurrentStatus = 'FAIL') AND (FreezeNo IS NULL) GROUP BY IntakeCode";
            temptbl = _DBOperations.SelectRows(sql);

            if (temptbl.Rows.Count > 0)
            {
                DataRow dr = finaltbl.NewRow();
                dr[0] = intakeCode;
                dr[1] = "FAIL";
                dr[2] = "FAIL";
                dr[3] = temptbl.Rows[0][0].ToString();
                dr[4] = temptbl.Rows[0][1] != DBNull.Value ? Convert.ToDateTime(temptbl.Rows[0][1]) : DBNull.Value;

                finaltbl.Rows.Add(dr);
            }

            return finaltbl;
        }

        public List<AgeFilter> FilterByAge(string intakeCode)
        {
            DataTable tmpTable = new();

            List<AgeFilter> list = new();

            if (intakeCode != null)
            {
                string fieldList = "ApplicationCode, Concat(Initials,' ',Surname, ' ') as NameWithInitials, NIC, DOB, Concat(AgeYears,'Y ',AgeMonths, 'M ', AgeDays, 'D') as Age";
                string whereClause = "(IntakeCode = '" + intakeCode + "') AND (Overage = '') AND (Processed IS NULL OR Processed = '') AND (SaveStatus = 'OK')";
                string orderBy = "CAST(SUBSTRING(ApplicationCode, CHARINDEX('/', ApplicationCode, CHARINDEX('/', ApplicationCode) + 1) + 1, LEN(ApplicationCode)) AS INT)";
                tmpTable = _DBOperations.SelectRows("Application", fieldList, "", "", whereClause, orderBy);

                list = _UtilityFn.ConvertToList<AgeFilter>(tmpTable);
            }

            return list;
        }

        public List<ALFilter> FilterByAL_Org(string intakeCode, string currentStage)
        {
            //1.get the application codes who are PASS in the current stage
            //2.next check A/L results of those application codes 

            DataTable temptbl, scoretbl, maintbl, resulttbl;
            List<ALFilter> list = new();

            //get current stage passed applicants of the selected intakecode
            string sql = "SELECT ApplicationCode FROM FilteredData WHERE CurrentStage = '" + currentStage + "' AND CurrentStatus = 'PASS' AND IntakeCode = '" + intakeCode + "'";
            temptbl = _DBOperations.SelectRows(sql);

            //get A/L score of all the applicants in the selected intakecode
            sql = "SELECT B.ApplicationCode, B.ExamYear, B.ExamCode, Count(G.Rating) AS Score FROM SEResult A " +
                  "INNER JOIN SEExam B ON A.ApplicationCode = B.ApplicationCode " +
                  "INNER JOIN Application C ON B.ApplicationCode = C.ApplicationCode " +
                  "INNER JOIN Grade G ON A.Grade = G.GradeValue " +
                  "WHERE C.IntakeCode = '" + intakeCode + "' AND A.ExamCode = 'A/L' AND B.ExamCode = 'A/L' AND G.Rating > 0 " +
                  "GROUP BY B.ApplicationCode, B.ExamYear, B.ExamCode";
            scoretbl = _DBOperations.SelectRows(sql);

            //get all the applicant's name of the selected intakecode
            string fieldList = "ApplicationCode, Concat(Initials,' ',Surname, ' ') as NameWithInitials";
            string whereClause = "(IntakeCode = '" + intakeCode + "')";
            maintbl = _DBOperations.SelectRows("Application", fieldList, "", "", whereClause, "");

            //get grades of all the applicants of the selected intakecode
            sql = "SELECT A.ApplicationCode, A.Grade, Count(A.Grade) AS GradeCount FROM SEResult A " +
                  "INNER JOIN Application B ON A.ApplicationCode = B.ApplicationCode " +
                  "WHERE B.IntakeCode = '" + intakeCode + "' AND A.ExamCode = 'A/L' " +
                  "GROUP BY A.ApplicationCode, A.Grade " + 
                  "ORDER BY A.ApplicationCode";

            resulttbl = _DBOperations.SelectRows(sql);

            string applicationCode = "";
            string grades = "";
            int score = 0;
            DataRow[] drs;

            foreach (DataRow dr in temptbl.Rows)
            {
                applicationCode = dr[0].ToString();
                score = 0;
                grades = "";

                drs = scoretbl.Select("ApplicationCode = '" + applicationCode + "'");

                if (drs.Length > 0)
                {
                    score = Convert.ToInt16(drs[0]["Score"]);
                    if (score >= 3) //score is 3 when if all 3 subjects have a pass (S) or above grade. As we take count of grade rating >= 1 in the above sql
                    {
                        ALFilter item = new();
                        item.ApplicationCode = applicationCode;
                        item.ExamCode = drs[0]["ExamCode"].ToString();
                        item.ExamYear = drs[0]["ExamYear"].ToString();

                        //get applicant name
                        drs = maintbl.Select("ApplicationCode = '" + applicationCode + "'");
                        if (drs.Length > 0)
                        {
                            item.NameWithInitials = drs[0]["NameWithInitials"].ToString();
                        }

                        //get results
                        drs = resulttbl.Select("ApplicationCode = '" + applicationCode + "'");
                        foreach(DataRow drtemp in drs)
                        {
                            grades = grades + drtemp["Grade"].ToString() + drtemp["GradeCount"].ToString() + ",";
                        }

                        item.Grades = grades;

                        list.Add(item);
                    }
                }
            }

            return list;
        }

        public async Task<List<ALFilter>> FilterByAL(string intakeCode, string currentStage)
        {
            // Temporarily bypass FastAPI and use original method only
            return FilterByAL_Org(intakeCode, currentStage);
            
            /*
            try
            {
                return await FilterByAL_FastAPI(intakeCode, currentStage);
            }
            catch (Exception ex)
            {
                // Log the error and fallback to original method
                System.Diagnostics.Debug.WriteLine($"FastAPI A/L Filter failed: {ex.Message}");
                return FilterByAL_Org(intakeCode, currentStage);
            }
            */
        }

        private async Task<List<ALFilter>> FilterByAL_FastAPI(string intakeCode, string currentStage)
        {
            string examcode = "A/L";

            //1.get the application codes who are PASS in the current stage
            //2.next check A/L results of those application codes 

            DataTable temptbl, scoretbl, maintbl, resulttbl, examtbl;
            DataTable appDataTable;
            List<ALFilter> list = new();
            ApiResponse apiResponse;

            //get current stage passed applicants of the selected intakecode
            string sql = "SELECT A.* FROM FilteredData F INNER JOIN Application A ON F.ApplicationCode = A.ApplicationCode WHERE F.CurrentStage = '" + currentStage + "' AND F.CurrentStatus = 'PASS' AND F.IntakeCode = '" + intakeCode + "'";
            appDataTable = _DBOperations.SelectRows(sql);

            //fast API requires the date in yyyy-MM-dd format. but sql returns the DOB in local system date format
            //DOB col in datatable auto formatted as Date type
            //convert the DOB col data type to string and replace a dummy value as Age calc is not needed from the API
            //but laith does not want to change the API to discard DOB
            //so all the hassel done below to avoid that - 21/11/2024
            temptbl = appDataTable.Clone();
            temptbl.Columns["DOB"].DataType = typeof(string);
            
            foreach (DataRow row in appDataTable.Rows)
            {
                temptbl.ImportRow(row);
            }

            foreach (DataRow drtemp in temptbl.Rows)
            {
                drtemp["DOB"] = "2000-01-01"; // Convert.ToDateTime(drtemp["DOB"]).ToString("0:yyyy-MM-dd");
            }
            temptbl.AcceptChanges();
            //------------------------------------------

            _UtilityFn.ConvertToCSV(temptbl, fileNameAppData);

            //get current stage passed applicants results of the selected intakecode
            sql = "SELECT R.* FROM SEResult R INNER JOIN FilteredData F ON R.ApplicationCode = F.ApplicationCode WHERE F.CurrentStage = '" + currentStage + "' AND F.CurrentStatus = 'PASS' AND F.IntakeCode = '" + intakeCode + "'";
            resulttbl = _DBOperations.SelectRows(sql);

            //update all O/L results as 'A' as Fast API checks both A/L and O/L results same time. But at this stage we need only eligible applicants based on A/L only
            resulttbl = UpdateDummyResults(resulttbl, "O/L");
            _UtilityFn.ConvertToCSV(resulttbl, fileNameResultData);

            //delete all the uploaded data
            apiResponse = await _fastAPIProcess.DeleteAllData();

            //upload the new data as csv to FastAPI
            apiResponse = await _fastAPIProcess.UploadApplicationData(fileNameAppData, fileNameResultData);

            //get the eligible applicants from the fastApi
            scoretbl = new DataTable();
            var jobPositionCode = intakeCode.Substring(0,intakeCode.IndexOf('/'));
            apiResponse = await _fastAPIProcess.FilterByPosition(jobPositionCode, intakeCode);
            
            if (apiResponse.result != null)
            {
                scoretbl = _UtilityFn.GetEligibleApplicationCodes(apiResponse.result.ToString());
            }

            //get all the applicant's name of the selected intakecode
            string fieldList = "ApplicationCode, Concat(Initials,' ',Surname,' ') as NameWithInitials";
            string whereClause = "(IntakeCode = '" + intakeCode + "')";
            maintbl = _DBOperations.SelectRows("Application", fieldList, "", "", whereClause, "");

            //get Examcode and Examyear of all the applicants of the selected intakecode 
            sql = "SELECT A.ApplicationCode, A.ExamYear, A.ExamCode FROM SEExam A " +
                  "INNER JOIN Application B ON A.ApplicationCode = B.ApplicationCode WHERE B.IntakeCode = '" + intakeCode + "' AND A.ExamCode = '" + examcode + "'";

            examtbl = _DBOperations.SelectRows(sql);

            //get grades of all the applicants of the selected intakecode
            sql = "SELECT A.ApplicationCode, A.Grade, Count(A.Grade) AS GradeCount FROM SEResult A " +
                  "INNER JOIN Application B ON A.ApplicationCode = B.ApplicationCode " +
                  "WHERE B.IntakeCode = '" + intakeCode + "' AND A.ExamCode = '" + examcode + "'" +
                  "GROUP BY A.ApplicationCode, A.Grade " +
                  "ORDER BY A.ApplicationCode";

            resulttbl = _DBOperations.SelectRows(sql);

            string applicationCode = "";
            string grades = "";
            int score = 0;
            DataRow[]? drs;

            foreach (DataRow dr in temptbl.Rows)
            {
                applicationCode = dr["ApplicationCode"].ToString();
                score = 0;
                grades = "";
                drs = null;

                if (scoretbl.Rows.Count > 0)
                {
                    drs = scoretbl.Select("ApplicationCode = '" + applicationCode + "'");
                }

                if (drs != null && drs.Length > 0)
                {
                    ALFilter item = new();
                    item.ApplicationCode = applicationCode;

                    drs = examtbl.Select("ApplicationCode = '" + applicationCode + "'");
                    if (drs.Length > 0)
                    {
                        item.ExamCode = drs[0]["ExamCode"].ToString();
                        item.ExamYear = drs[0]["ExamYear"].ToString();
                    }
                    
                    //get applicant name
                    drs = maintbl.Select("ApplicationCode = '" + applicationCode + "'");
                    if (drs.Length > 0)
                    {
                        item.NameWithInitials = drs[0]["NameWithInitials"].ToString();
                    }

                    //get results
                    drs = resulttbl.Select("ApplicationCode = '" + applicationCode + "'");
                    foreach (DataRow drtemp in drs)
                    {
                        grades = grades + drtemp["Grade"].ToString() + drtemp["GradeCount"].ToString() + ",";
                    }

                    if (grades.Length > 0)
                        item.Grades = grades.Substring(0, grades.Length - 1);

                    list.Add(item);
                }
            }

            return list;
        }

        public List<OLFilter> FilterByOL_Org(string intakeCode, string currentStage)
        {
            string examcode = "O/L";

            if (intakeCode.StartsWith("SALA/"))
                return FilterByOL_SALA(intakeCode, currentStage);

            // Position type flags
            // ITNW / SD / MO  → prefix ITN/
            // TTO              → prefix TTO/
            // Technician       → prefix TEC/
            // All others use the default rule (same as ITN but 3 credits for mandatory)
            bool isITNPosition = intakeCode.StartsWith("ITN/");
            bool isTTOPosition = intakeCode.StartsWith("TTO/");
            bool isTechnicianPosition = intakeCode.StartsWith("TEC/");

            DataTable temptbl, scoretbl, maintbl, resulttbl;
            List<OLFilter> list = new();

            string sql = "SELECT ALRequired FROM Intake WHERE IntakeCode = '" + intakeCode + "'";
            DataTable intakeTable = _DBOperations.SelectRows(sql);
            bool alRequired = false;

            if (intakeTable.Rows.Count > 0)
                alRequired = intakeTable.Rows[0]["ALRequired"].ToString() == "1" || intakeTable.Rows[0]["ALRequired"].ToString().ToLower() == "true";

            if (alRequired)
                sql = "SELECT ApplicationCode FROM FilteredData WHERE Stage2Status = 'PASS' AND CurrentStage = '2' AND CurrentStatus = 'PASS' AND IntakeCode = '" + intakeCode + "'";
            else
                sql = "SELECT ApplicationCode FROM FilteredData WHERE Stage1Status = 'PASS' AND CurrentStage = '1' AND CurrentStatus = 'PASS' AND IntakeCode = '" + intakeCode + "'";

            temptbl = _DBOperations.SelectRows(sql);

            // Include SubjectName so we can check specific mandatory subjects by name
            sql = "SELECT B.ApplicationCode, B.ExamYear, B.ExamCode, A.Attempt, A.SubjectName, A.Grade, G.Rating FROM SEResult A " +
                  "INNER JOIN SEExam B ON A.ApplicationCode = B.ApplicationCode AND B.Attempt = A.Attempt " +
                  "INNER JOIN Application C ON B.ApplicationCode = C.ApplicationCode " +
                  "INNER JOIN Grade G ON A.Grade = G.GradeValue " +
                  "WHERE C.IntakeCode = '" + intakeCode + "' AND A.ExamCode = 'O/L' AND B.ExamCode = 'O/L' " +
                  "ORDER BY B.ApplicationCode, A.Attempt";
            scoretbl = _DBOperations.SelectRows(sql);

            string fieldList = "ApplicationCode, Concat(Initials,' ',Surname, ' ') as NameWithInitials";
            string whereClause = "(IntakeCode = '" + intakeCode + "')";
            maintbl = _DBOperations.SelectRows("Application", fieldList, "", "", whereClause, "");

            sql = "SELECT A.ApplicationCode, A.Grade, A.Attempt, Count(A.Grade) AS GradeCount FROM SEResult A " +
                  "INNER JOIN Application B ON A.ApplicationCode = B.ApplicationCode " +
                  "WHERE B.IntakeCode = '" + intakeCode + "' AND A.ExamCode = 'O/L' " +
                  "GROUP BY A.ApplicationCode, A.Attempt, A.Grade " +
                  "ORDER BY A.ApplicationCode, A.Attempt";
            resulttbl = _DBOperations.SelectRows(sql);

            string applicationCode = "";
            string grades = "";
            string mandatoryGrades = "";
            string successfulAttempt = "";
            string successfulExamYear = "";
            bool isOk = false;
            DataRow[] drs;

            foreach (DataRow dr in temptbl.Rows)
            {
                applicationCode = dr[0].ToString();
                grades = "";
                mandatoryGrades = "";
                isOk = false;
                successfulAttempt = "";
                successfulExamYear = "";

                // Check up to 3 attempts (sittings). If they resit and pass all requirements in a subsequent sitting, it will be accepted.
                for (int i = 1; i <= 3; i++)
                {
                    int totalScore = 0, creditScore = 0;
                    bool hasMathsPass = false, hasMathsCredit = false;
                    bool hasSinhalaOrTamilPass = false, hasSinhalaOrTamilCredit = false;
                    bool hasEnglishPass = false, hasEnglishCredit = false;

                    drs = scoretbl.Select("ApplicationCode = '" + applicationCode + "' AND Attempt = '" + i.ToString() + "'");

                    foreach (DataRow drtemp in drs)
                    {
                        int rating = Convert.ToInt16(drtemp["Rating"]);
                        string subj = drtemp["SubjectName"].ToString().Trim().ToLowerInvariant();

                        if (rating >= 1) totalScore++;
                        if (rating >= 2) creditScore++;

                        if (subj.Contains("math"))
                        {
                            if (rating >= 1) hasMathsPass = true;
                            if (rating >= 2) hasMathsCredit = true;
                        }
                        if (subj.Contains("sinhala") || subj.Contains("tamil"))
                        {
                            if (rating >= 1) hasSinhalaOrTamilPass = true;
                            if (rating >= 2) hasSinhalaOrTamilCredit = true;
                        }
                        if (subj.Contains("english"))
                        {
                            if (rating >= 1) hasEnglishPass = true;
                            if (rating >= 2) hasEnglishCredit = true;
                        }

                        successfulExamYear = drtemp["ExamYear"].ToString();
                    }

                    isOk = IsOLEligible(intakeCode, totalScore, creditScore,
                                         hasMathsPass, hasMathsCredit,
                                         hasSinhalaOrTamilPass, hasSinhalaOrTamilCredit,
                                         hasEnglishPass, hasEnglishCredit);

                    if (isOk)
                    {
                        successfulAttempt = i.ToString();
                        break;
                    }
                }

                if (isOk)
                {
                    OLFilter item = new OLFilter();
                    item.ApplicationCode = applicationCode;
                    item.ExamCode = examcode;
                    item.ExamYear = successfulExamYear;

                    drs = maintbl.Select("ApplicationCode = '" + applicationCode + "'");
                    if (drs.Length > 0)
                        item.NameWithInitials = drs[0]["NameWithInitials"].ToString();

                    drs = resulttbl.Select("ApplicationCode = '" + applicationCode + "' AND Attempt = '" + successfulAttempt + "'");
                    foreach (DataRow drtemp in drs)
                        grades = grades + drtemp["Grade"].ToString() + drtemp["GradeCount"].ToString() + ",";

                    item.Grades = grades;
                    item.MandatoryGrades = mandatoryGrades;
                    item.Attempt = successfulAttempt;

                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>
        /// Determines O/L eligibility based on position type (derived from intakeCode prefix).
        /// Ratings: >= 1 = pass (S or above), >= 2 = credit (C or above)
        /// </summary>
        internal static bool IsOLEligible(
            string intakeCode,
            int totalScore, int creditScore,
            bool hasMathsPass, bool hasMathsCredit,
            bool hasSinhalaOrTamilPass, bool hasSinhalaOrTamilCredit,
            bool hasEnglishPass, bool hasEnglishCredit)
        {
            if (intakeCode.StartsWith("TEC/"))
                // 6 passes, 3 credits (any), Maths/Sinhala-Tamil/English each >= S
                return totalScore >= 6 && creditScore >= 3
                       && hasMathsPass && hasSinhalaOrTamilPass && hasEnglishPass;

            if (intakeCode.StartsWith("TTO/"))
                // 6 passes, 3 credits (any), Maths/Sinhala-Tamil/English each >= credit
                return totalScore >= 6 && creditScore >= 3
                       && hasMathsCredit && hasSinhalaOrTamilCredit && hasEnglishCredit;

            if (intakeCode.StartsWith("ITN/") || intakeCode.StartsWith("MKO/"))
                // ITN / MKO: 6 passes, 4 credits (any), Maths/Sinhala-Tamil/English each >= credit
                return totalScore >= 6 && creditScore >= 4
                       && hasMathsCredit && hasSinhalaOrTamilCredit && hasEnglishCredit;

            // Universal fallback: 6 passes, 3 credits (Maths/Sinhala-Tamil/English each >= credit)
            return totalScore >= 6 && creditScore >= 3
                   && hasMathsCredit && hasSinhalaOrTamilCredit && hasEnglishCredit;
        }

        private List<OLFilter> FilterByOL_SALA(string intakeCode, string currentStage)
        {
            string examcode = "O/L";

            DataTable temptbl, scoretbl, maintbl, resulttbl;
            List<OLFilter> list = new();

            // Determine whether current-stage source is Stage1 or Stage2.
            string sql = "SELECT ALRequired FROM Intake WHERE IntakeCode = '" + intakeCode + "'";
            DataTable intakeTable = _DBOperations.SelectRows(sql);
            bool alRequired = false;

            if (intakeTable.Rows.Count > 0)
            {
                alRequired = intakeTable.Rows[0]["ALRequired"].ToString() == "1" || intakeTable.Rows[0]["ALRequired"].ToString().ToLower() == "true";
            }

            if (alRequired)
                sql = "SELECT ApplicationCode FROM FilteredData WHERE Stage2Status = 'PASS' AND CurrentStage = '2' AND CurrentStatus = 'PASS' AND IntakeCode = '" + intakeCode + "'";
            else
                sql = "SELECT ApplicationCode FROM FilteredData WHERE Stage1Status = 'PASS' AND CurrentStage = '1' AND CurrentStatus = 'PASS' AND IntakeCode = '" + intakeCode + "'";

            temptbl = _DBOperations.SelectRows(sql);

            // Need subject-wise grades to enforce SALA special rules.
            sql = "SELECT B.ApplicationCode, B.ExamYear, A.Attempt, A.SubjectName, G.Rating FROM SEResult A " +
                  "INNER JOIN SEExam B ON A.ApplicationCode = B.ApplicationCode AND B.Attempt = A.Attempt " +
                  "INNER JOIN Application C ON B.ApplicationCode = C.ApplicationCode " +
                  "INNER JOIN Grade G ON A.Grade = G.GradeValue " +
                  "WHERE C.IntakeCode = '" + intakeCode + "' AND A.ExamCode = 'O/L' AND B.ExamCode = 'O/L' " +
                  "ORDER BY B.ApplicationCode, A.Attempt";
            scoretbl = _DBOperations.SelectRows(sql);

            string fieldList = "ApplicationCode, Concat(Initials,' ',Surname, ' ') as NameWithInitials";
            string whereClause = "(IntakeCode = '" + intakeCode + "')";
            maintbl = _DBOperations.SelectRows("Application", fieldList, "", "", whereClause, "");

            sql = "SELECT A.ApplicationCode, A.Grade, A.Attempt, Count(A.Grade) AS GradeCount FROM SEResult A " +
                  "INNER JOIN Application B ON A.ApplicationCode = B.ApplicationCode " +
                  "WHERE B.IntakeCode = '" + intakeCode + "' AND A.ExamCode = 'O/L' " +
                  "GROUP BY A.ApplicationCode, A.Attempt, A.Grade " +
                  "ORDER BY A.ApplicationCode, A.Attempt";
            resulttbl = _DBOperations.SelectRows(sql);

            string applicationCode = "";
            string grades = "";
            string successfulAttempt = "";
            string successfulExamYear = "";
            DataRow[] drs;

            foreach (DataRow dr in temptbl.Rows)
            {
                applicationCode = dr[0].ToString();
                bool isOk = false;
                successfulAttempt = "";
                successfulExamYear = "";

                // SALA rule: can combine results across 2 sittings (Attempts 1 and 2)
                Dictionary<string, int> bestGrades = new Dictionary<string, int>();

                drs = scoretbl.Select("ApplicationCode = '" + applicationCode + "' AND (Attempt = '1' OR Attempt = '2')");

                foreach (DataRow drtemp in drs)
                {
                    int rating = Convert.ToInt16(drtemp["Rating"]);
                    string subjectName = drtemp["SubjectName"].ToString().Trim().ToLowerInvariant();

                    if (!bestGrades.ContainsKey(subjectName))
                    {
                        bestGrades[subjectName] = rating;
                    }
                    else if (rating > bestGrades[subjectName])
                    {
                        bestGrades[subjectName] = rating;
                    }

                    successfulExamYear = drtemp["ExamYear"].ToString();
                }

                int totalScore = 0;
                int creditScore = 0;
                bool hasEnglishCredit = false;
                bool hasSinhalaOrTamilPass = false;
                bool hasMathsPass = false;

                foreach (var kvp in bestGrades)
                {
                    string subjectName = kvp.Key;
                    int rating = kvp.Value;

                    if (rating >= 1) totalScore++;
                    if (rating >= 2) creditScore++;

                    if (rating >= 2 && subjectName.Contains("english")) hasEnglishCredit = true;
                    if (rating >= 1 && (subjectName.Contains("sinhala") || subjectName.Contains("tamil"))) hasSinhalaOrTamilPass = true;
                    if (rating >= 1 && (subjectName.Contains("math") || subjectName.Contains("mathematics"))) hasMathsPass = true;
                }

                // SALA: 6 passes, 4 credits, English >= credit, Sinhala/Tamil >= S, Maths >= S.
                if (totalScore >= 6 && creditScore >= 4 && hasEnglishCredit && hasSinhalaOrTamilPass && hasMathsPass)
                {
                    successfulAttempt = "1,2";
                    isOk = true;
                }

                if (isOk)
                {
                    OLFilter item = new OLFilter();
                    item.ApplicationCode = applicationCode;
                    item.ExamCode = examcode;
                    item.ExamYear = successfulExamYear;

                    drs = maintbl.Select("ApplicationCode = '" + applicationCode + "'");
                    if (drs.Length > 0)
                    {
                        item.NameWithInitials = drs[0]["NameWithInitials"].ToString();
                    }

                    grades = "";
                    drs = resulttbl.Select("ApplicationCode = '" + applicationCode + "' AND (Attempt = '1' OR Attempt = '2')");
                    foreach (DataRow drtemp in drs)
                    {
                        grades = grades + drtemp["Grade"].ToString() + drtemp["GradeCount"].ToString() + ",";
                    }

                    item.Grades = grades;
                    item.Attempt = successfulAttempt;

                    list.Add(item);
                }
            }

            return list;
        }

        public async Task<List<OLFilter>> FilterByOL(string intakeCode, string currentStage)
        {
            // Temporarily bypass FastAPI and use original method only
            if (intakeCode.StartsWith("SALA/"))
                return FilterByOL_SALA(intakeCode, currentStage);

            return FilterByOL_Org(intakeCode, currentStage);
            
            /*
            try
            {
                return await FilterByOL_FastAPI(intakeCode, currentStage);
            }
            catch (Exception ex)
            {
                // Log the error and fallback to original method
                System.Diagnostics.Debug.WriteLine($"FastAPI O/L Filter failed: {ex.Message}");
                return FilterByOL_Org(intakeCode, currentStage);
            }
            */
        }

        private async Task<List<OLFilter>> FilterByOL_FastAPI(string intakeCode, string currentStage)
        {
            string examcode = "O/L";

            //1.get the application codes who are PASS in the current stage
            //2.next check O/L results of those application codes 

            DataTable temptbl, scoretbl, maintbl, resulttbl, mresulttbl, examtbl;
            DataTable appDataTable;
            List<OLFilter> list = new();
            ApiResponse apiResponse;

            //get intake requirements to determine which stage to query
            string sql = "SELECT ALRequired FROM Intake WHERE IntakeCode = '" + intakeCode + "'";
            DataTable intakeTable = _DBOperations.SelectRows(sql);
            bool alRequired = false;
            
            if (intakeTable.Rows.Count > 0)
            {
                alRequired = intakeTable.Rows[0]["ALRequired"].ToString() == "1" || intakeTable.Rows[0]["ALRequired"].ToString().ToLower() == "true";
            }

            //get current stage passed applicants of the selected intakecode
            //if A/L was required, check Stage2Status (A/L result), otherwise check Stage1Status (Age result)
            //IMPORTANT: Only get records at current stage to avoid re-processing already filtered records
            if (alRequired)
                sql = "SELECT A.* FROM FilteredData F INNER JOIN Application A ON F.ApplicationCode = A.ApplicationCode WHERE F.Stage2Status = 'PASS' AND F.CurrentStage = '2' AND F.CurrentStatus = 'PASS' AND A.IntakeCode = '" + intakeCode + "'";
            else
                sql = "SELECT A.* FROM FilteredData F INNER JOIN Application A ON F.ApplicationCode = A.ApplicationCode WHERE F.Stage1Status = 'PASS' AND F.CurrentStage = '1' AND F.CurrentStatus = 'PASS' AND A.IntakeCode = '" + intakeCode + "'";
            
            appDataTable = _DBOperations.SelectRows(sql);

            temptbl = appDataTable.Clone();
            temptbl.Columns["DOB"].DataType = typeof(string);

            foreach (DataRow row in appDataTable.Rows)
            {
                temptbl.ImportRow(row);
            }

            foreach (DataRow drtemp in temptbl.Rows)
            {
                drtemp["DOB"] = "2000-01-01";
            }
            temptbl.AcceptChanges();
            //-----------------------------


            _UtilityFn.ConvertToCSV(temptbl, fileNameAppData);

            //get current stage passed applicants results of the selected intakecode
            //IMPORTANT: Only get records at current stage to avoid re-processing already filtered records
            if (alRequired)
                sql = "SELECT R.* FROM SEResult R INNER JOIN FilteredData F ON R.ApplicationCode = F.ApplicationCode WHERE F.Stage2Status = 'PASS' AND F.CurrentStage = '2' AND F.CurrentStatus = 'PASS' AND F.IntakeCode = '" + intakeCode + "'";
            else
                sql = "SELECT R.* FROM SEResult R INNER JOIN FilteredData F ON R.ApplicationCode = F.ApplicationCode WHERE F.Stage1Status = 'PASS' AND F.CurrentStage = '1' AND F.CurrentStatus = 'PASS' AND F.IntakeCode = '" + intakeCode + "'";
            
            resulttbl = _DBOperations.SelectRows(sql);

            //update all A/L results as 'A' as Fast API checks both A/L and O/L results same time. But at this stage we need only eligible applicants based on O/L only
            resulttbl = UpdateDummyResults(resulttbl, "A/L");
            _UtilityFn.ConvertToCSV(resulttbl, fileNameResultData);

            //delete all the uploaded data
            apiResponse = await _fastAPIProcess.DeleteAllData();

            //upload the new data as csv to FastAPI
            apiResponse = await _fastAPIProcess.UploadApplicationData(fileNameAppData, fileNameResultData);

            //get the eligible applicants from the fastApi
            scoretbl = new DataTable();
            var jobPositionCode = intakeCode.Substring(0, intakeCode.IndexOf('/'));
            apiResponse = await _fastAPIProcess.FilterByPosition(jobPositionCode, intakeCode);

            if (apiResponse.result != null)
            {
                scoretbl = _UtilityFn.GetEligibleApplicationCodes(apiResponse.result.ToString());
            }

            //get Examcode and Examyear of all the applicants of the selected intakecode 
            sql = "SELECT A.ApplicationCode, A.ExamYear, A.ExamCode FROM SEExam A " +
                  "INNER JOIN Application B ON A.ApplicationCode = B.ApplicationCode WHERE B.IntakeCode = '" + intakeCode + "' AND A.ExamCode = '" + examcode + "'";
            examtbl = _DBOperations.SelectRows(sql);

            //get all the applicant's name of the selected intakecode
            string fieldList = "ApplicationCode, Concat(Initials,' ',Surname, ' ') as NameWithInitials";
            string whereClause = "(IntakeCode = '" + intakeCode + "')";
            maintbl = _DBOperations.SelectRows("Application", fieldList, "", "", whereClause, "");

            //get mandatory subject results of all applicants of the selected intake code
            sql = "SELECT A.ApplicationCode, A.Attempt, A.Grade, Count(A.Grade) AS GradeCount FROM SEResult A " +
                  "INNER JOIN Application B ON A.ApplicationCode = B.ApplicationCode " +
                  "INNER JOIN Subject S ON A.SubjectName = S.SubjectName AND A.ExamCode = S.ExamCode " +
                  "WHERE B.IntakeCode = '" + intakeCode + "' AND A.ExamCode = 'O/L' AND S.Mandatory = 'YES' " +
                  "GROUP BY A.ApplicationCode, A.Attempt, A.Grade " +
                  "ORDER BY A.ApplicationCode, A.Attempt";
            mresulttbl = _DBOperations.SelectRows(sql);

            //get total grades count of all the applicants of the selected intakecode
            sql = "SELECT A.ApplicationCode, A.Grade, A.Attempt, Count(A.Grade) AS GradeCount FROM SEResult A " +
                  "INNER JOIN Application B ON A.ApplicationCode = B.ApplicationCode " +
                  "WHERE B.IntakeCode = '" + intakeCode + "' AND A.ExamCode = '" + examcode + "'" +
                  "GROUP BY A.ApplicationCode, A.Attempt, A.Grade " +
                  "ORDER BY A.ApplicationCode, A.Attempt";
            resulttbl = _DBOperations.SelectRows(sql);

            string applicationCode = "";
            string grades = "";
            string mandatoryGrades = "";
            string successfulAttempt = "";
            string attempts = "";
            string successfulExamYear = "";
            bool isOk = false;

            int mandatoryScore = 0, totalScore = 0, rating = 0;
            DataRow[]? drs;

            foreach (DataRow dr in temptbl.Rows) //current stage passed records
            {
                applicationCode = dr["ApplicationCode"].ToString();
                grades = "";
                mandatoryGrades = "";
                drs = null;

                if (scoretbl.Rows.Count > 0)
                {
                    drs = scoretbl.Select("ApplicationCode = '" + applicationCode + "'");
                }

                if (drs != null && drs.Length > 0)
                {
                    OLFilter item = new OLFilter();
                    item.ApplicationCode = applicationCode;

                    drs = examtbl.Select("ApplicationCode = '" + applicationCode + "'");
                    foreach (DataRow drtemp in drs)
                    {
                        item.ExamCode = drtemp["ExamCode"].ToString();
                        item.ExamYear = item.ExamYear + drtemp["ExamYear"].ToString() + "|";
                    }

                    if (item.ExamYear.Length > 0)
                        item.ExamYear = item.ExamYear.Substring(0, item.ExamYear.Length - 1);

                    //get Mandatory O/L Subject results
                    for (int i = 1; i <= 3; i++)
                    {
                        mandatoryGrades = "";

                        drs = mresulttbl.Select("ApplicationCode = '" + applicationCode + "' AND Attempt = '" + i.ToString() + "'");
                        foreach (DataRow drtemp in drs)
                        {
                            mandatoryGrades = mandatoryGrades + drtemp["Grade"].ToString() + drtemp["GradeCount"].ToString() + ",";
                        }

                        if (mandatoryGrades.Length > 0)
                        {
                            mandatoryGrades = mandatoryGrades.Substring(0, mandatoryGrades.Length - 1);

                            if (item.MandatoryGrades.Length > 0)
                                item.MandatoryGrades = item.MandatoryGrades + "|" + mandatoryGrades;
                            else
                                item.MandatoryGrades = mandatoryGrades;
                        }
                    }

                    //get applicant name
                    drs = maintbl.Select("ApplicationCode = '" + applicationCode + "'");
                    if (drs.Length > 0)
                    {
                        item.NameWithInitials = drs[0]["NameWithInitials"].ToString();
                    }

                    //get results
                    for (int i = 1; i <= 3; i++)
                    {
                        grades = "";
                        attempts = "";

                        drs = resulttbl.Select("ApplicationCode = '" + applicationCode + "' AND Attempt = '" + i.ToString() + "'");
                        foreach (DataRow drtemp in drs)
                        {
                            grades = grades + drtemp["Grade"].ToString() + drtemp["GradeCount"].ToString() + ",";
                        }

                        if (grades.Length > 0)
                        {
                            grades = grades.Substring(0, grades.Length - 1);
                            attempts = i.ToString();

                            if (item.Grades.Length > 0)
                            {
                                item.Grades = item.Grades + "|" + grades;
                                item.Attempt = item.Attempt + "|" + attempts;
                            }
                            else
                            {
                                item.Grades = grades;
                                item.Attempt = attempts;
                            }
                        }
                    }

                    list.Add(item);
                }
            }

            return list;
        }

        public List<HEPQFilter> FilterByHEPQ(string intakeCode, string currentStage, string currentStatus = "PASS")
        {
            //1.get the application codes who are PASS in the current stage
            //2.next get higher/prof education results of those application codes 

            DataTable temptbl, maintbl, highertbl, profqualtbl;
            List<HEPQFilter> list = new();
            string sql = "";

            //get current stage passed applicants of the selected intakecode with proper numeric sorting
            //IMPORTANT: Always check CurrentStage and CurrentStatus to avoid re-processing already filtered records
            sql = "SELECT ApplicationCode FROM FilteredData WHERE CurrentStage = '" + currentStage + "' AND CurrentStatus = '" + currentStatus + "' AND IntakeCode = '" + intakeCode + "' ORDER BY CAST(SUBSTRING(ApplicationCode, LEN(ApplicationCode) - CHARINDEX('/', REVERSE(ApplicationCode)) + 2, LEN(ApplicationCode)) AS INT)";
            
            temptbl = _DBOperations.SelectRows(sql);

            //get higher edu. of all the applicants in the selected intakecode
            sql = "SELECT Q.ApplicationCode, (Q.QualName + '-' + Q.HEInstituteName + '-' + QualStatus) AS HEQual FROM HEQualification Q " +
                  "INNER JOIN Application A ON A.ApplicationCode = Q.ApplicationCode " +
                  "WHERE A.IntakeCode = '" + intakeCode + "' " +
                  "ORDER BY A.ApplicationCode";

            highertbl = _DBOperations.SelectRows(sql);

            //get prof. qual of all the applicants in the selected intakecode
            sql = "SELECT Q.ApplicationCode, (Q.MembershipType + '-' + Q.PQInsituteName + '-' + Q.MembershipNo) AS ProfQual FROM ProfQualification Q " +
                  "INNER JOIN Application A ON A.ApplicationCode = Q.ApplicationCode " +
                  "WHERE A.IntakeCode = '" + intakeCode + "' " +
                  "ORDER BY A.ApplicationCode";

            profqualtbl = _DBOperations.SelectRows(sql);

            //get all the applicant's name of the selected intakecode
            string fieldList = "ApplicationCode, Concat(Initials,' ',Surname, ' ') as NameWithInitials";
            string whereClause = "(IntakeCode = '" + intakeCode + "')";
            maintbl = _DBOperations.SelectRows("Application", fieldList, "", "", whereClause, "");

            string applicationCode = "";
            string qualification = "";

            DataRow[] drs;

            foreach (DataRow dr in temptbl.Rows) //current stage passed records
            {
                HEPQFilter item = new();

                applicationCode = dr[0].ToString();
                item.ApplicationCode = applicationCode;

                //get HE qual. 
                drs = highertbl.Select("ApplicationCode = '" + applicationCode + "'");
                qualification = "";

                foreach (DataRow drstemp in drs)
                {
                    qualification = qualification + drstemp["HEQual"].ToString() + "|";
                }

                if (qualification.Length > 0)
                    item.HEQual = qualification.Remove(qualification.Length-1,1);

                //get Prof. qual
                drs = profqualtbl.Select("ApplicationCode = '" + applicationCode + "'");
                qualification = "";

                foreach (DataRow drstemp in drs)
                {
                    qualification = qualification + drstemp["ProfQual"].ToString() + "|";
                }

                if (qualification.Length > 0)
                    item.ProfQual = qualification.Remove(qualification.Length - 1, 1);

                //get applicant name
                drs = maintbl.Select("ApplicationCode = '" + applicationCode + "'");
                if (drs.Length > 0)
                {
                    item.NameWithInitials = drs[0]["NameWithInitials"].ToString();
                }

                list.Add(item);
                
            }

            return list;
        }

        public List<FinalFilter> FilterFinal(string intakeCode, string currentStage)
        {
            //1.get the application codes who are PASS in the current stage
            //2.next get work experience of those application codes 

            DataTable temptbl, maintbl, workexptbl;
            List<FinalFilter> list = new();

            //get current stage passed applicants of the selected intakecode
            //IMPORTANT: Only get records at Stage 4 with PASS status (not TO-CHECK) to avoid re-processing
            string sql = "SELECT ApplicationCode FROM FilteredData WHERE CurrentStage = '4' AND CurrentStatus = 'PASS' AND IntakeCode = '" + intakeCode + "'";
            temptbl = _DBOperations.SelectRows(sql);

            //get work experience of all the applicants in the selected intakecode
            sql = "SELECT Q.ApplicationCode, (Q.CompanyName + '-' + Q.PositionHeld + '-' + JobStatus) AS WorkExp FROM WorkExperience Q " +
                  "INNER JOIN Application A ON A.ApplicationCode = Q.ApplicationCode " +
                  "WHERE A.IntakeCode = '" + intakeCode + "' " +
                  "ORDER BY A.ApplicationCode";

            workexptbl = _DBOperations.SelectRows(sql);

            //get all the applicant's name of the selected intakecode
            string fieldList = "ApplicationCode, Concat(Initials,' ',Surname, ' ') as NameWithInitials";
            string whereClause = "(IntakeCode = '" + intakeCode + "')";
            maintbl = _DBOperations.SelectRows("Application", fieldList, "", "", whereClause, "");

            string applicationCode = "";
            string qualification = "";

            DataRow[] drs;

            foreach (DataRow dr in temptbl.Rows) //current stage passed records
            {
                FinalFilter item = new();

                applicationCode = dr[0].ToString();
                item.ApplicationCode = applicationCode;
                item.CurrentStatus = "PASS";

                //get work exp. 
                drs = workexptbl.Select("ApplicationCode = '" + applicationCode + "'");
                qualification = "";

                foreach (DataRow drstemp in drs)
                {
                    qualification = qualification + drstemp["WorkExp"].ToString() + "|";
                }

                if (qualification.Length > 0)
                    item.WorkExp = qualification.Remove(qualification.Length - 1, 1);

                //get applicant name
                drs = maintbl.Select("ApplicationCode = '" + applicationCode + "'");
                if (drs.Length > 0)
                {
                    item.NameWithInitials = drs[0]["NameWithInitials"].ToString();
                }

                list.Add(item);

            }

            return list;
        }

        public List<FinalFilter> ShowFinal(string intakeCode, string currentStage, int? freezeNo = 0)
        {
            //1.get the application codes who are PASS in the current (final) stage

            DataTable temptbl, maintbl;
            List<FinalFilter> list = new();

            //get current stage (FINAL) passed applicants of the selected intakecode
            string sql = "SELECT ApplicationCode, CurrentStatus, FinalRemarks AS Remarks FROM FilteredData WHERE CurrentStage = '" + currentStage + "' AND CurrentStatus = 'PASS' AND IntakeCode = '" + intakeCode + "'";

            if (freezeNo.HasValue && freezeNo > 0)
            {
                sql = sql + " AND FreezeNo = " + freezeNo.Value;
            }
            else
            {
                sql = sql + " AND FreezeNo IS NULL OR FreezeNo = ''";
            }
            temptbl = _DBOperations.SelectRows(sql);

            //get all the applicant's name of the selected intakecode
            string fieldList = "ApplicationCode, Concat(Initials,' ',Surname, ' ') as NameWithInitials";
            string whereClause = "(IntakeCode = '" + intakeCode + "')";
            maintbl = _DBOperations.SelectRows("Application", fieldList, "", "", whereClause, "");

            string applicationCode = "";

            DataRow[] drs;

            foreach (DataRow dr in temptbl.Rows) //current stage passed records
            {
                FinalFilter item = new();

                applicationCode = dr[0].ToString();
                item.ApplicationCode = applicationCode;
                item.CurrentStatus = dr["CurrentStatus"].ToString();
                item.Remarks = dr["Remarks"].ToString();

                //get applicant name
                drs = maintbl.Select("ApplicationCode = '" + applicationCode + "'");
                if (drs.Length > 0)
                {
                    item.NameWithInitials = drs[0]["NameWithInitials"].ToString();
                }

                list.Add(item);
            }

            return list;
        }

        public List<FailedApplicants> ShowFail(string intakeCode, string currentStage, int? freezeNo = 0)
        {
            //1.get the application codes who are FAIL in the any stage

            DataTable temptbl, maintbl;
            List<FailedApplicants> list = new();

            //get current stage FAIL applicants of the selected intakecode
            string sql = "SELECT * FROM FilteredData WHERE CurrentStatus = 'FAIL' AND IntakeCode = '" + intakeCode + "'";
            if (freezeNo.HasValue && freezeNo > 0)
            {
                sql = sql + " AND FreezeNo = " + freezeNo.Value;
            }
            else
            {
                sql = sql + " AND FreezeNo IS NULL OR FreezeNo = ''";
            }
            temptbl = _DBOperations.SelectRows(sql);

            //get all the applicant's name of the selected intakecode
            string fieldList = "ApplicationCode, Concat(Initials,' ',Surname, ' ') as NameWithInitials";
            string whereClause = "(IntakeCode = '" + intakeCode + "')";
            maintbl = _DBOperations.SelectRows("Application", fieldList, "", "", whereClause, "");

            string applicationCode = "";
            string remarkField = "";

            DataRow[] drs;

            foreach (DataRow dr in temptbl.Rows) //current stage passed records
            {
                FailedApplicants item = new();

                applicationCode = dr["ApplicationCode"].ToString();
                item.ApplicationCode = applicationCode;
                item.FailedStatus = dr["CurrentStatus"].ToString();
                item.FailedStage = dr["CurrentStage"].ToString();

                if (item.FailedStage != "FINAL")
                    remarkField = "Stage" + item.FailedStage + "Remarks";
                else
                    remarkField = "FinalRemarks";

                item.FailedRemarks = dr[remarkField].ToString();

                //get applicant name
                drs = maintbl.Select("ApplicationCode = '" + applicationCode + "'");
                if (drs.Length > 0)
                {
                    item.NameWithInitials = drs[0]["NameWithInitials"].ToString();
                }

                list.Add(item);
            }

            return list;
        }

        public List<FilterProgress> ShowProgress(string applicationCode)
        {
            DataTable temptbl;
            List<FilterProgress> list = new();
            string finalReversedBy = "";
            DateTime? finalReversedOn = DateTime.MinValue;

            //get progress of the selected application code
            string sql = "SELECT F.*, Concat(A.Initials,' ',A.Surname, ' ') as NameWithInitials FROM FilteredData F INNER JOIN Application A ON A.ApplicationCode = F.ApplicationCode WHERE F.ApplicationCode = '" + applicationCode + "'";
            temptbl = _DBOperations.SelectRows(sql);

            if (temptbl.Rows.Count > 0)
            {
                DataRow dr = temptbl.Rows[0];
                string fieldName = "";
                string stage = "";

                for (int i = 1; i <= 5; i++)
                {
                    FilterProgress item = new();
                    stage = i.ToString();

                    if (i == 5)
                    {
                        fieldName = "Final";
                        stage = "Final";
                        if (dr["FinalStatusReversedOn"] != DBNull.Value)
                        {
                            finalReversedOn = DateTime.Parse(dr["FinalStatusReversedOn"].ToString());
                            finalReversedBy = dr["FinalStatusReversedBy"].ToString();
                        }
                    }
                    else
                        fieldName = "Stage" + i.ToString();

                    item.ApplicationCode = applicationCode;
                    item.Stage = stage;
                    item.Status = dr[fieldName + "Status"].ToString();
                    item.Remarks = dr[fieldName + "Remarks"].ToString();

                    if (dr[fieldName + "UpdatedOn"].ToString() != "")
                        item.UpdatedOn = DateTime.Parse(dr[fieldName + "UpdatedOn"].ToString());

                    item.UpdatedBy = dr[fieldName + "UpdatedBy"].ToString();

                    list.Add(item);
                }

                if (finalReversedOn != DateTime.MinValue) {
                    FilterProgress item = new();
                    item.ApplicationCode = applicationCode;
                    item.Stage = "Final Reverse";
                    item.Remarks = "Updated as Eligible";
                    item.UpdatedOn = finalReversedOn;
                    item.UpdatedBy = finalReversedBy;
                    list.Add(item);
                }

                string colname = "";
                string[] stagex = { "ExamSelected", "InterviewSelected", "JobSelected" };
                for (int i = 0; i < 3; i++)
                {
                    FilterProgress item = new();
                    item.Stage = stagex[i].Substring(0,stagex[i].IndexOf("Selected")) + " Selected";
                    item.Status = dr[stagex[i]].ToString();
                    
                    colname = stagex[i] + "UpdatedBy";
                    item.UpdatedBy = dr[colname].ToString();

                    colname = stagex[i] + "UpdatedOn";
                    if (dr[colname] != DBNull.Value)
                        item.UpdatedOn = Convert.ToDateTime(dr[colname]);

                    list.Add(item);
                }

            }

            return list;
        }

        public ApiResponse ConfirmFinal(string intakeCode, string intakeType)
        {
            ApiResponse apiResponse = new();
            string userId = "";

            User user = _UtilityFn.GetCurrentUser();
            userId = user.UserId;

            if (intakeType == "TP")
            {
                //first update the freeze no against each application code in the filtereddata tbl
                //next insert a new record to FreezeSummary tbl
                int freezeNo = 0;
                DataTable tmpTable = new();

                string sql = "SELECT Max(FreezeNo) as FreezeNo FROM FreezeSummary WHERE IntakeCode = '" + intakeCode + "'";
                tmpTable = _DBOperations.SelectRows(sql);

                if (tmpTable.Rows.Count > 0 && tmpTable.Rows[0][0] != DBNull.Value)
                {
                    freezeNo = Convert.ToInt16(tmpTable.Rows[0][0]);
                }

                freezeNo++;

                //filteredData table update sql
                sql = "UPDATE FilteredData SET FreezeNo = " + freezeNo + " WHERE CurrentStage = 'FINAL' AND FreezeNo IS NULL AND IntakeCode = '" + intakeCode + "' AND CurrentStatus != 'TO-CHECK'";
                string message = _DBOperations.UpdateRecords(sql);

                //update freezesummary table
                if (message == "SUCCESS")
                {
                    DataTable dataTable = new();
                    dataTable.Columns.Add("IntakeCode");
                    dataTable.Columns.Add("FreezeNo");
                    dataTable.Columns.Add("FreezedBy");
                    dataTable.Columns.Add("FreezedOn");

                    dataTable.Rows.Add(intakeCode, freezeNo, userId, DateTime.Now);

                    message = _DBOperations.InsertRecords("FreezeSummary", dataTable, false);

                    apiResponse.message = message;
                    apiResponse.result = freezeNo;
                    apiResponse.isSuccess = true;
                }
            }
            else
            {
                DataTable tmpTable = new DataTable();
                tmpTable.Columns.Add("IntakeCode");
                tmpTable.Columns.Add("FinalConfirmed");
                tmpTable.Columns.Add("FinalConfirmedBy");
                tmpTable.Columns.Add("FinalConfirmedOn");

                tmpTable.Rows.Add(intakeCode, 1, userId, DateTime.Now);
                string message = _DBOperations.UpdateRecords("INTAKE", tmpTable, "INTAKECODE", intakeCode);

                if (message == "SUCCESS")
                {
                    apiResponse.message = message;
                    apiResponse.result = "";
                    apiResponse.isSuccess = true;
                }
            }

            return apiResponse;
        }

        private DataTable UpdateDummyResults(DataTable dataTable, string examCode)
        {
            DataRow[] drs = dataTable.Select("ExamCode = '" + examCode + "'");
            foreach (DataRow dr in drs)
            {
                dr["Grade"] = "A";
            }

            dataTable.AcceptChanges();
            return dataTable;
        }

        public List<ShortListed> ShowShortListed(string intakeCode, string currentStage, int? freezeNo)
        {
            //1.get the application codes who are PASS in the current (final) stage

            DataTable temptbl, maintbl;
            string sql = "";
            List<ShortListed> list = new();

            if (currentStage == "EXAM")
            {
                //get current stage (FINAL) passed applicants of the selected intakecode
                sql = "SELECT ApplicationCode, ExamSelected FROM FilteredData WHERE CurrentStage = 'FINAL' AND CurrentStatus = 'PASS' AND IntakeCode = '" + intakeCode + "'";
            }
            else if (currentStage == "INTERVIEW")
            {
                sql = "SELECT ApplicationCode, ExamSelected, InterviewSelected FROM FilteredData WHERE ExamSelected IS NOT NULL AND IntakeCode = '" + intakeCode + "'";
            }
            else if (currentStage == "JOB")
            {
                sql = "SELECT ApplicationCode, ExamSelected, InterviewSelected, JobSelected FROM FilteredData WHERE ExamSelected IS NOT NULL AND InterviewSelected IS NOT NULL AND IntakeCode = '" + intakeCode + "'";
            }
            else
            {
                sql = "SELECT ApplicationCode, ExamSelected, InterviewSelected, JobSelected FROM FilteredData WHERE ExamSelected IS NOT NULL AND InterviewSelected IS NOT NULL AND JobSelected IS NOT NULL AND IntakeCode = '" + intakeCode + "'";
            }

            if (freezeNo.HasValue)
            {
                sql = sql + " AND FreezeNo = " + freezeNo.Value;
            }
            
            temptbl = _DBOperations.SelectRows(sql);

            //get all the applicant's name of the selected intakecode
            string fieldList = "ApplicationCode, Concat(Initials,' ',Surname, ' ') as NameWithInitials";
            string whereClause = "(IntakeCode = '" + intakeCode + "')";
            maintbl = _DBOperations.SelectRows("Application", fieldList, "", "", whereClause, "");

            string applicationCode = "";

            DataRow[] drs;

            foreach (DataRow dr in temptbl.Rows) //current stage passed records
            {
                ShortListed item = new();

                applicationCode = dr[0].ToString();
                item.ApplicationCode = applicationCode;

                if (currentStage == "EXAM")
                    item.ExamSelected = dr["ExamSelected"].ToString() == "" ? "YES" : dr["ExamSelected"].ToString();

                if (currentStage == "INTERVIEW")
                {
                    item.ExamSelected = dr["ExamSelected"].ToString();
                    item.InterviewSelected = dr["InterviewSelected"].ToString() == "" ? "NO" : dr["InterviewSelected"].ToString();
                }

                if (currentStage == "JOB")
                {
                    item.ExamSelected = dr["ExamSelected"].ToString();
                    item.InterviewSelected = dr["InterviewSelected"].ToString();
                    item.JobSelected = dr["JobSelected"].ToString() == "" ? "NO" : dr["JobSelected"].ToString();
                }

                if (currentStage == "")
                {
                    item.ExamSelected = dr["ExamSelected"].ToString();
                    item.InterviewSelected = dr["InterviewSelected"].ToString();
                    item.JobSelected = dr["JobSelected"].ToString();
                }

                //get applicant name
                drs = maintbl.Select("ApplicationCode = '" + applicationCode + "'");
                if (drs.Length > 0)
                {
                    item.NameWithInitials = drs[0]["NameWithInitials"].ToString();
                }

                list.Add(item);
            }

            return list;

        }

        public string[] CountShortListed(string intakeCode, int? freezeNo)
        {
            string sql = "SELECT SUM(CASE WHEN ExamSelected = 'YES' THEN 1 ELSE 0 END) AS CountX,SUM(CASE WHEN InterviewSelected = 'YES' THEN 1 ELSE 0 END) AS CountY,SUM(CASE WHEN JobSelected = 'YES' THEN 1 ELSE 0 END) AS CountZ FROM FilteredData WHERE IntakeCode = '" + intakeCode + "'";

            if (freezeNo.HasValue)
            {
                sql = sql + " AND FreezeNo = " + freezeNo.Value;
            }

            DataTable tmptbl = _DBOperations.SelectRows(sql);

            string[] shortListedCount = new string[3];

            if (tmptbl.Rows.Count > 0 )
            {
                shortListedCount[0] = tmptbl.Rows[0][0].ToString();
                shortListedCount[1] = tmptbl.Rows[0][1].ToString();
                shortListedCount[2] = tmptbl.Rows[0][2].ToString();
            }

            return shortListedCount;
        }
    }
}
