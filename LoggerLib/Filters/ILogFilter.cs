namespace LoggerLib.Filters;

public interface ILogFilter
{
    string Filter(string messageToFilter);
}