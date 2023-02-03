using System;

namespace LoggerLib.Filters;

public class AddDateFilter : ILogFilter
{
    public string Filter(string messageToFilter)
    {
        return DateTime.Now + " " + messageToFilter;
    }
}