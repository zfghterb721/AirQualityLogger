// See https://aka.ms/new-console-template for more information
using System.IO.Ports;

var serialPortName = promptPort();
var serialPort = new SerialPort(serialPortName, 9600);
serialPort.Open();
Console.WriteLine("Listening on " + serialPortName);

while (true)
{
    var aqData = new AirQualityData(serialPort).getData();

    Console.WriteLine("PM2.5 =    " + aqData.PM2_5 + " ug/m3 PM10 = " + aqData.PM10 + " ug/m3");


}

static string promptPort()
{
    List<string> ports = SerialPort.GetPortNames().ToList();
    int selection = 0;
    while (true)
    {
        Console.Clear();
        Console.WriteLine("Select a port to use");
        for (int i = 0; i < ports.Count; i++)
        {
            if (i == selection)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine(ports[i]);
            Console.ResetColor();
        }
        ConsoleKeyInfo key = Console.ReadKey();
        if (key.Key == ConsoleKey.UpArrow)
        {
            selection--;
            if (selection < 0)
            {
                selection = ports.Count - 1;
            }
        }
        else if (key.Key == ConsoleKey.DownArrow)
        {
            selection++;
            if (selection >= ports.Count)
            {
                selection = 0;
            }
        }
        else if (key.Key == ConsoleKey.Enter)
        {
            break;
        }

    }
    return ports[selection];
}

class AirQualityData
{
    public double PM2_5 { get; set; }
    public double PM10 { get; set; }
    public DateTime Time { get; set; }

    private SerialPort serialPort { get; set; }

    public AirQualityData(SerialPort serialPort)
    {
        this.serialPort = serialPort;
    }

    public AirQualityData getData() {
        var data = getSDSFrame(serialPort);
        PM2_5 = (data[0] + data[1] * 256) / 10.0;
        PM10 = (data[2] + data[3] * 256) / 10.0;
        Time = DateTime.Now;
        return this;
    }

    private byte[] getSDSFrame(SerialPort serialPort)
    {
        byte[] data = new byte[7];
        while (true)
        {
            //Read the first byte
            var b = serialPort.ReadByte();
            if (b == 0xAA)
            {
                //Read the second byte
                b = serialPort.ReadByte();
                if (b == 0xC0)
                {
                    //Read 7 bytes to data
                    for (int i = 0; i < 7; i++)
                    {
                        b = serialPort.ReadByte();
                        data[i] = (byte)b;
                    }
                    //Read the last byte
                    b = serialPort.ReadByte();
                    if (b == 0xAB)
                    {
                        //We have a valid frame
                        return data;
                    }
                }
            }
        }
    }

}
