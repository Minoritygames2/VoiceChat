using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class TCPClientSession : MonoBehaviour
    {
        public UnityEvent OnClientConnected = new UnityEvent();
        public UnityEvent OnClientDisconnected = new UnityEvent();
        public VoiceEvent OnReceiveVoicePacket = new VoiceEvent();

        private TCPSession _tcpSession;

        private TCPReceiveBuffer _receiveBuffer = new TCPReceiveBuffer(4096 * 100);
        public void OnConnectClient(Socket socket)
        {
            _tcpSession = new TCPSession(socket, OnSendPacket, OnReceivedPacket);
        }

        public void DisconnectClient()
        {
            if (_tcpSession != null)
                _tcpSession.SessionClose();
        }

        private ConcurrentQueue<ArraySegment<byte>> _sendQueue = new ConcurrentQueue<ArraySegment<byte>>();
        private int _isAlbleSendQueue = 0;
        private int INT_ENABLE_SEND = 0;
        private int INT_UNABLE_SEND = 1;

        private ConcurrentQueue<ArraySegment<byte>> _receiveQueue = new ConcurrentQueue<ArraySegment<byte>>();
        private int _isAlbleReceiveQueue = 0;
        private int INT_ENABLE_RECEIVE = 0;
        private int INT_UNABLE_RECEIVE = 1;

        private void OnSendPacket(ArraySegment<byte> packet)
        {
            Interlocked.Exchange(ref _isAlbleReceiveQueue, INT_ENABLE_RECEIVE);
        }

        private void OnReceivedPacket(ArraySegment<byte> packet)
        {
            _receiveQueue.Enqueue(packet);
        }

        private void Update()
        {
            if (_receiveQueue.Count > 0)
            {
                if (Interlocked.CompareExchange(ref _isAlbleReceiveQueue, INT_UNABLE_RECEIVE, INT_ENABLE_RECEIVE) == INT_UNABLE_RECEIVE)
                {
                    ArraySegment<byte> result;
                    while (_receiveQueue.TryDequeue(out result))
                    {
                        if(_receiveBuffer.SetBuffer(result.Array))
                        {
                            CheckPacket(_receiveBuffer.GetBuffer().Array);
                        }
                        
                        Interlocked.Exchange(ref _isAlbleReceiveQueue, INT_ENABLE_RECEIVE);
                    }
                }
            }

            if (_sendQueue.Count > 0)
            {
                if (Interlocked.CompareExchange(ref _isAlbleSendQueue, INT_UNABLE_SEND, INT_ENABLE_SEND) == INT_UNABLE_SEND)
                {
                    ArraySegment<byte> result;
                    while (_sendQueue.TryDequeue(out result))
                    {
                        _tcpSession.SendPacket((int)VoicePacketType.VOICE, result.Array);
                    }
                }
            }
        }

        private void CheckPacket(byte[] packet)
        {
            int size = 0;
            var packetSize = BitConverter.ToInt32(packet, size);
            size += 4;
            var playerId = BitConverter.ToInt32(packet, size);
            size += 4;
            var packetType = BitConverter.ToInt32(packet, size);
            size += 4;
            var channel = BitConverter.ToInt32(packet, size);
            size += 4;
            var message = new byte[packet.Length - size];
            Buffer.BlockCopy(packet, size, message, 0, packet.Length - size);


            if (playerId == 0)
                ServerPacket(packetType, message);
            else
                ClientPacket(playerId, packetType, message);
        }

        private void ServerPacket(int packetType, byte[] message)
        {
            switch ((VoicePacketType)packetType)
            {
                case VoicePacketType.ACCEPT:
                    var playerId = BitConverter.ToInt32(message, 0);
                    Debug.Log("voiceChat :: 플레이어 등록 " + playerId);
                    _tcpSession.VoiceClientStatus.PlayerId = playerId;

                    //플레이어 등록이 되면 접속으로침
                    OnClientConnected?.Invoke();
                    break;
                default:
                    break;
            }
        }

        private void ClientPacket(int playerId, int packetType, byte[] message)
        {
            if (_tcpSession.VoiceClientStatus.PlayerId == playerId)
                return;

            switch ((VoicePacketType)packetType)
            {
                case VoicePacketType.VOICE:
                    ParseVoicePacket(playerId, message);
                    break;
                default:
                    break;
            }
        }

        private void ParseVoicePacket(int playerId, byte[] message)
        {
            int size = 0;
            var voiceId = BitConverter.ToInt32(message, size);
            size += 4;

            var voiceIndex = BitConverter.ToInt32(message, size);
            size += 4;
            var voicePacket = new byte[message.Length - size];
            Buffer.BlockCopy(message, size, voicePacket, 0, message.Length - size);
            OnReceiveVoicePacket?.Invoke(new VoiceData() { networkId = playerId, voiceID = voiceId, voiceIndex = voiceIndex, voiceArray = voicePacket });
        }

        /// <summary>
        /// voice Packet
        /// </summary>
        public void SendPacket(VoiceData voiceData)
        {
            int nowPosition = 0;
            var sendBuffer = new TCPSendBuffer(4096 * 100);
            var voiceBuffer = sendBuffer.OpenBuffer();
            var voiceIdByte = BitConverter.GetBytes(voiceData.voiceID);
            Buffer.BlockCopy(voiceIdByte, 0, voiceBuffer.Array, nowPosition, voiceIdByte.Length);
            nowPosition += voiceIdByte.Length;

            var voiceIndex = BitConverter.GetBytes(voiceData.voiceIndex);
            Buffer.BlockCopy(voiceIndex, 0, voiceBuffer.Array, nowPosition, voiceIndex.Length);
            nowPosition += voiceIndex.Length;

            Buffer.BlockCopy(voiceData.voiceArray, 0, voiceBuffer.Array, nowPosition, voiceData.voiceArray.Length);
            nowPosition += voiceData.voiceArray.Length;

            var rsltCloseBuffer = sendBuffer.CloseBuffer(nowPosition);
            Buffer.BlockCopy(voiceBuffer.Array, 0, rsltCloseBuffer.Array, 0, rsltCloseBuffer.Count);

            _sendQueue.Enqueue(rsltCloseBuffer.Array);
        }

        private void OnDestroy()
        {
            if (_tcpSession.IsConnected())
                _tcpSession.SessionClose();
        }
    }
}

