using System.Net.Sockets;
using Microsoft.VisualBasic;

namespace api.Util;

public static class SocketHttpClient
{
    public static HttpClient HttpClient(string server)
    {
        var socketPath = $"/run/{server}.sock";

        var handler = new SocketsHttpHandler
        {
            ConnectCallback = async (context, cancellationToken) =>
            {
                var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                var endpoint = new UnixDomainSocketEndPoint(socketPath);
                await socket.ConnectAsync(endpoint, cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
        };

        var client = new HttpClient(handler);
        return client;
    }
}