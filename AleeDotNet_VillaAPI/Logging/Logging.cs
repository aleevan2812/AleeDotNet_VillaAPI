namespace AleeDotNet_VillaAPI.Logging;

public class Logging : ILogging
{
    public void Log(string message, string type)
    {
        if (type == "error")
        {
            Console.WriteLine("ERROR - " + message);
        }
        else if (type == "warning")
        {
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("WARNING - " + message);
            Console.BackgroundColor = ConsoleColor.Black;
        }
        else
        {
            Console.WriteLine(message);
        }
         
    }
}