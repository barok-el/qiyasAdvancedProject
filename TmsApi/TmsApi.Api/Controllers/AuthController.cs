using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class AuthController : ControllerBase
{
    private readonly UserManager<TmsUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    
    private readonly TmsDbContext _context;
    private readonly TokenService _tokenService;


    public AuthController(
    UserManager<TmsUser> userManager,
    RoleManager<IdentityRole> roleManager,
    TmsDbContext context,
    TokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _tokenService = tokenService;
    }

    public record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName);
    public record RefreshRequest(string RefreshToken);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(
        string Email,
        string Token,
        string NewPassword);

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt =>
                rt.Token == request.RefreshToken);

        if (storedToken == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid refresh token."
            });
        }

        // Token was already used → possible token theft
        if (storedToken.IsUsed)
        {
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId)
                .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            return Unauthorized(new
            {
                detail =
                    "Token theft detected. All user sessions revoked."
            });
        }

        if (storedToken.IsRevoked ||
            storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized(new
            {
                detail = "Refresh token expired or revoked."
            });
        }

        // Invalidate current refresh token
        storedToken.IsUsed = true;

        // Generate replacement refresh token
        var newRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = storedToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshToken);

        var user =
            await _userManager.FindByIdAsync(storedToken.UserId);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "User no longer exists."
            });
        }

        var roles = await _userManager.GetRolesAsync(user);

        var newAccessToken =
            _tokenService.GenerateJwt(user, roles);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken.Token
        });
    }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            var existingUser =
                await _userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Account already exists",
                    Detail = "An account is already registered with this email address.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new TmsUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName
                };

                var result =
                    await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors
                        .Select(e => e.Description);

                    return BadRequest(new { errors });
                }

                const string publicRegistrationRole = "Student";

                if (!await _roleManager.RoleExistsAsync(publicRegistrationRole))
                {
                    var roleResult = await _roleManager.CreateAsync(
                        new IdentityRole(publicRegistrationRole));

                    if (!roleResult.Succeeded)
                    {
                        return BadRequest(new
                        {
                            errors = roleResult.Errors.Select(error => error.Description)
                        });
                    }
                }

                var assignmentResult = await _userManager.AddToRoleAsync(
                    user,
                    publicRegistrationRole);

                if (!assignmentResult.Succeeded)
                {
                    return BadRequest(new
                    {
                        errors = assignmentResult.Errors.Select(error => error.Description)
                    });
                }

                _context.Students.Add(new Student
                {
                    UserId = user.Id,
                    RegistrationNumber = $"TMS-{Guid.NewGuid():N}"[..20],
                    Name = $"{user.FirstName} {user.LastName}".Trim(),
                    GPA = 0m,
                    IsActive = true
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return Ok(new
            {
                message = "Registration successful."
            });
        }

    [EnableRateLimiting("AuthLimiter")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is not null)
        {
            _ = await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        return Accepted(new
        {
            message = "If an account exists for this email address, password reset instructions will be sent."
        });
    }

    [EnableRateLimiting("AuthLimiter")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Password reset failed",
                Detail = "The password reset request is invalid or has expired.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _userManager.ResetPasswordAsync(
            user,
            request.Token,
            request.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                errors = result.Errors.Select(error => error.Description)
            });
        }

        var activeRefreshTokens = await _context.RefreshTokens
            .Where(token => token.UserId == user.Id && !token.IsRevoked)
            .ToListAsync();

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.IsRevoked = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Password reset successfully. Please sign in with your new password."
        });
    }

    public record LoginRequest(
        string Email,
        string Password);

    [EnableRateLimiting("AuthLimiter")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user =
            await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(423, new
            {
                detail =
                    "Account locked due to multiple failed login attempts. " +
                    "Try again in 15 minutes."
            });
        }

        var validPassword =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);

            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        

        // Reset failed attempt counter on successful login
        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken =
            _tokenService.GenerateJwt(user, roles);
        
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken,
            refreshToken = refreshToken.Token,
            userId = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            roles
        });
    }
    
}
