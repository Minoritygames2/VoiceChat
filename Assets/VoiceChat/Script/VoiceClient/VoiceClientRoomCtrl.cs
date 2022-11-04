﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoiceChat
{
    public class VoiceClientRoomCtrl : MonoBehaviour
    {
        [SerializeField]
        private Transform _playerTransform;
        [SerializeField]
        private GameObject _playerPrefab;

        [SerializeField]
        private ClientRoomCanvas _roomCanvasCtrl;

        [Header("Network")]
        [SerializeField]
        private TCPNetworkClient _myTcpClient;

        [Header("MIC")]
        [SerializeField]
        private MicrophoneController _micCtrl;

        List<VoiceClient> _voiceClients = new List<VoiceClient>();

        public void StartVoiceChat(string micName, string serverIp)
        {
            //VoiceChat Room Active상태로
            _roomCanvasCtrl.ActiveRoomArea(serverIp);

            //TCP시작되면 마이크 캡쳐 시작
            _myTcpClient.StartTcpClient(serverIp,
                () => {
                    _micCtrl.StartCapture(micName, (voiceData)
                        => { _myTcpClient.SendVoicePacket(voiceData); });
                }
                ,
                (voiceData) => { SetVoiceData(voiceData); }
                );
        }

        public void SetVoiceData(VoiceData voiceData)
        {
            VoiceClient client;
            if (!IsHasClient(voiceData.networkId))
                client = AddVoiceClient(voiceData.networkId);
            else
                client = GetClient(voiceData.networkId);

            client.SetVoiceData(voiceData.voiceArray, voiceData.voiceID, voiceData.voiceIndex, voiceData.timeSamples);
        }

        public VoiceClient AddVoiceClient(int networkId)
        {
            var client = Instantiate(_playerPrefab, _playerTransform).GetComponent<VoiceClient>();
            client.StartVoiceClient(networkId);
            _voiceClients.Add(client);
            return client;
        }

        private bool IsHasClient(int networkId)
        {
            for(int index = 0; index < _voiceClients.Count; index++)
                if (_voiceClients[index].NetworkId == networkId)
                    return true;
            return false;
        }

        private VoiceClient GetClient(int networkId)
        {
            for (int index = 0; index < _voiceClients.Count; index++)
                if (_voiceClients[index].NetworkId == networkId)
                    return _voiceClients[index];
            return null;
        }
    }
}

