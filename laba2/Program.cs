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

            bool anyResponse=false;
            //создание ip адресов выхода и входа
            Stopwatch track = new Stopwatch();
            IPAddress ipdest = IPAddress.Parse(Console.ReadLine());
            IPEndPoint endp = new IPEndPoint(ipdest, 1);
            IPEndPoint src = new IPEndPoint(IPAddress.Any, 1);

            Random rnd = new Random();
            ushort value = (ushort)rnd.Next();
            byte[] icmpData = new byte[40];
            icmpData[0] = (byte)8;
            icmpData[1] = (byte)0;



            IPEndPoint pingDestination = new IPEndPoint(ipdest, 0);

            short TTL = 1;
            Socket icmp_s = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            icmp_s.ReceiveTimeout = 1000;
            icmp_s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, TTL);
            icmp_s.Bind(src);

            
            ushort checksumValue;
            IPHostEntry name = new IPHostEntry();
            var recivebuf = new byte[200];
            while (TTL < 127)
            {
                Console.Write("{0}", TTL);
                try
                {
                    icmpData[2] = 0;
                    icmpData[3] = 0;
                    icmpData[6] = (byte)(value);
                    icmpData[7] = (byte)(value >> 8);

                    checksumValue = ComputeChecksum(icmpData);

                    icmpData[2] = (byte)(checksumValue);
                    icmpData[3] = (byte)(checksumValue >> 8);


            //        Socket icmp_s = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            //        icmp_s.ReceiveTimeout = 10000;
                      icmp_s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, TTL);
                    //        icmp_s.Bind(src);
                    anyResponse = false;
                    for (int i = 0; i < 3; i++)
                    {


                        try
                        {
                            track.Reset();
                            
                            track.Start();
                            icmp_s.SendTo(icmpData, pingDestination);
                            var tmp = icmp_s.Receive(recivebuf);
                            track.Stop();
                            anyResponse = true;
                            if (track.ElapsedMilliseconds > 0)
                            {
                                Console.Write("  {0,3} ms  ", track.ElapsedMilliseconds);
                            }
                            else
                            {
                                Console.Write("  <1 ms   ");
                            }
                            value = (ushort)((recivebuf[54] << 8) + recivebuf[53] + 1);

                            icmpData[2] = 0;
                            icmpData[3] = 0;
                            icmpData[6] = (byte)(value);
                            icmpData[7] = (byte)(value >> 8);

                            checksumValue = ComputeChecksum(icmpData);

                            icmpData[2] = (byte)(checksumValue);
                            icmpData[3] = (byte)(checksumValue >> 8);




                        }
                        catch (SocketException e)
                        {
                            Console.Write("    *    ");
                        }





                    }

                    // icmp_s.Close();
                    if (anyResponse)
                    {
                        int HeaderLength = (recivebuf[0] & 0xf) * 4;



                        Console.Write(recivebuf[12] + "." + recivebuf[13] + "." + recivebuf[14] + "." + recivebuf[15] + "   ");


                        try
                        {

                            name = Dns.GetHostEntry(recivebuf[12] + "." + recivebuf[13] + "." + recivebuf[14] + "." + recivebuf[15]);
                            Console.WriteLine(name.HostName);
                        }
                        catch (SocketException e)
                        {
                            Console.WriteLine("этот хост неизвестен");
                        }


                        if (recivebuf[HeaderLength] == 0)
                            break;
                    }
                    else
                    {
                        Console.WriteLine("адрес не найден") ;
                        
                    }
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
            Console.WriteLine("Трассировка завершена.");
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