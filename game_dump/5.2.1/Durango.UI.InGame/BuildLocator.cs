using System;
using System.Collections.Generic;
using System.Linq;
using Building;
using Durango.Logic.Estate;
using Durango.Render.Camera;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Estate;
using Shared.Etc;
using Shared.Region;
using UnityEngine;

namespace Durango.UI.InGame;

public class BuildLocator : Singleton<BuildLocator>
{
	public enum BuildGridState
	{
		Invalid,
		Vaild,
		Estate,
		Effected
	}

	[Serializable]
	private struct StateColor
	{
		public Color GridColor;

		public Color PreviewColor;
	}

	public struct Arguments
	{
		public Blueprint Blueprint;

		public ArtifactDisplay? Display;

		public Point2 Size;

		public RectInt? Area;

		public int? Floor;

		public int? Stories;

		public bool HasRoof;

		public int RotatableDirections;

		public Biome[] BuildableBiomes;

		public bool Exterior;

		public bool Interior;

		public float? MinDepth;

		public float? MaxDepth;

		public bool Dump;

		public Func<Rotation, IEnumerable<Point2>> GetEffectTilesFunc;

		public static Arguments MakeFrom([NotNull] Blueprint blueprint)
		{
			Arguments result = default(Arguments);
			result.Blueprint = blueprint;
			result.Size = blueprint.Size;
			result.RotatableDirections = blueprint.RotatableDirections;
			result.BuildableBiomes = blueprint.BuildableBiomes;
			result.Exterior = blueprint.Exterior;
			result.Interior = blueprint.Interior;
			result.MinDepth = blueprint.MinBuildableDepth;
			result.MaxDepth = blueprint.MaxBuildableDepth;
			result.GetEffectTilesFunc = blueprint.GetEffectTiles;
			return result;
		}
	}

	[SerializeField]
	private GameObject _buildGridPrefab;

	[SerializeField]
	private float _previewMinAlpha;

	[SerializeField]
	private float _previewMaxAlpha;

	[EnumList(typeof(BuildGridState), false, 0, -1)]
	[SerializeField]
	private StateColor[] _gridStateColors;

	[SerializeField]
	private Material _previewMaterial;

	public Color WateringTileColor;

	public Color FertilizingTileColor;

	private BuildGrid _buildGrid;

	private Transform _previewObject;

	private ModelComponent _previewModel;

	private readonly List<Material> _previewMaterials = new List<Material>();

	private bool _isPresetPreview;

	private bool _isReplacePreivewMaterial;

	private bool _isPreviewVisible;

	private Arguments _arguments;

	public static bool IsAreaInAndOut { get; private set; }

	public static BuildGridState CurrentGridMinState { get; private set; }

	public static BuildGridState CurrentGridMaxState { get; private set; }

	[ExposedInEditor(false, null)]
	public Point2 WorldTilePos { get; private set; }

	public Point2 Size { get; private set; }

	public Vector2 CenterTile
	{
		get
		{
			int x = Size.x;
			int y = Size.y;
			Vector2 vector = ((!IsPerpendicular) ? new Vector2(x, y) : new Vector2(y, x));
			return WorldTilePos.ToVector2() + vector * 0.5f;
		}
	}

	public bool IsPerpendicular
	{
		get
		{
			if (Rotation != Rotation.Quarter)
			{
				return Rotation == Rotation.ThreeQuarter;
			}
			return true;
		}
	}

	public Rotation Rotation { get; set; }

	public event Action PreviewPositionUpdated;

