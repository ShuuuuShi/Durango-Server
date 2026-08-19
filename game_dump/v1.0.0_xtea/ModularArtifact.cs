using System.Collections;
using System.Collections.Generic;
using Messages;
using Shared.Building;
using Shared.Etc;
using UnityEngine;

public class ModularArtifact : ArtifactComponent
{
	public enum VisibleStateEnum
	{
		Normal,
		PlayerEnter,
		Obstructions
	}

	private struct AlphaTweenStruct
	{
		public ModelComponent Component;

		public float Alpha;

		public float Speed;
	}

	public const string Wall = "Wall";

	public const string Pillar = "Pillar";

	public const string Roof = "Roof";

	public const string Tile = "Tile";

	private BoxCollider _builtCollider;

	private Artifact[] _interiors;

	private readonly ModularAddons _addons = new ModularAddons();

	private readonly List<ModularArtifact> _viewObstructions = new List<ModularArtifact>();

	private readonly List<AlphaTweenStruct> _alphaTweenList = new List<AlphaTweenStruct>();

	private bool _isPlayAlphaTweenRoutine;

	public VisibleStateEnum VisibleState { get; private set; }

	public override int Height => 2;

	protected override string ConsiteAssetPath => "Models/Prop/module/site/site.prefab";

	protected override string ScaffoldingAssetPath => "Models/Prop/system/scaffolding/scaffolding_01_1x1_module.prefab";

	public string WallModel { get; private set; }

	public string RoofModel { get; private set; }

	public string TileModel { get; private set; }

	public string PillarModel { get; private set; }

	public bool HasWall => !string.IsNullOrEmpty(WallModel);

	public override Vector3 InteractionPositionOffset => Vector3.up * 100f;

	public override void PostInit(string artifactId, int worldTileX, int worldTileY, Rotation rotation, Point2 size)
	{
		_interiors = new Artifact[base.Size.x * base.Size.y];
	}

