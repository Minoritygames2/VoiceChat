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
        #region �÷��̾� ����
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

        //0 : ������������ 1 : ������ 2 : ���ӿϷ�
        private int _isConnected = 0;

        private ConcurrentQueue<ArraySegment<byte>> _receiveQueue = new ConcurrentQueue<ArraySegment<byte>>();
        private int _isAlbleReceiveQueue = 0;
        private int INT_ENABLE_RECEIVE = 0;
        private int INT_UNABLE_RECEIVE = 1;
        private TCPReceiveBuffer _receiveBuffer = new TCPReceiveBuffer(4096 * 100);
        #region ����
        /// <summary>
        /// TCPŬ���̾�Ʈ ����
        /// </summary>
        public void StartTcpClient(string ipStr, UnityAction OnClientConnected, UnityAction<VoiceChatPacket> OnReceiveVoicePacket)
        {
            if (_isConnected != 0)
            {
                Debug.Log("VoiceChat :: �̹� ������ �������Դϴ�");
                return;
            }

            this.OnClientConnected.AddListener(OnClientConnected);
            this.OnReceiveVoicePacket.AddListener(OnReceiveVoicePacket);

            try
            {
                _isConnected = 1;
                _tcpClient = new TcpClient();
                _tcpClient.BeginConnect(IPAddress.Parse(ipStr), 5555, new AsyncCallback(ConnectCallback), _tcpClient.Client);
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: ��Ʈ��ũ :: Ŭ���̾�Ʈ ���� ���� : " + e.Message);
            }
        }

        /// <summary>
        /// TCPŬ���̾�Ʈ ����
        /// </summary>
        public void StartTcpClient(TcpClient tcpClient, UnityAction<VoiceChatPacket> OnReceiveVoicePacket)
        {
            if (_isConnected != 0)
            {
                Debug.Log("VoiceChat :: �̹� ������ �������Դϴ�");
                return;
            }
            this.OnReceiveVoicePacket.AddListener(OnReceiveVoicePacket);
            _isConnected = 1;
            _tcpClient = tcpClient;
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            _isConnected = 2;
            OnClientConnected.Invoke();
        }
        #endregion

        #region �۽�
        /// <summary>
        /// ��Ŷ�۽�
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
                Debug.Log("VoiceChat :: ��Ʈ��ũ ::  �۽ſ��� :: " + e.Message);
                SessionClose();
            }
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                Socket socket = (Socket)asyncResult.AsyncState;
                var rsltSize = socket.EndSend(asyncResult);
                OnSendPacket();
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: ��Ʈ��ũ :: �۽� ���� :: " + e.Message);
            }
        }

        private void OnSendPacket()
        {
            Interlocked.Exchange(ref _isAlbleReceiveQueue, INT_ENABLE_RECEIVE);
        }
        #endregion

        #region ����
        /// <summary>
        /// �޼��� ���� ����
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
                    return;

                AsyncObject asyncObj = (AsyncObject)asyncResult.AsyncState;
                var rslt = asyncObj.receiveObj[0];
                var rsltSize = asyncObj.socket.EndReceive(asyncResult);

                if (rsltSize > 0)
                    _receiveQueue.Enqueue(new ArraySegment<byte>(rslt.Array, 0, rsltSize));
                StartClientBeginReceive();
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: ��Ʈ��ũ :: ���� ���� :: " + e.Message);
                AsyncObject asyncObj = (AsyncObject)asyncResult.AsyncState;
                asyncObj.socket.EndReceive(asyncResult);
                SessionClose();
            }
        }
        #endregion


        #region ��Ŷ����


        private void ParseVoicePacket(int playerId, byte[] message)
        {
            int size = 0;
            var voiceId = BitConverter.ToInt32(message, size);
            size += 4;

            var voiceIndex = BitConverter.ToInt32(message, size);
            size += 4;

            var voicePacket = new byte[message.Length - size];
            Buffer.BlockCopy(message, size, voicePacket, 0, message.Length - size);
            VoiceData voiceData = new VoiceData() { networkId = playerId, voiceID = voiceId, voiceIndex = voiceIndex, voiceArray = voicePacket };
        }

        /// <summary>
        /// ��Ŷ�۽�
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

            //��ó�� ����� ��Ŷ������ ������
            var sendDataSizeByte = BitConverter.GetBytes(sendDataSize + nowPosition);
            Buffer.BlockCopy(sendDataSizeByte, 0, rsltBuffer.Array, 0, 4);
            return rsltBuffer;
        }

        #endregion

        private void Update()
        {
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
            var message = new byte[packet.Length - size];
            Buffer.BlockCopy(packet, size, message, 0, packet.Length - size);
        }

        public void SessionClose()
        {
            _isConnected = 0;
            _tcpClient.Client.Close();
            OnClientConnected.RemoveAllListeners();
            OnClientDisconnected.RemoveAllListeners();
            OnReceiveVoicePacket.RemoveAllListeners();
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
            this.voiceData = voiceData;
        }
        public int playerId;
        public VoicePacketType packetType;
        public int channel;
        public byte[] message;
        public VoiceData voiceData;
    }

}
