using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Resources.Tools.Additional;

public class Logging
{
    private string _session;
    private string _sessionPath;
    private string _fileExtension = "log";

    private BlockingCollection<string> _blockingCollection = new BlockingCollection<string>();
    private StreamWriter _streamWriter = null;
    private Task _task = null;
    
    public bool IsRun { get; private set; } = false;

    private static Logging _logging = null;

    public static Logging GetInstance()
    {
        return _logging ??= new Logging();
    }

    private Logging()
    {
        
    }

    public void StartSession()
    {
        var dir = @"Logs\" + DateTime.Today.Date.ToString(CultureInfo.CurrentCulture).Split(' ')[0];
        
        _session = DateTime.Now.TimeOfDay.ToString().Split('.')[0].Replace(':', '.');
        _sessionPath = dir + @"\" + _session + "." + _fileExtension;
        
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        _streamWriter = new StreamWriter(File.Create(_sessionPath));
        
        IsRun = true;
        
        _task = Task.Run(() =>
        {
            while (IsRun)
                _streamWriter.WriteLine(_blockingCollection.Take());
        });
        
        Log("Start session");
        //await LogAsync("Start session");
    }

    /*public async void EndSessionAsync()
    {
        await LogAsync("End session");
    }*/

    public void EndSession()
    {
        Log("End session");

        IsRun = false;
        
        _task.Wait();
        _task.Dispose();
        _streamWriter.Close();
        _streamWriter.Dispose();
    }

    public void Log(string text)
    {
        var date = DateTime.Now.TimeOfDay.ToString();
        var memberFullName = new StackTrace().GetFrame(1).GetMethod().DeclaringType.FullName;
        var memberClassName = memberFullName.Split('+')[0];
        var memberMethodName = "";

        if (memberFullName.Split('+').Length >= 2)
        {
            memberMethodName = memberFullName.Split('+')[1];
            memberMethodName = "." +
                memberMethodName.Substring(memberMethodName.IndexOf('<') + 1, memberMethodName.IndexOf('>') - 1) + "()";
        }
        
        date = "[" + date.Substring(0, date.Length - 4) + "]";
        text = "\n" + text;
        text = text.Replace("\n", "\n".PadRight(date.Length + 2));

        if (!string.IsNullOrEmpty(memberFullName))
            _blockingCollection.Add(date + " (" + memberClassName + memberMethodName + ")" + text);
        else
            _blockingCollection.Add(date + " " + text);
    }

    /*public async Task LogAsync(string text)
    {
        if (!IsRun) return;

        var date = DateTime.Now.TimeOfDay.ToString();
        
        date = "[" + date.Substring(0, date.Length - 4) + "]";
        text = text.Replace("\n", "\n".PadRight(date.Length + 2));

        byte[] outBytes = Encoding.Unicode.GetBytes(text);

        await using var sourceStream = new FileStream(_sessionPath, 
                                                      FileMode.Open, FileAccess.Write, 
                                                      FileShare.None, 4096, useAsync:true);
        
        await sourceStream.WriteAsync(outBytes, 0, outBytes.Length);

        //WaitFile();
        /*await using var stream = new StreamWriter(_sessionPath, true);
        
        
        await stream.WriteLineAsync(date + " " + text);
        await stream.FlushAsync();#1#
    }

    public void Log(string text)
    {
        if (!IsRun) return;

        var date = DateTime.Now.TimeOfDay.ToString();
        
        date = "[" + date.Substring(0, date.Length - 4) + "]";
        text = text.Replace("\n", "\n".PadRight(date.Length + 2));

        byte[] outBytes = Encoding.Unicode.GetBytes(text);
        
        using var sourceStream = new FileStream(_sessionPath, 
                                                FileMode.Open, FileAccess.Write, 
                                                FileShare.None, 4096, useAsync:true);
        
        sourceStream.WriteAsync(outBytes, 0, outBytes.Length);
        
        //WaitFile();
        /*using var stream = new StreamWriter(_sessionPath, true);
        
        
        stream.WriteLine(date + " " + text);
        stream.Flush();#1#
    }

    private void WaitFile()
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
    }*/
}