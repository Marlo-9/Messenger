using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Resources.Tools.Additional;

public static class Logging
{
    private static bool _isStarted = false;
    private static string _session;
    private static string _sessionPath;
    private static string _fileExtension = "log";

    public static async void StartSession()
    {
        var dir = @"Logs\" + DateTime.Today.Date.ToString(CultureInfo.CurrentCulture).Split(' ')[0];
        
        _session = DateTime.Now.TimeOfDay.ToString().Split('.')[0].Replace(':', '.');
        _sessionPath = dir + @"\" + _session + "." + _fileExtension;
        
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        File.Create(_sessionPath).Close();

        _isStarted = true;
        
        await LogAsync("Start session");
    }

    public static async void EndSessionAsync()
    {
        await LogAsync("End session");
    }

    public static void EndSession()
    {
        Log("End session");
    }

    public static async Task LogAsync(string text)
    {
        if (!_isStarted) return;

        var date = DateTime.Now.TimeOfDay.ToString();
        
        date = "[" + date.Substring(0, date.Length - 4) + "]";

        //WaitFile();
        await using var stream = new StreamWriter(_sessionPath, true);
        
        text = text.Replace("\n", "\n".PadRight(date.Length + 2));
        
        await stream.WriteLineAsync(date + " " + text);
        await stream.FlushAsync();
    }

    public static void Log(string text)
    {
        if (!_isStarted) return;

        var date = DateTime.Now.TimeOfDay.ToString();
        
        date = "[" + date.Substring(0, date.Length - 4) + "]";

        //WaitFile();
        using var stream = new StreamWriter(_sessionPath, true);
        
        text = text.Replace("\n", "\n".PadRight(date.Length + 2));
        
        stream.WriteLine(date + " " + text);
        stream.Flush();
    }

    private static void WaitFile()
    {
        var isReady = false;

        while (!isReady)
        {
            try
            {
                using var inputStream = File.Open(_sessionPath, FileMode.Open, FileAccess.Read, FileShare.None);

                isReady = inputStream.Length >= 0;
            }
            catch
            {
                // ignored
            }
        }
    }
}