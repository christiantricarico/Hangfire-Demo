﻿using Hangfire.Annotations;
using Hangfire.Dashboard;
using System.Net.Http.Headers;
using System.Text;

namespace Hangfire.Sqlite.Dashboard;

public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly DashboardAuthorizationFilterOptions _options;

    public DashboardAuthorizationFilter() : this(new DashboardAuthorizationFilterOptions())
    {
    }

    public DashboardAuthorizationFilter(DashboardAuthorizationFilterOptions options)
    {
        _options = options;
    }

    private bool Challenge(HttpContext context)
    {
        context.Response.StatusCode = 401;
        context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
        return false;
    }

    public bool Authorize([NotNull] DashboardContext _context)
    {
        var context = _context.GetHttpContext();
        if (_options.SslRedirect == true && context.Request.Scheme != "https")
        {
            string redirectUri = new UriBuilder("https", context.Request.Host.ToString(), 443, context.Request.Path).ToString();

            context.Response.StatusCode = 301;
            context.Response.Redirect(redirectUri);
            return false;
        }

        if (_options.RequireSsl == true && context.Request.IsHttps == false)
        {
            return false;
        }

        string? header = context.Request.Headers.Authorization;

        if (string.IsNullOrWhiteSpace(header) == false)
        {
            AuthenticationHeaderValue authValues = AuthenticationHeaderValue.Parse(header);

            if ("Basic".Equals(authValues.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                string parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
                var parts = parameter.Split(':');

                if (parts.Length > 1)
                {
                    string login = parts[0];
                    string password = parts[1];

                    if (string.IsNullOrWhiteSpace(login) == false && string.IsNullOrWhiteSpace(password) == false)
                    {
                        bool anyUser = _options.Users
                            .Any(user => user.Validate(login, password, _options.LoginCaseSensitive));

                        return anyUser || Challenge(context);
                    }
                }
            }
        }

        return Challenge(context);
    }
}
