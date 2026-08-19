public struct WallJointMaterial
{
	public readonly string Model;

	public readonly string Pattern;

	public WallJointMaterial(string model)
	{
		Model = model;
		Pattern = null;
	}

	public WallJointMaterial(string model, string pattern)
	{
		Model = model;
		Pattern = pattern;
	}

	public bool IsEmpty()
	{
		return string.IsNullOrEmpty(Model);
	}

	public override int GetHashCode()
	{
		return ((Model != null) ? Model.GetHashCode() : 0) ^ ((Pattern != null) ? Pattern.GetHashCode() : 0);
	}
}
