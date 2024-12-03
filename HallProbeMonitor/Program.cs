using System;
using System.IO;
using System.IO.Ports;
using System.Globalization;
using System.Threading;

namespace HallProbeMonitor
{
    public static class Program
    {
        private static int counter = 0;
        private static bool continueReading = true;

        public static void Main()
        {
            // Set port parameters
            string portName = "COM3";
            int baudRate = 9600;

            // Create serial port object with specific settings
            SerialPort serialPort = new SerialPort(portName,
                baudRate)
            {
                NewLine = "\n", // Set the newline character
                ReadTimeout = 500, // Add timeout
                WriteTimeout = 500,
                DtrEnable = true, // Enable DTR
                RtsEnable = true, // Enable RTS
                Handshake = Handshake.None, // Disable handshake
                Parity = Parity.None, // No parity
                DataBits = 8, // 8 data bits
                StopBits = StopBits.One // 1 stop bit
            };

            // Set port data received event handler
            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            // Open port with error handling
            try
            {
                serialPort.Open();
                Console.WriteLine($"Port {portName} opened successfully.");
                Console.WriteLine("Waiting for data...");
                Console.WriteLine("Format: X-axis(mT), Y-axis(mT), Z-axis(mT)");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: Port {portName} is in use by another application.");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening port: {ex.Message}");
                return;
            }

            // Start a new thread to listen for a key press to stop the program
            Thread stopThread = new Thread(StopProgram);
            stopThread.Start();

            // Main loop to keep the program running
            while (continueReading)
            {
                Thread.Sleep(100); // Sleep to reduce CPU usage
            }

            // Properly close the port
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                Console.WriteLine("Port closed.");
            }
        }

        private static void DataReceivedHandler(object sender,
            SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                string data = sp.ReadLine();
                string[] values = data.Split(',');

                if (values.Length == 3)
                {
                    float xField_mT = float.Parse(values[0],
                        CultureInfo.InvariantCulture);
                    float yField_mT = float.Parse(values[1],
                        CultureInfo.InvariantCulture);
                    float zField_mT = float.Parse(values[2],
                        CultureInfo.InvariantCulture);
                    Console.WriteLine($"Data Received: X={xField_mT}, Y={yField_mT}, Z={zField_mT}");
                    SaveToCSV(xField_mT,
                        yField_mT,
                        zField_mT,
                        "HallProbeData.csv");
                }
                else
                {
                    Console.WriteLine("Error: Received data does not contain exactly 3 values.");
                }

                // Console.WriteLine("Data Received: " + data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void SaveToCSV(float xField_mT,
            float yField_mT,
            float zField_mT,
            string filePath)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath,
                           true))
                {
                    sw.WriteLine($"{xField_mT},{yField_mT},{zField_mT}");
                }

                // Console.WriteLine("Data saved to CSV file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ërror saving to CSV file: {ex.Message}");
                throw;
            }
        }

        private static void StopProgram()
        {
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
            continueReading = false;
        }
    }
}