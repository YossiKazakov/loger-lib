using System;
using System.Threading;
using System.Threading.Tasks;

namespace LoggerLib;

public class FileWriter : IFileWriter
{
    public void Write(string line)
    {
        Console.WriteLine(line);
        // Delay backend printing (simulate IO delay - DO NOT CHANGE)
        Thread.Sleep(20);
    }
}