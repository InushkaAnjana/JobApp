using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JobAppHR.Models
{
    public class JobPosition
    {
        [Required]
        public int JobPositionID { get; set; }

        [DisplayName("Job Position Name")]
        [MaxLength(50)]
        [Required]
        public string JobPositionName { get; set; } = string.Empty;

        [DisplayName("Job Position Code")]
        [MaxLength(50)]
        [Required]
        public string JobPositionCode { get; set; } = string.Empty;

        [DisplayName("Job Template")]
        [MaxLength(50)]
        [Required]
        public string JobTemplate { get; set; } = string.Empty;

        [DisplayName("Talent Pool Applicable Job?")]
        public string TalentPoolJob { get; set; } = string.Empty;

        [DisplayName("O/L Required")]
        [Required]
        public int OLRequired { get; set; }

        [DisplayName("A/L Required")]
        [Required]
        public int ALRequired { get; set; }

        [DisplayName("Higher Education Required")]
        [Required]
        public int HERequired { get; set; }

        [DisplayName("Professional Qualification Required")]
        [Required]
        public int PQRequired { get; set; }

        [DisplayName("Work Experience Required")]
        [Required]
        public int WERequired { get; set; }

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
}
