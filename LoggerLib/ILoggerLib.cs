namespace LoggerLib
{
    interface ILoggerLib
    {
        void PrintLogLine(string message);

        void WaitForThePrintsToFinish();
    }
}