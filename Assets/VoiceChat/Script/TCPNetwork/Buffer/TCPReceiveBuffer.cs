using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TCPReceiveBuffer
{
    public TCPReceiveBuffer(int bufferSize)
    {
        _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
    }
    private ArraySegment<byte> _buffer;
    private int _packetSize = 0;
    private int _remainSize = 0;

    public bool SetBuffer(ArraySegment<byte> receiveSegment)
    {
        //패킷사이즈 정의
        if (_packetSize == 0)
            if(_remainSize == 0)
                GetPacketSize(receiveSegment.Array);
            else
                GetPacketSize(_buffer.Array);

        var totalDataLength = _remainSize + receiveSegment.Count;

        if (_buffer.Count < totalDataLength)
        {
            var buffer = new ArraySegment<byte>(new byte[totalDataLength], 0, totalDataLength);
            Buffer.BlockCopy(_buffer.Array, 0, buffer.Array, 0, _remainSize);
            Buffer.BlockCopy(receiveSegment.Array, 0, buffer.Array, _remainSize, receiveSegment.Count);
            _buffer = buffer;
        }
        else
        {
            Buffer.BlockCopy(receiveSegment.Array, 0, _buffer.Array, _remainSize, receiveSegment.Count);
        }

        _remainSize = totalDataLength;

        if (_packetSize <= _remainSize)
            return true;
        return false;
    }

    public ArraySegment<byte> GetBuffer()
    {
        if (_packetSize == 0)
            return null;
        //결과값 복제
        var result = new ArraySegment<byte>(new byte[_packetSize], 0, _packetSize);
        Buffer.BlockCopy(_buffer.Array, 0, result.Array, 0, _packetSize);

        //남은 값 Remain에 넣기
        var remainBuffer = new ArraySegment<byte>(new byte[_buffer.Count - _packetSize], 0, _buffer.Count - _packetSize);
        Buffer.BlockCopy(_buffer.Array, _packetSize, remainBuffer.Array, 0, _buffer.Count - _packetSize);
        _buffer = remainBuffer;

        _remainSize -= _packetSize;

        _packetSize = 0;

        return result;
    }
    private void GetPacketSize(byte[] receiveByte)
    {
        _packetSize = BitConverter.ToInt32(receiveByte, 0);
        Debug.Log("_packetSize : " + _packetSize);
    }
}
