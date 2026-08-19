using Messages;

namespace ItemSystem;

public class Reins
{
	public int Capacity;

	public ItemData[] Contents;

	public int VehicleEntityType;

	public string PetName;

	public int Size;

	public Gauge Hungry;

	public string[] EatableTags;

	public int ItemSize;

	public Reins(Messages.Reins msg)
	{
		Capacity = msg.Capacity;
		ItemSize = 0;
		if (msg.Contents != null)
		{
			Contents = new ItemData[msg.Contents.Length];
			for (int i = 0; i < Contents.Length; i++)
			{
				Contents[i] = new ItemData(msg.Contents[i]);
				ItemSize += Contents[i].Size;
			}
		}
		VehicleEntityType = msg.VehicleEntityType;
		PetName = msg.PetName;
		Size = msg.Size;
		Hungry = msg.Hungry;
		EatableTags = msg.EatableTags;
	}
}
