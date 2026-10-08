namespace PFDETBM.Helpers
{
    public static class TimeAgoHelper
    {
        public static string Describe(DateTime timestamp)
        {
            var span = DateTime.Now - timestamp;

            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} hour{((int)span.TotalHours == 1 ? "" : "s")} ago";
            if (span.TotalDays < 30) return $"{(int)span.TotalDays} day{((int)span.TotalDays == 1 ? "" : "s")} ago";

            return timestamp.ToString("MMM d, yyyy");
        }
    }
}