	public override void OnUpdateCollider()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3((float)base.Size.x, 0f, (float)base.Size.y) * 200f;
		val.y = ((!string.IsNullOrEmpty(RoofModel)) ? (Height * 200) : 0);
		Vector3 center = val * 0.5f;
		val.x += 100f;
		val.z += 100f;
		base.Artifact.CreateCollider(val, center);
	}

	public override bool OnUpdateDisplay(ArtifactDisplay msg)
	{
		UpdateTiles(msg.Parts.Get("tile"));
		UpdateRoof(msg.Parts.Get("roof"));
		UpdatePillars(msg.Parts.Get("pillar"), knockNeighborhood: false);
		_addons.Set(msg.AddOns);
		UpdateWalls(msg.Parts.Get("wall"), _addons);
		OnUpdateCollider();
		return true;
	}

	public ModularAddons GetAddons()
	{
		return _addons;
	}

	private void UpdateRoof(string roofModel)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		ModelComponent category = base.Artifact.Models.GetCategory("Roof");
		Point2 size = base.Artifact.Size;
		RoofModel = roofModel;
		if (string.IsNullOrEmpty(roofModel))
		{
			category.Clear();
			return;
		}
		category.BeginLoad();
		int num = Mathf.Min(size.x, size.y);
		int num2 = Mathf.Max(size.x, size.y);
		Direction direction = ((num != size.y) ? Direction.SouthEast : Direction.SouthWest);
		Vector2 directionPivot = KUtility.GetDirectionPivot(direction);
		Vector3 val = KUtility.DirectionToAngle(direction);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector((float)num * (1f - directionPivot.y), 0f, (float)num * (1f - directionPivot.x));
		val2 *= 0.5f;
		Vector3 val3 = new Vector3((float)size.x, 0f, (float)size.y) - val2;
		category.Load("start", roofModel, $"{num}t_a").SetPosition(val2 * 200f).SetAngle(val);
		category.Load("end", roofModel, $"{num}t_a").SetPosition(val3 * 200f).SetAngle(val + Vector3.up * 180f);
		Vector3 val4 = ((direction != 0) ? Vector3.forward : Vector3.right);
		Vector3 val5 = val2 + val4 * ((float)num * 0.25f + 0.5f);
		int i = 0;
		for (int num3 = num2 - num; i < num3; i++)
		{
			category.Load($"center_{i}", roofModel, $"{num}t_b").SetPosition((val5 + val4 * (float)i) * 200f).SetAngle(val);
		}
		category.EndLoad();
	}

	private void UpdateTiles(string tileModel)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		ModelComponent category = base.Models.GetCategory("Tile");
		Point2 size = base.Size;
		TileModel = tileModel;
		if (string.IsNullOrEmpty(tileModel))
		{
			category.Clear();
			return;
		}
		category.BeginLoad();
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.y; j++)
			{
				string key = $"{i}_{j}";
				Vector3 position = new Vector3((float)i + 0.5f, 0f, (float)j + 0.5f) * 200f;
				category.Load(key, tileModel, "1f").SetPosition(position);
			}
		}
		category.EndLoad();
	}

	private void UpdatePillars(string pillarModel, bool knockNeighborhood)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		ModelComponent category = base.Models.GetCategory("Pillar");
		Point2 size = base.Size;
		PillarModel = pillarModel;
		if (string.IsNullOrEmpty(pillarModel))
		{
			category.Clear();
			return;
		}
		category.BeginLoad();
		Direction[] array = new Direction[4]
		{
			Direction.South,
			Direction.East,
			Direction.North,
			Direction.West
		};
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			Direction direction = array[i];
			string key = direction.ToString();
			Vector2 directionPivot = KUtility.GetDirectionPivot(direction);
			Vector3 val = KUtility.DirectionToAngle(direction);
			Vector3 position = new Vector3(directionPivot.x * (float)size.x, 0f, directionPivot.y * (float)size.y) * 200f;
			float angleOffset = 0f;
			string postfixFormat = ((i % 2 != 0) ? "{0}_b" : "{0}_a");
			if (UpdatePillarModel(direction, knockNeighborhood, ref angleOffset, ref postfixFormat))
			{
				category.Load(key, pillarModel, string.Format(postfixFormat, "1f")).SetPosition(position).SetAngle(val + Vector3.up * angleOffset);
			}
			else
			{
				category.Unload(key);
			}
		}
		category.EndLoad();
	}

	public void UpdateWalls(string wallModel, ModularAddons addons)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		ModelComponent category = base.Models.GetCategory("Wall");
		Point2 size = base.Size;
		WallModel = wallModel;
		if (string.IsNullOrEmpty(wallModel))
		{
			category.Clear();
			return;
		}
		category.BeginLoad();
		int num = (size.x + size.y) * 2;
		for (int i = 0; i < num; i++)
		{
			WallIndexToPos(i, out var tile, out var dir);
			string wallPosKey = GetWallPosKey(tile, dir);
			Direction direction = dir;
			string category2 = ((direction != 0 && direction != Direction.SouthEast) ? "North" : "South");
			Vector2 directionPivot = KUtility.GetDirectionPivot(dir);
			Vector3 angle = KUtility.DirectionToAngle(dir);
			Vector3 position = new Vector3((float)tile.x + directionPivot.x, 0f, (float)tile.y + directionPivot.y) * 200f;
			ModularAddon modularAddon = addons.Get(i);
			string modelPostfix = modularAddon?.GetWallPostfix();
			category.Load(wallPosKey, modularAddon?.ModelKey, null, "Addon").SetPosition(position).SetAngle(angle);
			category.Load(wallPosKey, wallModel, modelPostfix, category2).SetPosition(position).SetAngle(angle);
		}
		category.EndLoad();
	}

	private bool UpdatePillarModel(Direction dir, bool knockNeighborhood, ref float angleOffset, ref string postfixFormat)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Point2 size = base.Size;
		ModularArtifact[] array = new ModularArtifact[3];
		Vector2 directionPivot = KUtility.GetDirectionPivot(dir);
		Point2 point = new Point2((int)directionPivot.x * (size.x - 1), (int)directionPivot.y * (size.y - 1)) + base.WorldTile;
		Point2 point2 = new Point2((int)Mathf.Sign(directionPivot.x - 0.5f), (int)Mathf.Sign(directionPivot.y - 0.5f));
		bool flag = dir == Direction.South || dir == Direction.North;
		TileObject tileObject = TerrainA6.GetTileObject(point + ((!flag) ? (Point2.up * point2.y) : (Point2.right * point2.x)), warning: false);
		array[0] = ((tileObject != null && !((Object)(object)tileObject.Artifact == (Object)null)) ? tileObject.Artifact.GetArtifactComponent<ModularArtifact>() : null);
		TileObject tileObject2 = TerrainA6.GetTileObject(point + point2, warning: false);
		array[1] = ((tileObject2 != null && !((Object)(object)tileObject2.Artifact == (Object)null)) ? tileObject2.Artifact.GetArtifactComponent<ModularArtifact>() : null);
		TileObject tileObject3 = TerrainA6.GetTileObject(point + ((!flag) ? (Point2.right * point2.x) : (Point2.up * point2.y)), warning: false);
		array[2] = ((tileObject3 != null && !((Object)(object)tileObject3.Artifact == (Object)null)) ? tileObject3.Artifact.GetArtifactComponent<ModularArtifact>() : null);
		bool flag2 = false;
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (array[i] != null)
			{
				if (string.IsNullOrEmpty(array[i].PillarModel))
				{
					array[i] = null;
				}
				else
				{
					flag2 = true;
				}
			}
		}
		bool result = true;
		if (flag2)
		{
			ulong num2 = KUtility.Max(base.EntityId, (array[0] != null) ? array[0].EntityId : 0, (array[1] != null) ? array[1].EntityId : 0, (array[2] != null) ? array[2].EntityId : 0);
			bool flag3 = num2 == base.EntityId;
			if (array[1] == null)
			{
				if (array[0] != null && array[2] != null)
				{
					return false;
				}
				if (array[0] != null)
				{
					if (flag3)
					{
						angleOffset = -45f;
						postfixFormat = "{0}_d";
					}
					else
					{
						result = false;
					}
				}
				else if (array[2] != null)
				{
					if (flag3)
					{
						angleOffset = 45f;
						postfixFormat = "{0}_d";
					}
					else
					{
						result = false;
					}
				}
			}
			else if (array[0] != null && array[2] != null)
			{
				int num3 = 0;
				ulong num4 = 0uL;
				int j = -1;
				for (int num5 = array.Length; j < num5; j++)
				{
					ulong num6 = ((j != -1) ? array[j].EntityId : base.EntityId);
					if (num6 != num2 && num6 > num4)
					{
						num4 = num6;
					}
					if (num6 == num2)
					{
						num3++;
					}
				}
				if (num3 == 1)
				{
					if (flag3)
					{
						postfixFormat = "{0}_e";
					}
					else
					{
						result = false;
					}
				}
				else if (num4 == base.EntityId)
				{
					postfixFormat = "{0}_e";
				}
				else
				{
					result = false;
				}
			}
			else if (array[0] != null)
			{
				if (flag3 || array[0].EntityId == array[1].EntityId)
				{
					angleOffset = -90f;
					postfixFormat = "{0}_c";
				}
				else
				{
					result = false;
				}
			}
			else if (array[2] != null)
			{
				if (flag3 || array[2].EntityId == array[1].EntityId)
				{
					angleOffset = 90f;
					postfixFormat = "{0}_c";
				}
				else
				{
					result = false;
				}
			}
			else if (flag3)
			{
				postfixFormat = "{0}_e";
			}
			else
			{
				result = false;
			}
		}
		if (knockNeighborhood)
		{
			int k = 0;
			for (int num7 = array.Length; k < num7; k++)
			{
				if (array[k] != null)
				{
					array[k].UpdatePillars(array[k].PillarModel, knockNeighborhood: false);
				}
			}
		}
		return result;
	}

	private void MakeBuiltCollider()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_builtCollider == (Object)null)
		{
			_builtCollider = ((Component)base.Artifact).gameObject.AddComponent<BoxCollider>();
		}
		Vector3 val = new Vector3((float)base.Size.x, (float)Height, (float)base.Size.y) * 200f;
		_builtCollider.center = val * 0.5f;
		_builtCollider.size = val;
	}

	private void RemoveBuiltCollider()
	{
		if ((Object)(object)_builtCollider != (Object)null)
		{
			Object.Destroy((Object)(object)_builtCollider);
		}
	}

	public override void OnPlayerEnter()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		base.OnPlayerEnter();
		SetVisibleState(VisibleStateEnum.PlayerEnter);
		Point2 size = base.Size;
		Vector3 position = ((Component)base.Artifact).transform.position;
		MainCamera mainCamera = KSingleton<MainCamera>.Instance();
		Vector3 position2 = ((Component)mainCamera).transform.position;
		int num = LayerMask.op_Implicit(LayerHelper.PropMask);
		int i = 0;
		Ray val3 = default(Ray);
		for (int num2 = size.x + size.y + 1; i < num2; i++)
		{
			Vector3 val;
			if (i == 0)
			{
				val = position;
			}
			else if (i <= size.x)
			{
				int num3 = i;
				val = position + Vector3.right * (float)num3 * 200f;
			}
			else
			{
				int num4 = i - size.x;
				val = position + Vector3.forward * (float)num4 * 200f;
			}
			Vector3 val2 = val - position2;
			((Ray)(ref val3))._002Ector(position2, val2);
			RaycastHit[] array = Physics.RaycastAll(val3, ((Vector3)(ref val2)).magnitude - 100f, num);
			int j = 0;
			for (int num5 = array.Length; j < num5; j++)
			{
				RaycastHit val4 = array[j];
				if ((Object)(object)((RaycastHit)(ref val4)).collider != (Object)null)
				{
					AddViewObstructions(((Component)((RaycastHit)(ref val4)).collider).GetComponentInParent<Artifact>());
				}
			}
		}
	}

	public override void OnPlayerExit()
	{
		base.OnPlayerExit();
		SetVisibleState(VisibleStateEnum.Normal);
		ClearViewObstructions();
	}

	private void AddViewObstructions(Artifact artifact)
	{
		if (!((Object)(object)artifact == (Object)null))
		{
			ModularArtifact artifactComponent = artifact.GetArtifactComponent<ModularArtifact>();
			if (artifactComponent != null && artifactComponent != this && !_viewObstructions.Contains(artifactComponent))
			{
				artifactComponent.SetVisibleState(VisibleStateEnum.Obstructions);
				_viewObstructions.Add(artifactComponent);
			}
		}
	}

	private void ClearViewObstructions()
	{
		int i = 0;
		for (int count = _viewObstructions.Count; i < count; i++)
		{
			_viewObstructions[i].SetVisibleState(VisibleStateEnum.Normal);
		}
		_viewObstructions.Clear();
	}

	public void SetVisibleState(VisibleStateEnum state)
	{
		VisibleState = state;
		switch (state)
		{
		case VisibleStateEnum.Normal:
			ShowWallAndRoof();
			break;
		case VisibleStateEnum.PlayerEnter:
			HideFrontWallAndRoof();
			break;
		case VisibleStateEnum.Obstructions:
			HideWallAndRoof();
			break;
		}
	}

	private void AlphaTweenArtifactComponent(ModelComponent comp, float alpha, float speed)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		int i = 0;
		for (int count = _alphaTweenList.Count; i < count; i++)
		{
			if (_alphaTweenList[i].Component == comp)
			{
				num = i;
				break;
			}
		}
		if (comp.Color.a == alpha)
		{
			if (num != -1)
			{
				_alphaTweenList.RemoveAt(num);
			}
			return;
		}
		AlphaTweenStruct alphaTweenStruct = default(AlphaTweenStruct);
		alphaTweenStruct.Component = comp;
		alphaTweenStruct.Alpha = alpha;
		alphaTweenStruct.Speed = speed;
		AlphaTweenStruct alphaTweenStruct2 = alphaTweenStruct;
		if (num == -1)
		{
			_alphaTweenList.Add(alphaTweenStruct2);
		}
		else
		{
			_alphaTweenList[num] = alphaTweenStruct2;
		}
		if (!_isPlayAlphaTweenRoutine && _alphaTweenList.Count > 0)
		{
			((MonoBehaviour)base.Artifact).StartCoroutine(CoAlphaTweenArtifact());
		}
	}

	private IEnumerator CoAlphaTweenArtifact()
	{
		_isPlayAlphaTweenRoutine = true;
		while (_alphaTweenList.Count > 0)
		{
			UpdateAlphaTween();
			yield return null;
		}
		_isPlayAlphaTweenRoutine = false;
	}

	private void UpdateAlphaTween()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		for (int num = _alphaTweenList.Count - 1; num >= 0; num--)
		{
			AlphaTweenStruct alphaTweenStruct = _alphaTweenList[num];
			float a = alphaTweenStruct.Component.Color.a;
			float num2 = alphaTweenStruct.Alpha * base.Artifact.Color.a;
			if (a == num2)
			{
				_alphaTweenList.RemoveAt(num);
			}
			else
			{
				float num3 = num2 - a;
				float speed = alphaTweenStruct.Speed;
				float num4 = ((!(speed <= 0f) && !(Mathf.Abs(num3) < Time.deltaTime / speed)) ? (a + Time.deltaTime / speed * Mathf.Sign(num3)) : num2);
				Color color = base.Artifact.Color;
				color.a *= num4;
				alphaTweenStruct.Component.SetColor(color);
			}
		}
	}

	private void HideWallAndRoof()
	{
		ModelComponent category = base.Models.GetCategory("Roof");
		AlphaTweenArtifactComponent(category, 0f, 0.2f);
		ModelComponent category2 = base.Models.GetCategory("Wall");
		int i = 0;
		for (int childCount = category2.ChildCount; i < childCount; i++)
		{
			ModelComponent category3 = category2.GetCategory(i);
			AlphaTweenArtifactComponent(category3, 0.5f, 0.2f);
		}
	}

	private void HideFrontWallAndRoof()
	{
		ModelComponent category = base.Models.GetCategory("Roof");
		AlphaTweenArtifactComponent(category, 0f, 0.2f);
		ModelComponent category2 = base.Models.GetCategory("Wall");
		int i = 0;
		for (int childCount = category2.ChildCount; i < childCount; i++)
		{
			ModelComponent category3 = category2.GetCategory(i);
			AlphaTweenArtifactComponent(category3, (!(category3.Category == "South")) ? 1f : 0.5f, 0.2f);
		}
	}

	private void ShowWallAndRoof()
	{
		ModelComponent category = base.Models.GetCategory("Roof");
		AlphaTweenArtifactComponent(category, 1f, 0.2f);
		ModelComponent category2 = base.Models.GetCategory("Wall");
		int i = 0;
		for (int childCount = category2.ChildCount; i < childCount; i++)
		{
			ModelComponent category3 = category2.GetCategory(i);
			AlphaTweenArtifactComponent(category3, 1f, 0.2f);
		}
	}

	public void WallIndexToPos(int index, out Point2 tile, out Direction dir)
	{
		Point2 size = base.Size;
		if (index < size.x)
		{
			tile.x = index;
			tile.y = 0;
			dir = Direction.SouthEast;
		}
		else if (index - size.x < size.y)
		{
			tile.x = size.x - 1;
			tile.y = index - size.x;
			dir = Direction.NorthEast;
		}
		else if (index - size.x - size.y < size.x)
		{
			tile.x = size.x - 1 - (index - size.x - size.y);
			tile.y = size.y - 1;
			dir = Direction.NorthWest;
		}
		else
		{
			tile.x = 0;
			tile.y = size.y - 1 - (index - size.x * 2 - size.y);
			dir = Direction.SouthWest;
		}
	}

	public int WallPosToIndex(Point2 tile, Direction dir)
	{
		Point2 size = base.Size;
		switch (dir)
		{
		case Direction.SouthEast:
			if (tile.y == 0)
			{
				return tile.x;
			}
			break;
		case Direction.NorthEast:
			if (tile.x == size.x - 1)
			{
				return size.x + tile.y;
			}
			break;
		case Direction.NorthWest:
			if (tile.y == size.y - 1)
			{
				return size.x * 2 + size.y - 1 - tile.x;
			}
			break;
		case Direction.SouthWest:
			if (tile.x == 0)
			{
				return (size.x + size.y) * 2 - 1 - tile.y;
			}
			break;
		}
		return -1;
	}

	public static string GetWallPosKey(Point2 tile, Direction dir)
	{
		return $"{tile.x},{tile.y}:{dir}";
	}

	public override void ArtifactPlaced()
	{
		base.ArtifactPlaced();
		UpdatePillars(PillarModel, knockNeighborhood: true);
	}

	public override bool ShadowSkipFunction(MeshRenderer meshRenderer)
	{
		return ((Object)meshRenderer).name.StartsWith("Tile");
	}

	public void SetInterior(Point2 pos, Artifact artifact)
	{
		int num = pos.x + pos.y * base.Size.x;
		if (_interiors != null && num >= 0 && num < _interiors.Length)
		{
			_interiors[num] = artifact;
		}
	}

	public Artifact GetInterior(Point2 pos)
	{
		int num = pos.x + pos.y * base.Size.x;
		if (_interiors == null || num < 0 || num >= _interiors.Length)
		{
			return null;
		}
		return _interiors[num];
	}

	public override void OnRemoved()
	{
		int i = 0;
		for (int size = KUtility.GetSize(_interiors); i < size; i++)
		{
			Artifact artifact = _interiors[i];
			if (!((Object)(object)artifact == (Object)null))
			{
				KSingleton<StaticObjectManager>.Instance().RemoveImmovable(artifact.WorldTile, artifact.EntityId, -1.0);
			}
		}
	}

	public override void OnUpdateBuildState()
	{
		BuildingState buildState = base.Artifact.BuildState;
		if (buildState == BuildingState.Built)
		{
			MakeBuiltCollider();
		}
		else
		{
			RemoveBuiltCollider();
		}
	}
}
