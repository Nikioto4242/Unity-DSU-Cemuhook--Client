using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class NewUDPReceive : MonoBehaviour
{
    public string ServerAddress = "127.0.0.1";
    public int ServerPort = 26760;

    public int localPort = 50000;

    private UdpClient socket;
    private IPEndPoint endPoint;

    private Action<byte[]> outerReceiveCallback;
    private void Start()
    {
        endPoint = new IPEndPoint(IPAddress.Parse(ServerAddress), ServerPort);
        Connect(localPort);
    }
    public void SetCallback(Action<byte[]> callback)
    {
        outerReceiveCallback = callback;
    }
    public void SendData(byte[] _data)
    {
        try
        {
            if (socket == null)
            {
                Debug.LogError("socket is null");
                return;
            }
            socket.BeginSend(_data, _data.Length, null, null);
        }
        catch (Exception ex)
        {
            print(ex);
        }
        
    }

    public void Connect(int _localPort)
    {
        socket = new UdpClient(_localPort);
        socket.Connect(endPoint);

        socket.BeginReceive(ReceieveCallback, null);
    }
    private void ReceieveCallback(IAsyncResult _result)
    {
        try
        {
            byte[] _data = socket.EndReceive(_result, ref endPoint);
            socket.BeginReceive(ReceieveCallback, null);
            //
            HandleData(_data);
        }
        catch (Exception ex)
        {
            print($"error: {ex}");
        }
    }
    private void HandleData(byte[] _data)
    {
        //print($"data recieved, bytes: {_data.Length}");
        outerReceiveCallback?.Invoke(_data);
    }
}
