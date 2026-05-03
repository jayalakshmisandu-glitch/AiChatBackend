using System.ComponentModel.DataAnnotations;

namespace AiChatBackend.DTOs
{
    public class LoginDto
    {

        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
        
}
