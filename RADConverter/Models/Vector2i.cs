using System;

public class Vector2i
{
    public int X;
    public int Y;

    public void PopulateWithData(byte[] data, ref int offset)
    {
        X = BitConverter.ToInt32(data, offset);
        offset += 4;
        Y = BitConverter.ToInt32(data, offset);
        offset += 4;
    }
}