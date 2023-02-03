namespace LoggerLib.Filters
{
    public class RemoveSpecialCharactersFilter : ReplaceRegexFilter
    {
        public RemoveSpecialCharactersFilter()
            : base(@"[^ a-zA-Z0-9]+", "")
        { }
    }
}