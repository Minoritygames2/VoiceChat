using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace VoiceChat
{
    public class ClientSettingCanvas : MonoBehaviour
    {
        public class EndSettingEvent : UnityEvent<string, string> { }
        private EndSettingEvent _endSettingCallback = new EndSettingEvent();

        [SerializeField]
        private Transform _settingArea;

        [Header("마이크설정")]
        [SerializeField]
        private Dropdown _micDropDown;
        [SerializeField]
        private Button _micSearchBtn;

        [Header("서버 서치")]
        [SerializeField]
        private InputField _inputPort;
        [SerializeField]
        private Button _btnServerSearch;
        [SerializeField]
        private Text _txtSearchTime;
        [SerializeField]
        private Transform _serverItemTransform;
        [SerializeField]
        private GameObject _serverButtonItem;

        List<BTN_ServerSearchItem> _serverSearchBtnItem = new List<BTN_ServerSearchItem>();

        private WaitForSeconds _wait = new WaitForSeconds(0.1f);
        private bool _isSearch = false;
        private int _searchWaitTime = 0;

        public void ActiveSettingArea(Func<List<string>> searchMic, UnityAction<int> searchServer, UnityAction serverSearchTimeOutEvent, UnityAction<string, string> endSettingCallback )
        {
            //세팅이 끝나면 콜백
            _endSettingCallback.RemoveAllListeners();
            _endSettingCallback.AddListener((micName, ipAddress) => {
                if (_isSearch)
                {
                    _isSearch = false;
                    serverSearchTimeOutEvent?.Invoke();
                }
                endSettingCallback(micName, ipAddress);
            });

            //마이크 서치 버튼 클릭 ==>> 마이크 서치
            _micSearchBtn.onClick.AddListener(() => { SearchMic(searchMic); });

            //서버 서치버튼 클릭 ==> 서버 서치
            _btnServerSearch.onClick.AddListener(() => { SearchServer(searchServer, serverSearchTimeOutEvent); });

            _settingArea.gameObject.SetActive(true);

            //처음 세팅용으로 한번 플레이
            SearchMic(searchMic);
        }

        public void UnactiveSettingArea()
        {
            _micSearchBtn.onClick.RemoveAllListeners();
        }

        #region 마이크 설정
        /// <summary>
        /// 마이크 서치
        /// </summary>
        public void SearchMic(Func<List<string>> searchMicMethod)
        {
            _micDropDown.ClearOptions();
            List<string> searchRslt = searchMicMethod();
            searchRslt.Add(MicrophoneCapture.MicName_NONE);
            _micDropDown.AddOptions(searchRslt);
        }
        #endregion

        #region 서버 서치
        /// <summary>
        /// 서버 서치
        /// </summary>
        public void SearchServer(UnityAction<int> searchServer, UnityAction TimeOutSearch)
        {
            if (_isSearch)
                return;

            //리스트 Clear
            ClearSeachedItem();

            if (_inputPort.text.Equals(string.Empty))
            {
                Debug.Log("VoiceChat :: 포트번호를 입력해주세요");
                return;
            }

            int portNo = 0;
            if (!int.TryParse(_inputPort.text, out portNo))
            {
                Debug.Log("VoiceChat :: 포트번호가 옳지않습니다");
                return;
            }
            
            _isSearch = true;

            searchServer?.Invoke(portNo);
            StartCoroutine(ISearchServer(TimeOutSearch));
        }

        private IEnumerator ISearchServer(UnityAction TimeOutSearch)
        {
            _txtSearchTime.gameObject.SetActive(true);

            while (_isSearch)
            {
                yield return _wait;
                _searchWaitTime++;

                _txtSearchTime.text = string.Format("{0}초남음", (100 - _searchWaitTime) / 10);
                if (_searchWaitTime > 100)
                    break;
            }

            _isSearch = false;
            TimeOutSearch?.Invoke();
        }

        /// <summary>
        /// 서버에서 쏘는 브로드캐스팅을 받음 -> 서버 목록에 추가
        /// </summary>
        public void OnSearchedServer(ServerDetected serverDetected)
        {
            if (CheckHasServerItem(serverDetected.ServerIP))
                return;

            var serverItem = Instantiate(_serverButtonItem, _serverItemTransform).GetComponent<BTN_ServerSearchItem>();
            serverItem.SetButtonSetting(serverDetected, (ipAddress)=> {
                _endSettingCallback.Invoke(_micDropDown.options[_micDropDown.value].text, ipAddress);
            });
            serverItem.gameObject.SetActive(true);
            _serverSearchBtnItem.Add(serverItem);
        }

        /// <summary>
        /// 이미 존재하는 서버인지 확인
        /// </summary>
        private bool CheckHasServerItem(string serverName)
        {
            for(int index = 0; index < _serverSearchBtnItem.Count; index++)
                if (_serverSearchBtnItem[index].GetServerName.Equals(serverName))
                    return true;
            return false;
        }

        /// <summary>
        /// 서치된 아이템들 Clear
        /// </summary>
        private void ClearSeachedItem()
        {
            foreach(var serchedItem in _serverSearchBtnItem)
            {
                _serverSearchBtnItem.Remove(serchedItem);
                Destroy(serchedItem.gameObject);
            }
            
        }
        #endregion
    }

}
