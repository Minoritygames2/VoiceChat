using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
namespace VoiceChat
{
    public class TCPNetworkListener
    {
        public delegate void TCPSessionEvent(TCPSession tcpSession);
        public TCPSessionEvent OnClientConnect;
        public TCPSession.PacketEvent OnReceivePacket;
        private TcpListener _listener;
        private List<TCPSession> _tcpClients = new List<TCPSession>();

        public void StartTcpListener(int port, UnityAction ServerStartedEvent, TCPSessionEvent onClientConnect, TCPSession.PacketEvent receivePacket)
        {
            Debug.Log("VoiceChat :: 네트워크 :: TCP 서버를 실행합니다");
            
            OnClientConnect = onClientConnect;
            OnReceivePacket = receivePacket;

            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), _listener);

            ServerStartedEvent?.Invoke();
        }

        private void AcceptCallback(IAsyncResult asyncResult)
        {
            try
            {
                var listener = (TcpListener)asyncResult.AsyncState;
                var client = listener.EndAcceptTcpClient(asyncResult);
                var tcpSession = new TCPSession(client.Client, (packet) => {  }, (packet) => { OnReceivePacket?.Invoke(packet); });
                _tcpClients.Add(tcpSession);

                OnClientConnect?.Invoke(tcpSession);
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: 네트워크 :: 수신 에러 :: " + e.Message);
            }
            finally
            {
                _listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), _listener);
            }
        }

        public void SendAllClients(ArraySegment<byte> packet)
        {
            Debug.Log("Server : SendAllClients");
            for (int index = 0; index < _tcpClients.Count; index++)
                if (_tcpClients[index].IsConnected())
                    _tcpClients[index].SendPacket(packet);
        }

        public void StopTCPListener()
        {
            OnClientConnect -= OnClientConnect;
            OnReceivePacket -= OnReceivePacket;
            for (int index = 0; index < _tcpClients.Count; index++)
                _tcpClients[index].SessionClose();
            if(_listener != null)
                _listener.Stop();
        }
    }

}
