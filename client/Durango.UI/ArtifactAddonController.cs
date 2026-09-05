using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Render.Camera;
using Durango.Render.Particle;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Etc;
using UnityEngine;

namespace Durango.UI;

public class ArtifactAddonController : MonoBehaviour
{
	private const string PreviewKey = "preview";

	private const string ReplacementKey = "replacement";

	[SerializeField]
	private GameObject _touchBox;

	[SerializeField]
	private ParticleType _placeParticle;

	[SerializeField]
	private SoundEventType _placeSound;

	[SerializeField]
	private Material _previewMaterial;

	private ModularArtifact _modular;

	private readonly ModularAddons _modifyAddons = new ModularAddons();

	private ModelComponent _preview;

	private bool _needPreviewUpdate;

	private int _prevIndex;

	private ModelComponent.IModel _addonPreview = ModelComponent.InvalidModel;

	private ModelComponent.IModel _wallPreview = ModelComponent.InvalidModel;

	private ModelComponent.IModel _replacement = ModelComponent.InvalidModel;

	private readonly List<ModelComponent.IModel> _originWalls = new List<ModelComponent.IModel>();

	private readonly List<ModelComponent.IModel> _selectedPlaceWalls = new List<ModelComponent.IModel>();

	private Vector3 _unselectBeginWorldPos;

	private Vector3 _unselectEndUIPos;

	private float _unselectBeginTime;

	private float _unselectEndTime;

	private ItemData _unselectItem;

	private bool _isUnselectAnimation;

	[CanBeNull]
	private Artifact Artifact => (_modular == null) ? null : _modular.Artifact;

