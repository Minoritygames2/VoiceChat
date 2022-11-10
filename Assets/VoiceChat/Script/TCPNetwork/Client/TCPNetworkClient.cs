using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class TCPNetworkClient : MonoBehaviour
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
        #region 플레이어 세팅
        private int _playerId = 0;
        public int PlayerId { get=> _playerId; set=> _playerId = value; }
        private int _channel = 0;
        public int Channel { get => _channel; set => _channel = value; }
        #endregion

        public class VoiceChatPacketEvnet : UnityEvent<VoiceChatPacket> { }
        private TcpClient _tcpClient;
        public UnityEvent OnClientConnected = new UnityEvent();
        public UnityEvent OnClientDisconnected = new UnityEvent();
        public VoiceChatPacketEvnet OnReceiveVoicePacket = new VoiceChatPacketEvnet();

        //0 : 접속하지않음 1 : 접속중 2 : 접속완료
        private int _isConnected = 0;
        private int _chConnected = 0;

        private ConcurrentQueue<ArraySegment<byte>> _receiveQueue = new ConcurrentQueue<ArraySegment<byte>>();
        private int _isAlbleReceiveQueue = 0;
        private int INT_ENABLE_RECEIVE = 0;
        private int INT_UNABLE_RECEIVE = 1;
        private TCPReceiveBuffer _receiveBuffer = new TCPReceiveBuffer(4096 * 100);
        #region 접속
        /// <summary>
        /// TCP클라이언트 접속
        /// </summary>
        public void StartTcpClient(string ipStr, UnityAction OnClientConnected, UnityAction<VoiceChatPacket> OnReceiveVoicePacket)
        {
            if (_isConnected != 0)
            {
                Debug.Log("VoiceChat :: 이미 서버에 접속중입니다");
                return;
            }

            this.OnClientConnected.AddListener(OnClientConnected);
            this.OnReceiveVoicePacket.AddListener(OnReceiveVoicePacket);

            try
            {
                _chConnected = 1;
                _tcpClient = new TcpClient();
                _tcpClient.BeginConnect(IPAddress.Parse(ipStr), 5555, new AsyncCallback(ConnectCallback), _tcpClient.Client);
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: 네트워크 :: 클라이언트 접속 실패 : " + e.Message);
            }
        }

        /// <summary>
        /// TCP클라이언트 접속
        /// </summary>
        public void StartTcpClient(TcpClient tcpClient, UnityAction<VoiceChatPacket> OnReceiveVoicePacket)
        {
            if (_isConnected != 0)
            {
                Debug.Log("VoiceChat :: 이미 서버에 접속중입니다");
                return;
            }
            this.OnReceiveVoicePacket.AddListener(OnReceiveVoicePacket);
            _chConnected = 2;
            _isConnected = 2;
            _tcpClient = tcpClient;
            StartClientBeginReceive();
        }


        private void ConnectCallback(IAsyncResult asyncResult)
        {
            _chConnected = 2;
        }
        #endregion

        #region 송신
        /// <summary>
        /// 패킷송신
        /// </summary>
        private void SendPacket(ArraySegment<byte> buffer)
        {
            if (_isConnected != 2)
                return;

            try
            {
                IList<ArraySegment<byte>> bufferList = new List<System.ArraySegment<byte>>();
                bufferList.Add(buffer);
                _tcpClient.Client.BeginSend(bufferList, SocketFlags.None, new AsyncCallback(SendCallback), _tcpClient.Client);
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: 네트워크 ::  송신에러 :: " + e.Message);
                SessionClose();
            }
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

        #endregion

        #region 수신
        /// <summary>
        /// 메세지 수신 시작 
        /// </summary>
        public void StartClientBeginReceive()
        {
            if (_isConnected != 2)
                return;
            var receiveBuffer = new ArraySegment<byte>(new byte[4096 * 100], 0, 4096 * 100);
            IList<ArraySegment<byte>> bufferList = new List<System.ArraySegment<byte>>();
            bufferList.Add(receiveBuffer);
            _tcpClient.Client.BeginReceive(bufferList, SocketFlags.None, new AsyncCallback(ReceiveCallback), new AsyncObject(bufferList, _tcpClient.Client));
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                if (_isConnected != 2)
                {
                    StartClientBeginReceive();
                    return;
                }

                AsyncObject asyncObj = (AsyncObject)asyncResult.AsyncState;
                var rslt = asyncObj.receiveObj[0];
                var rsltSize = asyncObj.socket.EndReceive(asyncResult);
                if (rsltSize > 0)
                    _receiveQueue.Enqueue(new ArraySegment<byte>(rslt.Array, 0, rsltSize));
                StartClientBeginReceive();
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: 네트워크 :: 수신 에러 :: " + e.Message);
                SessionClose();
            }
        }
        #endregion


        #region 패킷관리

        /// <summary>
        /// 패킷송신
        /// </summary>
        public void SendPacket(VoiceChatPacket voiceChatPacket)
        {
            int nowPosition = 0;
            var sendBuffer = new TCPSendBuffer(4096 * 100);
            var rsltBuffer = MakeHeader(voiceChatPacket.playerId, voiceChatPacket.channel, (int)voiceChatPacket.packetType, sendBuffer.OpenBuffer(), voiceChatPacket.message.Length, out nowPosition);

            if (voiceChatPacket.packetType == VoicePacketType.VOICE)
            {
                var voiceData = voiceChatPacket.voiceData;

                var voiceIdByte = BitConverter.GetBytes(voiceData.voiceID);
                Buffer.BlockCopy(voiceIdByte, 0, rsltBuffer.Array, nowPosition, voiceIdByte.Length);
                nowPosition += voiceIdByte.Length;

                var voiceIndex = BitConverter.GetBytes(voiceData.voiceIndex);
                Buffer.BlockCopy(voiceIndex, 0, rsltBuffer.Array, nowPosition, voiceIndex.Length);
                nowPosition += voiceIndex.Length;

                Buffer.BlockCopy(voiceData.voiceArray, 0, rsltBuffer.Array, nowPosition, voiceData.voiceArray.Length);
                nowPosition += voiceData.voiceArray.Length;
            }
            else
            {
                Buffer.BlockCopy(voiceChatPacket.message, 0, rsltBuffer.Array, nowPosition, voiceChatPacket.message
                .Length);
                nowPosition += voiceChatPacket.message.Length;
            }
            var nowPositionByte = BitConverter.GetBytes(nowPosition);
            Buffer.BlockCopy(nowPositionByte, 0, rsltBuffer.Array, 0, nowPositionByte.Length);
            var rsltCloseBuffer = sendBuffer.CloseBuffer(nowPosition);
            Buffer.BlockCopy(rsltBuffer.Array, 0, rsltCloseBuffer.Array, 0, rsltCloseBuffer.Count);
            SendPacket(rsltCloseBuffer);
        }

        protected ArraySegment<byte> MakeHeader(int playerId, int playerChannel, int packetType, ArraySegment<byte> rsltBuffer, int sendDataSize, out int nowPosition)
        {
            nowPosition = 4;
            var playerIdByte = BitConverter.GetBytes(playerId);
            Buffer.BlockCopy(playerIdByte, 0, rsltBuffer.Array, nowPosition, playerIdByte.Length);
            nowPosition += playerIdByte.Length;

            var packetTypeByte = BitConverter.GetBytes(packetType);
            Buffer.BlockCopy(packetTypeByte, 0, rsltBuffer.Array, nowPosition, packetTypeByte.Length);
            nowPosition += packetTypeByte.Length;

            var channel = BitConverter.GetBytes(playerChannel);
            Buffer.BlockCopy(channel, 0, rsltBuffer.Array, nowPosition, channel.Length);
            nowPosition += packetTypeByte.Length;

            //맨처음 헤더에 패킷사이즈 보내기
            var sendDataSizeByte = BitConverter.GetBytes(sendDataSize + nowPosition);
            Buffer.BlockCopy(sendDataSizeByte, 0, rsltBuffer.Array, 0, 4);
            return rsltBuffer;
        }

        #endregion

        private void Update()
        {
            if(_chConnected != _isConnected)
            {
                _isConnected = _chConnected;
                if (_chConnected == 0)
                {
                    SessionClose();
                    OnClientDisconnected?.Invoke();
                }
                else if (_chConnected == 1)
                {

                }
                else if (_chConnected == 2)
                {
                    StartClientBeginReceive();
                    OnClientConnected?.Invoke();
                }
            }

            if (_receiveQueue.Count > 0)
            {
                if (Interlocked.CompareExchange(ref _isAlbleReceiveQueue, INT_UNABLE_RECEIVE, INT_ENABLE_RECEIVE) == INT_UNABLE_RECEIVE)
                {
                    ArraySegment<byte> result;
                    while (_receiveQueue.Count > 0)
                    {
                        while (_receiveQueue.TryDequeue(out result))
                        {
                            if (_receiveBuffer.SetBuffer(result))
                            {
                                
                                CheckPacket(_receiveBuffer.GetBuffer().Array);
                            }
                        }
                    }
                    Interlocked.Exchange(ref _isAlbleReceiveQueue, INT_ENABLE_RECEIVE);
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
            if(packetType != (int)VoicePacketType.VOICE)
            {
                var message = new byte[packet.Length - size];
                Buffer.BlockCopy(packet, size, message, 0, packet.Length - size);
                OnReceiveVoicePacket?.Invoke(new VoiceChatPacket(playerId, (VoicePacketType)packetType, channel, message));
            }
            else
            {
                var voiceIdByte = BitConverter.ToInt32(packet, size);
                size += 4;

                var voiceIndex = BitConverter.ToInt32(packet, size);
                size += 4;

                var message = new byte[packet.Length - size];
                Buffer.BlockCopy(packet, size, message, 0, packet.Length - size);
                OnReceiveVoicePacket?.Invoke(new VoiceChatPacket(playerId, (VoicePacketType)packetType, channel, new VoiceData() {
                    networkId = playerId, voiceID = voiceIdByte, voiceIndex = voiceIndex, voiceArray = message
                }));
            }
            
            
        }

        public void SessionClose()
        {
            _chConnected = 0;
            if(_tcpClient.Client != null && _tcpClient.Connected)
                _tcpClient.Client.Close();
            OnClientConnected.RemoveAllListeners();
            OnClientDisconnected.RemoveAllListeners();
            OnReceiveVoicePacket.RemoveAllListeners();
        }

        private void OnDestroy()
        {
            SessionClose();
        }

    }


    public class VoiceChatPacket
    {
        public VoiceChatPacket(int playerId, VoicePacketType packetType, int channel, byte[] message)
        {
            this.playerId = playerId;
            this.packetType = packetType;
            this.channel = channel;
            this.message = message;
        }
        public VoiceChatPacket(int playerId, VoicePacketType packetType, int channel, VoiceData voiceData)
        {
            this.playerId = playerId;
            this.packetType = packetType;
            this.channel = channel;
            this.message = new byte[0];
            this.voiceData = voiceData;
        }
        public int playerId;
        public VoicePacketType packetType;
        public int channel;
        public byte[] message;
        public VoiceData voiceData;
    }

}
