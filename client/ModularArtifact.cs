using System;
using System.Collections.Generic;
using Durango.Terrain;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Shared.Etc;
using UnityEngine;

public class ModularArtifact : EnterableArtifact
{
	public const string Wall = "wall";

	public const string Pillar = "pillar";

	public const string Roof = "roof";

	public const string Tile = "tile";

	public const string Addon = "addon";

	public const string AddonDeco = "addonDeco";

	private const string IndoorMaskAssetPath = "Models/Prop/module/roof/module_indoor_mask.prefab";

	private const string WindowBloomAssetPath = "Particle/Prefabs/Durango_01/06_Structure/FX_Prop_Window_Bloom_01.prefab";

	private bool _indoorMaskRequested;

	private GameObject _indoorMaskObject;

	private Color _indoorMaskDefaultColor;

	private Color? _indoorMaskColor;

	private Material _indoorMaskMaterial;

	private BoxCollider[] _rooftopColliders;

	private readonly ModularAddons _addons = new ModularAddons();

	private bool _needToGroupMaterials;

	private bool _isDirtyTile;

	private bool _isDirtyRooftopCollider;

	public override string ContextIcon => "act_modular_info";

	public override int Height => 2;

	public override bool HasWall => !string.IsNullOrEmpty(GetModel("wall"));

	public override void PostInit(string blueprintId, Point2 worldTile, Rotation rotation, Point2 size)
	{
		base.PostInit(blueprintId, worldTile, rotation, size);
		base.Models.MaterialPropertyChanged += GroupMaterialsByCategory;
		Observable<int?> stories = base.Artifact.Stories;
		stories.Changed = (Action<int?>)Delegate.Combine(stories.Changed, (Action<int?>)delegate
		{
			UpdateIndoorMaskSize();
			_isDirtyRooftopCollider = true;
		});
		Observable<bool?> hasRoof = base.Artifact.HasRoof;
		hasRoof.Changed = (Action<bool?>)Delegate.Combine(hasRoof.Changed, (Action<bool?>)delegate
		{
			UpdateIndoorMaskSize();
			_isDirtyRooftopCollider = true;
		});
		_isDirtyRooftopCollider = true;
	}

	public override void Update()
	{
		base.Update();
		if (_isDirtyTile)
		{
			_isDirtyTile = false;
			UpdateTiles();
		}
		if (_isDirtyRooftopCollider)
		{
			_isDirtyRooftopCollider = false;
			UpdateRooftopColliders();
		}
	}

