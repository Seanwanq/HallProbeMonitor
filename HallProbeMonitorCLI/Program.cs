using HallProbeLibrary;

namespace HallProbeMonitorCLI;

public static class Program
{
    public static void Main()
    {
        // Scan and display available serial ports
        string[] ports = System.IO.Ports.SerialPort.GetPortNames();
        Console.WriteLine("Available serial ports: ");
        foreach (var port in ports)
        {
            Console.WriteLine(port);
        }
        
        // Get user input for port name and baud rate
        Console.Write("Enter the port name you want to connect to: (e.g. COM3) ");
        var portName = Console.ReadLine();
        if (string.IsNullOrEmpty(portName))
        {
            Console.WriteLine("Invalid port name.");
            return;
        }

        Console.Write("Enter the baud rate: (e.g. 9600) ");
        var baudRateInput = Console.ReadLine();
        if (!int.TryParse(baudRateInput, out var baudRate))
        {
            Console.WriteLine("Invalid baud rate.");
            return;
        }
        
        // Create HallProbe object and start reading
        var hallProbe = new HallProbe(portName, baudRate);
        try
        {
            hallProbe.OpenPort();
            hallProbe.StartReading();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        finally
        {
            hallProbe.ClosePort();
        }
    }
}

