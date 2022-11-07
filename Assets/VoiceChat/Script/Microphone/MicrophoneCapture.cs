using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VoiceChat
{
    public class MicrophoneCapture : MonoBehaviour
    {
        public static string MicName_NONE = "None";

        [SerializeField]
        private AudioSource _micAudioSource;
        private string _settingMicName = string.Empty;

        private bool _isStartCapture = false;
        private WaitForSeconds _wait = new WaitForSeconds(0.05f);

        private int _voiceID = 0;

        /// <summary>
        /// ����ũ ĸ�� ����
        /// </summary>
        public void StartCapture(string micName, UnityAction<VoiceData> SendPacket)
        {
            try
            {
                if (micName.Equals(MicName_NONE))
                    return;

                //����ũ ������ �Ǿ����� ��� ����
                if (!_settingMicName.Equals(string.Empty))
                {
                    Microphone.End(_settingMicName);
                    _settingMicName = string.Empty;
                }

                _settingMicName = micName;

                Debug.Log("VoiceChat :: ����ũ ĸ�ĸ� �����մϴ�" + micName);

                _micAudioSource.clip = Microphone.Start(_settingMicName, true, 1, AudioSettings.outputSampleRate);
                _micAudioSource.loop = true;
                _micAudioSource.Play();

                _isStartCapture = true;

                StartCoroutine(SendVoice(SendPacket));
            }
            catch (Exception e)
            {
                Debug.Log("VoiceChat :: ����ũ ĸ�� �����Ͽ����ϴ� :: " + e.Message);
            }
        }

        private IEnumerator SendVoice(UnityAction<VoiceData> SendPacket)
        {
            while (_isStartCapture)
            {
                _voiceID++;
                if (_voiceID > 10000)
                    _voiceID = 0;

                var voiceValues = new float[24000];

                _micAudioSource.clip.GetData(voiceValues, _micAudioSource.timeSamples);
                Debug.Log(_micAudioSource.timeSamples);

                for (int index = 0; index < 3; index++)
                {
                    var byteValue = new byte[voiceValues.Length * 4 / 3];

                    var byteValueIndex = 0;
                    for (int voiceValueIndex = voiceValues.Length / 3 * index; voiceValueIndex < voiceValues.Length / 3 * (index + 1); voiceValueIndex++)
                    {
                        var data = BitConverter.GetBytes(voiceValues[voiceValueIndex]);
                        Array.Copy(data, 0, byteValue, byteValueIndex, 4);
                        byteValueIndex += 4;
                    }
                    SendPacket?.Invoke(new VoiceData() { voiceID = _voiceID, voiceIndex = index, voiceArray = byteValue });
                }
                yield return _wait;
            }
        }
    }

}
