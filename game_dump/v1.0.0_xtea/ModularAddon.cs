using ItemSystem;

public class ModularAddon
{
	public int Index;

	public string ModelKey;

	public string Type;

	public ItemData Item;

	public string GetWallPostfix()
	{
		return Type switch
		{
			"door" => "door", 
			"window" => "window", 
			_ => null, 
		};
	}
}
