using System.ComponentModel.DataAnnotations;

namespace BlazorChatApp.Models 
{
    public class User 
    {
        [Required]
        [StringLength(15, ErrorMessage="Name too long")]
        public string UserName { get; set; }
    }
}