	private ModelComponent Preview
	{
		get
		{
			if (Artifact == null)
			{
				return null;
			}
			GameObject gameObject = Artifact.gameObject;
			if (_preview == null)
			{
				_preview = new ModelComponent(gameObject);
			}
			else if (_preview.Parent != gameObject)
			{
				_preview.Reset(gameObject);
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
			foreach (ModelComponent.IModel originWall in _originWalls)
			{
				originWall.SetActive(active: true);
			}
			_originWalls.Clear();
			if (_prevIndex == -1)
			{
				return;
			}
			ModularArtifact.WallIndexToPos(PrevIndex, _modular.Size, out var floor, out var tile, out var dir);
			_originWalls.AddRange(_modular.GetWallModels(floor, tile, dir));
			foreach (ModelComponent.IModel originWall2 in _originWalls)
			{
				originWall2.SetActive(active: false);
			}
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
		if (IsSelectAddon)
		{
			ModelComponent preview = Preview;
			if (preview != null)
			{
				Color color = Color.white;
				if (SelectIndex != -1)
				{
					float time = Time.time;
					float t = (Mathf.Cos(time % 1f * (float)Math.PI * 2f) + 1f) * 0.5f;
					color = Color.Lerp(Color.gray, Color.white, t);
				}
				preview.GetCategory("preview").SetColor(color);
			}
		}
		UnselectAnimationUpdate();
	}

	public void SetArtifact([NotNull] ModularArtifact modular)
	{
		_modular = modular;
		_modifyAddons.Set(modular.GetAddons());
		IsModified = false;
		BuildSystem.GetAddons(modular, OnAddons);
	}

	private int CurrentFloor()
	{
		// [แก้เอง] 31 ส.ค. 2026 — เดิมเรียก LocalPlayer.Floor ตรง ๆ
		// 🐛 OnTouchScreen ถูกผูกกับ UICamera ตั้งแต่ UI ถูกสร้าง = ทำงานตั้งแต่**ก่อนมีตัวละครในโลก**
		//    (หน้าไตเติ้ล/หน้าเลือกเซิร์ฟ) ตอนนั้น LocalPlayer ยังเป็น null ⇒ NRE **ทุกครั้งที่แตะจอ**
		//    ผลที่เห็น: กดอะไรก็ไม่ตอบสนอง เหมือนเกมค้าง (เจอจริง 28 NRE ใน log รอบเดียว)
		if (PlayerBehavior.LocalPlayer == null)
		{
			return 0;
		}
		int b = (byte)PlayerBehavior.LocalPlayer.Floor;
		int a = 0;
		Artifact artifact = Artifact;
		if (artifact != null)
		{
			a = artifact.Stories.Value.GetValueOrDefault() - 1;
			if (!artifact.HasRoof.Value.GetValueOrDefault(true))
			{
				a--;
			}
			a = Mathf.Max(0, a);
		}
		return Mathf.Min(a, b);
	}

	private void OnAddons(AddOns addons)
	{
		if (_modular == null || addons._AddOns == null)
		{
			return;
		}
		ModularAddons addons2 = _modular.GetAddons();
		foreach (KeyValuePair<int, Item> addOn in addons._AddOns)
		{
			ItemData item = new ItemData(addOn.Value);
			ModularAddon modularAddon = addons2.Get(addOn.Key);
			if (modularAddon != null)
			{
				modularAddon.Item = item;
			}
			ModularAddon modularAddon2 = _modifyAddons.Get(addOn.Key);
			if (modularAddon2 != null)
			{
				modularAddon2.Item = item;
			}
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

	public void UnselectAddon()
	{
		if (_isUnselectAnimation)
		{
			_unselectEndTime = Time.time;
			UnselectAnimationUpdate();
		}
		PrevIndex = -1;
		SelectIndex = -1;
		SelectedAddon = null;
		if (Preview != null)
		{
			Preview.Clear();
		}
		foreach (ModelComponent.IModel selectedPlaceWall in _selectedPlaceWalls)
		{
			selectedPlaceWall.SetActive(active: true);
		}
		_selectedPlaceWalls.Clear();
		_originWalls.Clear();
		_addonPreview = ModelComponent.InvalidModel;
		_wallPreview = ModelComponent.InvalidModel;
		_replacement = ModelComponent.InvalidModel;
		if (this.AddonSelected != null)
		{
			this.AddonSelected(null);
		}
	}

	public void UnselectAnimation(Vector3 uiPos)
	{
		if (!(Artifact == null))
		{
			_isUnselectAnimation = true;
			_unselectBeginWorldPos = ((!_addonPreview.IsNull()) ? _addonPreview.GetPosition() : _wallPreview.GetPosition()) + Artifact.transform.position;
			_unselectEndUIPos = uiPos;
			Vector3 unselectBeginWorldPos = _unselectBeginWorldPos;
			Vector3 vector = MainCamera.NGUIPosToWorldPos(_unselectEndUIPos);
			Vector3 vector2 = vector - unselectBeginWorldPos;
			_unselectBeginTime = Time.time;
			_unselectEndTime = _unselectBeginTime + vector2.magnitude / 3000f;
		}
	}

	private void UnselectAnimationUpdate()
	{
		if (_isUnselectAnimation && !(Artifact == null))
		{
			float time = Time.time;
			float num = _unselectEndTime - _unselectBeginTime;
			float num2 = time - _unselectBeginTime;
			float num3 = num2 / num;
			Vector3 unselectBeginWorldPos = _unselectBeginWorldPos;
			Vector3 vector = MainCamera.NGUIPosToWorldPos(_unselectEndUIPos);
			Vector3 vector2 = vector - unselectBeginWorldPos;
			Vector3 vector3 = new Vector3(0f - vector2.z, 0f, vector2.x).normalized;
			if (vector2.x < vector2.z)
			{
				vector3 = -vector3;
			}
			Vector3 position = Vector3.Lerp(unselectBeginWorldPos, vector, num3) - Artifact.transform.position;
			float num4 = num3 * 2f - 1f;
			num4 = 1f - num4 * num4;
			position += vector3 * num4 * vector2.magnitude * 0.1f;
			_addonPreview.SetPosition(position);
			_wallPreview.SetPosition(position);
			if (num3 >= 1f)
			{
				_isUnselectAnimation = false;
				OnRemovedAddon();
			}
		}
	}

	private void UpdateAddonPreview(ModularAddon addon)
	{
		_addonPreview = ModelComponent.InvalidModel;
		_wallPreview = ModelComponent.InvalidModel;
		_replacement = ModelComponent.InvalidModel;
		ModelComponent preview = Preview;
		if (!IsSelectAddon)
		{
			preview.Clear();
			return;
		}
		Material previewMaterial = _previewMaterial;
		bool flag = addon.IsEmptyDoor();
		string modelKey = ((!flag) ? addon.ModelKey : _modular.GetModel("wall"));
		_addonPreview = preview.Load("preview_addon", modelKey, null, "preview").SetMaterial(previewMaterial).SetActive(active: false);
		string modelKey2 = ((!flag) ? _modular.GetModel("wall") : null);
		string modelPostfix = ((!flag) ? addon.GetWallPostfix() : null);
		_wallPreview = preview.Load("preview_wall", modelKey2, modelPostfix, "preview").SetMaterial(previewMaterial).SetActive(active: false);
		if (PrevIndex == -1)
		{
			preview.GetCategory("replacement").Clear();
			return;
		}
		ModularArtifact.GetWallPosition(PrevIndex, _modular.Size, out var pos, out var angle);
		_replacement = preview.Load("wall", modelKey2, null, "replacement").SetMaterial(previewMaterial).SetActive(active: false)
			.SetPosition(pos)
			.SetAngle(angle);
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
		if (_needPreviewUpdate)
		{
			UpdateAddonPreview(SelectedAddon);
			_needPreviewUpdate = false;
		}
		if (SelectIndex != -1 && SelectIndex == wallIndex)
		{
			return;
		}
		foreach (ModelComponent.IModel selectedPlaceWall in _selectedPlaceWalls)
		{
			selectedPlaceWall.SetActive(active: true);
		}
		_selectedPlaceWalls.Clear();
		SelectIndex = wallIndex;
		if (wallIndex == -1)
		{
			AttachPreveiwToPoint();
			return;
		}
		ModularArtifact.WallIndexToPos(wallIndex, _modular.Size, out var floor, out var tile, out var dir);
		if (wallIndex != PrevIndex)
		{
			_selectedPlaceWalls.AddRange(_modular.GetWallModels(floor, tile, dir));
		}
		_addonPreview.SetActive(!SelectedAddon.IsEmptyDoor());
		_wallPreview.SetActive(active: true);
		ModularArtifact.GetWallPosition(floor, tile, dir, out var pos, out var angle);
		_addonPreview.SetPosition(pos).SetAngle(SelectedAddon.GetAngle(dir));
		_wallPreview.SetPosition(pos).SetAngle(angle);
		foreach (ModelComponent.IModel selectedPlaceWall2 in _selectedPlaceWalls)
		{
			selectedPlaceWall2.SetActive(active: false);
		}
		if (PrevIndex != -1)
		{
			if (PrevIndex == wallIndex)
			{
				_replacement.SetActive(active: false);
			}
			else
			{
				_replacement.SetActive(_originWalls.Count > 0);
			}
		}
		else
		{
			_replacement.SetActive(active: false);
		}
	}

	private void AttachPreveiwToPoint()
	{
		if (!(Artifact == null))
		{
			UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
			Vector3 position = MainCamera.ScreenPosToWorldPos(currentTouch.pos) - Artifact.transform.position;
			_selectedPlaceWalls.Clear();
			_addonPreview.SetActive(active: true);
			_wallPreview.SetActive(_addonPreview.IsNull());
			_replacement.SetActive(active: true);
			_addonPreview.SetPosition(position).SetAngle(Vector3.zero);
			_wallPreview.SetPosition(position).SetAngle(Vector3.zero);
		}
	}

	private void OnDragScreen(GameObject obj, Vector2 delta)
	{
		if (IsSelectAddon)
		{
			int wallIndex = -1;
			if (GetTouchedArtifactWall(out var tile, out var dir))
			{
				int num = CurrentFloor();
				wallIndex = _modular.WallPosToIndex(tile, dir) + num * (_modular.Size.x + _modular.Size.y) * 2;
			}
			AttachPreveiwToWall(wallIndex);
		}
		else if (obj == _touchBox)
		{
			UIManager.SetCurrentUITouchEvent(enable: false);
		}
	}

	private void OnTouchScreen(GameObject obj, bool press)
	{
		if (press)
		{
			int num = CurrentFloor();
			if (GetTouchedArtifactWall(out var tile, out var dir))
			{
				int wallIndex = _modular.WallPosToIndex(tile, dir) + num * (_modular.Size.x + _modular.Size.y) * 2;
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
		if (_modular == null || Artifact == null)
		{
			tile = -Point2.one;
			dir = Direction.Invalid;
			return false;
		}
		UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
		Ray ray = Singleton<MainCamera>.Instance().ScreenPointToRay(currentTouch.pos);
		Vector3 position = Artifact.transform.position;
		Vector2 vector = _modular.Size.ToVector2() * 200f;
		KeyValuePair<Plane, Direction>[] array = new KeyValuePair<Plane, Direction>[4]
		{
			new KeyValuePair<Plane, Direction>(new Plane(Vector3.right, position), Direction.SouthWest),
			new KeyValuePair<Plane, Direction>(new Plane(Vector3.forward, position + Vector3.right * vector.x), Direction.SouthEast),
			new KeyValuePair<Plane, Direction>(new Plane(Vector3.left, position + Vector3.right * vector.x + Vector3.forward * vector.y), Direction.NorthEast),
			new KeyValuePair<Plane, Direction>(new Plane(Vector3.back, position + Vector3.forward * vector.y), Direction.NorthWest)
		};
		int num = CurrentFloor();
		float num2 = num * 200;
		float num3 = num2 + 200f;
		tile = -Point2.one;
		dir = Direction.Invalid;
		float num4 = float.MaxValue;
		int i = 0;
		for (int num5 = array.Length; i < num5; i++)
		{
			if (!array[i].Key.Raycast(ray, out var enter))
			{
				continue;
			}
			Vector3 vector2 = ray.origin + ray.direction * enter;
			if (vector2.y < num2 || vector2.y >= num3)
			{
				continue;
			}
			float num6 = Mathf.Abs(vector2.y - Mathf.Lerp(num2, num3, 0.5f));
			if (!(num6 < num4))
			{
				continue;
			}
			vector2 -= position;
			Point2 point = new Point2(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.z));
			if (point.x < 0 || (float)point.x > vector.x || point.y < 0 || (float)point.y > vector.y)
			{
				continue;
			}
			tile = point / 200;
			dir = array[i].Value;
			if (dir != Direction.NorthWest)
			{
				if (dir == Direction.NorthEast)
				{
					tile.x--;
				}
			}
			else
			{
				tile.y--;
			}
			num4 = num6;
		}
		return dir != Direction.Invalid;
	}

	private void ConfirmAddon()
	{
		ModularAddon modularAddon = null;
		if (PrevIndex == -1)
		{
			modularAddon = _modifyAddons.Set(SelectIndex, SelectedAddon);
		}
		else
		{
			_modifyAddons.Move(PrevIndex, SelectIndex);
		}
		_modular.UpdateWalls(_modifyAddons);
		if (PrevIndex != SelectIndex)
		{
			Vector3 vector = ((!_addonPreview.IsNull()) ? _addonPreview.GetPosition() : _wallPreview.GetPosition());
			Vector3 euler = ((!_addonPreview.IsNull()) ? _addonPreview.GetAngle() : _wallPreview.GetAngle());
			if (Artifact != null)
			{
				vector += Artifact.transform.position;
			}
			ParticleManager.Emit(_placeParticle.Path, vector, Quaternion.Euler(euler));
			SoundManager.PlayEvent(_placeSound, SoundPosition.Fix(vector));
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
				foreach (ModelComponent.IModel selectedPlaceWall in _selectedPlaceWalls)
				{
					selectedPlaceWall.SetActive(active: true);
				}
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
		(Preview?.GetCategory("replacement"))?.Clear();
		if (index != -1)
		{
			_modifyAddons.Remove(index);
			_modular.UpdateWalls(_modifyAddons);
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
}
