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
    private int _lastPosition = 0;
    private int _writedPosition = 0;

    public bool IsAbleToReceive(int receiveSize)
    {
        if (receiveSize > _buffer.Count - _writedPosition)
            return false;
        return true;
    }
    public ArraySegment<byte> GetReadSegment(int receiveSize) 
    {
        _writedPosition += receiveSize;
        return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _lastPosition, _writedPosition - _lastPosition);
    }
    public ArraySegment<byte> GetWriteSegment()
    {
        return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _lastPosition, _buffer.Count - _writedPosition);
    }
    public void Clear()
    {
        if(_writedPosition - _lastPosition == 0)
        {
            _writedPosition = 0;
            _lastPosition = 0;
        }
        else
        {
            _buffer = new ArraySegment<byte>(_buffer.Array, _lastPosition, _buffer.Count - _lastPosition);
            _lastPosition = 0;
            _writedPosition = _writedPosition - _lastPosition;
        }
    }
}
