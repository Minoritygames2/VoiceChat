using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
namespace VoiceChat
{
    public class TCPNetworkListener : MonoBehaviour
    {
        private UnityEvent ServerStartedEvent = new UnityEvent();
        private TcpListener _listener;
        [SerializeField]
        private TCPServerSession _tcpServerSession;
        public void StartTCPListener(int port, UnityAction ServerStartedEvent)
        {
            Debug.Log("VoiceChat :: 네트워크 :: TCP 서버를 실행합니다");
            this.ServerStartedEvent.RemoveAllListeners();
            this.ServerStartedEvent.AddListener(ServerStartedEvent);

            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), _listener);

            ServerStartedEvent?.Invoke();
        }

        public void StopTCPListener()
        {
            if (_listener != null)
                _listener.Stop();
        }

        private void AcceptCallback(IAsyncResult asyncResult)
        {
            try
            {
                var listener = (TcpListener)asyncResult.AsyncState;
                var client = listener.EndAcceptTcpClient(asyncResult);

                _tcpServerSession.AddPlayer(client);
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

        

        
    }

}
