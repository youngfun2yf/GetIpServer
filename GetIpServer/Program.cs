
// See https://aka.ms/new-console-template for more information
using System;
using System.Threading;

Console.WriteLine("Hello, World!");

var port = args?.Length > 0 ? int.Parse(args[0]) : 6996;
var s = new GetIpServer.IpServer(port);

while (true)
{
    Thread.Sleep(1);
}


