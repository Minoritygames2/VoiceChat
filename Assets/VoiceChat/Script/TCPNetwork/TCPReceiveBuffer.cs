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

    public ArraySegment<byte> GetReadSegment(byte[] sourceArray, int bufferlength)
    {
        var rsltByte = new byte[bufferlength];
        Buffer.BlockCopy(sourceArray, 0, rsltByte, 0, bufferlength);
        _buffer = new ArraySegment<byte>(rsltByte);
        return _buffer;
    }
}
