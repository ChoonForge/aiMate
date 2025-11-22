using Hangfire.Dashboard;

namespace AiMate.Web;

/// <summary>
/// Authorization filter for Hangfire dashboard - only allow authenticated admin users
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow access only to authenticated users with Admin role
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("Admin");
    }
}
