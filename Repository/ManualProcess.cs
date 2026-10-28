using JobAppHR.Models;
using System.Data;

namespace JobAppHR.Repository
{
    public class ManualProcess: IManualProcess
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;

        public ManualProcess(IDBOperations dbOperations, IUtilityFn utilityFn)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
        }

        public DataTable FilterSummary(string intakeCode)
        {
            DataTable temptbl = new DataTable();
            DataTable finaltbl = new DataTable();
            string sql = "";
            int stage = 0;
            string updatedDateField = "";
            string[] stages = new string[] { "NOT-PROCESSED" };

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

            //get stage FINAL passed records from FilteredData Table 
            //sql = "SELECT Count(*) FROM FilteredData WHERE (IntakeCode = '" + intakeCode + "') AND (CurrentStage = 'FINAL' AND CurrentStatus = 'PASS' AND FreezeNo IS NULL) GROUP BY IntakeCode";
            updatedDateField = "Max(FinalUpdatedOn)";
            sql = "SELECT SUM(CASE WHEN CurrentStatus = 'TO-CHECK' THEN 1 ELSE 0 END) AS ToCheck_Count,SUM(CASE WHEN CurrentStatus = 'PASS' THEN 1 ELSE 0 END) AS Pass_Count," + updatedDateField + " FROM FilteredData WHERE (IntakeCode = '" + intakeCode + "') AND (CurrentStage = 'FINAL') AND (FreezeNo IS NULL)";

            temptbl = _DBOperations.SelectRows(sql);

            if (temptbl.Rows.Count > 0 && (temptbl.Rows[0][0] != DBNull.Value || temptbl.Rows[0][1] != DBNull.Value))
            {
                DataRow dr = finaltbl.NewRow();
                dr[0] = intakeCode;
                dr[1] = "FINAL";
                dr[2] = "FINAL";
                //dr[3] = temptbl.Rows[0][0].ToString();
                dr[3] = temptbl.Rows[0][0].ToString() + " | " + temptbl.Rows[0][1].ToString();
                dr[4] = temptbl.Rows[0][2] != DBNull.Value ? Convert.ToDateTime(temptbl.Rows[0][2]) : DBNull.Value;

                finaltbl.Rows.Add(dr);
            }

            //get FAIL records at any stage from FilteredData Table 
            updatedDateField = "Max(FinalUpdatedOn)";
            sql = "SELECT Count(*), " + updatedDateField + " FROM FilteredData WHERE (IntakeCode = '" + intakeCode + "') AND (CurrentStatus = 'FAIL')  AND (FreezeNo IS NULL) GROUP BY IntakeCode";
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

        public List<ManualFilter> FilterByAll(string intakeCode, string currentStage, string currentStatus = "PASS")
        {
            DataTable maintbl;
            string sql;
            string whereClause;
            string fieldList1 = "A.ApplicationCode, Concat(A.Initials,' ',A.Surname, ' ') as NameWithInitials, A.NIC, A.DOB, Concat(A.AgeYears,'Y ',A.AgeMonths, 'M ', A.AgeDays, 'D') as Age, A.Overage";
            string fieldList2 = "A.HouseNo + ',' + A.AddressLine1 + ',' + A.AddressLine2 + ',' + A.AddressLine3 + ',' + A.AddressLine4 AS Address, A.Email, A.ContactNo1 + ',' + A.ContactNo2 As ContactNo";
            string fieldList3 = "F.CurrentStatus,F.CurrentStage,F.FinalRemarks";
            
            maintbl = new();

            if (currentStage == "")
            {
                sql = "SELECT " + fieldList1 + "," + fieldList2 + "," + fieldList3 + " FROM Application A LEFT JOIN FilteredData F ON F.ApplicationCode = A.ApplicationCode WHERE (A.IntakeCode = '" + intakeCode + "') AND (SaveStatus = 'OK')";
                maintbl = _DBOperations.SelectRows(sql);
            }
            else if (currentStage == "0")
            {
                whereClause = "(A.IntakeCode = '" + intakeCode + "') AND (A.Processed IS NULL OR A.Processed = '') AND (SaveStatus = 'OK')";
                string orderBy = "CAST(SUBSTRING(A.ApplicationCode, CHARINDEX('/', A.ApplicationCode, CHARINDEX('/', A.ApplicationCode) + 1) + 1, LEN(A.ApplicationCode)) AS INT)";
                maintbl = _DBOperations.SelectRows("Application A", fieldList1, "", "", whereClause, orderBy);
            }
            else if (currentStage == "FINAL")
            {
                //get current stage FINAL passed or to-check applicants of the selected intakecode
                sql = "SELECT " + fieldList1 + "," + fieldList2 + "," + fieldList3 + " FROM FilteredData F INNER JOIN Application A ON F.ApplicationCode = A.ApplicationCode WHERE F.CurrentStage = 'FINAL' AND F.CurrentStatus = '" + currentStatus + "' AND F.IntakeCode = '" + intakeCode + "' AND F.FreezeNo IS NULL";
                maintbl = _DBOperations.SelectRows(sql);
            }
            else if (currentStage == "FAIL")
            {
                //get current status failed applicants of the selected intakecode, current stage can be any
                sql = "SELECT " + fieldList1 + "," + fieldList2 + "," + fieldList3 + " FROM FilteredData F INNER JOIN Application A ON F.ApplicationCode = A.ApplicationCode WHERE F.CurrentStatus = 'FAIL' AND F.IntakeCode = '" + intakeCode + "' AND F.FreezeNo IS NULL";
                maintbl = _DBOperations.SelectRows(sql);
            }

            List<ManualFilter> list = GetAllData(intakeCode,currentStage,maintbl);

            return list;
        }

        public List<ManualFilter> GetAllData(string intakeCode, string currentStage, DataTable maintbl)
        {
            DataTable resulttbl, mresulttbl, highertbl, profqualtbl, workexptbl;
            string sql;
            List<ManualFilter> list = new();

            //get grades of both A/L & O/L of all the applicants of the selected intakecode
            sql = "SELECT A.ApplicationCode, A.ExamCode, A.Attempt, A.Grade, Count(A.Grade) AS GradeCount FROM SEResult A " +
                  "INNER JOIN Application B ON A.ApplicationCode = B.ApplicationCode " +
                  "WHERE B.IntakeCode = '" + intakeCode + "'" +
                  "GROUP BY A.ApplicationCode, A.ExamCode, A.Attempt, A.Grade " +
                  "ORDER BY A.ApplicationCode";

            resulttbl = _DBOperations.SelectRows(sql);

            //get mandatory subject results of all applicants of the selected intake code
            sql = "SELECT A.ApplicationCode, A.Attempt, A.Grade, Count(A.Grade) AS GradeCount FROM SEResult A " +
                  "INNER JOIN Application B ON A.ApplicationCode = B.ApplicationCode " +
                  "INNER JOIN Subject S ON A.SubjectName = S.SubjectName AND A.ExamCode = S.ExamCode " +
                  "WHERE B.IntakeCode = '" + intakeCode + "' AND A.ExamCode = 'O/L' AND S.Mandatory = 'YES' " +
                  "GROUP BY A.ApplicationCode, A.Attempt, A.Grade " +
                  "ORDER BY A.ApplicationCode";

            mresulttbl = _DBOperations.SelectRows(sql);

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

            //get work experience of all the applicants in the selected intakecode
            sql = "SELECT Q.ApplicationCode, (Q.CompanyName + '-' + Q.PositionHeld + '-' + JobStatus) AS WorkExp FROM WorkExperience Q " +
                  "INNER JOIN Application A ON A.ApplicationCode = Q.ApplicationCode " +
                  "WHERE A.IntakeCode = '" + intakeCode + "' " +
                  "ORDER BY A.ApplicationCode";

            workexptbl = _DBOperations.SelectRows(sql);

            string applicationCode = "";
            string grades = "", qualification = "";
            DataRow[] drs;

            foreach (DataRow dr in maintbl.Rows)
            {
                applicationCode = dr["ApplicationCode"].ToString();
                grades = "";
                qualification = "";

                ManualFilter item = new();
                item.ApplicationCode = applicationCode;
                item.NameWithInitials = dr["NameWithInitials"].ToString();
                item.DOB = Convert.ToDateTime(dr["DOB"]);
                item.Age = dr["Age"].ToString();
                item.Overage = dr["Overage"].ToString();

                if (currentStage == "" || currentStage == "FINAL" || currentStage == "FAIL")
                {
                    item.DOB = DateTime.Parse(dr["DOB"].ToString());
                    item.NIC = dr["NIC"].ToString();
                    item.Address = dr["Address"].ToString();
                    item.Email = dr["Email"].ToString();
                    item.ContactNo = dr["ContactNo"].ToString();

                    item.CurrentStatus = dr["CurrentStatus"].ToString();
                    item.Stage = dr["CurrentStage"].ToString();
                    item.Remarks = dr["FinalRemarks"].ToString();
                }

                //get A/L results
                drs = resulttbl.Select("ApplicationCode = '" + applicationCode + "' AND ExamCode = 'A/L' AND Attempt = 1");
                foreach (DataRow drtemp in drs)
                {
                    grades = grades + drtemp["Grade"].ToString() + drtemp["GradeCount"].ToString() + ",";
                }

                item.ALGrades = grades;


                //get O/L results
                for (int i = 1; i <= 3; i++)
                {
                    grades = "";

                    drs = resulttbl.Select("ApplicationCode = '" + applicationCode + "' AND ExamCode = 'O/L' AND Attempt = '" + i.ToString() + "'");
                    foreach (DataRow drtemp in drs)
                    {
                        grades = grades + drtemp["Grade"].ToString() + drtemp["GradeCount"].ToString() + ",";
                    }

                    if (grades.Length > 0)
                        item.OLGrades = item.OLGrades + grades + "|";
                }

                if (!String.IsNullOrEmpty(item.OLGrades))
                    item.OLGrades = item.OLGrades.Remove(item.OLGrades.Length - 1, 1);

                //get Mandatory O/L Subject results
                for (int i = 1; i <= 3; i++)
                {
                    grades = "";

                    drs = mresulttbl.Select("ApplicationCode = '" + applicationCode + "' AND Attempt = '" + i.ToString() + "'");
                    foreach (DataRow drtemp in drs)
                    {
                        grades = grades + drtemp["Grade"].ToString() + drtemp["GradeCount"].ToString() + ",";
                    }

                    if (grades.Length > 0)
                        item.OLMandatoryGrades = item.OLMandatoryGrades + grades + "|";
                }

                if (!String.IsNullOrEmpty(item.OLMandatoryGrades))
                    item.OLMandatoryGrades = item.OLMandatoryGrades.Remove(item.OLMandatoryGrades.Length - 1, 1);

                //get HE qual. 
                drs = highertbl.Select("ApplicationCode = '" + applicationCode + "'");
                qualification = "";

                foreach (DataRow drstemp in drs)
                {
                    qualification = qualification + drstemp["HEQual"].ToString() + "|";
                }

                if (qualification.Length > 0)
                    item.HEQual = qualification.Remove(qualification.Length - 1, 1);

                //get Prof. qual
                drs = profqualtbl.Select("ApplicationCode = '" + applicationCode + "'");
                qualification = "";

                foreach (DataRow drstemp in drs)
                {
                    qualification = qualification + drstemp["ProfQual"].ToString() + "|";
                }

                if (qualification.Length > 0)
                    item.ProfQual = qualification.Remove(qualification.Length - 1, 1);


                //get work exp. 
                drs = workexptbl.Select("ApplicationCode = '" + applicationCode + "'");
                qualification = "";

                foreach (DataRow drstemp in drs)
                {
                    qualification = qualification + drstemp["WorkExp"].ToString() + "|";
                }

                if (qualification.Length > 0)
                    item.WorkExp = qualification.Remove(qualification.Length - 1, 1);


                list.Add(item);
            }

            return list;
        }

        public List<FullReportModel> GetFullReportData(string intakeCode, string currentStage, int? freezeNo = 0, bool showAll = false, string currentStatus = "PASS")
        {
            DataTable maintbl, resulttbl, mresulttbl, highertbl, profqualtbl, workexptbl, remarkstbl;
            string sql;
            List<FullReportModel> list = new();

            //get applicant main data
            string whereClause;
            string fieldList1 = "A.ApplicationCode, A.AppliedDate, A.Salutation, A.Initials, A.Surname, A.Fullname, Concat(A.Initials,' ',A.Surname, ' ') as NameWithInitials, A.NIC, A.DOB, A.AgeYears, A.AgeMonths, A.AgeDays, A.Overage";
            string fieldList2 = "A.HouseNo, A.AddressLine1, A.AddressLine2, A.AddressLine3, A.AddressLine4, A.Email, A.ContactNo1, A.ContactNo2";
            //string fieldList3 = "F.CurrentStatus,F.CurrentStage,Stage4Remarks F.FinalRemarks";
            string fieldList3 = "F.CurrentStatus,F.CurrentStage,CASE WHEN F.Stage4Remarks IS NOT NULL AND F.Stage4Remarks != '' THEN CONCAT(F.Stage4Remarks, '|', F.FinalRemarks) ELSE F.FinalRemarks END AS FinalRemarks";

            maintbl = new();

            if (currentStage == "FINAL")
            {
                if (string.IsNullOrWhiteSpace(currentStatus))
                    currentStatus = "PASS";

                //get current stage FINAL passed applicants of the selected intakecode
                sql = "SELECT " + fieldList1 + "," + fieldList2 + "," + fieldList3 + " FROM FilteredData F INNER JOIN Application A ON F.ApplicationCode = A.ApplicationCode WHERE F.CurrentStage = 'FINAL' AND F.CurrentStatus = '" + currentStatus + "' AND F.IntakeCode = '" + intakeCode + "'";
                if (freezeNo.HasValue && freezeNo > 0)
                {
                    sql = sql + " AND FreezeNo = " + freezeNo.Value;
                }
                else
                {
                    sql = sql + " AND F.FreezeNo IS NULL";
                }

                maintbl = _DBOperations.SelectRows(sql);
            }
            else if (currentStage == "FAIL")
            {
                //get current status failed applicants of the selected intakecode, current stage can be any
                sql = "SELECT " + fieldList1 + "," + fieldList2 + "," + fieldList3 + " FROM FilteredData F INNER JOIN Application A ON F.ApplicationCode = A.ApplicationCode WHERE F.CurrentStatus = 'FAIL' AND F.IntakeCode = '" + intakeCode + "'";
                if (freezeNo.HasValue && freezeNo > 0)
                {
                    sql = sql + " AND FreezeNo = " + freezeNo.Value;
                }
                else
                {
                    sql = sql + " AND F.FreezeNo IS NULL";
                }
                maintbl = _DBOperations.SelectRows(sql);
            }
            else
            {
                sql = "SELECT " + fieldList1 + "," + fieldList2 + " FROM Application A  WHERE A.IntakeCode = '" + intakeCode + "'";
                if (!showAll)
                {
                    sql = sql + " AND A.SaveStatus = 'OK'";
                }
                maintbl = _DBOperations.SelectRows(sql);
            }

            //get grades of both A/L & O/L of all the applicants of the selected intakecode
            sql = "SELECT A.ApplicationCode, A.ExamCode, A.Attempt, A.Grade, Count(A.Grade) AS GradeCount FROM SEResult A " +
                  "INNER JOIN Application B ON A.ApplicationCode = B.ApplicationCode " +
                  "WHERE B.IntakeCode = '" + intakeCode + "'" +
                  "GROUP BY A.ApplicationCode, A.ExamCode, A.Attempt, A.Grade " +
                  "ORDER BY A.ApplicationCode";

            resulttbl = _DBOperations.SelectRows(sql);

            //get mandatory subject results of all applicants of the selected intake code
            sql = "SELECT A.ApplicationCode, A.Attempt, A.Grade, Count(A.Grade) AS GradeCount FROM SEResult A " +
                  "INNER JOIN Application B ON A.ApplicationCode = B.ApplicationCode " +
                  "INNER JOIN Subject S ON A.SubjectName = S.SubjectName AND A.ExamCode = S.ExamCode " +
                  "WHERE B.IntakeCode = '" + intakeCode + "' AND A.ExamCode = 'O/L' AND S.Mandatory = 'YES' " +
                  "GROUP BY A.ApplicationCode, A.Attempt, A.Grade " +
                  "ORDER BY A.ApplicationCode";

            mresulttbl = _DBOperations.SelectRows(sql);

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

            //get work experience of all the applicants in the selected intakecode
            sql = "SELECT Q.ApplicationCode, (Q.CompanyName + '-' + Q.PositionHeld + '-' + JobStatus) AS WorkExp FROM WorkExperience Q " +
                  "INNER JOIN Application A ON A.ApplicationCode = Q.ApplicationCode " +
                  "WHERE A.IntakeCode = '" + intakeCode + "' " +
                  "ORDER BY A.ApplicationCode";

            workexptbl = _DBOperations.SelectRows(sql);

            //get remarks of all the applicants in the selected intakecode
            sql = "SELECT D.ApplicationCode, D.Remarks FROM OtherDocument D " +
                  "INNER JOIN Application A ON A.ApplicationCode = D.ApplicationCode " +
                  "WHERE A.IntakeCode = '" + intakeCode + "' " +
                  "ORDER BY A.ApplicationCode";

            remarkstbl = _DBOperations.SelectRows(sql);

            string applicationCode = "";
            string grades = "", qualification = "";
            DataRow[] drs;

            foreach (DataRow dr in maintbl.Rows)
            {
                applicationCode = dr["ApplicationCode"].ToString();
                grades = "";
                qualification = "";

                FullReportModel item = new();
                item.ApplicationCode = applicationCode;
                DateTime appliedDateTime = Convert.ToDateTime(dr["AppliedDate"]);
                item.AppliedDate = appliedDateTime.ToString("yyyy-MM-dd");
                item.AppliedTime = appliedDateTime.ToString("HH:mm");
                item.Salutation = dr["Salutation"].ToString();
                item.Initials = dr["Initials"].ToString();
                item.Surname = dr["Surname"].ToString();
                item.FullName = dr["FullName"].ToString();
                item.NameWithInitials = dr["NameWithInitials"].ToString();

                item.NIC = dr["NIC"].ToString();
                item.DOB = Convert.ToDateTime(dr["DOB"]);
                item.AgeYears = dr["AgeYears"].ToString();
                item.AgeMonths = dr["AgeMonths"].ToString();
                item.AgeDays = dr["AgeDays"].ToString();
                item.Overage = dr["Overage"].ToString();

                item.HouseNo = dr["HouseNo"].ToString();
                item.AddressLine1 = dr["AddressLine1"].ToString();
                item.AddressLine2 = dr["AddressLine2"].ToString();
                item.AddressLine3 = dr["AddressLine3"].ToString();
                item.AddressLine4 = dr["AddressLine4"].ToString();

                item.ContactNo1 = dr["ContactNo1"].ToString();
                item.ContactNo2 = dr["ContactNo2"].ToString();
                item.Email = dr["Email"].ToString();

                if (currentStage == "FINAL" || currentStage == "FAIL")
                {
                    item.CurrentStatus = dr["CurrentStatus"].ToString();
                    item.Stage = dr["CurrentStage"].ToString();
                    item.FinalRemarks = dr["FinalRemarks"].ToString();
                }

                //get A/L results
                drs = resulttbl.Select("ApplicationCode = '" + applicationCode + "' AND ExamCode = 'A/L' AND Attempt = 1");
                foreach (DataRow drtemp in drs)
                {
                    grades = grades + drtemp["Grade"].ToString() + drtemp["GradeCount"].ToString() + ",";
                }

                item.ALGrades = grades;

                //get O/L results
                for (int i = 1; i <= 3; i++)
                {
                    grades = "";

                    drs = resulttbl.Select("ApplicationCode = '" + applicationCode + "' AND ExamCode = 'O/L' AND Attempt = '" + i.ToString() + "'");
                    foreach (DataRow drtemp in drs)
                    {
                        grades = grades + drtemp["Grade"].ToString() + drtemp["GradeCount"].ToString() + ",";
                    }

                    if (grades.Length > 0)
                        item.OLGrades = item.OLGrades + grades + "|";
                }

                if (!String.IsNullOrEmpty(item.OLGrades))
                    item.OLGrades = item.OLGrades.Remove(item.OLGrades.Length - 1, 1);

                //get Mandatory O/L Subject results
                for (int i = 1; i <= 3; i++)
                {
                    grades = "";

                    drs = mresulttbl.Select("ApplicationCode = '" + applicationCode + "' AND Attempt = '" + i.ToString() + "'");
                    foreach (DataRow drtemp in drs)
                    {
                        grades = grades + drtemp["Grade"].ToString() + drtemp["GradeCount"].ToString() + ",";
                    }

                    if (grades.Length > 0)
                        item.OLMandatoryGrades = item.OLMandatoryGrades + grades + "|";
                }

                if (!String.IsNullOrEmpty(item.OLMandatoryGrades))
                    item.OLMandatoryGrades = item.OLMandatoryGrades.Remove(item.OLMandatoryGrades.Length - 1, 1);

                //get HE qual. 
                drs = highertbl.Select("ApplicationCode = '" + applicationCode + "'");
                qualification = "";
                var attrName = "HEQual";
                int ind = 1;

                foreach (DataRow drstemp in drs)
                {
                    //qualification = qualification + drstemp["HEQual"].ToString() + "|";
                    attrName = "HEQual" + ind.ToString();
                    //item.HEQual1 = drstemp["HEQual"].ToString();
                    item.GetType().GetProperty(attrName).SetValue(item, drstemp["HEQual"].ToString());
                    ind++;
                }

                //if (qualification.Length > 0)
                //    item.HEQual1 = qualification.Remove(qualification.Length - 1, 1);

                //get Prof. qual
                drs = profqualtbl.Select("ApplicationCode = '" + applicationCode + "'");
                qualification = "";

                foreach (DataRow drstemp in drs)
                {
                    qualification = qualification + drstemp["ProfQual"].ToString() + "|";
                }

                if (qualification.Length > 0)
                    item.ProfQual = qualification.Remove(qualification.Length - 1, 1);


                //get work exp. 
                drs = workexptbl.Select("ApplicationCode = '" + applicationCode + "'");
                qualification = "";

                foreach (DataRow drstemp in drs)
                {
                    qualification = qualification + drstemp["WorkExp"].ToString() + "|";
                }

                if (qualification.Length > 0)
                    item.WorkExp = qualification.Remove(qualification.Length - 1, 1);


                //get remarks 
                drs = remarkstbl.Select("ApplicationCode = '" + applicationCode + "'");

                if (drs.Length > 0)
                    item.Remarks = drs[0]["Remarks"].ToString();

                list.Add(item);
            }

            return list;
        }
    }
}
