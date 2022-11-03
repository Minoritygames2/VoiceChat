using System.Collections;
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

        List<VoiceClient> _voiceClients = new List<VoiceClient>();

        public void SetVoiceData(VoiceData voiceData)
        {
            VoiceClient client;
            if (!IsHasClient(voiceData.networkId))
                client = AddVoiceClient(voiceData.networkId);
            else
                client = GetClient(voiceData.networkId);

            client.SetVoiceData(voiceData.voiceArray, voiceData.voiceID, voiceData.voiceIndex);
        }

        public VoiceClient AddVoiceClient(int networkId)
        {
            var client = Instantiate(_playerPrefab, _playerTransform).GetComponent<VoiceClient>();
            client.StartVoiceClient(networkId);
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

