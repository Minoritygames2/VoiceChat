using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VoiceChat
{
    public class CanvasPlayerItem : MonoBehaviour
    {
        [SerializeField]
        protected Text _txtPlayerId;
        protected int _playerId = 0;
        public void InitPlayerItem(int playerId)
        {
            _playerId = playerId;
            _txtPlayerId.text = playerId.ToString();
        }
        public int GetPlayerId()
        {
            return _playerId;
        }
    }
}