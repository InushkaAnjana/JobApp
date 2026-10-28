using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JobAppHR.Models
{
    public class Intake
    {
        [Required]
        public int IntakeID { get; set; }

        [DisplayName("Job Position")]
        [Required]
        public int JobPositionID { get; set; }

        [DisplayName("Start Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime StartDate { get; set; }

        [DisplayName("Closing Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime ClosingDate { get; set; }

        [DisplayName("Age Limit")]
        [Required]
        public int AgeLimit { get; set; }

        [DisplayName("Intake Year & Month")]
        public string IntakeYearMonth { get; set; }

        [DisplayName("Intake Code")]
        [MaxLength(50)]
        [Required]
        public string IntakeCode { get; set; } = string.Empty;

        [DisplayName("Filter Mode")]
        [Required]
        public string FilterMode { get; set; } = string.Empty;

        [DisplayName("O/L Required")]
        public int OLRequired { get; set; }

        [DisplayName("A/L Required")]
        public int ALRequired { get; set; }

        [DisplayName("Higher Edu. Required")]
        public int HERequired { get; set; }

        [DisplayName("Final List Confirmed?")]
        public int FinalConfirmed { get; set; }

        [DisplayName("Final List Confirmed By")]
        public string? FinalConfirmedBy { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm:ss tt}")]
        [DisplayName("Final List Confirmed On")]
        public DateTime? FinalConfirmedOn { get; set; } = null;

        [DisplayName("Inserted By")]
        public string? InsertedBy { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm:ss tt}")]
        [DisplayName("Inserted On")]
        public DateTime? InsertedOn { get; set; } = null;

        [DisplayName("Updated By")]
        public string? UpdatedBy { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm:ss tt}")]
        [DisplayName("Updated On")]
        public DateTime? UpdatedOn { get; set; } = null;
    }

    public class IntakeViewModel: Intake
    {
        [DisplayName("Job Position Name")]
        [Required]
        public string JobPositionName { get; set; }
    }

    public class TalentPoolIntake
    {
        [Required]
        public int IntakeID { get; set; }

        [DisplayName("Job Position")]
        [Required]
        public int JobPositionID { get; set; }

        [DisplayName("Intake Code")]
        [MaxLength(50)]
        [Required]
        public string IntakeCode { get; set; } = string.Empty;

        [DisplayName("Age Limit")]
        [Required]
        public int AgeLimit { get; set; }

        [DisplayName("Filter Mode")]
        [Required]
        public string FilterMode { get; set; } = "MANUAL";

        [DisplayName("O/L Required")]
        public int OLRequired { get; set; }

        [DisplayName("A/L Required")]
        public int ALRequired { get; set; }

        [DisplayName("Higher Edu. Required")]
        public int HERequired { get; set; }
        public string? InsertedBy { get; set; } = string.Empty;
        public DateTime? InsertedOn { get; set; } = null;

    }
}
