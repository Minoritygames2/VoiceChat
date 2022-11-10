using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class UDPNetworkReceiver : MonoBehaviour
    {
        public class ServerDetectedPacketReceive : UnityEvent<ServerDetected> { }
        private ServerDetectedPacketReceive _receiveCallback = new ServerDetectedPacketReceive();
        public struct UDPReceiveState
        {
            public UdpClient udpClient;
            public IPEndPoint ipEndPoint;
        }
        private UdpClient _udpClient;
        private IPEndPoint _ipEndPoint;

        private ConcurrentQueue<ServerDetected> _queue = new ConcurrentQueue<ServerDetected>();
        private bool _isStartedNetwork = false;

        private int _isAlbleQueue = 0;
        private int INT_ENABLE_CHANGE = 0;
        private int INT_UNABLE_CHANGE = 1;

        /// <summary>
        /// 패킷리시브 시작
        /// </summary>
        public void StartUDPBroadcastReceiver(int port, UnityAction<ServerDetected> callback)
        {
            _receiveCallback.RemoveAllListeners();
            _receiveCallback.AddListener(callback);

            try
            {
                _isStartedNetwork = true;

                _ipEndPoint = new IPEndPoint(IPAddress.Any, port);
                _udpClient = new UdpClient();
                _udpClient.EnableBroadcast = true;
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpClient.ExclusiveAddressUse = false;
                _udpClient.Client.Bind(_ipEndPoint);
                _udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), new UDPReceiveState()
                {
                    udpClient = _udpClient,
                    ipEndPoint = _ipEndPoint
                });
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: 네트워크 :: 수신 시작 에러 :: " + e.Message);

                StopUDPReceiver();
            }
        }

        /// <summary>
        /// 패킷 받음
        /// </summary>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            //Debug.Log("VoiceChat :: 네트워크 :: 수신");
            try
            {
                if (!_isStartedNetwork)
                    return;
                var udpState = (UDPReceiveState)asyncResult.AsyncState;
                byte[] receiveByte = udpState.udpClient.EndReceive(asyncResult, ref udpState.ipEndPoint);
                string receiveString = Encoding.UTF8.GetString(receiveByte);

                //Debug.Log("VoiceChat :: 네트워크 :: 수신데이터 : " + receiveString);
                ServerDetected serverDetectedPacket = PacketJsonConverter.FromJson<ServerDetected>(receiveString);
                EnqueQueue(serverDetectedPacket);
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: 네트워크 :: 수신 에러 :: " + e.Message);
            }
            finally
            {
                if (_isStartedNetwork)
                {
                    _udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), new UDPReceiveState()
                    {
                        udpClient = _udpClient,
                        ipEndPoint = _ipEndPoint
                    });
                }    
            }
        }

        public void EnqueQueue(ServerDetected packet)
        {
            while (Interlocked.CompareExchange(ref _isAlbleQueue, INT_UNABLE_CHANGE, INT_ENABLE_CHANGE) == INT_ENABLE_CHANGE)
            {
            }
            if (!_isStartedNetwork)
                return;

            _queue.Enqueue(packet);
            Interlocked.Exchange(ref _isAlbleQueue, INT_ENABLE_CHANGE);
        }

        private void Update()
        {
            if (!_isStartedNetwork)
                return;
            if (_queue.Count > 0)
            {
                if (Interlocked.CompareExchange(ref _isAlbleQueue, INT_UNABLE_CHANGE, INT_ENABLE_CHANGE) == INT_UNABLE_CHANGE)
                {
                    var result = new ServerDetected();
                    while (_queue.TryDequeue(out result))
                    {
                        _receiveCallback?.Invoke(result);
                        Interlocked.Exchange(ref _isAlbleQueue, INT_ENABLE_CHANGE);
                    }
                }
            }
        }

        public void StopUDPReceiver()
        {
            if (!_isStartedNetwork)
                return;
            _isStartedNetwork = false;
            _udpClient.Close();
            _queue = new ConcurrentQueue<ServerDetected>();
            _receiveCallback.RemoveAllListeners();
        }
    }
}
