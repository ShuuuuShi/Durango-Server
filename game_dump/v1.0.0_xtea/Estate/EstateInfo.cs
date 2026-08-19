using System.Collections.Generic;
using Shared.Estate;
using UnityEngine;

namespace Estate;

public class EstateInfo
{
	public enum StateEnum
	{
		None,
		Loading,
		Loaded
	}

	public const int EstateSize = 4;

	public const int ClanEstateSize = 8;

	public const ulong RestrictedAreaId = 1uL;

	private static readonly Point2[] DirectionVector = new Point2[4]
	{
		new Point2(1, 0),
		new Point2(0, 1),
		new Point2(-1, 0),
		new Point2(0, -1)
	};

	public ulong Id;

	public ulong Owner;

	public OwnerType OwnerType;

	public double Since;

	public double Until;

	public int ExtendCost;

	public EstateLicense License;

	public EstateOnWarWith OnWarWith;

	public StateEnum State;

	public List<Point2> Units;

	private bool _needUpdateSide;

	private bool _needUpdateFence;

	private ModelComponent _estateFences;

	private ModelComponent _estateLines;

	private bool _isShowEstateLines;

	public Rect Bound { get; private set; }

	public bool RestrictedArea => Id == 1;

	public EstateInfo(ulong id)
	{
		Id = id;
		Units = new List<Point2>();
		_estateFences = new ModelComponent(((Component)KSingleton<StaticObjectManager>.Instance()).gameObject);
		_estateLines = new ModelComponent(((Component)KSingleton<StaticObjectManager>.Instance()).gameObject);
	}

	public void Set(EstateJson json)
	{
		Owner = json.owner_id;
		OwnerType = json.owner_type;
		Since = json.valid_since;
		Until = json.valid_until;
		ExtendCost = json.extend_cost;
		License = json.license;
		OnWarWith = json.on_war_with;
	}

	public void AddUnit(Point2 unit)
	{
		if (!Units.Contains(unit))
		{
			Units.Add(unit);
			SetDirtyUnits();
		}
	}

	public void RemoveUnit(Point2 unit)
	{
		Units.Remove(unit);
		SetDirtyUnits();
	}

	private void SetDirtyUnits()
	{
		_needUpdateSide = true;
		_needUpdateFence = true;
	}

