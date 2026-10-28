using DocumentFormat.OpenXml.Drawing.Charts;
using Fingers10.ExcelExport.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JobAppHR.Models
{
    public class AgeFilter: ApplicantViewModel
    {
        public string Stage { get; set; } = "1";

        [DisplayName("Eligible")] 
        public string CurrentStatus { get; set; } = "PASS";
        public string Remarks { get; set; }
    }

    public class ALFilter : ApplicantViewModel
    {
        public string Stage { get; set; } = "2";

        [DisplayName("Exam")]
        public string ExamCode { get; set; }

        [DisplayName("Year")]
        public string ExamYear { get; set; }

        public string Grades { get; set; }

        [DisplayName("Eligible")]
        public string CurrentStatus { get; set; } = "PASS";
        public string Remarks { get; set; }
    }

    public class OLFilter : ApplicantViewModel
    {
        public string Stage { get; set; } = "3";

        [DisplayName("Exam")]
        public string ExamCode { get; set; } = "";

        [DisplayName("Year")]
        public string ExamYear { get; set; } = "";    
        public string Attempt { get; set; } = "";
        public string Grades { get; set; } = "";
        public string MandatoryGrades { get; set; } = "";

        [DisplayName("Eligible")]
        public string CurrentStatus { get; set; } = "PASS";
        public string Remarks { get; set; }
    }

    public class HEPQFilter : ApplicantViewModel
    {
        public string Stage { get; set; } = "4";

        [DisplayName("Higher Education")]
        public string? HEQual { get; set; }

        [DisplayName("Professional Qualification")]
        public string? ProfQual { get; set; }

        [DisplayName("Eligible")]
        public string CurrentStatus { get; set; } = "TO-CHECK";
        public string? Remarks { get; set; }
    }

    public class FinalFilter : ApplicantViewModel
    {
        public string Stage { get; set; } = "5";

        [DisplayName("Work Experience")]
        public string WorkExp { get; set; }

        [DisplayName("Eligible")]
        public string CurrentStatus { get; set; } = "PASS";
        public string Remarks { get; set; }
    }

    public class FailedApplicants : ApplicantViewModel
    {
        [DisplayName("Failed @ Stage")]
        public string FailedStage { get; set; }

        [DisplayName("Status")]
        public string FailedStatus { get; set; }

        [DisplayName("Reason for Failure")]
        public string? FailedRemarks { get; set; }

        [DisplayName("Eligible")]
        public string CurrentStatus { get; set; } = "FAIL";
        public string? Remarks { get; set; }
    }

    public class FilterProgress: ApplicantViewModel
    {
        public string Stage { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public string UpdatedBy { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm:ss tt}")]
        public DateTime? UpdatedOn { get; set; }
    }

    public class FilterSummary
    {
        [DisplayName("Intake Code")]
        public string IntakeCode { get; set; }

        [DisplayName("Current Stage")]
        public string CurrentStage { get; set; }

        [DisplayName("Filtered By")]
        public string StageName { get; set; }

        [DisplayName("Count")]
        public string StageCount { get; set; }

        [DisplayName("Last Processed Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm:ss tt}")]
        public DateTime? LastProcessedDate { get; set; }
    }

    public class FreezeSummary
    {
        [DisplayName("Intake Code")]
        public string IntakeCode { get; set; }

        [DisplayName("Freeze No")]
        public int FreezeNo { get; set; }

        [DisplayName("Freezed Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm:ss tt}")]
        public DateTime? FreezedOn { get; set; }
        public string FreezedBy { get; set; }
    }

    public class ShortListed : ApplicantViewModel
    {
        [DisplayName("Shortlisted to Exam")]
        public string? ExamSelected { get; set; }

        [DisplayName("Shortlisted to Interview")]
        public string? InterviewSelected { get; set; }

        [DisplayName("Final Selected")]
        public string? JobSelected { get; set; }
    }

    public class ManualFilter: ApplicantViewModel
    {
        public string Stage { get; set; } = "FINAL";

        [DisplayName("O/L")]
        public string? OLGrades { get; set; }

        [DisplayName("M-O/L")]
        public string? OLMandatoryGrades { get; set; }

        [DisplayName("A/L")]
        public string? ALGrades { get; set; }

        [DisplayName("Higher Edu.")]
        public string? HEQual { get; set; }

        [DisplayName("Prof. Qual.")]
        public string? ProfQual { get; set; }

        [DisplayName("Work Exp.")]
        public string? WorkExp { get; set; }

        [DisplayName("Eligible")]
        public string CurrentStatus { get; set; } = "TO-CHECK";
        public string? Remarks { get; set; }
    }

}
