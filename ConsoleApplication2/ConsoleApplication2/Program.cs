using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Collections;
using System.IO;
using System.Net;


namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                String dataString;
                System.IO.StreamWriter file1 = new System.IO.StreamWriter("c:\\Computer_info.xml");
                //Console.WriteLine("<?xml version='1.0' encoding='UTF-8'?>\n<DeviceList>");
                dataString = "<DeviceList>";
                file1.WriteLine("<?xml version='1.0' encoding='UTF-8'?>\n<DeviceList>");
                //Console.WriteLine(GetRAM());
                dataString += OverView();
                file1.WriteLine(OverView());
                dataString += GetHardDisk();
                file1.WriteLine(GetHardDisk());
                dataString += GetDrives();
                file1.WriteLine(GetDrives());
                dataString += "</DiskDrive>";
                //Console.WriteLine("</DiskDrive>");
                file1.WriteLine("</DiskDrive>");
                dataString += GetNetwork();
                file1.WriteLine(GetNetwork());
                dataString += GetMotherBoard();
                file1.WriteLine(GetMotherBoard());
                dataString += GetCdRom();
                file1.WriteLine(GetCdRom());
                dataString += GetRAM();
                file1.WriteLine(GetRAM());
                // Console.WriteLine("<CdRom>" +"<Drive>" + GetCdRomName() + "</Drive>" + "<Name>" + GetCdRomName() + "</Name>" + "<Manufacturer>" + GetCdRomMaker() + "</Manufacturer>" + "</Cdrom>");
                // Console.WriteLine("</DeviceList>");
                dataString += "</DeviceList>";
                file1.WriteLine("</DeviceList>");
                file1.Close();
           
          //Console.WriteLine(wresp);
            TextReader tr = new StreamReader("c://serv");
            String url = tr.ReadLine();
            String ID="a";
            Console.WriteLine(url);
            Console.WriteLine(httpPost(url, ID, dataString).ToString());
            }
            catch (System.UnauthorizedAccessException e)
            {
                Console.WriteLine("Please Run As Administrator"+e.Message);
            }
           Console.ReadKey();

            
        }
        static String httpPost( String url,String ID,String data)
        {
            String DATA = "ID=" + ID + "&DATA=" + data ;
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
           try
            {
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();
                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
                String resp=((HttpWebResponse)response).StatusDescription;
                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                String responseFromServer = reader.ReadToEnd();
                // Display the content.
                return (resp+responseFromServer);
                // Clean up the streams.
                // reader.Close();
                // dataStream.Close();
                // response.Close();
           }
           catch ( WebException e)
          {
               return e.Message;
           }
        }

            static string GetDrives()
            { 
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_LogicalDisk");
                StringBuilder sb = new StringBuilder();
               sb.Append("<DiskDrive>");
                foreach (ManagementObject wmi in searcher.Get())
                {
                    try
                        {
                           UInt64 Disksize=(UInt64)wmi.GetPropertyValue("Size");
                            sb.Append("<Drive>");
                            sb.Append("<ID> " + wmi.GetPropertyValue("DeviceID").ToString() + "</ID> ");
                            sb.Append("<caption>" + wmi.GetPropertyValue("Caption").ToString() + "</caption>");
                        sb.Append("<VolumeSerialNumber>" + wmi.GetPropertyValue("VolumeSerialNumber").ToString() +"</VolumeSerialNumber>"+ Environment.NewLine);
                        sb.Append("<TotalSpace>" + String.Format("{0:0.##}", ((float)(UInt64)wmi.GetPropertyValue("Size") / (1024 * 1024 * 1024))) + "GB" + "</TotalSpace>");
                        sb.Append("<FreeSpace>" + String.Format("{0:0.##}", ((float)(UInt64)wmi.GetPropertyValue("FreeSpace") / (1024 * 1024 * 1024))) + " GB" + "</FreeSpace>");
                        sb.Append("</Drive>");
                    }
                    catch
                    {
                        return sb.ToString();
                    }
                }
                //sb.Append("</DiskDrive>");
                return sb.ToString();
            }


       static string GetNetwork()
        {
            string query = "SELECT * FROM Win32_NetworkAdapterConfiguration"
                 + " WHERE IPEnabled = 'TRUE'";
            ManagementObjectSearcher moSearch = new ManagementObjectSearcher(query);
            ManagementObjectCollection moCollection = moSearch.Get();
            string sb;
            sb = "<Network>";
            // Every record in this collection is a network interface
            foreach (ManagementObject mo in moCollection)
            {
                sb += "<HostName>" + mo["DNSHostName"] + "</HostName>";
                sb += "<Description>" + mo["Description"] + "</Description>";

                // IPAddresses, probably have more than one value
                string[] addresses = (string[])mo["IPAddress"];
                foreach (string ipaddress in addresses)
                {
                    sb += "<IPAddress>" + ipaddress + "</IPAddress>";
                }

                // IPSubnets, probably have more than one value
                string[] subnets = (string[])mo["IPSubnet"];
                foreach (string ipsubnet in subnets)
                {
                    sb += "<IPSubnet>" + ipsubnet + "</IPSubnet>";
                }

                // DefaultIPGateways, probably have more than one value
                string[] defaultgateways = (string[])mo["DefaultIPGateway"];
                foreach (string defaultipgateway in defaultgateways)
                {
                    sb += "<DefaultIPGateway>" + defaultipgateway + "</DefaultIPGateway>";
                }

            }
            sb += "</Network>";
            return sb;
        }
       static string GetMotherBoard()
       {
           string query = "SELECT * FROM Win32_BaseBoard ";
           ManagementObjectSearcher moSearch = new ManagementObjectSearcher("root\\CIMV2" ,query);
           ManagementObjectCollection moCollection = moSearch.Get();
           string sb;
           sb = "<MotherBoard>";
           foreach (ManagementObject mo in moCollection)
           {
               try{
               sb += "<Name>" + mo["Name"] + "</Name>";
               }
                   catch
               {
                   sb += "<Name>" + "NotFound" + "</Name>";
                   }
               try
               {
                   sb += "<Manufacturer>" + mo["Manufacturer"].ToString() + "</Manufacturer>";
               }
               catch
               {
                   sb += "<Manufacturer>" + "NotFound" + "</Manufacturer>";
               }
               try
               {
                   sb += "<Product>" + mo["Product"].ToString() + "</Product>";
               }
               catch
               {
                   sb += "<Product>" + "NotFound" + "</Product>";
               }
               try
               {
                   sb += "<Model>" + mo["Model"].ToString() + "</Model>";
               }
               catch
               {
                   sb += "<Model>" + "NotFound" + "</Model>";
               }
               try
               {
                   sb += "<SerialNumber>" + mo["SerialNumber"] + "</SerialNumber>";
               }
               catch
               {
                   sb += "<SerialNumber>" + "NotFound" + "</SerialNumber>";
               }
           }
           sb += "</MotherBoard>";
           return sb;
       }

       static string GetCdRom()
       {
           string query = "SELECT * FROM Win32_CDROMDrive ";
           ManagementObjectSearcher moSearch = new ManagementObjectSearcher("root\\CIMV2", query);
           ManagementObjectCollection moCollection = moSearch.Get();
           string sb;
           sb = "<CdRom>";
           foreach (ManagementObject mo in moCollection)
           {
               
               sb += "<Disk>";
               try
               {
                   sb += "<Name>" + mo["Name"] + "</Name>";
               }
               catch
               {
                   sb += "<Name>" + "NotFound" + "</Name>";
               }
               try
               {
                   sb += "<Drive>" + mo["Drive"] + "</Drive>";
               }
               catch
               {
                   sb += "<Drive>" + "NotFound" + "</Drive>";
               }
               try
               {
                   sb += "<Manufacturer>" + mo["Manufacturer"].ToString() + "</Manufacturer>";
               }
               catch
               {
                   sb += "<Manufacturer>" + "NotFound" + "</Manufacturer>";
               }
               try
               {
                   sb += "<Product>" + mo["Product"].ToString() + "</Product>";
               }
               catch
               {
                   sb += "<Product>" + "NotFound" + "</Product>";
               }
               try
               {
                   sb += "<Model>" + mo["Model"].ToString() + "</Model>";
               }
               catch
               {
                   sb += "<Model>" + "NotFound" + "</Model>";
               }
               try
               {
                   sb += "<SerialNumber>" + mo["SerialNumber"].ToString() + "</SerialNumber>";
               }
               catch
               {
                   sb += "<SerialNumber>" + "NotFound" + "</SerialNumber>";
               }
               sb += "</Disk>";
           }
           sb += "</CdRom>";
           return sb;
       }

       static string GetHardDisk()
       {
           string query = "SELECT * FROM Win32_DiskDrive ";
           ManagementObjectSearcher moSearch = new ManagementObjectSearcher("root\\CIMV2", query);
           ManagementObjectCollection moCollection = moSearch.Get();
           string sb;
           sb = "<HardDisk>";
           foreach (ManagementObject mo in moCollection)
           {
               sb += "<Disk>"+Environment.NewLine;
               try
               {
                   sb += "<Name>" + mo["Name"] + "</Name>";
               }
               catch
               {
                   sb += "<Name>" + "NotFound" + "</Name>";
               }
               //try
               //{
               //    sb += "<Description>\'" + mo["PNPDeviceID"] + "\'</Description>";
               //}
               //catch
               {
                   sb += "<Description>" + "NotFound" + "</Description>";
               }
               try
               {
                   sb += "<Manufacturer>" + mo["Manufacturer"].ToString() + "</Manufacturer>";
               }
               catch
               {
                   sb += "<Manufacturer>" + "NotFound" + "</Manufacturer>";
               }
               try
               {
                   sb += "<Model>" + mo["Model"].ToString() + "</Model>";
               }
               catch
               {
                   sb += "<Model>" + "NotFound" + "</Model>";
               }
               try
               {
                   sb += "<SerialNumber>" + mo["Signature"].ToString() + "</SerialNumber>";
               }
               catch
               {
                   sb += "<SerialNumber>" + "NotFound" + "</SerialNumber>";
               }
               try
               {
                   sb += "<Size>" + String.Format("{0:0.##}",((float)(UInt64)mo["Size"]/(1024*1024*1024))) + "GB</Size>";
               }
               catch
               {
                   sb += "<Size>" + "NotFound" + "</Size>";
               }
               try
               {
                   sb += "<Partitions>" + mo["Partitions"].ToString() + "</Partitions>";
               }
               catch
               {
                   sb += "<Partitions>" + "NotFound" + "</Partitions>";
               }
               sb += "</Disk>";
           }
           sb += "</HardDisk>";
           return sb;
       }

       static string GetRAM()
       {
           string query = "SELECT * FROM Win32_PhysicalMemory ";
           ManagementObjectSearcher moSearch = new ManagementObjectSearcher("root\\CIMV2", query);
           ManagementObjectCollection moCollection = moSearch.Get();
           string sb;
           sb = "<RAM>";
           foreach (ManagementObject mo in moCollection)
           {
               sb += "<Disk>";
               try
               {
                   sb += "<Bank>" + mo["BankLabel"] + "</Bank>";
               }
               catch
               {
                   sb += "<Bank>" + "NotFound" + "</Bamk>";
               }
               try
               {
                   sb += "<Description>" + mo["Description"] + "</Description>";
               }
               catch
               {
                   sb += "<Description>" + "NotFound" + "</Description>";
               }
               try
               {
                   sb += "<Manufacturer>" + mo["Manufacturer"].ToString() + "</Manufacturer>";
               }
               catch
               {
                   sb += "<Manufacturer>" + "NotFound" + "</Manufacturer>";
               }
               try
               {
                   sb += "<Model>" + mo["Model"].ToString() + "</Model>";
               }
               catch
               {
                   sb += "<Model>" + "NotFound" + "</Model>";
               }
               try
               {
                   sb += "<SerialNumber>" + mo["Signature"].ToString() + "</SerialNumber>";
               }
               catch
               {
                   sb += "<SerialNumber>" + "NotFound" + "</SerialNumber>";
               }
               try
               {
                   sb += "<Size>" + String.Format("{0:0.##}",((float)(UInt64)mo["Capacity"] / (1024 * 1024 * 1024))) + "GB</Size>";
               }
               catch
               {
                   sb += "<Size>" + "NotFound" + "</Size>";
               }
               int mtype = (UInt16)mo["MemoryType"];
               sb += "<DiskType>"; 
               switch (mtype)
               {
                   case 20:
                       sb+= "DDR";
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

               sb += "</DiskType>";
               sb += "</Disk>";
           }
           sb += "</RAM>";
           return sb;
       }
       static string OverView()
       {
           string query = "SELECT * FROM Win32_ComputerSystem ";
           ManagementObjectSearcher moSearch = new ManagementObjectSearcher("root\\CIMV2", query);
           ManagementObjectCollection moCollection = moSearch.Get();
           string sb = "<Overview>";
           foreach (ManagementObject mo in moCollection)
           {
               sb += "<a>";
               try
               {
                   sb += "<Name>" + mo["Name"] + "</Name>";
               }
               catch
               {
                   sb += "<Name>" + "NotFound" + "</Name>";
               }
               try
               {
                   sb += "<Manufacturer>" + mo["Manufacturer"] + "</Manufacturer>";
               }
               catch
               {
                   sb += "<Manufacturer>" + "NotFound" + "</Manufacturer>";
               }
               try
               {
                   sb += "<Model>" + mo["Model"] + "</Model>";
               }
               catch
               {
                   sb += "<Model>" + "NotFound" + "</Model>";
               }
                try
               {
                   sb += "<SystemType>" + mo["SystemType"] + "</SystemType>";
               }
               catch
               {
                   sb += "<SystemType>" + "NotFound" + "</SystemType>";
               }
               try
               {
                   sb += "<NumberOfProcessors>" + mo["NumberOfProcessors"] + "</NumberOfProcessors>";
               }
               catch
               {
                   sb += "<NumberOfProcessors>" + "NotFound" + "</NumberOfProcessors>";
               }
               try
               {
                   sb += "<PCSystemType>" + mo["PCSystemType"] + "</PCSystemType>";
               }
               catch
               {
                   sb += "<PCSystemType>" + "NotFound" + "</PCSystemType>";
               }
               try
               {
                   sb += "<NumberOfLogicalProcessors>" + mo["NumberOfLogicalProcessors"] + "</NumberOfLogicalProcessors>";
               }
               catch
               {
                   sb += "<NumberOfLogicalProcessors>" + "NotFound" + "</NumberOfLogicalProcessors>";
               }
              
               try
               {
                   sb += "<RAMUsable>" + String.Format("{0:0.##}", ((float)(UInt64)mo["TotalPhysicalMemory"] / (1024 * 1024 * 1024))) + "GB</RAMUsable>";
               }
               catch
               {
                   sb += "<RAMUsable>" + "NotFound" + "</RAMUsable>";
               }
               try
               {
               Object b =mo["OEMStringArray"];

               IEnumerable enumerable = b as IEnumerable;
               if (enumerable != null)
               {
                   sb += "<Chipset>";
                   foreach (object element in enumerable)
                   {
                       sb +=element.ToString() + " ";
                   }
                   sb+="</Chipset>";
               }
               }
               catch
               {
                   sb += "<Chipset>" + "NotFound" + "</Chipset>";
               }
               try
               {
                   sb += "<UserName>" + mo["UserName"] + "</UserName>";
               }
               catch
               {
                   sb += "<UserName>" + "NotFound" + "</UserName>";
               }
               string query2 = "SELECT caption FROM Win32_OperatingSystem ";
               ManagementObjectSearcher moSearch2 = new ManagementObjectSearcher("root\\CIMV2", query2);
               ManagementObjectCollection moCollection2 = moSearch2.Get();
               foreach (ManagementObject wmi in moSearch2.Get())
               {
                   sb += "<OSName>" + wmi.GetPropertyValue("caption") + "</OSName>";
               }
               OperatingSystem os = Environment.OSVersion;
              // sb += "<OS String>" + os.VersionString.ToString() + "</OS String>";
               sb += "<OSServicePack>" + os.ServicePack.ToString() + "</OSServicePack>";
               sb += "<OSPlatform>" + os.Platform.ToString() + "</OSPlatform>";
               sb += "<OSVersion>" + os.Version.ToString() + "</OSVersion>";
           }
           sb+="</a>";
           sb += "</Overview>";
           return sb;
       }


    }
}