	private void UpdateBound()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		if (!_needUpdateSide)
		{
			return;
		}
		_needUpdateSide = false;
		if (RestrictedArea)
		{
			Bound = default(Rect);
			return;
		}
		int num = int.MaxValue;
		int num2 = int.MinValue;
		int num3 = int.MaxValue;
		int num4 = int.MinValue;
		int i = 0;
		for (int count = Units.Count; i < count; i++)
		{
			Point2 point = Units[i];
			num = Mathf.Min(point.x, num);
			num2 = Mathf.Max(point.x, num2);
			num3 = Mathf.Min(point.y, num3);
			num4 = Mathf.Max(point.y, num4);
		}
		Bound = new Rect(new Vector2((float)num, (float)num3) * 4f, new Vector2((float)(num4 - num3 + 1), (float)(num2 - num + 1)) * 4f);
	}

	public bool IsValid()
	{
		return Owner != 0 && Since > 0.0 && Since <= Connections.Frontend.GetPredictedServerTime();
	}

	public bool OnWar()
	{
		return OnWarWith.EnemyClanId != 0L && OnWarWith.Until > Connections.Frontend.GetPredictedServerTime();
	}

	public void RefreshEstateFences()
	{
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		UpdateBound();
		if (!_needUpdateFence || State != StateEnum.Loaded)
		{
			return;
		}
		_needUpdateFence = false;
		bool valid = IsValid();
		bool clanFlag = OwnerType == OwnerType.ClanCapture || OwnerType == OwnerType.ClanEstate;
		bool[] array = new bool[DirectionVector.Length];
		_estateFences.BeginLoad();
		int i = 0;
		for (int count = Units.Count; i < count; i++)
		{
			Point2 point = Units[i];
			for (int j = 0; j < DirectionVector.Length; j++)
			{
				array[j] = Units.Contains(point + DirectionVector[j]);
			}
			for (int k = 0; k < DirectionVector.Length; k++)
			{
				if ((array[2] && (k == 2 || k == 3)) || (array[3] && (k == 0 || k == 3)))
				{
					continue;
				}
				int num = k;
				int num2 = (num + DirectionVector.Length - 1) % DirectionVector.Length;
				Vector2 val = (DirectionVector[num] + DirectionVector[num2]).ToVector2() * 0.5f;
				int index = 0;
				float num3 = (float)(num + 1) * -90f;
				if (array[num] && array[num2])
				{
					if (Units.Contains(point + DirectionVector[num] + DirectionVector[num2]))
					{
						continue;
					}
					num3 += 180f;
				}
				else if (array[num])
				{
					if (Units.Contains(point + DirectionVector[num] + DirectionVector[num2]))
					{
						num3 -= 90f;
					}
					else
					{
						index = 1;
					}
				}
				else if (array[num2])
				{
					if (Units.Contains(point + DirectionVector[num] + DirectionVector[num2]))
					{
						num3 += 90f;
					}
					else
					{
						index = 1;
						num3 += 90f;
					}
				}
				string key = $"{point.x}_{point.y}:{num}";
				string esateFencePath = KSingleton<StaticObjectManager>.Instance().GetEsateFencePath(index, valid, clanFlag);
				Vector2 tilePosition = point.ToVector2() * 4f + Vector2.one * 4f * 0.5f;
				Vector3 val2 = TerrainA6.TilePositionToClientPosition(tilePosition);
				val2 += (Vector3.right * val.x + Vector3.forward * val.y) * 800f;
				val2.y = 0.1f;
				_estateFences.PathLoad(key, esateFencePath).SetPosition(val2).SetAngle(Vector3.up * num3);
			}
		}
		_estateFences.EndLoad();
		if (_isShowEstateLines)
		{
			ShowEstateLines();
		}
	}

	public void ShowEstateLines()
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		_isShowEstateLines = true;
		bool valid = IsValid() && Owner == GameManager.PlayerId;
		_estateLines.BeginLoad();
		bool clanFlag = OwnerType == OwnerType.ClanCapture || OwnerType == OwnerType.ClanEstate;
		int num = 800;
		int i = 0;
		for (int count = Units.Count; i < count; i++)
		{
			Point2 point = Units[i];
			for (int j = 0; j < DirectionVector.Length; j++)
			{
				if (!Units.Contains(point + DirectionVector[j]))
				{
					int num2 = j;
					int num3 = (num2 + DirectionVector.Length - 1) % DirectionVector.Length;
					Vector2 val = (DirectionVector[num2] + DirectionVector[num3]).ToVector2() * 0.5f;
					string key = $"{point.x}_{point.y}:{j}";
					string estateLinePath = KSingleton<StaticObjectManager>.Instance().GetEstateLinePath(valid, clanFlag);
					Vector2 tilePosition = point.ToVector2() * 4f + Vector2.one * 4f * 0.5f;
					Vector3 val2 = TerrainA6.TilePositionToClientPosition(tilePosition);
					val2 += (Vector3.right * val.x + Vector3.forward * val.y) * (float)num;
					val2.y = 0.1f;
					float num4 = (float)j * -90f;
					_estateLines.PathLoad(key, estateLinePath).SetPosition(val2).SetAngle(Vector3.up * num4);
				}
			}
		}
		_estateLines.EndLoad();
	}

	public void HideEstateLines()
	{
		_isShowEstateLines = false;
		_estateLines.Clear();
	}

	public void Dispose()
	{
		_estateFences.Clear();
		_estateLines.Clear();
	}
}
