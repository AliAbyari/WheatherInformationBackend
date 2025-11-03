using Serilog;
using System.Runtime.CompilerServices;

namespace SharedLibrary.Helpers
{
    public static class Helpers
    {
        public static void InsertToLog(bool insertLog, string logText, string type = "Error", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            try
            {
                if (insertLog)
                    switch (type)
                    {
                        case "Error":
                            Log.Error(logText, lineNumber + " at line " + lineNumber + " (" + caller + ")" + "\n");
                            break;
                        case "Info":
                            Log.Information(logText, lineNumber + " at line " + lineNumber + " (" + caller + ")" + "\n");
                            break;
                        default:
                            break;
                    }
            }
            catch (Exception)
            {

            }
        }
    }
}
