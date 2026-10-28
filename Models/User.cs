using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace JobAppHR.Models
{
    public class User
    {
        [DisplayName("User Service No")]
        [MaxLength(6)]
        [Required]
        public string UserId { get; set; }

        [DisplayName("User Name")]
        [MaxLength(50)]
        [Required]
        public string UserName { get; set; }

        [DisplayName("Email")]
        [MaxLength(50)]
        public string UserEmail { get; set; }

        [DisplayName("Group")]
        public string UserGroup { get; set; }

        [DisplayName("Group Name")]
        public string GroupName { get; set; }
        public string UserRole { get; set; }

        [DisplayName("Active Status")]
        public string ActiveStatus { get; set; }
    }

}
