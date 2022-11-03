using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace VoiceChat
{
    [Serializable]
    public class ServerDetected
    {
        public string ServerIP;
    }
    public static class PacketJsonConverter
    {
        /// <summary>
        /// Json string => Class
        /// </summary>
        public static string JsonToString<T>(T packet)
        {
            return JsonUtility.ToJson(packet);
        }

        /// <summary>
        /// Class => Json string
        /// </summary>
        public static T FromJson<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
}
