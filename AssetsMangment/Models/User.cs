namespace AssetsMangment.Models
{
    public class User
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
    }
}
