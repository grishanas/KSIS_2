using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NETTEST1
{
    static class Program
    {
        static void Main()
        {
            byte[] recivebuf = new byte[200];
            
            //создание ip адресов выхода и входа
            Stopwatch track = new Stopwatch();
            IPAddress ipdest = IPAddress.Parse(Console.ReadLine());
            IPEndPoint endp = new IPEndPoint(ipdest, 1);
            IPEndPoint src = new IPEndPoint(IPAddress.Any, 1);
     



            short TTL = 1;
           
            while (TTL < 127)
            {
                try
                {
                    // создание сокета с icmp протоколом(ip заголовок)
                    Socket icmp_s = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
                    icmp_s.ReceiveTimeout = 1000;
                                 

                    //заполнение данных  icmp протокола
                    byte[] icmpData = new byte[40];
                    int offset = 0;
                    icmpData[offset++] = (byte)8; 
                    icmpData[offset++] = (byte)0; 
                    icmpData[offset++] = 0;       
                    icmpData[offset++] = 0;
                    ushort checksumValue;


                    checksumValue = ComputeChecksum(icmpData);
                  

                    icmpData[2] =(byte) (checksumValue);
                    icmpData[3] = (byte)(checksumValue>>8);

                    IPEndPoint pingDestination = new IPEndPoint(ipdest, 0);

                    icmp_s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, TTL);
                    icmp_s.Bind(src);
                    for (int i = 0; i < 3; i++)
                    {
                        track.Reset();
                        track.Start();
                        icmp_s.SendTo(icmpData, pingDestination);
                        var tmp = icmp_s.Receive(recivebuf);
                        track.Stop();
                        if(track.ElapsedMilliseconds>0)
                        {
                            Console.Write("  {0,3} ms  ", track.ElapsedMilliseconds);
                        }
                        else
                        {
                            Console.Write("  <1 ms   ");
                        }

                    }
                    icmp_s.Close();
                    int HeaderLength = (recivebuf[0] & 0xf)*4;
                    
                    Console.Write(recivebuf[12] + "."+ recivebuf[13] + "."+recivebuf[14] + "."+recivebuf[15]+"   ");

                    IPHostEntry name = Dns.GetHostEntry(recivebuf[12] + "." + recivebuf[13] + "." + recivebuf[14] + "." + recivebuf[15]);

                    Console.WriteLine(name.HostName);
                    if (recivebuf[HeaderLength] == 0)
                        break;
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    ++TTL;
                }
            }

            Console.ReadKey();
        }

        static public ushort ComputeChecksum(byte[] payLoad)
        {

            UInt32 chcksm = 0;
            
            int index = 0;

            while (index < payLoad.Length)
            {
                chcksm += Convert.ToUInt32(BitConverter.ToUInt16(payLoad, index));
                index += 2;
            }
            chcksm = (chcksm >> 16) + (chcksm & 0xffff);
            chcksm += (chcksm >> 16);
            return (UInt16)(~chcksm);

        }
    }
}