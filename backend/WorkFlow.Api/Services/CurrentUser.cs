using System.Security.Claims;
using WorkFlow.Api.Models;

namespace WorkFlow.Api.Services;

// Gir services tilgang til hvem som er innlogget, hentet fra JWT-tokenet
public interface ICurrentUser
{
    int UserId { get; }
    int CompanyId { get; }
    UserRole Role { get; }
    int? CustomerId { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal _principal;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _principal = accessor.HttpContext?.User ?? new ClaimsPrincipal();
    }

    public int UserId => int.Parse(Require(ClaimTypes.NameIdentifier));
    public int CompanyId => int.Parse(Require("companyId"));
    public UserRole Role => Enum.Parse<UserRole>(Require(ClaimTypes.Role));

    public int? CustomerId
    {
        get
        {
            var value = _principal.FindFirst("customerId")?.Value;
            return value is null ? null : int.Parse(value);
        }
    }

    private string Require(string claimType)
    {
        return _principal.FindFirst(claimType)?.Value
            ?? throw new UnauthorizedAccessException("Ikke innlogget.");
    }
}