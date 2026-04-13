using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GreenSwamp.Pages
{
    public class SubscribeModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}
