using System.Collections.Generic;
using System.IO;
using MsgPack;

public static class LocalizeSystem
{
    public static string UnpackGettextFromMsgPack(Unpacker unpacker)
    {
        if (unpacker.LastReadData.IsNil)
        {
            return null;
        }
        if (unpacker.IsMapHeader)
        {
            unpacker.ReadString(out var result);
            unpacker.Read();
            if (unpacker.LastReadData.IsNil)
            {
                return result;
            }
            int num = unpacker.LastReadData.AsInt32();
            for (int i = 0; i < num; i++)
            {
                unpacker.ReadString(out var _);
                unpacker.Read();
            }
            return result;
        }
        if (unpacker.LastReadData.UnderlyingType == typeof(string))
        {
            return unpacker.LastReadData.AsString();
        }
        return null;
    }

    public static object UnpackGettextArgumentFromMsgPack(Unpacker unpacker)
    {
        if (unpacker.IsMapHeader)
        {
            return UnpackGettextFromMsgPack(unpacker);
        }
        if (unpacker.IsArrayHeader)
        {
            int num = unpacker.LastReadData.AsInt32();
            object[] array = new object[num];
            for (int i = 0; i < num; i++)
            {
                unpacker.Read();
                array[i] = UnpackGettextArgumentFromMsgPack(unpacker);
            }
            return array;
        }
        return unpacker.LastReadData.ToObject();
    }
}
