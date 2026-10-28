using DocumentFormat.OpenXml.Drawing.Charts;
using Fingers10.ExcelExport.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JobAppHR.Models
{
    public class ApplicantViewModel
    {
        [DisplayName("Intake Code")]
        public string IntakeCode { get; set; }

        [DisplayName("Application Code")]
        public string? ApplicationCode { get; set; }

        [DisplayName("Full Name")]
        public string FullName   { get; set; }

        [DisplayName("Name With Initials")]
        public string? NameWithInitials { get; set; }

        public string? NIC { get; set; }

        public string? Age { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? DOB { get; set; }

        public string? Overage { get; set; }

        public string? Address { get; set; }

        public string? ContactNo { get; set; }

        public string? Email { get; set; }
    }

    public class FullReportModel
    {
        [DisplayName("Application Code")]
        [IncludeInReport(Order = 1)]
        public string ApplicationCode { get; set; }

        [DisplayName("Applied Date")]
        [IncludeInReport(Order = 2)]
        public string AppliedDate { get; set; }

        [DisplayName("Applied Time")]
        [IncludeInReport(Order = 3)]
        public string AppliedTime { get; set; }

        [IncludeInReport(Order = 4)]
        public string Salutation { get; set; }

        [IncludeInReport(Order = 5)]
        public string Initials { get; set; }

        [IncludeInReport(Order = 6)]
        public string Surname { get; set; }

        [IncludeInReport(Order = 7)]
        public string FullName { get; set; }

        [DisplayName("Name with Initials")]
        [IncludeInReport(Order = 8)]
        public string NameWithInitials { get; set; }

        [IncludeInReport(Order = 9)]
        public string NIC { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        [IncludeInReport(Order = 10)]
        public DateTime DOB { get; set; }

        [IncludeInReport(Order = 11)]
        public string Overage { get; set; }

        public string Age { get; set; }

        [IncludeInReport(Order = 12)]
        public string AgeYears { get; set; }

        [IncludeInReport(Order = 13)]
        public string AgeMonths { get; set; }

        [IncludeInReport(Order = 14)]
        public string AgeDays { get; set; }

        [IncludeInReport(Order = 15)]
        public string HouseNo { get; set; }

        [IncludeInReport(Order = 16)]
        public string AddressLine1 { get; set; }

        [IncludeInReport(Order = 17)]
        public string AddressLine2 { get; set; }

        [IncludeInReport(Order = 18)]
        public string AddressLine3 { get; set; }

        [IncludeInReport(Order = 19)]
        public string AddressLine4 { get; set; }

        [IncludeInReport(Order = 20)]
        public string ContactNo1 { get; set; }

        [IncludeInReport(Order = 21)]
        public string ContactNo2 { get; set; }

        [IncludeInReport(Order = 22)]
        public string Email { get; set; }

        [IncludeInReport(Order = 23)]
        public string Remarks { get; set; } //remarks by applicant

        //related tables data
        [DisplayName("O/L")]
        [IncludeInReport(Order = 24)]
        public string OLGrades { get; set; }

        [IncludeInReport(Order = 25)]
        public string OLMandatoryGrades { get; set; }

        [DisplayName("A/L")]
        [IncludeInReport(Order = 26)]
        public string ALGrades { get; set; }

        [DisplayName("Higher Edu.1")]
        [IncludeInReport(Order = 27)]
        public string HEQual1 { get; set; }

        [DisplayName("Higher Edu.2")]
        [IncludeInReport(Order = 28)]
        public string HEQual2 { get; set; }

        [DisplayName("Higher Edu.3")]
        [IncludeInReport(Order = 29)]
        public string HEQual3 { get; set; }

        [DisplayName("Prof. Qual.")]
        [IncludeInReport(Order = 30)]
        public string ProfQual { get; set; }

        [DisplayName("Work Exp.")]
        [IncludeInReport(Order = 31)]
        public string WorkExp { get; set; }

        [IncludeInReport(Order = 32)]
        public string CurrentStatus { get; set; }

        [IncludeInReport(Order = 33)]
        public string Stage { get; set; }

        [IncludeInReport(Order = 34)]
        public string FinalRemarks { get; set; }
    }

}
