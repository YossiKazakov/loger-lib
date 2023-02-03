using System.Threading.Tasks;

namespace LoggerLib;

public interface IFileWriter
{
    void Write(string line);
}