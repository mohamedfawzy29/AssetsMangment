using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AssetsMangment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthController(
            ApplicationDbContext context,
            IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = new PasswordHasher<User>();
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            var userExists = await _context.Users.AnyAsync(u => u.UserName == request.UserName);

            if (userExists)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid Input",
                    Detail = "Username already exists."
                });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = AssetUtilities.NormalizeValue(request.UserName)
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Created(string.Empty, null);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var normalizedUserName = AssetUtilities.NormalizeValue(request.UserName);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == normalizedUserName);

            if (user == null)
            {
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Authentication failed",
                    Detail = "Invalid username or password."
                });
            }
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Authentication failed",
                    Detail = "Invalid username or password."
                });
            }

            var token = _jwtService.GenerateToken(user);
            var response = new AuthResponse
            {
                Token = token
            };

            return Ok(response);
        }
    }
}
