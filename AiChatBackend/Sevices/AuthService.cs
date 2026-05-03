using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AiChatBackend.Sevices
{
    

   
        public class AuthService
        {
            
            public string HashPassword(string password)
            {
                return BCrypt.Net.BCrypt.HashPassword(password);
            }

            // 🔐 Verify password
            public bool VerifyPassword(string hash, string password)
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }

        // 📧 Email validation
        public (bool IsValid, string Error) ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required");

            try
            {
                var addr = new MailAddress(email);
                return (true, "");
            }
            catch
            {
                return (false, "Invalid email format");
            }
        }

        // 🔑 Password validation
        public (bool IsValid, string Error) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required");

            if (password.Length < 8)
                return (false, "Password must be at least 8 characters");

            if (!Regex.IsMatch(password, "[A-Z]"))
                return (false, "Must contain uppercase letter");

            if (!Regex.IsMatch(password, "[a-z]"))
                return (false, "Must contain lowercase letter");

            if (!Regex.IsMatch(password, @"\d"))
                return (false, "Must contain a number");

            if (!Regex.IsMatch(password, @"[\W_]"))
                return (false, "Must contain a special character");

            return (true, "");
        }
    }
    
}
