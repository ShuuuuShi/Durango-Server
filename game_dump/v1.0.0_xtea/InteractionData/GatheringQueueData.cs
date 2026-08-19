namespace InteractionData;

public struct GatheringQueueData
{
	public static int idCounter;

	private int id;

	private GatheringData data;

	public int ID => id;

	public GatheringData Data => data;

	public GatheringQueueData(GatheringData data)
	{
		id = idCounter++;
		this.data = data;
	}
}
