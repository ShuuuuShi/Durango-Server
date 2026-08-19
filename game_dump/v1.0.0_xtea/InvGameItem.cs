using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InvGameItem
{
	public enum Quality
	{
		Broken,
		Cursed,
		Damaged,
		Worn,
		Sturdy,
		Polished,
		Improved,
		Crafted,
		Superior,
		Enchanted,
		Epic,
		Legendary,
		_LastDoNotUse
	}

	[SerializeField]
	private int mBaseItemID;

	public Quality quality = Quality.Sturdy;

	public int itemLevel = 1;

	private InvBaseItem mBaseItem;

	public int baseItemID => mBaseItemID;

	public InvBaseItem baseItem
	{
		get
		{
			if (mBaseItem == null)
			{
				mBaseItem = InvDatabase.FindByID(baseItemID);
			}
			return mBaseItem;
		}
	}

	public string name
	{
		get
		{
			if (baseItem == null)
			{
				return null;
			}
			return quality.ToString() + " " + baseItem.name;
		}
	}

	public float statMultiplier
	{
		get
		{
			float num = 0f;
			switch (quality)
			{
			case Quality.Cursed:
				num = -1f;
				break;
			case Quality.Broken:
				num = 0f;
				break;
			case Quality.Damaged:
				num = 0.25f;
				break;
			case Quality.Worn:
				num = 0.9f;
				break;
			case Quality.Sturdy:
				num = 1f;
				break;
			case Quality.Polished:
				num = 1.1f;
				break;
			case Quality.Improved:
				num = 1.25f;
				break;
			case Quality.Crafted:
				num = 1.5f;
				break;
			case Quality.Superior:
				num = 1.75f;
				break;
			case Quality.Enchanted:
				num = 2f;
				break;
			case Quality.Epic:
				num = 2.5f;
				break;
			case Quality.Legendary:
				num = 3f;
				break;
			}
			float num2 = (float)itemLevel / 50f;
			return num * Mathf.Lerp(num2, num2 * num2, 0.5f);
		}
	}

	public Color color
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			Color result = Color.white;
			switch (quality)
			{
			case Quality.Cursed:
				result = Color.red;
				break;
			case Quality.Broken:
				((Color)(ref result))._002Ector(0.4f, 0.2f, 0.2f);
				break;
			case Quality.Damaged:
				((Color)(ref result))._002Ector(0.4f, 0.4f, 0.4f);
				break;
			case Quality.Worn:
				((Color)(ref result))._002Ector(0.7f, 0.7f, 0.7f);
				break;
			case Quality.Sturdy:
				((Color)(ref result))._002Ector(1f, 1f, 1f);
				break;
			case Quality.Polished:
				result = NGUIMath.HexToColor(3774856959u);
				break;
			case Quality.Improved:
				result = NGUIMath.HexToColor(2480359935u);
				break;
			case Quality.Crafted:
				result = NGUIMath.HexToColor(1325334783u);
				break;
			case Quality.Superior:
				result = NGUIMath.HexToColor(12255231u);
				break;
			case Quality.Enchanted:
				result = NGUIMath.HexToColor(1937178111u);
				break;
			case Quality.Epic:
				result = NGUIMath.HexToColor(2516647935u);
				break;
			case Quality.Legendary:
				result = NGUIMath.HexToColor(4287627519u);
				break;
			}
			return result;
		}
	}

	public InvGameItem(int id)
	{
		mBaseItemID = id;
	}

	public InvGameItem(int id, InvBaseItem bi)
	{
		mBaseItemID = id;
		mBaseItem = bi;
	}

	public List<InvStat> CalculateStats()
	{
		List<InvStat> list = new List<InvStat>();
		if (baseItem != null)
		{
			float num = statMultiplier;
			List<InvStat> stats = baseItem.stats;
			int i = 0;
			for (int count = stats.Count; i < count; i++)
			{
				InvStat invStat = stats[i];
				int num2 = Mathf.RoundToInt(num * (float)invStat.amount);
				if (num2 == 0)
				{
					continue;
				}
				bool flag = false;
				int j = 0;
				for (int count2 = list.Count; j < count2; j++)
				{
					InvStat invStat2 = list[j];
					if (invStat2.id == invStat.id && invStat2.modifier == invStat.modifier)
					{
						invStat2.amount += num2;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					InvStat invStat3 = new InvStat();
					invStat3.id = invStat.id;
					invStat3.amount = num2;
					invStat3.modifier = invStat.modifier;
					list.Add(invStat3);
				}
			}
			list.Sort(InvStat.CompareArmor);
		}
		return list;
	}
}
