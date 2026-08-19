using System;
using System.Collections.Generic;
using ItemSystem;
using JetBrains.Annotations;
using Messages;
using Shared.Etc;
using UnityEngine;

public class ArtifactAddonController : MonoBehaviour
{
	private const string PreveiwKey = "Preview";

	private const string ReplacementKey = "Replacement";

	[SerializeField]
	private GameObject _touchBox;

	[SerializeField]
	private ParticleType _placeParticle;

	[SerializeField]
	private AudioClipType _placeSound;

	private ModularArtifact _artifact;

	private ModularAddons _modifyAddons = new ModularAddons();

	private ModelComponent _preview;

	private bool _needPreviewUpdate;

	private int _prevIndex;

	private ModelComponent.IModel _addonPreview = ModelComponent.InvalidModel;

	private ModelComponent.IModel _wallPreview = ModelComponent.InvalidModel;

	private ModelComponent.IModel _replacement = ModelComponent.InvalidModel;

	private ModelComponent.IModel _originWall = ModelComponent.InvalidModel;

	private ModelComponent.IModel _originAddon = ModelComponent.InvalidModel;

	private ModelComponent.IModel _selectedPlaceWall = ModelComponent.InvalidModel;

	private ModelComponent.IModel _selectedPlaceAddon = ModelComponent.InvalidModel;

	private float _selectedAt;

	private Vector3 _unselectBeginWorldPos;

	private Vector3 _unselectEndUIPos;

	private float _unselectBeginTime;

	private float _unselectEndTime;

	private ItemData _unselectItem;

	private bool _isUnselectAnimation;

	private ModelComponent Preview
	{
		get
		{
			if (_artifact == null)
			{
				return null;
			}
			if (_preview == null)
			{
				_preview = new ModelComponent(((Component)_artifact.Artifact).gameObject);
				_preview.ModelLoaded += OnLoadedPreviewModel;
			}
			else if ((Object)(object)_preview.Parent != (Object)(object)((Component)_artifact.Artifact).gameObject)
			{
				_preview.Reset(((Component)_artifact.Artifact).gameObject);
			}
			return _preview;
		}
	}

	public bool IsSelectAddon => SelectedAddon != null;

	public ModularAddon SelectedAddon { get; private set; }

	public int PrevIndex
	{
		get
		{
			return _prevIndex;
		}
		private set
		{
			_prevIndex = value;
			_originWall.SetActive(active: true);
			_originAddon.SetActive(active: true);
			if (_prevIndex == -1)
			{
				_originWall = ModelComponent.InvalidModel;
				_originAddon = ModelComponent.InvalidModel;
				return;
			}
			ModelComponent category = _artifact.Models.GetCategory("Wall");
			_artifact.WallIndexToPos(PrevIndex, out var tile, out var dir);
			string wallPosKey = ModularArtifact.GetWallPosKey(tile, dir);
			string category2 = null;
			switch (dir)
			{
			case Direction.SouthWest:
			case Direction.SouthEast:
				category2 = "South";
				break;
			case Direction.NorthWest:
			case Direction.NorthEast:
				category2 = "North";
				break;
			}
			_originWall = category.GetModel(category2, wallPosKey);
			_originAddon = category.GetModel("Addon", wallPosKey);
			_originWall.SetActive(active: false);
			_originAddon.SetActive(active: false);
		}
	}

	public int SelectIndex { get; private set; }

	public ModularAddons ModifyAddons => _modifyAddons;

	public bool IsModified { get; private set; }

	public event Action<ModularAddon> AddonSelected;

	public event Action<int, int> AddonMoved;

	public event Action<ItemData> AddonPlaced;

	public event Action<ItemData> OnAddonRemove;

	public event Action<ItemData> AddonRemoved;

