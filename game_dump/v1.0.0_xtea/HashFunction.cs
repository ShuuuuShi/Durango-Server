public abstract class HashFunction
{
	public abstract uint GetHash(params int[] data);

	public virtual uint GetHash(int data)
	{
		return GetHash(new int[1] { data });
	}

	public virtual uint GetHash(int x, int y)
	{
		return GetHash(new int[2] { x, y });
	}

	public virtual uint GetHash(int x, int y, int z)
	{
		return GetHash(new int[3] { x, y, z });
	}

	public float Value(params int[] data)
	{
		return (float)GetHash(data) / 4.2949673E+09f;
	}

	public float Value(int data)
	{
		return (float)GetHash(data) / 4.2949673E+09f;
	}

	public float Value(int x, int y)
	{
		return (float)GetHash(x, y) / 4.2949673E+09f;
	}

	public float Value(int x, int y, int z)
	{
		return (float)GetHash(x, y, z) / 4.2949673E+09f;
	}

	public int Range(int min, int max, params int[] data)
	{
		return min + (int)(GetHash(data) % (max - min));
	}

	public int Range(int min, int max, int data)
	{
		return min + (int)(GetHash(data) % (max - min));
	}

	public int Range(int min, int max, int x, int y)
	{
		return min + (int)(GetHash(x, y) % (max - min));
	}

	public int Range(int min, int max, int x, int y, int z)
	{
		return min + (int)(GetHash(x, y, z) % (max - min));
	}

	public float Range(float min, float max, params int[] data)
	{
		return min + (float)GetHash(data) * (max - min) / 4.2949673E+09f;
	}

	public float Range(float min, float max, int data)
	{
		return min + (float)GetHash(data) * (max - min) / 4.2949673E+09f;
	}

	public float Range(float min, float max, int x, int y)
	{
		return min + (float)GetHash(x, y) * (max - min) / 4.2949673E+09f;
	}

	public float Range(float min, float max, int x, int y, int z)
	{
		return min + (float)GetHash(x, y, z) * (max - min) / 4.2949673E+09f;
	}
}
