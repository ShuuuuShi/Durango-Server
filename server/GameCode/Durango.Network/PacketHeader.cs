namespace Durango.Network;

public struct PacketHeader
{
	public byte Size;

	public double Time;

	public uint Seq;

	public uint ReplyOf;

	public uint TypeCode;

	public int PayloadSize;
}
