using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Authentication.Queue.Models
{
    public class AuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            return true;
        }
    }
}
