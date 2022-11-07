using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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

        private ConcurrentQueue<TcpClient> _acceptQueue = new ConcurrentQueue<TcpClient>();
        private int _isAlbleAcceptQueue = 0;
        private int INT_ENABLE_ACCEPT = 0;
        private int INT_UNABLE_ACCEPT = 1;

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

                if (Interlocked.CompareExchange(ref _isAlbleAcceptQueue, INT_UNABLE_ACCEPT, INT_ENABLE_ACCEPT) == INT_UNABLE_ACCEPT)
                {
                    _acceptQueue.Enqueue(client);
                    Interlocked.Exchange(ref _isAlbleAcceptQueue, INT_ENABLE_ACCEPT);
                }

                
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

        private void Update()
        {
            if (_acceptQueue.Count > 0)
            {
                if (Interlocked.CompareExchange(ref _isAlbleAcceptQueue, INT_UNABLE_ACCEPT, INT_ENABLE_ACCEPT) == INT_UNABLE_ACCEPT)
                {
                    TcpClient result;
                    while (_acceptQueue.Count > 0)
                    {
                        while (_acceptQueue.TryDequeue(out result))
                        {
                            _tcpServerSession.AddPlayer(result);
                        }
                    }
                    Interlocked.Exchange(ref _isAlbleAcceptQueue, INT_ENABLE_ACCEPT);
                }
            }
        }



    }

}
