using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Management;
using System.Collections;
using System.Net;

namespace Mechnet
{
    public partial class Mechauth : ServiceBase
    {
        public Mechauth()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            String dataString;
            TextWriter tw = new StreamWriter("C://mech.xml");
            tw.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
            dataString = "<DeviceList>";
            dataString += OverView();
            dataString += GetHardDisk();
            dataString += GetDrives();
            dataString += "</DiskDrive>";
            dataString += GetNetwork();
            dataString += GetMotherBoard();
            dataString += GetCdRomDrive();
            dataString += GetRAM();
            dataString += "</DeviceList>";
            tw.WriteLine(dataString);
            //TextReader tr = new StreamReader("serv");
            String url = "http://10.0.53.91/audit1/fetch1.php";
            String  ID = "a";
            //tw.WriteLine(httpPost(url, ID, dataString));
            //Console.ReadKey();
            tw.Close();
        }

        static String httpPost(String url, String ID, String data)
        {
            String resp;
            String DATA = "ID=" + ID + "&DATA=" + data;
            // Create a request using a URL that can receive a post. 
            WebRequest request = WebRequest.Create(url);
            // Set the Method property of the request to POST.
            request.Method = "POST";
            // Create POST data and convert it to a byte array.
            String postData = DATA;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            resp=((HttpWebResponse)response).StatusDescription;
            if (resp == "OK")
            {

            }
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            String responseFromServer = reader.ReadToEnd();
            // Display the content.
            return (responseFromServer);
            // Clean up the streams.
            // reader.Close();
            // dataStream.Close();
            // response.Close();
        }