	private void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnTouchScreen));
		UICamera.onDrag = (UICamera.VectorDelegate)Delegate.Combine(UICamera.onDrag, new UICamera.VectorDelegate(OnDragScreen));
	}

	private void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnTouchScreen));
		UICamera.onDrag = (UICamera.VectorDelegate)Delegate.Remove(UICamera.onDrag, new UICamera.VectorDelegate(OnDragScreen));
	}

	private void Update()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (IsSelectAddon && _preview != null)
		{
			Color color = Color.white;
			if (SelectIndex != -1)
			{
				float num = Time.time - _selectedAt;
				float num2 = (Mathf.Cos(num % 1f * (float)Math.PI * 2f) + 1f) * 0.5f;
				color = Color.Lerp(PresetColor.UILightGreen, Color.white, num2);
			}
			_preview.GetCategory("Preview").SetColor(color);
		}
		UnselectAnimationUpdate();
	}

	public void SetArtifact([NotNull] ModularArtifact artifact)
	{
		_artifact = artifact;
		_modifyAddons.Set(artifact.GetAddons());
		IsModified = false;
		BuildSystem.GetAddons(artifact, OnAddons);
	}

	private void OnAddons(AddOns addons)
	{
		ModularAddons addons2 = _artifact.GetAddons();
		foreach (KeyValuePair<int, Item> addOn in addons._AddOns)
		{
			ItemData item = new ItemData(addOn.Value);
			ModularAddon modularAddon = addons2.Get(addOn.Key);
			modularAddon.Item = item;
			ModularAddon modularAddon2 = _modifyAddons.Get(addOn.Key);
			modularAddon2.Item = item;
		}
	}

	public void SelectAddon(ModularAddon addon)
	{
		if (!IsSelectAddon)
		{
			SelectModularAddon(addon);
			AttachPreveiwToWall(-1);
		}
	}

	private void SelectModularAddon(ModularAddon addon)
	{
		UnselectAddon();
		SelectedAddon = addon;
		_needPreviewUpdate = true;
		if (this.AddonSelected != null)
		{
			this.AddonSelected(SelectedAddon);
		}
	}

	private void UnselectAddon()
	{
		if (_isUnselectAnimation)
		{
			_unselectEndTime = Time.time;
			UnselectAnimationUpdate();
		}
		PrevIndex = -1;
		SelectIndex = -1;
		SelectedAddon = null;
		Preview.Clear();
		_selectedPlaceWall.SetActive(active: true);
		_selectedPlaceAddon.SetActive(active: true);
		_addonPreview = ModelComponent.InvalidModel;
		_wallPreview = ModelComponent.InvalidModel;
		_replacement = ModelComponent.InvalidModel;
		_originWall = ModelComponent.InvalidModel;
		_originAddon = ModelComponent.InvalidModel;
		_selectedPlaceWall = ModelComponent.InvalidModel;
		_selectedPlaceAddon = ModelComponent.InvalidModel;
		if (this.AddonSelected != null)
		{
			this.AddonSelected(null);
		}
	}

	public void UnselectAnimation(Vector3 uiPos)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		_isUnselectAnimation = true;
		_unselectBeginWorldPos = ((!_addonPreview.IsNull()) ? _addonPreview.GetPosition() : _wallPreview.GetPosition()) + ((Component)_artifact.Artifact).transform.position;
		_unselectEndUIPos = uiPos;
		Vector3 unselectBeginWorldPos = _unselectBeginWorldPos;
		Vector3 val = MainCamera.NGUIPosToWorldPos(_unselectEndUIPos);
		Vector3 val2 = val - unselectBeginWorldPos;
		_unselectBeginTime = Time.time;
		_unselectEndTime = _unselectBeginTime + ((Vector3)(ref val2)).magnitude / 3000f;
	}

	private void UnselectAnimationUpdate()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (_isUnselectAnimation)
		{
			float time = Time.time;
			float num = _unselectEndTime - _unselectBeginTime;
			float num2 = time - _unselectBeginTime;
			float num3 = num2 / num;
			Vector3 unselectBeginWorldPos = _unselectBeginWorldPos;
			Vector3 val = MainCamera.NGUIPosToWorldPos(_unselectEndUIPos);
			Vector3 val2 = val - unselectBeginWorldPos;
			Vector3 val3 = default(Vector3);
			((Vector3)(ref val3))._002Ector(0f - val2.z, 0f, val2.x);
			Vector3 val4 = ((Vector3)(ref val3)).normalized;
			if (val2.x < val2.z)
			{
				val4 = -val4;
			}
			Vector3 val5 = Vector3.Lerp(unselectBeginWorldPos, val, num3) - ((Component)_artifact.Artifact).transform.position;
			float num4 = num3 * 2f - 1f;
			num4 = 1f - num4 * num4;
			val5 += val4 * num4 * ((Vector3)(ref val2)).magnitude * 0.1f;
			_addonPreview.SetPosition(val5);
			_wallPreview.SetPosition(val5);
			if (num3 >= 1f)
			{
				_isUnselectAnimation = false;
				OnRemovedAddon();
			}
		}
	}

	private void UpdateAddonPreview(ModularAddon addon)
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		_addonPreview = ModelComponent.InvalidModel;
		_wallPreview = ModelComponent.InvalidModel;
		_replacement = ModelComponent.InvalidModel;
		if (!IsSelectAddon)
		{
			Preview.Clear();
			return;
		}
		_addonPreview = Preview.Load("Preview", addon.ModelKey, null, "Addon").SetActive(active: false);
		string wallModel = _artifact.WallModel;
		string wallPostfix = addon.GetWallPostfix();
		_wallPreview = Preview.Load("Preview", wallModel, wallPostfix, "Wall").SetActive(active: false);
		if (PrevIndex == -1)
		{
			Preview.GetCategory("Replacement").Clear();
		}
		else
		{
			_replacement = Preview.Load("Replacement", wallModel, null, "Wall").SetActive(active: false).SetPosition(_originWall.GetPosition())
				.SetAngle(_originWall.GetAngle());
		}
	}

	private void SelectWallAddon(int wallIndex)
	{
		if (!IsSelectAddon)
		{
			ModularAddon modularAddon = _modifyAddons.Get(wallIndex);
			if (modularAddon != null)
			{
				SelectModularAddon(modularAddon);
				PrevIndex = wallIndex;
				AttachPreveiwToWall(wallIndex);
			}
		}
	}

	private void AttachPreveiwToWall(int wallIndex)
	{
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		if (_needPreviewUpdate)
		{
			UpdateAddonPreview(SelectedAddon);
			_needPreviewUpdate = false;
		}
		if (SelectIndex != -1 && SelectIndex == wallIndex)
		{
			return;
		}
		_selectedPlaceWall.SetActive(active: true);
		_selectedPlaceAddon.SetActive(active: true);
		SelectIndex = wallIndex;
		if (wallIndex == -1)
		{
			AttachPreveiwToPoint();
			return;
		}
		_artifact.WallIndexToPos(wallIndex, out var tile, out var dir);
		_selectedAt = Time.time;
		ModelComponent category = _artifact.Models.GetCategory("Wall");
		string wallPosKey = ModularArtifact.GetWallPosKey(tile, dir);
		string category2 = null;
		switch (dir)
		{
		case Direction.SouthWest:
		case Direction.SouthEast:
			category2 = "South";
			break;
		case Direction.NorthWest:
		case Direction.NorthEast:
			category2 = "North";
			break;
		}
		_selectedPlaceWall = category.GetModel(category2, wallPosKey);
		_selectedPlaceAddon = category.GetModel("Addon", wallPosKey);
		if (_selectedPlaceWall.IsNull())
		{
			AttachPreveiwToPoint();
		}
		else
		{
			_addonPreview.SetActive(active: true);
			_wallPreview.SetActive(active: true);
			Vector3 position = _selectedPlaceWall.GetPosition();
			Vector3 angle = _selectedPlaceWall.GetAngle();
			_addonPreview.SetPosition(position).SetAngle(angle);
			_wallPreview.SetPosition(position).SetAngle(angle);
			_selectedPlaceWall.SetActive(active: false);
			_selectedPlaceAddon.SetActive(active: false);
			if (PrevIndex != -1)
			{
				if (PrevIndex == wallIndex)
				{
					_replacement.SetActive(active: false);
				}
				else
				{
					_replacement.SetActive(!_originWall.IsNull());
				}
			}
			else
			{
				_replacement.SetActive(active: false);
			}
		}
		if (SelectIndex == PrevIndex)
		{
			_selectedPlaceWall = ModelComponent.InvalidModel;
			_selectedPlaceAddon = ModelComponent.InvalidModel;
		}
	}

	private void AttachPreveiwToPoint()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
		Vector3 position = MainCamera.ScreenPosToWorldPos(Vector2.op_Implicit(currentTouch.pos)) - ((Component)_artifact.Artifact).transform.position;
		_selectedPlaceWall = ModelComponent.InvalidModel;
		_selectedPlaceAddon = ModelComponent.InvalidModel;
		_addonPreview.SetActive(active: true);
		_wallPreview.SetActive(_addonPreview.IsNull());
		_replacement.SetActive(active: true);
		_addonPreview.SetPosition(position).SetAngle(Vector3.zero);
		_wallPreview.SetPosition(position).SetAngle(Vector3.zero);
	}

	private void OnDragScreen(GameObject obj, Vector2 delta)
	{
		if (IsSelectAddon)
		{
			int wallIndex = -1;
			if (GetTouchedArtifactWall(out var tile, out var dir))
			{
				wallIndex = _artifact.WallPosToIndex(tile, dir);
			}
			AttachPreveiwToWall(wallIndex);
		}
		else if ((Object)(object)obj == (Object)(object)_touchBox)
		{
			UIManager.SetCurrentUITouchEvent(enable: false);
		}
	}

	private void OnTouchScreen(GameObject obj, bool press)
	{
		if (press)
		{
			if (GetTouchedArtifactWall(out var tile, out var dir))
			{
				int wallIndex = _artifact.WallPosToIndex(tile, dir);
				SelectWallAddon(wallIndex);
			}
		}
		else if (IsSelectAddon)
		{
			if (SelectIndex == -1)
			{
				RemoveAddon(PrevIndex);
			}
			else
			{
				ConfirmAddon();
			}
		}
	}

	private bool GetTouchedArtifactWall(out Point2 tile, out Direction dir)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		if (_artifact == null)
		{
			tile = -Point2.one;
			dir = Direction.Invalid;
			return false;
		}
		UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
		Camera component = ((Component)KSingleton<MainCamera>.Instance()).GetComponent<Camera>();
		Ray val = component.ScreenPointToRay(Vector2.op_Implicit(currentTouch.pos));
		Vector3 position = ((Component)_artifact.Artifact).transform.position;
		Vector2 val2 = _artifact.Size.ToVector2() * 200f;
		KeyValuePair<Plane, Direction>[] array = new KeyValuePair<Plane, Direction>[4]
		{
			new KeyValuePair<Plane, Direction>(new Plane(Vector3.right, position), Direction.SouthWest),
			new KeyValuePair<Plane, Direction>(new Plane(Vector3.forward, position + Vector3.right * val2.x), Direction.SouthEast),
			new KeyValuePair<Plane, Direction>(new Plane(Vector3.left, position + Vector3.right * val2.x + Vector3.forward * val2.y), Direction.NorthEast),
			new KeyValuePair<Plane, Direction>(new Plane(Vector3.back, position + Vector3.forward * val2.y), Direction.NorthWest)
		};
		tile = -Point2.one;
		dir = Direction.Invalid;
		float num = float.MaxValue;
		int i = 0;
		float num3 = default(float);
		for (int num2 = array.Length; i < num2; i++)
		{
			Plane key = array[i].Key;
			if (!((Plane)(ref key)).Raycast(val, ref num3))
			{
				continue;
			}
			Vector3 val3 = ((Ray)(ref val)).origin + ((Ray)(ref val)).direction * num3;
			if (val3.y < 0f || val3.y > 200f)
			{
				continue;
			}
			float num4 = Mathf.Abs(val3.y - 100f);
			if (!(num4 < num))
			{
				continue;
			}
			val3 -= position;
			Point2 point = new Point2(Mathf.RoundToInt(val3.x), Mathf.RoundToInt(val3.z));
			if (point.x >= 0 && !((float)point.x > val2.x) && point.y >= 0 && !((float)point.y > val2.y))
			{
				tile = point / 200;
				dir = array[i].Value;
				switch (dir)
				{
				case Direction.NorthWest:
					tile.y--;
					break;
				case Direction.NorthEast:
					tile.x--;
					break;
				}
				num = num4;
			}
		}
		return dir != Direction.Invalid;
	}

	private void ConfirmAddon()
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		ModularAddon modularAddon = null;
		if (PrevIndex == -1)
		{
			modularAddon = _modifyAddons.Set(SelectIndex, SelectedAddon);
		}
		else
		{
			_modifyAddons.Move(PrevIndex, SelectIndex);
		}
		_artifact.UpdateWalls(_artifact.WallModel, _modifyAddons);
		if (PrevIndex != SelectIndex)
		{
			Vector3 val = ((!_addonPreview.IsNull()) ? _addonPreview.GetPosition() : _wallPreview.GetPosition());
			Vector3 val2 = ((!_addonPreview.IsNull()) ? _addonPreview.GetAngle() : _wallPreview.GetAngle());
			val += ((Component)_artifact.Artifact).transform.position;
			ParticleManager.Emit(_placeParticle.Path, val, Quaternion.Euler(val2));
			SoundManager.Play(_placeSound.Path, val);
			IsModified = true;
		}
		if (PrevIndex == -1)
		{
			if (this.AddonPlaced != null)
			{
				this.AddonPlaced(SelectedAddon.Item);
			}
			if (modularAddon != null)
			{
				_selectedPlaceAddon.SetActive(active: true);
				_selectedPlaceWall.SetActive(active: true);
				Vector3 position = ((!_addonPreview.IsNull()) ? _addonPreview.GetPosition() : _wallPreview.GetPosition());
				Vector3 angle = ((!_addonPreview.IsNull()) ? _addonPreview.GetAngle() : _wallPreview.GetAngle());
				UpdateAddonPreview(modularAddon);
				_unselectItem = modularAddon.Item;
				_addonPreview.SetPosition(position).SetAngle(angle).SetActive(active: true);
				_wallPreview.SetPosition(position).SetAngle(angle).SetActive(active: true);
				if (this.OnAddonRemove != null)
				{
					this.OnAddonRemove(modularAddon.Item);
				}
			}
			else
			{
				UnselectAddon();
			}
		}
		else
		{
			if (PrevIndex != SelectIndex && this.AddonMoved != null)
			{
				this.AddonMoved(PrevIndex, SelectIndex);
			}
			UnselectAddon();
		}
	}

	private ItemData GetModifyAddonItem(int index)
	{
		if (index == -1)
		{
			return (SelectedAddon != null) ? SelectedAddon.Item : null;
		}
		return _modifyAddons.Get(index)?.Item;
	}

	private void RemoveAddon(int index)
	{
		ItemData itemData = (_unselectItem = GetModifyAddonItem(index));
		Preview.GetCategory("Replacement").Clear();
		if (index != -1)
		{
			_modifyAddons.Remove(index);
			_artifact.UpdateWalls(_artifact.WallModel, _modifyAddons);
			IsModified = true;
		}
		if (itemData != null && this.OnAddonRemove != null)
		{
			this.OnAddonRemove(itemData);
		}
	}

	private void OnRemovedAddon()
	{
		ItemData unselectItem = _unselectItem;
		_unselectItem = null;
		if (unselectItem != null && this.AddonRemoved != null)
		{
			this.AddonRemoved(unselectItem);
		}
		UnselectAddon();
	}

	private void OnLoadedPreviewModel(ModelComponent.IModel model)
	{
		if (_artifact != null && !((Object)(object)model.GetObject() == (Object)null))
		{
			BuildingShadows component = ((Component)_artifact.Artifact).GetComponent<BuildingShadows>();
			component.MakeShadow(model.GetObject().GetComponentsInChildren<MeshRenderer>());
		}
	}
}
