using System.Collections.Generic;
using Durango.Network;
using Durango.Terrain;
using Durango.Utils;
using Messages;
using Shared.Estate;
using UnityEngine;

namespace Durango.Logic.Estate;

public class EstateInfo
{
	public const int EstateSize = 4;

	private static readonly Point2[] DirectionVector = new Point2[4]
	{
		new Point2(1, 0),
		new Point2(0, 1),
		new Point2(-1, 0),
		new Point2(0, -1)
	};

	public readonly string Id;

	public readonly List<Point2> Units;

	private readonly ModelComponent _estateFences;

	private readonly ModelComponent _estateLines;

	private bool _isShowEstateLines;

	private Material _estateLineMaterial;

	private double _estateLinesUpdateAt;

	private bool _isDirtyUnits;

	public EstateLicense License { get; private set; }

	public EstateInfo(string id)
	{
		Id = id;
		Units = new List<Point2>();
		_estateFences = new ModelComponent(Singleton<ArtifactManager>.Instance().gameObject);
		_estateLines = new ModelComponent(Singleton<ArtifactManager>.Instance().gameObject);
		_estateLines.ModelLoaded += OnLoadEstateLine;
	}

	public void Set(EstateLicense license)
	{
		License = license;
	}

	public void AddUnit(Point2 unit)
	{
		if (!Units.Contains(unit))
		{
			Units.Add(unit);
			SetDirtyUnits();
		}
	}

	public void RemoveAt(int index)
	{
		Units.RemoveAt(index);
		SetDirtyUnits();
	}

	private void SetDirtyUnits()
	{
		_isDirtyUnits = true;
	}

	private bool HasUnit(Point2 unit)
	{
		int i = 0;
		for (int size = KUtility.GetSize(Units); i < size; i++)
		{
			if (Units[i] == unit)
			{
				return true;
			}
		}
		return false;
	}

	public void RefreshEstateArea(bool visibleEstateLines)
	{
		if (_isDirtyUnits && License.Type != OwnerType.System)
		{
			_estateFences.BeginLoad();
			int i = 0;
			for (int size = KUtility.GetSize(Units); i < size; i++)
			{
				Point2 point = Units[i];
				for (int j = 0; j < DirectionVector.Length; j++)
				{
					if (!HasUnit(point + DirectionVector[j]))
					{
						int num = j;
						int num2 = (num + DirectionVector.Length - 1) % DirectionVector.Length;
						Vector2 vector = (DirectionVector[num] + DirectionVector[num2]).ToVector2() * 0.5f;
						string key = $"{point.x}_{point.y}:{j}";
						string esateFencePath = Singleton<ArtifactManager>.Instance().GetEsateFencePath();
						Vector3 position = Durango.Terrain.Util.TilePositionToClientPosition(point.ToVector2() * 4f + Vector2.one * 4f * 0.5f);
						position += (Vector3.right * vector.x + Vector3.forward * vector.y) * 800f;
						float num3 = (float)j * -90f;
						_estateFences.PathLoad(key, esateFencePath).SetPosition(position).SetAngle(Vector3.up * num3);
					}
				}
			}
			_estateFences.EndLoad();
		}
		if (visibleEstateLines && _isShowEstateLines)
		{
			ShowEstateLines();
		}
		_isDirtyUnits = false;
	}

	public void ShowEstateLines()
	{
		_estateLinesUpdateAt = Connections.Frontend.GetPredictedServerTime();
		if (_isDirtyUnits || !_isShowEstateLines)
		{
			_estateLines.BeginLoad();
			int i = 0;
			for (int size = KUtility.GetSize(Units); i < size; i++)
			{
				Point2 point = Units[i];
				for (int j = 0; j < DirectionVector.Length; j++)
				{
					if (!HasUnit(point + DirectionVector[j]))
					{
						int num = j;
						int num2 = (num + DirectionVector.Length - 1) % DirectionVector.Length;
						Vector2 vector = (DirectionVector[num] + DirectionVector[num2]).ToVector2() * 0.5f;
						string key = $"{point.x}_{point.y}:{j}";
						string estateLinePath = Singleton<ArtifactManager>.Instance().GetEstateLinePath();
						Vector3 position = Durango.Terrain.Util.TilePositionToClientPosition(point.ToVector2() * 4f + Vector2.one * 4f * 0.5f);
						position += (Vector3.right * vector.x + Vector3.forward * vector.y) * 800f;
						position.y = 0.1f;
						float num3 = (float)j * -90f;
						_estateLines.PathLoad(key, estateLinePath).SetPosition(position).SetAngle(Vector3.up * num3);
					}
				}
			}
			_estateLines.EndLoad();
		}
		if (_estateLineMaterial != null)
		{
			_estateLineMaterial.color = GetEstateLineColor();
		}
		_isShowEstateLines = true;
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
		Object.Destroy(_estateLineMaterial);
	}

	public bool IsLocalPlayers()
	{
		switch (License.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			return License.OwnerId == GameManager.PlayerId;
		case OwnerType.ClanEstate:
		case OwnerType.ClanWarphole:
			return ClanSystem.IsMyClan(License.OwnerId);
		default:
			return false;
		}
	}

	private void OnLoadEstateLine(ModelComponent.IModel model)
	{
		GameObject @object = model.GetObject();
		if (@object == null)
		{
			return;
		}
		Renderer component = @object.GetComponent<Renderer>();
		if (!(component == null))
		{
			if (_estateLineMaterial == null)
			{
				_estateLineMaterial = component.material;
				_estateLineMaterial.color = GetEstateLineColor();
			}
			else
			{
				component.sharedMaterial = _estateLineMaterial;
			}
		}
	}

	private Color GetEstateLineColor()
	{
		Color result = Color.white;
		bool flag = IsLocalPlayers();
		switch (License.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			result = ((!flag) ? new Color32(byte.MaxValue, 131, 0, byte.MaxValue) : new Color32(0, 139, 196, byte.MaxValue));
			break;
		case OwnerType.ClanEstate:
			result = ((!flag) ? new Color32(byte.MaxValue, 131, 0, byte.MaxValue) : new Color32(53, 189, 0, byte.MaxValue));
			break;
		case OwnerType.ClanWarphole:
			if (License.ProtectedUntil.HasValue)
			{
				result = ((!(_estateLinesUpdateAt <= License.ProtectedUntil.Value)) ? ((Color)new Color32(byte.MaxValue, 0, 0, byte.MaxValue)) : ((Color)((!flag) ? new Color32(byte.MaxValue, 131, 0, byte.MaxValue) : new Color32(53, 189, 0, byte.MaxValue))));
			}
			break;
		}
		return result;
	}
}
