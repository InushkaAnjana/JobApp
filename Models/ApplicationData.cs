using MessagePack;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace JobAppHR.Models
{
    public class ApplicationData
    {
        public PersonalData PersonalData { get; set; }
        public SEExam OLExam1 { get; set; }
        public List<SEResult> OLResults1 { get; set; }
        public SEExam OLExam2 { get; set; } = new SEExam();
        public List<SEResult> OLResults2 { get; set; }
        public SEExam OLExam3 { get; set; } = new SEExam();
        public List<SEResult> OLResults3 { get; set; }
        public SEExam ALExam { get; set; } = new SEExam();
        public List<SEResult> ALResults { get; set; }
        public List<HEQualification>? HEQualifications { get; set; }
        public List<ProfQualification> ProfQualifications { get; set; }
        public List<WorkExperience> WorkExperiences { get; set; }
        public OtherDocument OtherDocuments { get; set; }
        public DocumentName DocumentNames { get; set; }
        public List<FilterProgress> FilterProgress { get; set; }
    }

    public class PersonalData
    {
        public int ApplicationID { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm tt}")]
        [DisplayName("Applied Date/Time")]
        public DateTime AppliedDate { get; set; }
        public string IntakeCode { get; set; }
        public string Salutation { get; set; }

        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Only alphabetic characters allowed.")]
        public string Initials { get; set; }

        [RegularExpression(@"^[a-zA-z]+([\s][a-zA-Z]+)*$", ErrorMessage = "Only alphabetic characters & spaces allowed.")]
        public string Surname { get; set; }

        [DisplayName("Full Name")]
        [RegularExpression(@"^[a-zA-z]+([\s][a-zA-Z]+)*$", ErrorMessage = "Only alphabetic characters & spaces allowed.")]
        public string FullName { get; set; }

        [DisplayName("NIC Number")]
        [RegularExpression(@"^[0-9,V,v,X,x]{9,12}$", ErrorMessage = "9 digits with V/X or 12 digits allowed.")]
        public string NIC { get; set; }
        public string DrivingLicenseNo { get; set; }

        [DisplayName("Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }
        public string HouseNo { get; set; }       
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? AddressLine4 { get; set; }
        
        [RegularExpression(@"^[a-z0-9][-a-z0-9._]+@([-a-z0-9]+\.)+[a-z]{2,5}$", ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [DisplayName("Personal Mobile Number")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Only 10 digits numbers are valid.")]
        public string ContactNo1 { get; set; }

        [DisplayName("Secondary Contact Number")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Only 10 digits numbers are valid.")]
        public string? ContactNo2 { get; set; }
       
        
        //system calculated values
        public string Overage { get; set; }
        
        [DisplayName("Years")]
        public int AgeYears { get; set; }
        
        [DisplayName("Months")]
        public int AgeMonths { get; set; }
        
        [DisplayName("Days")]
        public int AgeDays { get; set; }

        [DisplayName("Application Reference")]
        public string ApplicationCode { get; set; }

    }

    public class SEExam
    {
        [HiddenInput]
        public string? ApplicationCode { get; set; }
        public string? ExamCode { get; set; }

        [DisplayName("Index Number")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Only numbers are valid.")]
        public string? IndexNumber { get; set; }

        [DisplayName("Exam Year")]
        public int ExamYear { get; set; }
        public int Attempt { get; set; }

        [DisplayName("Attach Certificate (as a pdf, jpeg, jpg, png)")]
        //[FileExtensions(Extensions = "pdf,jpg,jpeg",ErrorMessage = "Only pdf, jpg, jpeg files are allowed as attachments.")]
        public IFormFile? AttachmentName { get; set; }

    }

    public class SEResult
    {
        [HiddenInput]
        public string? ApplicationCode { get; set; }
        public string? SubjectName { get; set; }
        public int? Attempt { get; set; }
        public string? ExamCode { get; set; }
        public string? Grade { get; set; }
    }

    public class HEQualification
    {
        [HiddenInput]
        public string? ApplicationCode { get; set; } = string.Empty;

        [DisplayName("Qualification")]
        public string? QualType { get; set; } = string.Empty;

        [DisplayName("Qualification Name")]
        public string? QualName { get; set; } = string.Empty;

        public string? OtherQualName { get; set; } = string.Empty;

        [DisplayName("Institute/University")]
        public string? HEInstituteName { get; set; } = string.Empty;

        public string? OtherInstitute { get; set; } = string.Empty;

        [DisplayName("NVQ Level")]
        public string? NVQLevel { get; set; } = string.Empty;

        [DisplayName("Specialized Area")]
        public string? SpecializedArea { get; set; } = string.Empty;

        [DisplayName("Current Status")]
        public string? QualStatus { get; set; } = string.Empty;

        [DisplayName("Awarded Year/Month")]
        public string? AwardedYearMonth { get; set; } = string.Empty;

        [DisplayName("Attach Certificate/Transcript (as a pdf, jpeg, jpg, png)")]
        public IFormFile? AttachmentName { get; set; }

    }

    public class ProfQualification
    {
        [HiddenInput]
        public string? ApplicationCode { get; set; } = string.Empty;

        [DisplayName("Membership Type")]
        public string? MembershipType { get; set; } = string.Empty;

        [DisplayName("Institute/University")] 
        public string? PQInsituteName { get; set; } = string.Empty;

        [DisplayName("Membership Number")]
        public string? MembershipNo { get; set; } = string.Empty;

        [DisplayName("Attach Membership Certificate (as a pdf, jpeg, jpg, png)")]
        public IFormFile? AttachmentName { get; set; }
    }

    public class WorkExperience
    {
        [HiddenInput]
        public string? ApplicationCode { get; set; } = string.Empty;

        [DisplayName("Company Name")]
        public string? CompanyName { get; set; } = string.Empty;

        [DisplayName("Position Held")]
        public string? PositionHeld { get; set; } = string.Empty;

        [DisplayName("Nature of Employment")]
        public string? EmploymentNature { get; set; } = string.Empty;

        [DisplayName("Current Status")]
        public string? JobStatus { get; set; } = string.Empty;

        [DisplayName("Date of Joining")]
        public string? StartYearMonth { get; set; } = string.Empty;

        [DisplayName("Date of Leaving")]
        public string? EndYearMonth { get; set; } = string.Empty;

        [DisplayName("Attach Service Letter (if available as a pdf, jpeg, jpg, png)")]
        public IFormFile? AttachmentName { get; set; }
    }

    public class OtherDocument
    {
        [HiddenInput]
        public string? ApplicationCode { get; set; } = string.Empty;

        [DisplayName("Cirriculum Vitae")]
        public IFormFile? CVName { get; set; }

        [DisplayName("NIC")]
        public IFormFile? NICName { get; set; }

        [DisplayName("Birth Certificate")]
        public IFormFile? BCName { get; set; }

        [DisplayName("Driving License")]
        public IFormFile? DLName { get; set; }

        public string? Remarks { get; set; } = string.Empty;

    }

    public class DocumentName
    {
        [HiddenInput]
        public string? OLExam_Attempt1 { get; set;}
        public string? OLExam_Attempt2 { get; set; }
        public string? OLExam_Attempt3 { get; set; }
        public string? ALExam_Attempt1 { get; set; }
        public string[]? HEQualification { get; set; }
        public string[]? ProfQualification { get; set; }
        public string[]? ServiceLetter { get; set; }

        [DisplayName("Cirriculum Vitae")]
        public string? CVName { get; set; }
        
        [DisplayName("NIC")]
        public string? NICName { get; set; }
        
        [DisplayName("Birth Certificate")] 
        public string? BCName { get; set; }
        
        [DisplayName("Driving License")] 
        public string? DLName { get; set; }

        [DisplayName("Remarks")]
        public string? Remarks { get; set;}
    }

    //public class Progress
    //{
    //    public string Stage { get; set; }
    //    public string Status { get; set; }
    //    public string Remarks { get; set; }
    //    public string UpdatedBy { get; set; }

    //    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm:ss tt}")]
    //    public DateTime? UpdatedOn { get; set; }
    //}
}
