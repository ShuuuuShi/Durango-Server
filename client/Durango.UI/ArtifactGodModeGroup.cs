using System;
using Durango.Logic.Item;
using Durango.Render.Camera;
using Durango.UI.InGame;
using Durango.Utils;
using InteractionData;
using UnityEngine;

namespace Durango.UI;

public class ArtifactGodModeGroup : UIBase
{
	private const int NearAreaTileSize = 2;

	private const float GodmodeZoom = 0.42f;

	[SerializeField]
	private GameObject _closeButton;

	[SerializeField]
	private ArtifactAddonController _artifactAddonController;

	[SerializeField]
	private ArtifactAddonSelector _artifactAddonSelector;

	[SerializeField]
	private UILabel _selectedAddonNameLabel;

	private ModularArtifact _artifact;

	private bool _isZoomOut;

	private float _prevZoom;

	protected override bool IsSoundOcclusion => false;

	private ModularArtifact Artifact
	{
		get
		{
			return _artifact;
		}
		set
		{
			if (_artifact != value)
			{
				if (_artifact != null)
				{
					_artifact.IsGodModeTarget = false;
				}
				_artifact = value;
				if (_artifact != null)
				{
					_artifact.IsGodModeTarget = true;
				}
			}
		}
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_closeButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			UIBase.CloseAllUI();
		});
		PlayerBehavior.LocalPlayer.TileChanged += OnPlayerTileChange;
		_artifactAddonController.AddonPlaced += OnAddonPlaced;
		_artifactAddonController.OnAddonRemove += OnPreAddonRemoved;
		_artifactAddonController.AddonRemoved += OnAddonRemoved;
		_artifactAddonController.AddonSelected += OnSelectModularAddon;
		ArtifactAddonSelector artifactAddonSelector = _artifactAddonSelector;
		artifactAddonSelector.AddonSelected = (Action<ModularAddon>)Delegate.Combine(artifactAddonSelector.AddonSelected, new Action<ModularAddon>(OnSelectInventoryAddon));
		ArtifactAddonSelector artifactAddonSelector2 = _artifactAddonSelector;
		artifactAddonSelector2.AddonItemTouched = (Action<ItemData, bool>)Delegate.Combine(artifactAddonSelector2.AddonItemTouched, new Action<ItemData, bool>(OnTouchAddonItem));
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.AddOnManage, delegate(InteractionObject o)
		{
			Open(o.GetTargetComponent<Artifact>());
		});
		SetChildrenActive(activated: false);
	}

	public void Open(Artifact artifact)
	{
		if (!(artifact == null))
		{
			Artifact = artifact.GetArtifactComponent<ModularArtifact>();
			Open();
		}
	}

	protected override bool TryOpen()
	{
		if (Artifact == null || !Artifact.HasWall)
		{
			Artifact = null;
			return false;
		}
		Rect rect = new Rect(Artifact.WorldTile.ToVector2() - Vector2.one * 2f, Artifact.Size.ToVector2() + Vector2.one * 2f * 2f);
		if (!rect.Contains(PlayerBehavior.LocalPlayer.CurrentTile.ToVector2()))
		{
			Artifact = null;
			return false;
		}
		RectGridArea rectGridArea = new RectGridArea();
		rectGridArea.Tile = new Point2(rect.position);
		rectGridArea.Size = new Point2(rect.size);
		rectGridArea.BgColor = PresetColor.UIYellow;
		RectGridArea rectGridArea2 = rectGridArea;
		Singleton<GridAreaViewer>.Instance().Show(new GridAreaBase[1] { rectGridArea2 });
		_artifactAddonController.SetArtifact(Artifact);
		global::Artifact.ArtifactDisplayChanged += OnChangeArtifactDisplay;
		OnChangeArtifactDisplay(Artifact.Artifact);
		_artifactAddonSelector.ResetAddonList();
		SetSelectedAddonName(null);
		SetZoomOutMode(enable: true);
		return base.TryOpen();
	}

	protected override bool TryClose()
	{
		global::Artifact.ArtifactDisplayChanged -= OnChangeArtifactDisplay;
		if (_artifactAddonController.IsModified)
		{
			BuildSystem.PlaceAddons(Artifact, _artifactAddonController.ModifyAddons);
		}
		else
		{
			Artifact.UpdateWalls(Artifact.GetAddons());
		}
		Artifact.Models.GetCategory("roof").SetActive(active: true);
		_artifactAddonController.UnselectAddon();
		Artifact = null;
		Singleton<GridAreaViewer>.Instance().Hide();
		SetZoomOutMode(enable: false);
		return base.TryClose();
	}

	private void OnPlayerTileChange(Point2 prev, Point2 current)
	{
		if (Artifact != null)
		{
			Point2 worldTile = Artifact.WorldTile;
			Point2 point = worldTile + Artifact.Size;
			if (current.x < worldTile.x - 2 || current.x > point.x + 2 || current.y < worldTile.y - 2 || current.y > point.y + 2)
			{
				UIBase.CloseAllUI();
			}
		}
	}

	private void SetZoomOutMode(bool enable)
	{
		if (_isZoomOut != enable)
		{
			_isZoomOut = enable;
			if (enable)
			{
				_prevZoom = Singleton<MainCamera>.Instance().Zoom;
				Singleton<CameraController>.Instance().ZoomRange(0.42f, 0.42f, 0.3f).Zoom(0.42f, 0.3f)
					.LockZoomControl(isLock: true);
			}
			else
			{
				Singleton<CameraController>.Instance().ZoomRange(0.42f, 2.2f, 0.3f).Zoom(_prevZoom, 0.3f)
					.LockZoomControl(isLock: false);
			}
		}
	}

	private void OnChangeArtifactDisplay(Artifact artifact)
	{
		if (Artifact != null && !(artifact != Artifact.Artifact))
		{
			Artifact.UpdateWalls(_artifactAddonController.ModifyAddons);
			Artifact.Models.GetCategory("roof").SetActive(active: false);
		}
	}

	private void OnAddonPlaced(ItemData item)
	{
		_artifactAddonSelector.PlacedAddon(item);
	}

	private void OnPreAddonRemoved(ItemData item)
	{
		Vector3 itemPosition = _artifactAddonSelector.GetItemPosition(item);
		_artifactAddonController.UnselectAnimation(itemPosition);
	}

	private void OnAddonRemoved(ItemData item)
	{
		_artifactAddonSelector.RemovedAddon(item);
	}

	private void OnSelectModularAddon(ModularAddon addon)
	{
		SetSelectedAddonName(addon?.Item);
	}

	private void OnSelectInventoryAddon(ModularAddon addon)
	{
		SetSelectedAddonName(addon.Item);
		_artifactAddonController.SelectAddon(addon);
	}

	private void OnTouchAddonItem(ItemData item, bool press)
	{
		SetSelectedAddonName((!press) ? null : item);
	}

	private void SetSelectedAddonName(ItemData item)
	{
		if (item == null)
		{
			_selectedAddonNameLabel.transform.parent.gameObject.SetActive(value: false);
			return;
		}
		_selectedAddonNameLabel.text = item.Name;
		_selectedAddonNameLabel.transform.parent.gameObject.SetActive(value: true);
	}
}
