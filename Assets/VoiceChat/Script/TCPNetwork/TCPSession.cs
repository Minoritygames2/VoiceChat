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

        private TCPSendBuffer _sendBuffer = new TCPSendBuffer(4096 * 100);
        private TCPReceiveBuffer _receiveBuffer = new TCPReceiveBuffer(100000);

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

            var rsltBuffer = MakeHeader(packetType, _sendBuffer.OpenBuffer(4048), out nowPosition);
            Buffer.BlockCopy(buffer, 0, rsltBuffer.Array, nowPosition, buffer.Length);
            nowPosition += buffer.Length;
            var rsltCloseBuffer = _sendBuffer.CloseBuffer(nowPosition);
            Buffer.BlockCopy(rsltBuffer.Array, 0, rsltCloseBuffer.Array, 0, rsltCloseBuffer.Count);

            SendPacket(rsltCloseBuffer);
        }

        /// <summary>
        /// voice Packet
        /// </summary>
        public void SendPacket(VoiceData voiceData)
        {
            int nowPosition = 0;
            
            var rsltBuffer = MakeHeader((int)VoicePacketType.VOICE, _sendBuffer.OpenBuffer(60000), out nowPosition);

            var voiceIdByte = BitConverter.GetBytes(voiceData.voiceID);
            Buffer.BlockCopy(voiceIdByte, 0, rsltBuffer.Array, nowPosition, voiceIdByte.Length);
            nowPosition += voiceIdByte.Length;

            var voiceIndex = BitConverter.GetBytes(voiceData.voiceIndex);
            Buffer.BlockCopy(voiceIndex, 0, rsltBuffer.Array, nowPosition, voiceIndex.Length);
            nowPosition += voiceIndex.Length;

            Buffer.BlockCopy(voiceData.voiceArray, 0, rsltBuffer.Array, nowPosition, voiceData.voiceArray.Length);
            nowPosition += voiceData.voiceArray.Length;

            var rsltCloseBuffer = _sendBuffer.CloseBuffer(nowPosition);
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

            _receiveBuffer.Clear();
            var receiveBuffer = _receiveBuffer.GetWriteSegment();
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
                AsyncObject asyncObj = (AsyncObject)asyncResult.AsyncState;
                var rslt = asyncObj.receiveObj[0];
                var rsltSize = asyncObj.socket.EndReceive(asyncResult);
                if (rsltSize > 0)
                {
                    if (_receiveBuffer.IsAbleToReceive(rsltSize))
                    {
                        var readBuffer = _receiveBuffer.GetReadSegment(rsltSize);
                        Buffer.BlockCopy(readBuffer.Array, 0, rslt.Array, 0, rsltSize);
                        OnPacketReceived?.Invoke(readBuffer);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: 네트워크 :: 수신 에러 :: " + e.Message);
            }
            finally
            {
                if (_isConnected)
                    StartClientBeginReceive();
            }
        }
    }

}
