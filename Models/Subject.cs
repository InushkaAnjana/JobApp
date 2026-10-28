using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace JobAppHR.Models
{
    public class Subject
    {
        [Required]
        public int SubjectId { get; set; }

        [DisplayName("Subject Name")]
        [MaxLength(50)]
        [Required]
        public string SubjectName { get; set; } = string.Empty;

        [DisplayName("Exam Code")]
        public string ExamCode { get; set; } = string.Empty;

        public string Mandatory { get; set; } = "NO";
    }
}
