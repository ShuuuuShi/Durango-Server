using MsgPack;

public delegate void PackFunc<T>(Packer packer, T msg, bool hint);
