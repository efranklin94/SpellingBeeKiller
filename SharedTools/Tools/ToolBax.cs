using System.Text.RegularExpressions;

namespace SharedTools.Tools;

public static class ToolBax
{
    public static bool ValidateUserName(string username)
    {
        Regex rx = new Regex(@"^(?=.{4,15}$)(\S+$)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        MatchCollection matches = rx.Matches(username);

        return matches.Count == 1;
    }
}
