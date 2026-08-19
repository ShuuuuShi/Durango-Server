namespace K1Network;

public struct PacketHeader
{
	public byte Size;

	public double Time;

	public ulong Seq;

	public ulong ReplyOf;

	public uint TypeCode;

	public int PayloadSize;
}
