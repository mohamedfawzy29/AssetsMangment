namespace AssetsMangment.DTOs.AuthRequst
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