        static String GetDrives()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_LogicalDisk");
            StringBuilder sb = new StringBuilder();
            sb.Append("<DiskDrive>" + Environment.NewLine);
            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    UInt64 Disksize = (UInt64)wmi.GetPropertyValue("Size");
                    sb.Append("<Drive>" + Environment.NewLine);
                    sb.Append("<ID> " + wmi.GetPropertyValue("DeviceID").ToString() + "</ID> " + Environment.NewLine);
                    sb.Append("<caption>" + wmi.GetPropertyValue("Caption").ToString() + "</caption>" + Environment.NewLine);
                    sb.Append("<VolumeSerialNumber>" + wmi.GetPropertyValue("VolumeSerialNumber").ToString() + "</VolumeSerialNumber>" + Environment.NewLine);
                    sb.Append("<TotalSpace>" + String.Format("{0:0.##}", ((float)(UInt64)wmi.GetPropertyValue("Size") / (1024 * 1024 * 1024))) + "GB" + "</TotalSpace>" + Environment.NewLine);
                    sb.Append("<FreeSpace>" + String.Format("{0:0.##}", ((float)(UInt64)wmi.GetPropertyValue("FreeSpace") / (1024 * 1024 * 1024))) + " GB" + "</FreeSpace>" + Environment.NewLine);
                    sb.Append("</Drive>" + Environment.NewLine);
                }
                catch
                {
                    return sb.ToString();
                }
            }
            //sb.Append("</DiskDrive>" + Environment.NewLine);
            return sb.ToString();
        }


        static String GetNetwork()
        {
            string query = "SELECT * FROM Win32_NetworkAdapterConfiguration"
                 + " WHERE IPEnabled = 'TRUE'";
            ManagementObjectSearcher moSearch = new ManagementObjectSearcher(query);
            ManagementObjectCollection moCollection = moSearch.Get();
            string sb;
            sb = "<Network>" + Environment.NewLine;
            // Every record in this collection is a network interface
            foreach (ManagementObject mo in moCollection)
            {
                sb+="<card>";
                sb += "<HostName>" + mo["DNSHostName"] + "</HostName>" + Environment.NewLine;
                sb += "<Description>" + mo["Description"] + "</Description>" + Environment.NewLine;

                // IPAddresses, probably have more than one value
                string[] addresses = (string[])mo["IPAddress"];
                foreach (string ipaddress in addresses)
                {
                    sb += "<IPAddress>" + ipaddress + "</IPAddress>" + Environment.NewLine;
                }

                // IPSubnets, probably have more than one value
                string[] subnets = (string[])mo["IPSubnet"];
                foreach (string ipsubnet in subnets)
                {
                    sb += "<IPSubnet>" + ipsubnet + "</IPSubnet>" + Environment.NewLine;
                }

                // DefaultIPGateways, probably have more than one value
                string[] defaultgateways = (string[])mo["DefaultIPGateway"];
                foreach (string defaultipgateway in defaultgateways)
                {
                    sb += "<DefaultIPGateway>" + defaultipgateway + "</DefaultIPGateway>" + Environment.NewLine;
                }
                sb += "</card>";
            }
            sb += "</Network>" + Environment.NewLine + Environment.NewLine;
            return sb;
        }
        static String GetMotherBoard()
        {
            string query = "SELECT * FROM Win32_BaseBoard ";
            ManagementObjectSearcher moSearch = new ManagementObjectSearcher("root\\CIMV2", query);
            ManagementObjectCollection moCollection = moSearch.Get();
            string sb;
            sb = "<MotherBoard>" + Environment.NewLine;
            foreach (ManagementObject mo in moCollection)
            {
                sb += "<board>";
                try
                {
                    sb += "<Name>" + mo["Name"] + "</Name>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Name>" + "NotFound" + "</Name>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Manufacturer>" + mo["Manufacturer"].ToString() + "</Manufacturer>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Manufacturer>" + "NotFound" + "</Manufacturer>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Product>" + mo["Product"].ToString() + "</Product>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Product>" + "NotFound" + "</Product>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Model>" + mo["Model"].ToString() + "</Model>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Model>" + "NotFound" + "</Model>" + Environment.NewLine;
                }
                try
                {
                    sb += "<SerialNumber>" + mo["SerialNumber"] + "</SerialNumber>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<SerialNumber>" + "NotFound" + "</SerialNumber>" + Environment.NewLine;
                }
                sb += "</board>";
            }
            sb += "</MotherBoard>" + Environment.NewLine + Environment.NewLine;
            return sb;
        }

        static String GetCdRomDrive()
        {
            string query = "SELECT * FROM Win32_CDROMDrive ";
            ManagementObjectSearcher moSearch = new ManagementObjectSearcher("root\\CIMV2", query);
            ManagementObjectCollection moCollection = moSearch.Get();
            string sb;
            sb = "<CdRom>" + Environment.NewLine;
            foreach (ManagementObject mo in moCollection)
            {
                sb += "<Disk>";
                try
                {
                    sb += "<Name>" + mo["Name"] + "</Name>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Name>" + "NotFound" + "</Name>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Drive>" + mo["Drive"] + "</Drive>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Drive>" + "NotFound" + "</Drive>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Manufacturer>" + mo["Manufacturer"].ToString() + "</Manufacturer>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Manufacturer>" + "NotFound" + "</Manufacturer>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Product>" + mo["Product"].ToString() + "</Product>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Product>" + "NotFound" + "</Product>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Model>" + mo["Model"].ToString() + "</Model>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Model>" + "NotFound" + "</Model>" + Environment.NewLine;
                }
                try
                {
                    sb += "<SerialNumber>" + mo["SerialNumber"].ToString() + "</SerialNumber>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<SerialNumber>" + "NotFound" + "</SerialNumber>" + Environment.NewLine;
                }
                sb += "</Disk>";
            }
            sb += "</CdRom>" + Environment.NewLine + Environment.NewLine;
            return sb;
        }

        static String GetHardDisk()
        {
            string query = "SELECT * FROM Win32_DiskDrive ";
            ManagementObjectSearcher moSearch = new ManagementObjectSearcher("root\\CIMV2", query);
            ManagementObjectCollection moCollection = moSearch.Get();
            string sb;
            sb = "<HardDisk>" + Environment.NewLine;
            foreach (ManagementObject mo in moCollection)
            {
                sb += "<Disk>" + Environment.NewLine;
                try
                {
                    sb += "<Name>" + mo["Name"] + "</Name>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Name>" + "NotFound" + "</Name>" + Environment.NewLine;
                }
                //try
                //{
                //    sb += "<Description>\'" + mo["PNPDeviceID"] + "\'</Description>" + Environment.NewLine;
                //}
                //catch
                {
                    sb += "<Description>" + "NotFound" + "</Description>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Manufacturer>" + mo["Manufacturer"].ToString() + "</Manufacturer>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Manufacturer>" + "NotFound" + "</Manufacturer>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Model>" + mo["Model"].ToString() + "</Model>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Model>" + "NotFound" + "</Model>" + Environment.NewLine;
                }
                try
                {
                    sb += "<SerialNumber>" + mo["Signature"].ToString() + "</SerialNumber>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<SerialNumber>" + "NotFound" + "</SerialNumber>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Size>" + String.Format("{0:0.##}", ((float)(UInt64)mo["Size"] / (1024 * 1024 * 1024))) + "GB</Size>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Size>" + "NotFound" + "</Size>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Partitions>" + mo["Partitions"].ToString() + "</Partitions>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Partitions>" + "NotFound" + "</Partitions>" + Environment.NewLine;
                }
                sb += "</Disk>" + Environment.NewLine;
            }
            sb += "</HardDisk>" + Environment.NewLine + Environment.NewLine;
            return sb;
        }

        static String GetRAM()
        {
            string query = "SELECT * FROM Win32_PhysicalMemory ";
            ManagementObjectSearcher moSearch = new ManagementObjectSearcher("root\\CIMV2", query);
            ManagementObjectCollection moCollection = moSearch.Get();
            string sb;
            sb = "<RAM>" + Environment.NewLine;
            foreach (ManagementObject mo in moCollection)
            {
                sb += "<Disk>" + Environment.NewLine;
                try
                {
                    sb += "<Bank>" + mo["BankLabel"] + "</Bank>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Bank>" + "NotFound" + "</Bamk>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Description>" + mo["Description"] + "</Description>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Description>" + "NotFound" + "</Description>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Manufacturer>" + mo["Manufacturer"].ToString() + "</Manufacturer>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Manufacturer>" + "NotFound" + "</Manufacturer>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Model>" + mo["Model"].ToString() + "</Model>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Model>" + "NotFound" + "</Model>" + Environment.NewLine;
                }
                try
                {
                    sb += "<SerialNumber>" + mo["Signature"].ToString() + "</SerialNumber>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<SerialNumber>" + "NotFound" + "</SerialNumber>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Size>" + String.Format("{0:0.##}", ((float)(UInt64)mo["Capacity"] / (1024 * 1024 * 1024))) + "GB</Size>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Size>" + "NotFound" + "</Size>" + Environment.NewLine;
                }
                int mtype = (UInt16)mo["MemoryType"];
                sb += "<DiskType>";
                switch (mtype)
                {
                    case 20:
                        sb += "DDR";
                        break;
                    case 21:
                        sb += "DDR-2";
                        break;
                    default:
                        if (mtype == 0 || mtype > 22)
                            sb += "DDR-3";
                        else
                            sb += "Other";
                        break;

                }

                sb += "</DiskType>" + Environment.NewLine;
                sb += "</Disk>" + Environment.NewLine;
            }
            sb += "</RAM>" + Environment.NewLine + Environment.NewLine;
            return sb;
        }
        static String OverView()
        {
            string query = "SELECT * FROM Win32_ComputerSystem ";
            ManagementObjectSearcher moSearch = new ManagementObjectSearcher("root\\CIMV2", query);
            ManagementObjectCollection moCollection = moSearch.Get();
            string sb = "<Overview>" + Environment.NewLine;
            foreach (ManagementObject mo in moCollection)
            {
                sb += "<a>";
                try
                {
                    sb += "<Name>" + mo["Name"] + "</Name>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Name>" + "NotFound" + "</Name>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Manufacturer>" + mo["Manufacturer"] + "</Manufacturer>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Manufacturer>" + "NotFound" + "</Manufacturer>" + Environment.NewLine;
                }
                try
                {
                    sb += "<Model>" + mo["Model"] + "</Model>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<Model>" + "NotFound" + "</Model>" + Environment.NewLine;
                }
                try
                {
                    sb += "<SystemType>" + mo["SystemType"] + "</SystemType>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<SystemType>" + "NotFound" + "</SystemType>" + Environment.NewLine;
                }
                try
                {
                    sb += "<NumberOfProcessors>" + mo["NumberOfProcessors"] + "</NumberOfProcessors>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<NumberOfProcessors>" + "NotFound" + "</NumberOfProcessors>" + Environment.NewLine;
                }
                try
                {
                    sb += "<PCSystemType>" + mo["PCSystemType"] + "</PCSystemType>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<PCSystemType>" + "NotFound" + "</PCSystemType>" + Environment.NewLine;
                }
                try
                {
                    sb += "<NumberOfLogicalProcessors>" + mo["NumberOfLogicalProcessors"] + "</NumberOfLogicalProcessors>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<NumberOfLogicalProcessors>" + "NotFound" + "</NumberOfLogicalProcessors>" + Environment.NewLine;
                }

                try
                {
                    sb += "<RAMUsable>" + String.Format("{0:0.##}", ((float)(UInt64)mo["TotalPhysicalMemory"] / (1024 * 1024 * 1024))) + "GB</RAMUsable>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<RAMUsable>" + "NotFound" + "</RAMUsable>" + Environment.NewLine;
                }
                try
                {
                    Object b = mo["OEMStringArray"];

                    IEnumerable enumerable = b as IEnumerable;
                    if (enumerable != null)
                    {
                        foreach (object element in enumerable)
                        {
                            sb += "<Chipset>" + element.ToString() + "</Chipset>" + Environment.NewLine;
                        }
                    }
                }
                catch
                {
                    sb += "<Chipset>" + "NotFound" + "</Chipset>" + Environment.NewLine;
                }
                try
                {
                    sb += "<UserName>" + mo["UserName"] + "</UserName>" + Environment.NewLine;
                }
                catch
                {
                    sb += "<UserName>" + "NotFound" + "</UserName>" + Environment.NewLine;
                }
                string query2 = "SELECT caption FROM Win32_OperatingSystem ";
                ManagementObjectSearcher moSearch2 = new ManagementObjectSearcher("root\\CIMV2", query2);
                ManagementObjectCollection moCollection2 = moSearch2.Get();
                foreach (ManagementObject wmi in moSearch2.Get())
                {
                    sb += "<OSName>" + wmi.GetPropertyValue("caption") + "</OSName>" + Environment.NewLine;
                }
                OperatingSystem os = Environment.OSVersion;
                // sb += "<OS String>" + os.VersionString.ToString() + "</OS String>" + Environment.NewLine;
                sb += "<OSServicePack>" + os.ServicePack.ToString() + "</OSServicePack>" + Environment.NewLine;
                sb += "<OSPlatform>" + os.Platform.ToString() + "</OSPlatform>" + Environment.NewLine;
                sb += "<OSVersion>" + os.Version.ToString() + "</OSVersion>" + Environment.NewLine;
                sb += "</a>";
            }
            sb += "</Overview>" + Environment.NewLine;
            return sb;
        }

        protected override void OnStop()
        {
        }
    }
}
