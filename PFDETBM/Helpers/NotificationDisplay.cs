namespace PFDETBM.Helpers
{
    public static class NotificationDisplay
    {
        public static string Severity(string title)
        {
            if (title.Contains("exceeded", StringComparison.OrdinalIgnoreCase)) return "danger";
            if (title.Contains("close", StringComparison.OrdinalIgnoreCase)
                || title.Contains("approaching", StringComparison.OrdinalIgnoreCase)
                || title.Contains("limit", StringComparison.OrdinalIgnoreCase)) return "warning";
            if (title.Contains("goal", StringComparison.OrdinalIgnoreCase)
                || title.Contains("achieved", StringComparison.OrdinalIgnoreCase)
                || title.Contains("reached", StringComparison.OrdinalIgnoreCase)) return "success";
            return "info";
        }

        public static string Icon(string severity) => severity switch
        {
            "danger" => "⛔",
            "warning" => "⚠️",
            "success" => "✅",
            _ => "🔔"
        };
    }
}
