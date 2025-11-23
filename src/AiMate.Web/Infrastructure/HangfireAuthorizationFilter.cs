using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace AiMate.Web;

/// <summary>
/// Authorization filter for Hangfire dashboard - only allow authenticated admin users
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // There is no direct way to get HttpContext from DashboardContext or DashboardRequest.
        // You may need to use a custom middleware or dependency injection to access HttpContext.
        // For now, deny access if HttpContext is not available.
        return false;
    }
}
