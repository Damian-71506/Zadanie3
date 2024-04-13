using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;

public class SystemMonitor
{
    private PerformanceCounter cpuCounter;
    private PerformanceCounter ramCounter;
    private EventLog eventLog;
    private string logFilePath = "SystemMonitorLog.txt";

    public SystemMonitor()
    {
        cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        ramCounter = new PerformanceCounter("Memory", "Available MBytes");

        if (!File.Exists(logFilePath))
        {
            using (StreamWriter writer = File.CreateText(logFilePath))
            {
                writer.WriteLine("System Monitor Log");
                writer.WriteLine("-------------------");
            }
        }
        string eventLogName = "SystemMonitorLog";
        if (!EventLog.SourceExists(eventLogName))
        {
            EventLog.CreateEventSource(eventLogName, "Application");
        }
        eventLog = new EventLog();
        eventLog.Source = eventLogName;
    }

    public void StartMonitoring()
    {
        Thread monitorThread = new Thread(MonitorThread);
        monitorThread.IsBackground = true;
        monitorThread.Start();
    }

    private void MonitorThread()
    {
        Console.WriteLine("MonitorThread started.");
        while (true)
        {
            Console.WriteLine("Monitoring...");
            
            float cpuUsage = cpuCounter.NextValue();
            float availableRAM = ramCounter.NextValue();

            if (cpuUsage > 80 || availableRAM < 1024)
            {
                LogToFile($"CPU Usage: {cpuUsage}%, RAM Available: {availableRAM} MB");
            }
            LogToFile($"CPU Usage: {cpuUsage}%, RAM Available: {availableRAM} MB");
            Thread.Sleep(10000);
        }
    }
    private void LogToFile(string logMessage)
    {
        try
        {
            Console.WriteLine("Attempting to log to file...");
            

            if (logFilePath != null)
            {
                Console.WriteLine($"Logging to file: {logMessage}");

                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now.ToString()} - {logMessage}");
                }

                Console.WriteLine("Logged to file successfully.");
                Console.WriteLine($"Log file path: {logFilePath}");
            }
            else
            {
                Console.WriteLine("Log file path is null. Cannot log to file.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging to file: {ex.Message}");
            throw;
        }
    }
    public void SaveConfiguration(string configFilePath)
    {
        try
        {
            var config = new Configuration()
            {
                LogFilePath = logFilePath
            };

            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configFilePath, json);

            Console.WriteLine("Configuration saved successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
        }
    }

    public void LoadConfiguration(string configFilePath)
    {
        try
        {
            if (File.Exists(configFilePath))
            {
                string json = File.ReadAllText(configFilePath);
                var config = JsonConvert.DeserializeObject<Configuration>(json);

                if (config != null)
                {
                    logFilePath = config.LogFilePath;
                    Console.WriteLine("Configuration loaded successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to load configuration. Invalid format.");
                }
            }
            else
            {
                Console.WriteLine("Configuration file does not exist.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
        }
    }
    public static void Main(string[] args)
    {
        SystemMonitor monitor = new SystemMonitor();
        monitor.StartMonitoring();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
    private void LogEvent(string eventName, string eventMessage)
    {
        eventLog.WriteEntry(eventMessage, EventLogEntryType.Warning, 101, 1);
    }
}
public class Configuration
{
    public string LogFilePath { get; set; }
}
