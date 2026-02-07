using System.Text.RegularExpressions;

namespace LuauGotoStripper;

public static class Cleaner
{
    private static readonly Regex LabelLine = new(@"^\s*::label_\d+::\s*(--.*)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex GotoLine = new(@"^\s*goto\s+label_\d+\b.*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex IfThenGotoEnd = new(@"\bthen\s+goto\s+label_\d+\s+end\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex LabelToken = new(@"::label_\d+::",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex GotoToken = new(@"\bgoto\s+label_\d+\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex CommentHasGotoOrLabel = new(@"\bgoto\s+label_\d+\b|::label_\d+::",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string Clean(string input, out int removedLines)
    {
        removedLines = 0;

        var newline = input.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var endsWithNewline = input.EndsWith(newline, StringComparison.Ordinal);

        var lines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var outLines = new List<string>(lines.Length);

        foreach (var rawLine in lines)
        {
            var line = rawLine;

            if (LabelLine.IsMatch(line) || GotoLine.IsMatch(line))
            {
                removedLines++;
                continue;
            }

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("--", StringComparison.Ordinal) &&
                CommentHasGotoOrLabel.IsMatch(trimmed))
            {
                removedLines++;
                continue;
            }

            if (line.Contains("goto", StringComparison.Ordinal))
            {
                var replaced = IfThenGotoEnd.Replace(line, "then end");
                if (replaced != line)
                {
                    line = replaced;
                }
                else if (GotoToken.IsMatch(line))
                {
                    removedLines++;
                    continue;
                }
            }

            if (line.Contains("::label_", StringComparison.Ordinal))
            {
                line = LabelToken.Replace(line, "");
            }

            var commentIdx = line.IndexOf("--", StringComparison.Ordinal);
            if (commentIdx >= 0)
            {
                var comment = line.Substring(commentIdx);
                if (CommentHasGotoOrLabel.IsMatch(comment))
                {
                    line = line.Substring(0, commentIdx).TrimEnd();
                }
            }

            outLines.Add(line);
        }

        var output = string.Join(newline, outLines);
        if (endsWithNewline)
            output += newline;

        return output;
    }
}
