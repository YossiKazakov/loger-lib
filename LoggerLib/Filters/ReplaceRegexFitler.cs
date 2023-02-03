using System.Text.RegularExpressions;


namespace LoggerLib.Filters;

public class ReplaceRegexFilter : ILogFilter
{
    private readonly string _pattern;
    private readonly string _replaceWith;

    public ReplaceRegexFilter(string pattern, string replaceWith)
    {
        _pattern = pattern;
        _replaceWith = replaceWith;
    }

    public string Filter(string messageToFilter)
    {
        return Regex.Replace(messageToFilter, _pattern, _replaceWith);
    }
}