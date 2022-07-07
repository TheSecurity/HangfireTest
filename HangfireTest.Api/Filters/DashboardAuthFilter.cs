using Hangfire.Dashboard;

namespace HangfireTest.Api.Filters
{
    public class DashboardAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // Allow all users to open dashboard
            return true;
        }
    }
}