	private void LoadIndoorMask()
	{
		if (!HasWall || _indoorMaskRequested)
		{
			return;
		}
		_indoorMaskRequested = true;
		Singleton<AssetBundleManager>.Instance().RequestAsset("Models/Prop/module/roof/module_indoor_mask.prefab", typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!(asset == null))
			{
				GameObject gameObject = asset as GameObject;
				if (!(gameObject == null) && !(base.Artifact == null))
				{
					_indoorMaskObject = UnityEngine.Object.Instantiate(gameObject, base.Artifact.transform);
					UpdateIndoorMaskSize();
					_indoorMaskObject.gameObject.SetActive(base.Visible == VisibleState.PlayerInside);
					UpdateIndoorMaskColor();
				}
			}
		});
	}

	private void UpdateIndoorMaskSize()
	{
		if (!(_indoorMaskObject == null))
		{
			int num = base.Stories;
			if (!base.HasRoof)
			{
				num = Mathf.Max(1, num - 1);
			}
			float num2 = (float)(num * 200) + 30f;
			Vector3 vector = new Vector3(base.Size.x * 200, base.Size.y * 200, num2);
			_indoorMaskObject.transform.localScale = vector - new Vector3(1.5f, 1.5f) * 12f;
			_indoorMaskObject.transform.localPosition = new Vector3((vector.x - 12f) * 0.5f, num2 * 0.5f, (vector.y - 12f) * 0.5f);
		}
	}

	private void UpdateRooftopColliders()
	{
		Point2 size = base.Size;
		int num = ((base.PlayerEntered && !base.HasRoof && base.Artifact.BuildCompleted) ? ((size.x + size.y) * 2) : 0);
		if (num != KUtility.GetSize(_rooftopColliders))
		{
			if (_rooftopColliders != null)
			{
				BoxCollider[] rooftopColliders = _rooftopColliders;
				foreach (BoxCollider boxCollider in rooftopColliders)
				{
					if (!(boxCollider == null))
					{
						UnityEngine.Object.Destroy(boxCollider.gameObject);
					}
				}
				_rooftopColliders = null;
			}
			if (num > 0)
			{
				_rooftopColliders = new BoxCollider[num];
				for (int j = 0; j < _rooftopColliders.Length; j++)
				{
					BoxCollider boxCollider2 = base.Artifact.gameObject.AddChild<BoxCollider>();
					_rooftopColliders[j] = boxCollider2;
					boxCollider2.size = new Vector3(20f, 200f, 200f);
					boxCollider2.center = Vector3.up * 100f;
				}
			}
		}
		if (_rooftopColliders == null)
		{
			return;
		}
		int num2 = Mathf.Max(0, base.Stories - 1);
		for (int k = 0; k < num; k++)
		{
			WallIndexToPos(k, size, out var tile, out var dir);
			GetWallPosition(num2, tile, dir, out var pos, out var angle);
			Transform transform = _rooftopColliders[k].transform;
			transform.localPosition = pos;
			transform.localEulerAngles = angle;
			Point2 point = Point2.zero;
			switch (dir)
			{
			case Direction.SouthWest:
				point = new Point2(-1, 0);
				break;
			case Direction.SouthEast:
				point = new Point2(0, -1);
				break;
			case Direction.NorthWest:
				point = new Point2(0, 1);
				break;
			case Direction.NorthEast:
				point = new Point2(1, 0);
				break;
			}
			bool enabled = true;
			Point2 worldTile = base.Artifact.WorldTile;
			if (point != Point2.zero)
			{
				TileObject tileObject = Singleton<TerrainBase>.Instance().GetTileObject(worldTile + tile + point, warning: false);
				if (tileObject != null && tileObject.Artifact != null)
				{
					Artifact artifact = tileObject.Artifact;
					if (artifact.Stories.Value.HasValue)
					{
						int? value = artifact.Stories.Value;
						if (value.HasValue && value.GetValueOrDefault() > num2)
						{
							enabled = false;
						}
					}
				}
			}
			_rooftopColliders[k].enabled = enabled;
		}
	}

	private void UpdateIndoorMaskColor()
	{
		if (_indoorMaskObject == null)
		{
			return;
		}
		if (_indoorMaskMaterial == null)
		{
			Color? indoorMaskColor = _indoorMaskColor;
			if (!indoorMaskColor.HasValue)
			{
				return;
			}
		}
		Renderer component = _indoorMaskObject.GetComponent<Renderer>();
		if (_indoorMaskMaterial == null)
		{
			Material[] sharedMaterials = component.sharedMaterials;
			if (sharedMaterials.Length <= 1)
			{
				return;
			}
			_indoorMaskMaterial = new Material(component.sharedMaterials[1]);
			sharedMaterials[1] = _indoorMaskMaterial;
			component.sharedMaterials = sharedMaterials;
			_indoorMaskDefaultColor = _indoorMaskMaterial.color;
		}
		Material indoorMaskMaterial = _indoorMaskMaterial;
		Color? indoorMaskColor2 = _indoorMaskColor;
		indoorMaskMaterial.color = ((!indoorMaskColor2.HasValue) ? _indoorMaskDefaultColor : _indoorMaskColor.Value);
	}

	public override bool OnUpdateDisplay(ArtifactDisplay msg)
	{
		FillModels(this, base.Models, msg, base.Size, base.Stories, base.HasRoof, Vector2.zero);
		CheckTilesIndoor();
		_indoorMaskColor = ((msg.IndoorColor == null) ? null : new Color?(msg.IndoorColor.ToColor()));
		UpdateIndoorMaskColor();
		return true;
	}

	public ModularAddons GetAddons()
	{
		return _addons;
	}

	public static bool FillModels(ModelComponent models, ArtifactDisplay msg, Point2 size, int stories, bool hasRoof, Vector2 pivot)
	{
		return FillModels(null, models, msg, size, stories, hasRoof, pivot);
	}

	private static bool FillModels(ModularArtifact owner, ModelComponent models, ArtifactDisplay msg, Point2 size, int stories, bool hasRoof, Vector2 pivot)
	{
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		if (msg.Parts != null)
		{
			text = msg.Parts.Get("tile");
			text2 = msg.Parts.Get("roof");
			text3 = msg.Parts.Get("pillar");
			text4 = msg.Parts.Get("wall");
		}
		if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(text2) && string.IsNullOrEmpty(text3) && string.IsNullOrEmpty(text4))
		{
			return false;
		}
		string texture = null;
		string texture2 = null;
		string texture3 = null;
		if (msg.Textures != null)
		{
			texture = msg.Textures.Get("wall");
			texture2 = msg.Textures.Get("tile");
			texture3 = msg.Textures.Get("roof");
		}
		models.BeginLoad();
		Vector3 offset = -new Vector3((float)size.x * pivot.x, 0f, (float)size.y * pivot.y) * 200f;
		UpdateTiles(models, text, texture2, size, stories, offset, owner?.Artifact.GetInteriors());
		if (hasRoof)
		{
			UpdateRoof(models, text2, texture3, size, stories, offset);
		}
		UpdatePillars(owner?.EntityId, models, text3, size, stories, hasRoof, offset, owner?.WorldTile, knockNeighborhood: false);
		ModularAddons modularAddons = ((owner != null) ? owner._addons : new ModularAddons());
		modularAddons.Set(msg.AddOns);
		UpdateWalls(models, text4, texture, size, stories, hasRoof, offset, modularAddons);
		models.EndLoad();
		return true;
	}

	private static void UpdateRoof(ModelComponent models, string roofModel, string texture, Point2 size, int stories, Vector3 offset)
	{
		ModelComponent category = models.GetCategory("roof");
		if (string.IsNullOrEmpty(roofModel))
		{
			category.Clear();
			return;
		}
		category.BeginLoad();
		int num = Mathf.Min(size.x, size.y);
		int num2 = Mathf.Max(size.x, size.y);
		Direction direction = ((num != size.y) ? Direction.SouthEast : Direction.SouthWest);
		Vector2 directionPivot = ArtifactUtil.GetDirectionPivot(direction);
		Vector3 vector = ArtifactUtil.DirectionToAngle(direction);
		Vector3 vector2 = new Vector3((float)num * (1f - directionPivot.y), 0f, (float)num * (1f - directionPivot.x));
		vector2 *= 0.5f;
		Vector3 vector3 = new Vector3(size.x, 0f, size.y) - vector2;
		vector2.y = (vector3.y = stories - 1);
		category.Load("start", roofModel, $"{num}t_a").SetPosition(vector2 * 200f + offset).SetAngle(vector);
		category.Load("end", roofModel, $"{num}t_a").SetPosition(vector3 * 200f + offset).SetAngle(vector + Vector3.up * 180f);
		Vector3 vector4 = ((direction != 0) ? Vector3.forward : Vector3.right);
		Vector3 vector5 = vector2 + vector4 * ((float)num * 0.25f + 0.5f);
		int i = 0;
		for (int num3 = num2 - num; i < num3; i++)
		{
			category.Load($"center_{i}", roofModel, $"{num}t_b").SetPosition((vector5 + vector4 * i) * 200f + offset).SetAngle(vector);
		}
		category.SetPatternTex(texture);
		category.EndLoad();
	}

	private void UpdateTiles()
	{
		UpdateTiles(base.Models, GetModel("tile"), GetTexture("tile"), base.Size, base.Stories, Vector3.zero, base.Artifact.GetInteriors());
	}

	private static void UpdateTiles(ModelComponent models, string tileModel, string texture, Point2 size, int stories, Vector3 offset, [CanBeNull] Artifact[] interiors)
	{
		ModelComponent category = models.GetCategory("tile");
		if (string.IsNullOrEmpty(tileModel))
		{
			category.Clear();
			return;
		}
		category.BeginLoad();
		Reusable<HashSet<int>> reusable = null;
		if (interiors != null)
		{
			for (int i = 0; i < stories; i++)
			{
				for (int j = 0; j < size.x; j++)
				{
					for (int k = 0; k < size.y; k++)
					{
						int num = j + k * size.x + i * size.x * size.y;
						if (num < 0 || num >= interiors.Length)
						{
							continue;
						}
						Artifact artifact = interiors[num];
						if (artifact == null)
						{
							continue;
						}
						int height = artifact.Height;
						if (height <= 1)
						{
							continue;
						}
						int num2 = j + k * size.x + (i - 1) * size.x * size.y;
						if (num2 >= 0 && num2 < interiors.Length && !(artifact != interiors[num2]))
						{
							if (reusable == null)
							{
								reusable = ReusableHashSet<int>.Pop();
							}
							reusable.Value.Add(num);
						}
					}
				}
			}
		}
		for (int l = 0; l < stories; l++)
		{
			ModelComponent category2 = category.GetCategory(l.ToString());
			for (int m = 0; m < size.x; m++)
			{
				for (int n = 0; n < size.y; n++)
				{
					if (reusable != null)
					{
						int item = m + n * size.x + l * size.x * size.y;
						if (reusable.Value.Contains(item))
						{
							continue;
						}
					}
					string key = $"{m},{n}";
					Vector3 position = new Vector3((float)m + 0.5f, 0f, (float)n + 0.5f) * 200f + offset;
					position.y = l * 200;
					category2.Load(key, tileModel, "1f").SetPosition(position);
				}
			}
		}
		reusable?.Dispose();
		category.SetPatternTex(texture);
		category.EndLoad();
	}

	private void UpdatePillars(bool knockNeighborhood)
	{
		UpdatePillars(base.EntityId, base.Models, GetModel("pillar"), base.Size, base.Stories, base.HasRoof, Vector3.zero, base.WorldTile, knockNeighborhood);
	}

	private static void UpdatePillars(string entityId, ModelComponent models, string pillarModel, Point2 size, int stories, bool hasRoof, Vector3 offset, Point2? worldTile, bool knockNeighborhood)
	{
		ModelComponent category = models.GetCategory("pillar");
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
		Vector3 one = Vector3.one;
		if (hasRoof)
		{
			one.y = 0.3f + 0.7f * (float)stories;
		}
		else
		{
			one.y = 0.1f + 0.7f * (float)Mathf.Max(0, stories - 1);
		}
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			Direction dir = array[i];
			string key = dir.ToString();
			Vector2 directionPivot = ArtifactUtil.GetDirectionPivot(dir);
			Vector3 vector = ArtifactUtil.DirectionToAngle(dir);
			Vector3 position = new Vector3(directionPivot.x * (float)size.x, 0f, directionPivot.y * (float)size.y) * 200f + offset;
			float angleOffset = 0f;
			string postfixFormat = ((i % 2 != 0) ? "{0}_b" : "{0}_a");
			if (UpdatePillarModel(entityId, dir, size, worldTile, knockNeighborhood, ref angleOffset, ref postfixFormat))
			{
				category.Load(key, pillarModel, string.Format(postfixFormat, "1f")).SetPosition(position).SetAngle(vector + Vector3.up * angleOffset)
					.SetScale(one);
			}
			else
			{
				category.Unload(key);
			}
		}
		category.EndLoad();
	}

	public void UpdateWalls(ModularAddons addons)
	{
		UpdateWalls(base.Models, GetModel("wall"), GetTexture("wall"), base.Size, base.Stories, base.HasRoof, Vector3.zero, addons);
	}

	private static void UpdateWalls(ModelComponent models, string wallModel, string texture, Point2 size, int stories, bool hasRoof, Vector3 offset, ModularAddons addons)
	{
		ModelComponent category = models.GetCategory("wall");
		if (string.IsNullOrEmpty(wallModel))
		{
			category.Clear();
			return;
		}
		category.BeginLoad();
		int num = (size.x + size.y) * 2;
		int num2 = ((!hasRoof) ? Mathf.Max(1, stories - 1) : stories);
		for (int i = 0; i < num2; i++)
		{
			ModelComponent category2 = category.GetCategory(i.ToString());
			for (int j = 0; j < num; j++)
			{
				WallIndexToPos(j, size, out var tile, out var dir);
				string wallPosKey = GetWallPosKey(tile, dir);
				bool flag = dir == Direction.SouthEast || dir == Direction.SouthWest;
				GetWallPosition(i, tile, dir, out var pos, out var angle);
				pos += offset;
				string text = ((!flag) ? "North" : "South");
				ModularAddon modularAddon = addons.Get(i * num + j);
				bool flag2 = modularAddon?.IsEmptyDoor() ?? false;
				if (flag2)
				{
					category2.Load(wallPosKey, null, null, text + "addon");
					category2.Load(wallPosKey, null, null, text);
				}
				else
				{
					string text2 = modularAddon?.ModelKey;
					string modelPostfix = modularAddon?.GetWallPostfix();
					if (modularAddon != null && string.IsNullOrEmpty(text2) && modularAddon.Type == "door")
					{
						category2.Load(wallPosKey, wallModel, modelPostfix, text + "addon").SetPosition(pos).SetAngle(angle);
						category2.Load(wallPosKey, null, null, text);
					}
					else
					{
						category2.Load(wallPosKey, text2, null, text + "addon").SetPosition(pos).SetAngle(modularAddon?.GetAngle(dir) ?? angle);
						category2.Load(wallPosKey, wallModel, modelPostfix, text).SetPosition(pos).SetAngle(angle);
					}
				}
				if (!flag && modularAddon != null && modularAddon.Type == "window")
				{
					category2.PathLoad(wallPosKey, "Particle/Prefabs/Durango_01/06_Structure/FX_Prop_Window_Bloom_01.prefab", "addonDeco", isFullPath: true).SetPosition(pos).SetAngle(angle);
				}
				if (flag && modularAddon != null && modularAddon.NeedInnerShadow())
				{
					Vector3 vector = ((!flag2) ? ((dir != Direction.SouthEast) ? new Vector3(20f, 0f, 0f) : new Vector3(0f, 0f, 20f)) : Vector3.zero);
					category2.PathLoad(wallPosKey, "Prop/module/shadow/shadow_01_inner.prefab", "addonDeco").SetPosition(pos + vector).SetAngle(-angle);
				}
			}
		}
		category.SetPatternTex(texture);
		category.EndLoad();
	}

	private static bool UpdatePillarModel(string id, Direction dir, Point2 size, Point2? worldTile, bool knockNeighborhood, ref float angleOffset, ref string postfixFormat)
	{
		bool flag = false;
		ModularArtifact[] array = null;
		if (worldTile.HasValue)
		{
			array = new ModularArtifact[3];
			Vector2 directionPivot = ArtifactUtil.GetDirectionPivot(dir);
			Point2 point = new Point2((int)directionPivot.x * (size.x - 1), (int)directionPivot.y * (size.y - 1)) + worldTile.Value;
			Point2 point2 = new Point2((int)Mathf.Sign(directionPivot.x - 0.5f), (int)Mathf.Sign(directionPivot.y - 0.5f));
			bool flag2 = dir == Direction.South || dir == Direction.North;
			TileObject tileObject = Singleton<TerrainBase>.Instance().GetTileObject(point + ((!flag2) ? (Point2.up * point2.y) : (Point2.right * point2.x)), warning: false);
			array[0] = ((tileObject != null && !(tileObject.Artifact == null)) ? tileObject.Artifact.GetArtifactComponent<ModularArtifact>() : null);
			TileObject tileObject2 = Singleton<TerrainBase>.Instance().GetTileObject(point + point2, warning: false);
			array[1] = ((tileObject2 != null && !(tileObject2.Artifact == null)) ? tileObject2.Artifact.GetArtifactComponent<ModularArtifact>() : null);
			TileObject tileObject3 = Singleton<TerrainBase>.Instance().GetTileObject(point + ((!flag2) ? (Point2.right * point2.x) : (Point2.up * point2.y)), warning: false);
			array[2] = ((tileObject3 != null && !(tileObject3.Artifact == null)) ? tileObject3.Artifact.GetArtifactComponent<ModularArtifact>() : null);
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				if (array[i] != null)
				{
					if (string.IsNullOrEmpty(array[i].GetModel("pillar")))
					{
						array[i] = null;
					}
					else
					{
						flag = true;
					}
				}
			}
		}
		bool result = true;
		if (flag)
		{
			string text = id;
			for (int j = 0; j > 3; j++)
			{
				string text2 = ((array[j] != null) ? array[j].EntityId : string.Empty);
				if (string.CompareOrdinal(text2, text) > 0)
				{
					text = text2;
				}
			}
			bool flag3 = text == id;
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
				int num2 = 0;
				string text3 = string.Empty;
				int k = -1;
				for (int num3 = array.Length; k < num3; k++)
				{
					string text4 = ((k != -1) ? array[k].EntityId : id);
					if (text4 != text && string.CompareOrdinal(text4, text3) > 0)
					{
						text3 = text4;
					}
					if (text4 == text)
					{
						num2++;
					}
				}
				if (num2 == 1)
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
				else if (text3 == id)
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
		if (knockNeighborhood && array != null)
		{
			int l = 0;
			for (int num4 = array.Length; l < num4; l++)
			{
				if (array[l] != null)
				{
					array[l].UpdatePillars(knockNeighborhood: false);
				}
			}
		}
		return result;
	}

	public override void OnPlayerEnter()
	{
		base.OnPlayerEnter();
		_isDirtyRooftopCollider = true;
	}

	public override void OnPlayerExit()
	{
		base.OnPlayerExit();
		_isDirtyRooftopCollider = true;
	}

	public override void OnPlayerFloorChange()
	{
		base.OnPlayerFloorChange();
		if (base.PlayerEntered)
		{
			HideUpperFloor((byte)PlayerBehavior.LocalPlayer.Floor);
		}
	}

	public void HideUpperFloor(int? floor)
	{
		ModelComponent category = base.Models.GetCategory("tile");
		for (int i = 0; i < category.ChildCount; i++)
		{
			ModelComponent child = category.GetChild(i);
			int result;
			if (!floor.HasValue)
			{
				child.SetActive(active: true);
			}
			else if (int.TryParse(child.Category, out result))
			{
				child.SetActive(result <= floor.Value);
			}
			else
			{
				child.SetActive(active: false);
			}
		}
		ModelComponent category2 = base.Models.GetCategory("wall");
		for (int j = 0; j < category2.ChildCount; j++)
		{
			ModelComponent child2 = category2.GetChild(j);
			int result2;
			float num = ((!floor.HasValue) ? 1f : ((!int.TryParse(child2.Category, out result2)) ? 0f : ((result2 == floor.Value) ? 0.5f : ((result2 >= floor.Value) ? 0f : 1f))));
			for (int k = 0; k < child2.ChildCount; k++)
			{
				ModelComponent child3 = child2.GetChild(k);
				switch (child3.Category)
				{
				case "South":
				case "Southaddon":
					if (num > 0f)
					{
						child3.SetActive(active: true);
						child3.SetColor(child3.GetColor().WithA(num));
					}
					else
					{
						child3.SetActive(active: false);
					}
					break;
				case "addonDeco":
					child3.SetActive(num >= 1f);
					break;
				}
			}
		}
		Artifact[] interiors = base.Artifact.GetInteriors();
		if (interiors == null)
		{
			return;
		}
		Artifact[] array = interiors;
		foreach (Artifact artifact in array)
		{
			if (!(artifact == null) && artifact.Floor.HasValue)
			{
				artifact.Models.SetActive(!floor.HasValue || (floor.HasValue && artifact.Floor.Value <= floor.GetValueOrDefault()));
			}
		}
	}

	protected override void UpdateVisibleState()
	{
		switch (base.Visible)
		{
		case VisibleState.Normal:
			ShowWallAndRoof();
			HideUpperFloor(null);
			break;
		case VisibleState.PlayerInside:
			GroupMaterialsByCategory();
			HideRoof();
			HideUpperFloor((byte)PlayerBehavior.LocalPlayer.Floor);
			break;
		case VisibleState.Obstruction:
			GroupMaterialsByCategory();
			HideWallAndRoof();
			HideUpperFloor(0);
			break;
		}
		bool flag = base.Visible == VisibleState.PlayerInside;
		if (_indoorMaskObject != null)
		{
			_indoorMaskObject.gameObject.SetActive(flag);
		}
		else if (flag)
		{
			LoadIndoorMask();
		}
		ShowAddonOutline(base.IsGodModeTarget);
	}

	public override void ResourcesLoadCompleted()
	{
		_needToGroupMaterials = true;
		base.ResourcesLoadCompleted();
	}

	private void GroupMaterialsByCategory()
	{
		if (_needToGroupMaterials)
		{
			ModelComponent category = base.Models.GetCategory("roof");
			Dictionary<Material, Material> dictionary = new Dictionary<Material, Material>();
			category.SetMaterialsToBeShared(dictionary);
			ModelComponent category2 = base.Models.GetCategory("wall");
			for (int i = 0; i < category2.ChildCount; i++)
			{
				ModelComponent child = category2.GetChild(i);
				ModelComponent category3 = child.GetCategory("South");
				dictionary.Clear();
				category3.SetMaterialsToBeShared(dictionary);
			}
			dictionary.Clear();
			for (int j = 0; j < category2.ChildCount; j++)
			{
				ModelComponent child2 = category2.GetChild(j);
				ModelComponent category4 = child2.GetCategory("North");
				category4.SetMaterialsToBeShared(dictionary);
			}
			ModelComponent category5 = base.Models.GetCategory("tile");
			dictionary.Clear();
			for (int k = 0; k < category5.ChildCount; k++)
			{
				ModelComponent child3 = category5.GetChild(k);
				child3.SetMaterialsToBeShared(dictionary);
			}
			ModelComponent category6 = base.Models.GetCategory("pillar");
			dictionary.Clear();
			category6.SetMaterialsToBeShared(dictionary);
			_needToGroupMaterials = false;
		}
	}

	private void HideWallAndRoof()
	{
		ModelComponent category = base.Models.GetCategory("roof");
		AlphaTweenArtifactComponent(category, 0f, 0.2f);
		ModelComponent category2 = base.Models.GetCategory("wall");
		int i = 0;
		for (int childCount = category2.ChildCount; i < childCount; i++)
		{
			ModelComponent child = category2.GetChild(i);
			int j = 0;
			for (int childCount2 = child.ChildCount; j < childCount2; j++)
			{
				ModelComponent child2 = child.GetChild(j);
				string category3 = child2.Category;
				if (category3 != null && category3 == "addonDeco")
				{
					AlphaTweenArtifactComponent(child2, 0f, 0.2f);
				}
				else
				{
					AlphaTweenArtifactComponent(child2, 0.5f, 0.2f);
				}
			}
		}
	}

	private void HideRoof()
	{
		ModelComponent category = base.Models.GetCategory("roof");
		AlphaTweenArtifactComponent(category, 0f, 0.2f);
	}

	private void ShowWallAndRoof()
	{
		ModelComponent category = base.Models.GetCategory("roof");
		AlphaTweenArtifactComponent(category, 1f, 0.2f);
		ModelComponent category2 = base.Models.GetCategory("wall");
		int i = 0;
		for (int childCount = category2.ChildCount; i < childCount; i++)
		{
			ModelComponent child = category2.GetChild(i);
			int j = 0;
			for (int childCount2 = child.ChildCount; j < childCount2; j++)
			{
				ModelComponent child2 = child.GetChild(j);
				AlphaTweenArtifactComponent(child2, 1f, 0.2f);
			}
		}
	}

	private void ShowAddonOutline(bool on)
	{
		ModelComponent category = base.Models.GetCategory("wall");
		int i = 0;
		for (int childCount = category.ChildCount; i < childCount; i++)
		{
			ModelComponent child = category.GetChild(i);
			int j = 0;
			for (int childCount2 = child.ChildCount; j < childCount2; j++)
			{
				ModelComponent child2 = child.GetChild(j);
				switch (child2.Category)
				{
				case "Southaddon":
				case "Northaddon":
					child2.SetOutlineColor((!on) ? Color.clear : new Color(1f, 1f, 1f, 0.39f));
					break;
				}
			}
		}
	}

	public IEnumerable<ModelComponent.IModel> GetWallModels(int floor, Point2 tile, Direction dir)
	{
		string key = GetWallPosKey(tile, dir);
		ModelComponent category = base.Models.GetCategory("wall").GetCategory(floor.ToString());
		int i = 0;
		for (int iMax = category.ChildCount; i < iMax; i++)
		{
			ModelComponent.IModel j = category.GetChild(i).GetModel(key);
			if (!j.IsNull())
			{
				yield return j;
			}
		}
	}

	public static void GetWallPosition(int index, Point2 size, out Vector3 pos, out Vector3 angle)
	{
		WallIndexToPos(index, size, out var floor, out var tile, out var dir);
		GetWallPosition(floor, tile, dir, out pos, out angle);
	}

	public static void GetWallPosition(int floor, Point2 tile, Direction dir, out Vector3 pos, out Vector3 angle)
	{
		Vector2 directionPivot = ArtifactUtil.GetDirectionPivot(dir);
		angle = ArtifactUtil.DirectionToAngle(dir);
		pos = new Vector3((float)tile.x + directionPivot.x, 0f, (float)tile.y + directionPivot.y) * 200f;
		pos.y = floor * 200;
	}

	public static void WallIndexToPos(int index, Point2 size, out int floor, out Point2 tile, out Direction dir)
	{
		int num = (size.x + size.y) * 2;
		if (num <= 0)
		{
			floor = 0;
			tile = Point2.zero;
			dir = Direction.Invalid;
		}
		else
		{
			floor = index / num;
			WallIndexToPos(index % num, size, out tile, out dir);
		}
	}

	private static void WallIndexToPos(int index, Point2 size, out Point2 tile, out Direction dir)
	{
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
		UpdatePillars(knockNeighborhood: true);
	}

	public override void OnChangeInterior()
	{
		base.OnChangeInterior();
		_isDirtyTile = true;
	}

	public string GetModel(string key)
	{
		return base.Artifact.Display.Parts?.Get(key);
	}

	public string GetTexture(string key)
	{
		return base.Artifact.Display.Textures?.Get(key);
	}
}
