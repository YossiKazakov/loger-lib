namespace LoggerLib
{
    internal interface ILoggerLib
    {
        void PrintLogLine(string message);

        void WaitForThePrintsToFinish();
    }
}