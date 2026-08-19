using System;
using System.Collections;
using System.Collections.Generic;
using Building_;
using Estate;
using JetBrains.Annotations;
using L10N;
using NCalc;
using Shared.Estate;
using Shared.Etc;
using Shared.Region;
using TerrainData;
using UnityEngine;

public class BuildManager : KSingleton<BuildManager>
{
	public enum BuildGridState
	{
		Invalid,
		Vaild,
		Estate
	}

	[Serializable]
	[EnumType(typeof(BuildGridState))]
	private class GridStateColors : EnumKeyList
	{
		[SerializeField]
		private List<StateColor> _values;

		public StateColor Get(BuildGridState state)
		{
			int num = IndexOf((int)state);
			return (num != -1) ? _values[num] : default(StateColor);
		}
	}

	[Serializable]
	private struct StateColor
	{
		public Color GridColor;

		public Color PreviewColor;
	}

	public delegate void PreviewPositionDelegate(Vector3 pos, Point2 size);

	private const string DefaultAssetPath = "Models/Prop/system/preview_shovel/preview_shovel.prefab";

	[SerializeField]
	private GameObject _buildGridPrefab;

	[SerializeField]
	private float _previewMinAlpha;

	[SerializeField]
	private float _previewMaxAlpha;

	[SerializeField]
	private GridStateColors _gridStateColors;

	private Point2 _worldTilePos;

	private Direction _direction;

	private BuildGrid _buildGrid;

	private GameObject _buildingPreview;

	private GameObject _previewAsset;

	private bool _isPreviewVisible;

	public static BuildGridState CurrentGridMinState { get; private set; }

	public static BuildGridState CurrentGridMaxState { get; private set; }

	[ExposedInEditor(false, null)]
	public Point2 WorldTilePos
	{
		get
		{
			return _worldTilePos;
		}
		private set
		{
			_worldTilePos = value;
			UpdateTransform();
		}
	}

	public Point2 Size { get; private set; }

