using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class VoiceEvent : UnityEvent<VoiceData> { };
    public struct VoiceData
    {
        public int networkId;
        public int voiceID;
        public int voiceIndex;
        public byte[] voiceArray;
    }
    public class MicrophoneDataSet : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _audioSource;
        private int _networkId;
        public int NetworkId { get => _networkId; }

        private int _nowVoiceId = 0;
        private List<VoiceData> _voiceByteDatas = new List<VoiceData>();
        /// <summary>
        /// 다른 클라이언트로 받은 Voice 시작
        /// </summary>
        public void StartVoiceClient(int networkId)
        {
            _networkId = networkId;
            _audioSource.clip = AudioClip.Create(string.Format("{0}_Voice", _networkId), 44100, 1, 44100, false);
            _audioSource.loop = true;
            _audioSource.mute = false;
            _audioSource.Play();
        }

        public void SetVoiceData(byte[] voiceArray, int voiceID, int voiceIndex)
        {
            if (voiceArray.Length <= 0)
                return;
            if (voiceID != _nowVoiceId)
            {
                _nowVoiceId = voiceID;
                _voiceByteDatas.Clear();
            }
            _voiceByteDatas.Add(new VoiceData() { voiceID = voiceID, voiceIndex = voiceIndex, voiceArray = voiceArray });

            if (CheckContainsIndex())
            {
                var receivedFloatData = new float[GetTotalDataLength() / 4];
                int nowPosition = 0;
                for (int index = 0; index < 3; index++)
                {
                    byte[] byteIndexData = GetVoiceData(index);
                    if (byteIndexData == null)
                        return;

                    var floatIndexData = new float[byteIndexData.Length / 4];
                    for (int byteDataIndex = 0; byteDataIndex < floatIndexData.Length; byteDataIndex++)
                    {
                        floatIndexData[byteDataIndex] = BitConverter.ToSingle(byteIndexData, byteDataIndex * 4);
                    }
                    Buffer.BlockCopy(floatIndexData, 0, receivedFloatData, nowPosition, floatIndexData.Length);
                    nowPosition += floatIndexData.Length;
                }
                _audioSource.clip.SetData(receivedFloatData, 0);
                _audioSource.Play();
            }
        }

        /// <summary>
        /// Voice 패킷 1,2,3이 다 들어왔는지 확인
        /// </summary>
        /// <returns>TRUE : 다 존재 FALSE : 존재하지않음</returns>
        private bool CheckContainsIndex()
        {
            return (_voiceByteDatas.Any(_ => _.voiceIndex == 0) &&
                _voiceByteDatas.Any(_ => _.voiceIndex == 1) &&
                _voiceByteDatas.Any(_ => _.voiceIndex == 2));
        }
        private int GetTotalDataLength()
        {
            int arrayCount = 0;
            for (int index = 0; index < _voiceByteDatas.Count; index++)
                arrayCount += _voiceByteDatas[index].voiceArray.Length;
            return arrayCount;
        }

        private byte[] GetVoiceData(int correctIndex)
        {
            for (int index = 0; index < _voiceByteDatas.Count; index++)
                if (_voiceByteDatas[index].voiceIndex == correctIndex)
                    return _voiceByteDatas[index].voiceArray;

            return null;
        }
    }
}
