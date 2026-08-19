public struct MusicId
{
	public int? Slot;

	public string SharedId;

	public static implicit operator MusicId(int value)
	{
		MusicId result = default(MusicId);
		result.Slot = value;
		return result;
	}

	public static implicit operator MusicId(string value)
	{
		MusicId result = default(MusicId);
		result.SharedId = value;
		return result;
	}

	public bool IsEqual(MusicId target)
	{
		if (Slot.HasValue)
		{
			return target.Slot.HasValue && Slot.Value == target.Slot.Value;
		}
		if (!string.IsNullOrEmpty(SharedId))
		{
			return SharedId == target.SharedId;
		}
		return false;
	}
}
