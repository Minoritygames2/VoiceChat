using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoiceChat
{
    public class VoicePlayer : MonoBehaviour
    {
        private int _playerId = 0;
        public int GetPlayerId(){ return _playerId; }
        [SerializeField]
        private TCPNetworkClient _networkClient;
        public TCPNetworkClient NetworkClient { get => _networkClient; }
        
        public void InitVoicePlayer(int playerId)
        {
            _playerId = playerId;
        }
    }

}
