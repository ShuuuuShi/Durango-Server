public struct Pair<T1, T2>
{
	private readonly T1 _item1;

	private readonly T2 _item2;

	public T1 Item1 => _item1;

	public T2 Item2 => _item2;

	public Pair(T1 item1, T2 item2)
	{
		_item1 = item1;
		_item2 = item2;
	}

	public override string ToString()
	{
		return $"Pair({Item1}, {Item2})";
	}
}
