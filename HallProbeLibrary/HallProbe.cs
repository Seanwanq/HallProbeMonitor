using System.Globalization;
using System.IO.Ports;

namespace HallProbeLibrary
{
    public class HallProbe
    {
        private bool _continueReading = true;
        private readonly SerialPort _serialPort;

        public HallProbe(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate)
            {
                NewLine = "\n",
                ReadTimeout = 500,
                WriteTimeout = 500,
                DtrEnable = true,
                RtsEnable = true,
                Handshake = Handshake.None,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One
            };
            _serialPort.DataReceived += DataReceivedHandler;
        }

        public void OpenPort()
        {
            try
            {
                _serialPort.Open();
                Console.WriteLine($"Port {_serialPort.PortName} opened successfully.");
                Console.WriteLine("Waiting for data...");
                Console.WriteLine("Format: X-axis(mT), Y-axis(mT), Z-axis(mT)");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Error: Port {_serialPort.PortName} is in use by another application.");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening port: {ex.Message}");
                throw;
            }
        }

        public void ClosePort()
        {
            if (!_serialPort.IsOpen) return;
            _serialPort.Close();
            Console.WriteLine("Port closed.");
        }

        public void StartReading()
        {
            var stopThread = new Thread(StopProgram);
            stopThread.Start();

            while (_continueReading)
            {
                Thread.Sleep(100);
            }
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var sp = (SerialPort)sender;
                var data = sp.ReadLine();
                var values = data.Split(',');
                if (values.Length == 3)
                {
                    var xFieldMt = float.Parse(values[0], CultureInfo.InvariantCulture);
                    var yFieldMt = float.Parse(values[1], CultureInfo.InvariantCulture);
                    var zFieldMt = float.Parse(values[2], CultureInfo.InvariantCulture);
                    Console.WriteLine($"Data Received: X={xFieldMt}, Y={yFieldMt}, Z={zFieldMt}");
                    SaveToCsv(xFieldMt, yFieldMt, zFieldMt, "HallProbeData.csv");
                }
                else
                {
                    Console.WriteLine("Error: Received data does not contain exactly 3 values.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void SaveToCsv(float xFieldMt, float yFieldMt, float zFieldMt, string filePath)
        {
            try
            {
                using var sw = new StreamWriter(filePath, true);
                sw.WriteLine($"{xFieldMt},{yFieldMt},{zFieldMt}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to CSV file: {ex.Message}");
                throw;
            }
        }

        private void StopProgram()
        {
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
            _continueReading = false;
        }
    }
}