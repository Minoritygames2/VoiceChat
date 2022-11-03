using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace VoiceChat
{
    public class TCPSession
    {
        public class AsyncObject
        {
            public AsyncObject(IList<ArraySegment<byte>> receiveObj, Socket socket)
            {
                this.receiveObj = receiveObj;
                this.socket = socket;
            }
            public IList<ArraySegment<byte>> receiveObj;
            public Socket socket;
        }
        public delegate void PacketEvent(ArraySegment<byte> packet);
        private PacketEvent OnPacketSended;
        private PacketEvent OnPacketReceived;
        private Socket _socket;
        private VoiceClientStatus _voiceClientStatus = new VoiceClientStatus();
        public VoiceClientStatus VoiceClientStatus { get=> _voiceClientStatus; set=> _voiceClientStatus = value; }
        private bool _isConnected = false;
        public TCPSession(Socket socket, PacketEvent sendCallback, PacketEvent receiveCallback)
        {
            _socket = socket;
            OnPacketReceived = receiveCallback;
            OnPacketSended = sendCallback;

            _isConnected = true;

            StartClientBeginReceive();
        }

        public void SessionClose()
        {
            _isConnected = false;
            _socket.Close();
            OnPacketSended -= OnPacketSended;
            OnPacketReceived -= OnPacketReceived;
        }
        public bool IsConnected()
        {
            return _socket.Connected;
        }

        /// <summary>
        /// 패킷송신
        /// </summary>
        public void SendPacket(int packetType, byte[] buffer)
        {
            int nowPosition = 0;
            var sendBuffer = new TCPSendBuffer(4096 * 100);
            var rsltBuffer = MakeHeader(packetType, sendBuffer.OpenBuffer(), out nowPosition);
            Buffer.BlockCopy(buffer, 0, rsltBuffer.Array, nowPosition, buffer.Length);
            nowPosition += buffer.Length;
            var rsltCloseBuffer = sendBuffer.CloseBuffer(nowPosition);
            Buffer.BlockCopy(rsltBuffer.Array, 0, rsltCloseBuffer.Array, 0, rsltCloseBuffer.Count);

            SendPacket(rsltCloseBuffer);
        }

        public void SendPacket(ArraySegment<byte> buffer)
        {
            IList<ArraySegment<byte>> bufferList = new List<System.ArraySegment<byte>>();
            bufferList.Add(buffer);
            _socket.BeginSend(bufferList, SocketFlags.None, new AsyncCallback(SendCallback), _socket);
        }

        private ArraySegment<byte> MakeHeader(int packetType, ArraySegment<byte> rsltBuffer, out int nowPosition)
        {
            nowPosition = 0;
            var playerIdByte = BitConverter.GetBytes(_voiceClientStatus.PlayerId);
            Buffer.BlockCopy(playerIdByte, 0, rsltBuffer.Array, nowPosition, playerIdByte.Length);
            nowPosition += playerIdByte.Length;

            var packetTypeByte = BitConverter.GetBytes(packetType);
            Buffer.BlockCopy(packetTypeByte, 0, rsltBuffer.Array, nowPosition, packetTypeByte.Length);
            nowPosition += packetTypeByte.Length;

            var channel = BitConverter.GetBytes(_voiceClientStatus.Channel);
            Buffer.BlockCopy(channel, 0, rsltBuffer.Array, nowPosition, channel.Length);
            nowPosition += packetTypeByte.Length;
            return rsltBuffer;
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                Socket socket = (Socket)asyncResult.AsyncState;
                var rsltSize = socket.EndSend(asyncResult);
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: 네트워크 :: 송신 에러 :: " + e.Message);
            }
        }

        /// <summary>
        /// 메세지 수신 시작
        /// </summary>
        public void StartClientBeginReceive()
        {
            if (!_isConnected)
                return;
            TCPReceiveBuffer tcpReceiveBuffer = new TCPReceiveBuffer(4096 * 100);
            var receiveBuffer = tcpReceiveBuffer.GetWriteSegment();
            IList<ArraySegment<byte>> bufferList = new List<System.ArraySegment<byte>>();
            bufferList.Add(receiveBuffer);
            _socket.BeginReceive(bufferList, SocketFlags.None, new AsyncCallback(ReceiveCallback), new AsyncObject(bufferList, _socket));
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                if (!_isConnected)
                    return;
                StartClientBeginReceive();
                AsyncObject asyncObj = (AsyncObject)asyncResult.AsyncState;
                var rslt = asyncObj.receiveObj[0];
                var rsltSize = asyncObj.socket.EndReceive(asyncResult);

                Debug.Log("Receive count : " + rslt.Count + " : " + rsltSize);

                if (rsltSize > 0)
                {
                    OnPacketReceived?.Invoke(new ArraySegment<byte>(rslt.Array));
                }
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: 네트워크 :: 수신 에러 :: " + e.Message);
            }
        }
    }

}
