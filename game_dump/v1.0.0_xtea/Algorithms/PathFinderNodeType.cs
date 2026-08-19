namespace Algorithms;

[Author("Franco, Gustavo")]
public enum PathFinderNodeType
{
	Start = 1,
	End = 2,
	Open = 4,
	Close = 8,
	Current = 0x10,
	Path = 0x20
}
