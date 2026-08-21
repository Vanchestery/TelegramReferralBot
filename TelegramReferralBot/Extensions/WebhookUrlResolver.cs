namespace ReferralBot.Extensions;

internal static class WebhookUrlResolver
{
    /// <summary>
    /// VS Dev Tunnels inject <c>VS_TUNNEL_URL</c> when F5 with an active public tunnel.
    /// Falls back to <c>REF_BOT_WEBHOOK_URL</c> (user-secrets / env).
    /// </summary>
    public static string? Resolve(IConfiguration config)
    {
        foreach (var key in new[] { "VS_TUNNEL_URL", "REF_BOT_WEBHOOK_URL" })
        {
            var value = config[key];
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim().TrimEnd('/');
        }

        return null;
    }
}
