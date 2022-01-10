using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;

namespace GetIpServer
{
	public class IpServer
	{

		private readonly Dictionary<string, Socket> _dicIp;

		private readonly Dictionary<string, string> _dicName;

		private Socket? _socketSend;
		private Socket _socketWatch;

		private Thread _acceptSocketThread;
		private Thread? _threadReceive;

		public IpServer(int port)
		{
			_dicIp = new Dictionary<string, Socket>();
			_dicName = new Dictionary<string, string>();
			_socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			var ip = IPAddress.Parse("127.0.0.1");
			var point = new IPEndPoint(ip, port);

			_socketWatch.Bind(point);
			_socketWatch.Listen(10);

			_acceptSocketThread = new Thread(new ParameterizedThreadStart(StartListen));
			_acceptSocketThread.IsBackground = true;
			_acceptSocketThread.Start(_socketWatch);
		}

        private void StartListen(object? obj)
        {
			var sw = obj as Socket;
            while (true)
            {
				_socketSend = sw!.Accept();

				var strIp = _socketSend.RemoteEndPoint!.ToString();
				_dicIp.Add(strIp!, _socketSend);

                Console.WriteLine($"remote : {strIp} connected");

				var tr = new Thread(new ParameterizedThreadStart(Receive));
				tr.IsBackground = true;
				tr.Start(_socketSend);
            }
        }

        private void Receive(object? obj)
        {
			var ss = obj as Socket;
			var buffer = new byte[2048];
			while (true)
            {
				var count = ss!.Receive(buffer);
                if (count == 0)
                {
					break;
                }

				var str = Encoding.UTF8.GetString(buffer, 0, count);
				var strReveiveMsg = $"receive : {ss.RemoteEndPoint} msg : {str}";
                Console.WriteLine(strReveiveMsg);

				_dicName[str] = ss.RemoteEndPoint.ToString();
				break;
			}

			var sb = new StringBuilder();

            foreach (var kv in _dicName)
            {
				sb.AppendLine($"{kv.Key} - {kv.Value}\n");
            }

			buffer = Encoding.UTF8.GetBytes(sb.ToString());

            foreach (var s in _dicIp.Values)
            {
                if (s != null)
                {
                    try
					{
						s.Send(buffer);
					}
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}

