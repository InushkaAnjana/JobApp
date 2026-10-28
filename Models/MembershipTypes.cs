using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JobAppHR.Models
{
    public class MembershipTypes
    {
        [Required]
        public int MembershipTypeID { get; set; }

        [DisplayName("Membership Type")]
        [MaxLength(50)]
        [Required]
        public string MembershipType { get; set; } = string.Empty;
    }
}
