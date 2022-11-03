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

    public ArraySegment<byte> GetWriteSegment()
    {
        return new ArraySegment<byte>(_buffer.Array, 0, _buffer.Count);
    }

    public ArraySegment<byte> GetReadSegment(byte[] rsltArray)
    {
        return new ArraySegment<byte>(rsltArray);
    }
}
