namespace AssetsMangment.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
