using UnityEngine;

[AddComponentMenu("NGUI/Examples/UI Storage Slot")]
public class UIStorageSlot : UIItemSlot
{
	public UIItemStorage storage;

	public int slot;

	protected override InvGameItem observedItem => (!((Object)(object)storage != (Object)null)) ? null : storage.GetItem(slot);

	protected override InvGameItem Replace(InvGameItem item)
	{
		return (!((Object)(object)storage != (Object)null)) ? item : storage.Replace(slot, item);
	}
}
