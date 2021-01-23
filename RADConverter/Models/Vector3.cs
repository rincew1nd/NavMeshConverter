using System;

public class Vector3
{
    public float X;
    public float Y;
    public float Z;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        X = BitConverter.ToSingle(data, offset);
        offset += 4;
        Y = BitConverter.ToSingle(data, offset);
        offset += 4;
        Z = BitConverter.ToSingle(data, offset);
        offset += 4;
    }

    public SharpNav.Geometry.Vector3 ToSn()
    {
        return new SharpNav.Geometry.Vector3(X, Y, Z);
    }
}