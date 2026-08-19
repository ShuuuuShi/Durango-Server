public interface IBitArray2d
{
	int Width { get; }

	int Height { get; }

	bool Get(int x, int y);

	void CopyTo(BitArray2d target);
}
