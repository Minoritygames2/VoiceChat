using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoiceChat
{
    public class TCPSendBuffer
    {
        public TCPSendBuffer(int bufferSize)
        {
            _buffer = new byte[bufferSize];
        }
        private byte[] _buffer;
        private int _writePosition = 0;

        public bool IsAbleWriteBuffer(int bufferSize)
        {
            return (bufferSize > GetRemainSize());
        }

        public ArraySegment<byte> OpenBuffer()
        {
            return new ArraySegment<byte>(_buffer, 0, _buffer.Length);
        }

        public ArraySegment<byte> CloseBuffer(int bufferSize)
        {
            var returnBuffer = new byte[bufferSize];
            Buffer.BlockCopy(_buffer, _writePosition, returnBuffer, 0, bufferSize);
            var arraysegment = new ArraySegment<byte>(returnBuffer, 0, bufferSize);
            _writePosition += bufferSize;
            return arraysegment;
        }
        /// <summary>
        /// 남은 버퍼 사이즈
        /// </summary>
        private int GetRemainSize()
        {
            return _buffer.Length - _writePosition;
        }
    }

}