	private void Start()
	{
		_previewObject = new GameObject("Preview").transform;
		_previewObject.parent = base.transform;
		_previewModel = new ModelComponent(_previewObject.gameObject);
		_previewMaterial = new Material(_previewMaterial);
		_previewModel.ModelLoaded += delegate(ModelComponent.IModel obj)
		{
			if (_isReplacePreivewMaterial && !(obj.GetObject() == null))
			{
				Renderer[] componentsInChildren2 = obj.GetObject().GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer2 in componentsInChildren2)
				{
					if ((renderer2 is MeshRenderer || renderer2 is SkinnedMeshRenderer) && renderer2.sharedMaterials != null && !renderer2.sharedMaterials.All((Material m) => m == null || m.renderQueue < 2000 || m.renderQueue > 2100))
					{
						renderer2.sharedMaterials = new Material[1] { _previewMaterial };
					}
				}
			}
		};
		_previewModel.LoadCompleted += delegate(bool success)
		{
			if (success)
			{
				if (!_isReplacePreivewMaterial)
				{
					_previewMaterials.Clear();
					Renderer[] componentsInChildren = _previewModel.Parent.GetComponentsInChildren<Renderer>();
					foreach (Renderer renderer in componentsInChildren)
					{
						if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
						{
							_previewMaterials.AddRange(renderer.materials);
						}
					}
				}
			}
			else if (_isPresetPreview)
			{
				LoadDefaultPreview();
			}
		};
	}

	private void Update()
	{
		if (!_isPreviewVisible)
		{
			return;
		}
		float num = _previewMaxAlpha - _previewMinAlpha;
		float num2 = Mathf.Sin(Time.time * 5f) * num + num * 0.5f + _previewMinAlpha;
		Color previewColor = _gridStateColors[(int)((!IsAreaInAndOut) ? CurrentGridMinState : BuildGridState.Invalid)].PreviewColor;
		if (_isPresetPreview)
		{
			previewColor.a = num2;
		}
		else
		{
			previewColor *= num2;
			previewColor.a = 1f;
		}
		foreach (Material previewMaterial in _previewMaterials)
		{
			previewMaterial.color = previewColor;
		}
	}

	public void SetArtifactBuildingMode(Arguments arguments)
	{
		_arguments = arguments;
		Size = _arguments.Size;
		Vector2 vec = Durango.Terrain.Util.WorldPositionToTilePosition(Durango.Terrain.Util.ClientPositionToWorldPosition(MainCamera.ScreenPosToWorldPos(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f))));
		WorldTilePos = ToIntoArea(new Point2(vec));
		UpdateTransform();
		EnableGridView();
		ShowPreview();
		GameSystem<InputSystem>.Instance().MoveLock = true;
	}

	public void ResetBuildingMode()
	{
		DisableGridView();
		HidePreview();
		GameSystem<InputSystem>.Instance().MoveLock = false;
	}

	private void ShowPreview()
	{
		if (_buildGrid != null)
		{
			_buildGrid.gameObject.SetActive(value: true);
		}
		_previewObject.gameObject.SetActive(value: true);
		LoadPreviewModel();
	}

	private void HidePreview()
	{
		if (_buildGrid != null)
		{
			_buildGrid.gameObject.SetActive(value: false);
		}
		_previewModel.Clear();
		_previewObject.gameObject.SetActive(value: false);
		_isPreviewVisible = false;
	}

	private void LoadPreviewModel()
	{
		_isPreviewVisible = true;
		InitBuildgrid();
		Vector2 vec = Durango.Terrain.Util.ClientPositionToTilePosition(MainCamera.ScreenPosToWorldPos(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f)));
		WorldTilePos = ToIntoArea(new Point2(vec));
		Rotation = Rotation.None;
		UpdateTransform();
		_isPresetPreview = true;
		_isReplacePreivewMaterial = false;
		string text = null;
		ArtifactDisplay? display = _arguments.Display;
		if (!display.HasValue)
		{
			Blueprint blueprint = _arguments.Blueprint;
			text = ((!string.IsNullOrEmpty(blueprint.Preview)) ? ModelComponent.GetAssetPath(blueprint.Preview) : ModelComponent.GetPreviewAssetPath(blueprint.DefaultLook));
		}
		_previewMaterials.Clear();
		if (string.IsNullOrEmpty(text))
		{
			LoadDefaultPreview();
			return;
		}
		_previewModel.BeginLoad();
		_previewModel.PathLoad("preview", text);
		_previewModel.EndLoad();
	}

	private void LoadDefaultPreview()
	{
		_isPresetPreview = false;
		_previewMaterials.Clear();
		_previewMaterials.Add(_previewMaterial);
		_previewModel.BeginLoad();
		_previewModel.SetColor(Color.white);
		ArtifactDisplay? display = _arguments.Display;
		ArtifactDisplay msg;
		if (!display.HasValue)
		{
			msg = _arguments.Blueprint.GetDefaultDisplay();
			_isReplacePreivewMaterial = true;
		}
		else
		{
			msg = _arguments.Display.Value;
			_isReplacePreivewMaterial = false;
		}
		if (_arguments.Blueprint.IsModular)
		{
			ModularArtifact.FillModels(_previewModel, msg, _arguments.Size, _arguments.Stories.GetValueOrDefault(1), _arguments.HasRoof, new Vector2(0.5f, 0.5f));
		}
		else
		{
			Artifact.FillModels(_previewModel, msg, Vector3.zero, Rotation.None);
		}
		_previewModel.EndLoad();
	}

	private Point2 ToIntoArea(Point2 tile)
	{
		RectInt? area = _arguments.Area;
		if (!area.HasValue)
		{
			return tile;
		}
		RectInt value = _arguments.Area.Value;
		Point2 point = _arguments.Size;
		if (IsPerpendicular)
		{
			point = new Point2(point.y, point.x);
		}
		if (tile.x + point.x >= value.xMax)
		{
			tile.x = value.xMax - point.x;
		}
		if (tile.y + point.y >= value.yMax)
		{
			tile.y = value.yMax - point.y;
		}
		if (tile.x < value.xMin)
		{
			tile.x = value.xMin;
		}
		if (tile.y < value.yMin)
		{
			tile.y = value.yMin;
		}
		return tile;
	}

	private void InitBuildgrid()
	{
		if (_buildGrid == null)
		{
			_buildGrid = _previewObject.gameObject.AddChild(_buildGridPrefab).GetComponent<BuildGrid>();
			NGUITools.SetLayer(_buildGrid.gameObject, OverlayCamera.Layer);
		}
		else
		{
			_buildGrid.gameObject.SetActive(value: true);
		}
		_buildGrid.Init(Size);
	}

	public void RotatePreview()
	{
		if (_isPreviewVisible)
		{
			Point2 centerTile = GetCenterTile();
			int num = (int)(Rotation + 1) % _arguments.RotatableDirections;
			Rotation = (Enum.IsDefined(typeof(Rotation), num) ? ((Rotation)num) : Rotation.None);
			Point2 centerTile2 = GetCenterTile();
			WorldTilePos = ToIntoArea(WorldTilePos + centerTile - centerTile2);
			if (!_isPresetPreview && _arguments.Blueprint.IsModular)
			{
				Point2 size = _arguments.Size;
				Point2 size2 = ((!IsPerpendicular) ? size : new Point2(size.y, size.x));
				ModularArtifact.FillModels(_previewModel, _arguments.Display.GetValueOrDefault(_arguments.Blueprint.GetDefaultDisplay()), size2, _arguments.Stories.GetValueOrDefault(1), _arguments.HasRoof, new Vector2(0.5f, 0.5f));
			}
			UpdateTransform();
		}
	}

	[ExposedInEditor(null)]
	private void EnableGridView()
	{
		GameSystem<InputSystem>.Instance().On(InputCommand.Touch, InputTouched);
		GridAreaViewer gridAreaViewer = Singleton<GridAreaViewer>.Instance();
		RectInt? area = _arguments.Area;
		if (!area.HasValue)
		{
			_arguments.Area = new RectInt(gridAreaViewer.GetTileOffset(), new Vector2Int(16, 16) * 3);
		}
		RectInt value = _arguments.Area.Value;
		RectGridArea rectGridArea = new RectGridArea
		{
			Tile = value.position,
			Size = value.size,
			TileColorFunc = GetTileStateColor
		};
		gridAreaViewer.Show(new GridAreaBase[1] { rectGridArea }, _arguments.Floor, GridAreaViewer.LayerType.Upper, tweenAlpha: false);
	}

	private bool GetTileStateColor(Point2 tile, out Color color)
	{
		BuildGridState tileBuildState = GetTileBuildState(tile);
		StateColor stateColor = _gridStateColors[(int)tileBuildState];
		color = stateColor.GridColor;
		return color.a > 0f;
	}

	private BuildGridState GetTileBuildState(Point2 tile)
	{
		TileObject tileObject = Singleton<TerrainBase>.Instance().GetTileObject(tile, warning: false);
		if (tileObject == null)
		{
			return BuildGridState.Invalid;
		}
		if (GameManager.Region.Role() == Role.Tutorial)
		{
			return BuildGridState.Invalid;
		}
		if (GameManager.Region.Role() == Role.Personal && !_arguments.Dump)
		{
			EstateInfo estateInfo = EstateSystem.GetEstateInfo(tile);
			if (estateInfo == null || estateInfo.License.Type == OwnerType.System)
			{
				return BuildGridState.Invalid;
			}
		}
		byte maskedBiome = Singleton<TerrainBase>.Instance().TilePositionToRawBiome(tile);
		if (Durango.Terrain.Util.IsCollidableMasked(maskedBiome) || Durango.Terrain.Util.IsNotPlantableMasked(maskedBiome))
		{
			return BuildGridState.Invalid;
		}
		if (_arguments.GetEffectTilesFunc != null)
		{
			IEnumerable<Point2> enumerable = _arguments.GetEffectTilesFunc(Rotation);
			if (enumerable != null && enumerable.Any((Point2 effectTile) => effectTile + WorldTilePos == tile))
			{
				return BuildGridState.Effected;
			}
		}
		Biome unmaskedBiome = Durango.Terrain.Util.GetUnmaskedBiome(maskedBiome);
		bool flag = _arguments.BuildableBiomes != null;
		bool flag2 = _arguments.MinDepth.HasValue || _arguments.MaxDepth.HasValue;
		if (flag || flag2)
		{
			if (flag && Array.IndexOf(_arguments.BuildableBiomes, unmaskedBiome) == -1)
			{
				return BuildGridState.Invalid;
			}
			if (flag2)
			{
				float tileMinDepth = Singleton<TerrainBase>.Instance().GetTileMinDepth(tile);
				if ((_arguments.MinDepth.HasValue && tileMinDepth < _arguments.MinDepth.Value) || (_arguments.MaxDepth.HasValue && tileMinDepth > _arguments.MaxDepth.Value))
				{
					return BuildGridState.Invalid;
				}
			}
		}
		else if (Durango.Terrain.Util.IsWater(unmaskedBiome))
		{
			return BuildGridState.Invalid;
		}
		if (_arguments.Exterior && tileObject.IsEmpty())
		{
			EstateInfo estateInfo2 = EstateSystem.GetEstateInfo(tile);
			if (estateInfo2 != null && estateInfo2.IsLocalPlayers())
			{
				return BuildGridState.Estate;
			}
			return BuildGridState.Vaild;
		}
		if (!_arguments.Interior)
		{
			return BuildGridState.Invalid;
		}
		Artifact artifact = tileObject.Artifact;
		if (artifact == null || !artifact.IsAvailableInterior())
		{
			return BuildGridState.Invalid;
		}
		if (artifact.Stories.Value.HasValue)
		{
			int num = artifact.Stories.Value.Value - _arguments.Floor.GetValueOrDefault();
			if (_arguments.Blueprint.Height > num)
			{
				return BuildGridState.Invalid;
			}
		}
		Point2 pos = tile - artifact.WorldTile;
		if (artifact.BuildCompleted)
		{
			int valueOrDefault = _arguments.Floor.GetValueOrDefault(0);
			if (artifact.IsOccupiablePos(pos))
			{
				int height = _arguments.Blueprint.Height;
				bool flag3 = true;
				for (int i = 0; i < height; i++)
				{
					if (artifact.GetInterior(pos, valueOrDefault + i) != null)
					{
						flag3 = false;
						break;
					}
				}
				if (flag3)
				{
					return BuildGridState.Vaild;
				}
			}
		}
		return BuildGridState.Invalid;
	}

	private void DisableGridView()
	{
		if (Singleton<GridAreaViewer>.HasInstance())
		{
			Singleton<GridAreaViewer>.Instance().Hide();
		}
		GameSystem<InputSystem>.Instance().Off(InputCommand.Touch, InputTouched);
	}

	private void UpdateTransform()
	{
		Point2 size = _arguments.Size;
		Point2 size2 = ((!IsPerpendicular) ? size : new Point2(size.y, size.x));
		Vector3 vector = Durango.Terrain.Util.TilePositionToClientPosition(WorldTilePos);
		vector += new Vector3(size2.x, 0f, size2.y) * 200f * 0.5f;
		if (_arguments.Floor.HasValue)
		{
			vector.y += _arguments.Floor.Value * 200;
		}
		GetAreaState(WorldTilePos, size2, out var min, out var max);
		CurrentGridMinState = min;
		CurrentGridMaxState = max;
		UpdateAreaInOut(WorldTilePos, size2);
		_previewObject.localEulerAngles = ((_isPresetPreview || !_arguments.Blueprint.IsModular) ? ArtifactUtil.DirectionToAngle(ArtifactUtil.RotationToDirection(Rotation)) : Vector3.zero);
		_previewObject.localPosition = vector + Vector3.up;
		if (this.PreviewPositionUpdated != null)
		{
			this.PreviewPositionUpdated();
		}
		Singleton<GridAreaViewer>.Instance().FillGridTexture();
	}

	private void GetAreaState(Point2 tile, Point2 size, out BuildGridState min, out BuildGridState max)
	{
		min = BuildGridState.Effected;
		max = BuildGridState.Invalid;
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.y; j++)
			{
				Point2 tile2 = tile + new Point2(i, j);
				BuildGridState tileBuildState = GetTileBuildState(tile2);
				min = ((tileBuildState >= min) ? min : tileBuildState);
				max = ((tileBuildState <= max) ? max : tileBuildState);
			}
		}
	}

	private void UpdateAreaInOut(Point2 tile, Point2 size)
	{
		int num = 0;
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.y; j++)
			{
				Point2 worldTile = tile + new Point2(i, j);
				TileObject tileObject = Singleton<TerrainBase>.Instance().GetTileObject(worldTile, warning: false);
				if (tileObject != null && tileObject.IsIndoor)
				{
					num++;
				}
			}
		}
		IsAreaInAndOut = num != 0 && num != size.x * size.y;
	}

	private Point2 GetCenterTile()
	{
		Point2 size = _arguments.Size;
		return new Point2((int)((float)((!IsPerpendicular) ? size.x : size.y) * 0.5f - 0.5f), (int)((float)((!IsPerpendicular) ? size.y : size.x) * 0.5f - 0.5f));
	}

	private void InputTouched(InputCommandMessage message)
	{
		if (!_isPreviewVisible)
		{
			return;
		}
		List<InputTouch.TouchEvent> touches = message.Touches;
		InputTouch.TouchEvent touchEvent = null;
		int num = 0;
		int count = touches.Count;
		for (int i = 0; i < count; i++)
		{
			InputTouch.TouchEvent touchEvent2 = touches[i];
			if (!(Math.Abs(touchEvent2.LastActivateTime - Time.timeSinceLevelLoad) > float.Epsilon) && !touchEvent2.IsNguiTouched && touchEvent2.Used != InputTouch.TouchEvent.UsedBy.Gesture)
			{
				touchEvent = touchEvent2;
				num++;
			}
		}
		if (touchEvent == null || num >= 2)
		{
			return;
		}
		Vector3 vector = touchEvent.CurrentPos;
		if (!(vector.x < 0f) && !(vector.y < 0f) && !(vector.x >= (float)Screen.width) && !(vector.y >= (float)Screen.height))
		{
			Vector2 vec = Durango.Terrain.Util.WorldPositionToTilePosition(Durango.Terrain.Util.ClientPositionToWorldPosition(MainCamera.ScreenPosToWorldPos(Input.mousePosition, _arguments.Floor.GetValueOrDefault() * 200)));
			Point2 point = ToIntoArea(new Point2(vec) - GetCenterTile());
			if (WorldTilePos != point)
			{
				UISound.PlayClick(UISound.ClickType.InteractionTarget);
				WorldTilePos = point;
				UpdateTransform();
			}
			GameSystem<InputSystem>.Instance().Touch.NotifyTouchProcessed();
		}
	}

	public BuildSystem.GridResult GetResult()
	{
		BuildSystem.GridResult gridResult = default(BuildSystem.GridResult);
		gridResult.Blueprint = _arguments.Blueprint;
		gridResult.Tile = WorldTilePos;
		gridResult.Size = _arguments.Size;
		gridResult.Floor = _arguments.Floor;
		gridResult.Stories = _arguments.Stories;
		gridResult.Rotation = Rotation;
		BuildSystem.GridResult result = gridResult;
		int? stories = result.Stories;
		if (!stories.HasValue && _arguments.Blueprint != null && _arguments.Blueprint.IsSizeVariable)
		{
			result.Stories = 1;
		}
		int? floor = result.Floor;
		if (!floor.HasValue)
		{
			TileObject tileObject = Singleton<TerrainBase>.Instance().GetTileObject(WorldTilePos, warning: false);
			if (tileObject != null && tileObject.Artifact != null && tileObject.Artifact.IsEnterable)
			{
				result.Floor = 0;
			}
		}
		return result;
	}
}