	public Vector2 CenterTile
	{
		get
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			int x = Size.x;
			int y = Size.y;
			Vector2 val = ((!Rotated) ? new Vector2((float)x, (float)y) : new Vector2((float)y, (float)x));
			Vector2 val2 = WorldTilePos.ToVector2();
			return val2 + val * 0.5f;
		}
	}

	public Vector3 Center => TerrainA6.TilePositionToClientPosition(CenterTile);

	public bool RotationDisabled => ArtifactBlueprint.RotationDisabled;

	public bool Rotated
	{
		get
		{
			return Direction == Direction.SouthEast;
		}
		private set
		{
			Direction = (value ? Direction.SouthEast : Direction.SouthWest);
		}
	}

	public Direction Direction
	{
		get
		{
			return (!RotationDisabled) ? _direction : Direction.SouthWest;
		}
		private set
		{
			_direction = value;
		}
	}

	public Point2 CorrectedWorldTilePos => WorldTilePos;

	public Blueprint ArtifactBlueprint { get; private set; }

	public GameObject BuildingPreview
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			if ((Object)(object)_buildingPreview == (Object)null)
			{
				_buildingPreview = new GameObject("Building Preivew");
				_buildingPreview.transform.parent = ((Component)this).transform;
			}
			return _buildingPreview;
		}
	}

	private bool IsPreviewVisible
	{
		get
		{
			return _isPreviewVisible;
		}
		set
		{
			_isPreviewVisible = value;
			BuildingPreview.SetActive(value);
		}
	}

	public event PreviewPositionDelegate PreviewPositionUpdated;

	public void SetArtifactBuildingMode([NotNull] Blueprint blueprint, Point2 size)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (KSingleton<GameManager>.Instance().Region.Role() == Role.Tutorial)
		{
			string comment = T._("앙코라에서는 건설을 할 수 없습니다");
			UIManager.SystemMsg(comment, -1f);
		}
		ArtifactBlueprint = blueprint;
		Size = ((size.x <= 0 || size.y <= 0) ? blueprint.Size : size);
		Vector3 clientPosition = MainCamera.ScreenPosToWorldPos(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f));
		clientPosition = TerrainA6.ClientPositionToWorldPosition(clientPosition);
		Vector2 vec = TerrainA6.WorldPositionToTilePosition(clientPosition);
		WorldTilePos = new Point2(vec);
		int gridSize = 0;
		if (blueprint.IsEstateFlag)
		{
			gridSize = 4;
		}
		if (blueprint.IsClanEstateFlag)
		{
			gridSize = 8;
		}
		EnableGridView(gridSize);
		ShowPreview();
		KSingleton<PlayerController>.Instance().MoveLock = true;
	}

	public void ResetBuildingMode()
	{
		DisableGridView();
		HidePreview();
		KSingleton<PlayerController>.Instance().MoveLock = false;
	}

	private void ShowPreview()
	{
		if (IsPreviewVisible)
		{
			HidePreview();
		}
		ShowArtifactPreview();
	}

	private void HidePreview()
	{
		if ((Object)(object)_buildGrid != (Object)null)
		{
			((Component)_buildGrid).gameObject.SetActive(false);
		}
		if ((Object)(object)_previewAsset != (Object)null)
		{
			Object.Destroy((Object)(object)_previewAsset);
			_previewAsset = null;
		}
		IsPreviewVisible = false;
	}

	private void ShowArtifactPreview()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (ArtifactBlueprint == null)
		{
			Debug.LogError((object)"ArtifactBlueprint is null: ");
			return;
		}
		IsPreviewVisible = true;
		InitBuildgrid();
		Vector3 clientPosition = MainCamera.ScreenPosToWorldPos(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f));
		clientPosition = TerrainA6.ClientPositionToWorldPosition(clientPosition);
		Vector2 vec = TerrainA6.WorldPositionToTilePosition(clientPosition);
		WorldTilePos = new Point2(vec);
		Rotated = false;
		UpdateTransform();
		string assetPath = ((!string.IsNullOrEmpty(ArtifactBlueprint.Preview)) ? ModelComponent.GetAssetPath(ArtifactBlueprint.Preview) : ModelComponent.GetPreviewAssetPath(ArtifactBlueprint.DefaultLook));
		KSingleton<AssetBundleManager>.Instance().RequestAsset(GetAssetDirectory(assetPath), typeof(GameObject), delegate(Object asset)
		{
			if (asset == (Object)null)
			{
				LoadDefaultPreview();
			}
			else
			{
				OnPreviewLoaded(asset);
			}
		});
	}

	private void InitBuildgrid()
	{
		if ((Object)(object)_buildGrid == (Object)null)
		{
			GameObject val = BuildingPreview.AddChild(_buildGridPrefab);
			NGUITools.SetLayer(val, OverlayCamera.Layer);
			_buildGrid = val.GetComponent<BuildGrid>();
		}
		else
		{
			((Component)_buildGrid).gameObject.SetActive(true);
		}
		_buildGrid.Init(Size);
	}

	private void LoadDefaultPreview()
	{
		KSingleton<AssetBundleManager>.Instance().RequestAsset("Models/Prop/system/preview_shovel/preview_shovel.prefab", typeof(GameObject), delegate(Object defaultAsset)
		{
			if (!(defaultAsset == (Object)null))
			{
				OnPreviewLoaded(defaultAsset);
			}
		});
	}

	private void OnPreviewLoaded(Object asset)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = (_previewAsset = (GameObject)Object.Instantiate(asset));
		Quaternion localRotation = val.transform.localRotation;
		val.transform.parent = BuildingPreview.transform;
		val.transform.localPosition = Vector3.zero;
		val.transform.localRotation = localRotation;
		NGUITools.SetLayer(val, OverlayCamera.Layer);
		UpdateTransform();
		((MonoBehaviour)this).StartCoroutine(CoPreviewArtifact());
	}

	private IEnumerator CoPreviewArtifact()
	{
		if ((Object)(object)_previewAsset == (Object)null)
		{
			yield break;
		}
		while (IsPreviewVisible)
		{
			float sinFactor0 = _previewMaxAlpha - _previewMinAlpha;
			float alpha = Mathf.Sin(Time.time * 5f) * sinFactor0 + sinFactor0 / 2f + _previewMinAlpha;
			if (!Object.op_Implicit((Object)(object)_previewAsset))
			{
				break;
			}
			try
			{
				MeshRenderer[] meshes = _previewAsset.GetComponentsInChildren<MeshRenderer>();
				for (int i = 0; i < meshes.Length; i++)
				{
					for (int j = 0; j < ((Renderer)meshes[i]).materials.Length; j++)
					{
						Material material = ((Renderer)meshes[i]).materials[j];
						Color color = _gridStateColors.Get(CurrentGridMinState).PreviewColor;
						material.SetColor("_Color", color);
						material.SetFloat("_Alpha", alpha);
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError((object)("Preview artifact material: " + e));
				break;
			}
			yield return null;
		}
	}

	public void RotatePreview()
	{
		if (IsPreviewVisible)
		{
			Point2 centerTile = GetCenterTile();
			Rotated = !Rotated;
			Point2 centerTile2 = GetCenterTile();
			WorldTilePos = WorldTilePos + centerTile - centerTile2;
			UpdateTransform();
		}
	}

	[ExposedInEditor(null)]
	private void EnableGridView(int gridSize = 0)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		KSingleton<PlayerController>.Instance().IsTouchProcessed += PlayerController_IsTouchProcessed;
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		currentPosition.x = (float)Mathf.FloorToInt(currentPosition.x / 200f) * 200f;
		currentPosition.y = 0f;
		currentPosition.z = (float)Mathf.FloorToInt(currentPosition.z / 200f) * 200f;
		Vector3 position = TerrainA6.ClientPositionToWorldPosition(currentPosition);
		Vector2 worldTile = TerrainA6.WorldPositionToTilePosition(position);
		KSingleton<SelectAreaUI>.Instance().ShowGrids(worldTile, GetTileStateColor, gridSize);
	}

	private Color GetTileStateColor(Point2 tile)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		BuildGridState tileBuildState = GetTileBuildState(tile);
		return _gridStateColors.Get(tileBuildState).GridColor;
	}

	private BuildGridState GetTileBuildState(Point2 tile)
	{
		Blueprint artifactBlueprint = ArtifactBlueprint;
		if (artifactBlueprint == null)
		{
			return BuildGridState.Invalid;
		}
		TileObject tileObject = TerrainA6.GetTileObject(tile, warning: false);
		if (tileObject == null)
		{
			return BuildGridState.Invalid;
		}
		if (KSingleton<GameManager>.Instance().Region.Role() == Role.Tutorial)
		{
			return BuildGridState.Invalid;
		}
		if ((artifactBlueprint.IsEstateFlag || artifactBlueprint.IsClanEstateFlag) && tileObject.EstateId != 0L)
		{
			return BuildGridState.Invalid;
		}
		byte maskedBiome = TerrainA6.TilePositionToRawBiome(tile);
		if (TerrainA6.IsCollidableMasked(maskedBiome) || TerrainA6.IsNotPlantableMasked(maskedBiome))
		{
			return BuildGridState.Invalid;
		}
		TerrainData.Biome unmaskedBiome = TerrainA6.GetUnmaskedBiome(maskedBiome);
		float minBuildableDepth = artifactBlueprint.MinBuildableDepth;
		float maxBuildableDepth = artifactBlueprint.MaxBuildableDepth;
		bool flag = artifactBlueprint.BuildableBiomes != null;
		bool flag2 = minBuildableDepth > 0f || maxBuildableDepth < 1f;
		if (flag || flag2)
		{
			if (flag && Array.IndexOf(artifactBlueprint.BuildableBiomes, unmaskedBiome) == -1)
			{
				return BuildGridState.Invalid;
			}
			if (flag2)
			{
				float tileMinDepth = TerrainA6.GetTileMinDepth(tile);
				if (tileMinDepth < minBuildableDepth || tileMinDepth > maxBuildableDepth)
				{
					return BuildGridState.Invalid;
				}
			}
		}
		else if (TerrainA6.IsWater(unmaskedBiome))
		{
			return BuildGridState.Invalid;
		}
		if (artifactBlueprint.IsClanEstateFlag && ClanSystem.HasClanEstate() && !ClanSystem.IsClanExtensibleTile(tile))
		{
			return BuildGridState.Invalid;
		}
		if (artifactBlueprint.Exterior && (Object)(object)tileObject.Artifact == (Object)null && tileObject.TileType == TileObject.Type.Empty)
		{
			if (tileObject.EstateId != 0L)
			{
				EstateInfo estateInfo = GameSystem<EstateSystem>.Instance().GetEstateInfo(tileObject.EstateId);
				if (estateInfo != null)
				{
					ulong playerId = GameManager.PlayerId;
					ulong clanId = PlayerBehavior.LocalPlayer.ClanId;
					switch (estateInfo.OwnerType)
					{
					case OwnerType.Player:
						if (estateInfo.Owner == playerId)
						{
							return BuildGridState.Estate;
						}
						break;
					case OwnerType.ClanEstate:
						if (estateInfo.Owner == clanId)
						{
							return BuildGridState.Estate;
						}
						break;
					}
				}
			}
			return BuildGridState.Vaild;
		}
		if (artifactBlueprint.Interior)
		{
			ModularArtifact modularArtifact = ((!((Object)(object)tileObject.Artifact == (Object)null)) ? tileObject.Artifact.GetArtifactComponent<ModularArtifact>() : null);
			if (modularArtifact != null)
			{
				Point2 pos = modularArtifact.Artifact.WorldTile - tile;
				if (modularArtifact.Artifact.BuildCompleted && (Object)(object)modularArtifact.GetInterior(pos) == (Object)null)
				{
					return BuildGridState.Vaild;
				}
			}
		}
		return BuildGridState.Invalid;
	}

	private void DisableGridView()
	{
		KSingleton<SelectAreaUI>.Instance().Hide();
		KSingleton<PlayerController>.Instance().IsTouchProcessed -= PlayerController_IsTouchProcessed;
	}

	private void PlayerController_IsTouchProcessed(List<PlayerController.TouchEvent> eventList, ref bool result)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		if (!IsPreviewVisible)
		{
			return;
		}
		PlayerController.TouchEvent touchEvent = null;
		int num = 0;
		int count = eventList.Count;
		for (int i = 0; i < count; i++)
		{
			PlayerController.TouchEvent touchEvent2 = eventList[i];
			if (!(Math.Abs(touchEvent2.LastActivateTime - Time.timeSinceLevelLoad) > float.Epsilon) && !touchEvent2.IsNguiTouched && touchEvent2.Used != PlayerController.TouchEvent.UsedBy.Gesture)
			{
				touchEvent = touchEvent2;
				num++;
			}
		}
		if (touchEvent != null && num < 2)
		{
			Vector3 val = Vector2.op_Implicit(touchEvent.CurrentPos);
			if (!(val.x < 0f) && !(val.y < 0f) && !(val.x >= (float)Screen.width) && !(val.y >= (float)Screen.height))
			{
				Vector3 clientPosition = MainCamera.ScreenPosToWorldPos(Input.mousePosition);
				clientPosition = TerrainA6.ClientPositionToWorldPosition(clientPosition);
				Vector2 vec = TerrainA6.WorldPositionToTilePosition(clientPosition);
				WorldTilePos = new Point2(vec) - GetCenterTile();
				UpdateTransform();
				result = true;
			}
		}
	}

	private void UpdateTransform()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		Point2 size = Size;
		Point2 size2 = ((!Rotated) ? size : new Point2(size.y, size.x));
		Vector3 localEulerAngles = KUtility.DirectionToAngle(Direction);
		Vector3 val = new Vector3((float)size2.x, 0f, (float)size2.y) * 200f * 0.5f;
		UpdateBuildGrid(Rotated);
		Point2 tilePosition = new Point2(WorldTilePos.x, WorldTilePos.y);
		GetAreaState(CorrectedWorldTilePos, size2, out var min, out var max);
		CurrentGridMinState = min;
		CurrentGridMaxState = max;
		BuildingPreview.transform.localEulerAngles = Vector3.zero;
		BuildingPreview.transform.localPosition = TerrainA6.WorldPositionToClientPosition(TerrainA6.TilePositionToWorldPosition(tilePosition));
		if (Object.op_Implicit((Object)(object)_previewAsset))
		{
			_previewAsset.transform.localEulerAngles = localEulerAngles;
			_previewAsset.transform.localPosition = val + new Vector3(0f, 1f, 0f);
		}
		if (this.PreviewPositionUpdated != null)
		{
			Vector3 localPosition = BuildingPreview.transform.localPosition;
			this.PreviewPositionUpdated(localPosition, size2);
		}
	}

	private void GetAreaState(Point2 tile, Point2 size, out BuildGridState min, out BuildGridState max)
	{
		min = BuildGridState.Estate;
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

	private void UpdateBuildGrid(bool rotated)
	{
		if (!((Object)(object)_buildGrid == (Object)null))
		{
			_buildGrid.UpdateGrids(rotated);
		}
	}

	private Point2 GetCenterTile()
	{
		return new Point2((int)((float)((!Rotated) ? Size.x : Size.y) * 0.5f - 0.5f), (int)((float)((!Rotated) ? Size.y : Size.x) * 0.5f - 0.5f));
	}

	public static int GetBlueprintSlotCountModifier(BlueprintSlot slot, Point2 size)
	{
		if (slot.SizeFactor == null)
		{
			return 1;
		}
		Expression sizeFactor = slot.SizeFactor;
		sizeFactor.Parameters["x"] = size.x;
		sizeFactor.Parameters["y"] = size.y;
		try
		{
			return (int)sizeFactor.Evaluate();
		}
		catch
		{
			return 1;
		}
	}

	public static string GetAssetDirectory(string resource)
	{
		return "Models/" + resource;
	}
}
