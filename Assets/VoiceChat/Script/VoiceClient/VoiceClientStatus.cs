using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoiceChat
{
    public class VoiceClientStatus
    {
        /// <summary>
        /// PlayerID값
        /// </summary>
        public int PlayerId = 0;
        /// <summary>
        /// 채널값
        /// 0 : ALL
        /// </summary>
        public int Channel = 0;
    }

    public enum VoicePacketType
    {
        UNKNOWN,
        ACCEPT,
        CONNECT_REQUEST,
        CONNECT_RESPONCE,
        DISCONNECT,
        MESSAGE,
        VOICE
    }
}
