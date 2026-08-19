using System;
using ItemSystem;
using Shared.System;
using UnityEngine;

public class ArtifactGodModeGroup : UIBase
{
	private const int NearAreaTileSize = 2;

	[SerializeField]
	private GameObject _closeButton;

	[SerializeField]
	private ArtifactAddonController _artifactAddonController;

	[SerializeField]
	private ArtifactAddonSelector _artifactAddonSelector;

	[SerializeField]
	private UILabel _selectedAddonNameLabel;

	private ModularArtifact _artifact;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_closeButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			ForceClose();
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
		base.OnClose();
	}

	public void Open(Artifact artifact)
	{
		if (!((Object)(object)artifact == (Object)null))
		{
			_artifact = artifact.GetArtifactComponent<ModularArtifact>();
			Open();
		}
	}

	protected override bool OnOpen()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if (_artifact == null || !_artifact.HasWall)
		{
			_artifact = null;
			return false;
		}
		Rect val = default(Rect);
		((Rect)(ref val))._002Ector(_artifact.WorldTile.ToVector2() - Vector2.one * 2f, _artifact.Size.ToVector2() + Vector2.one * 2f * 2f);
		if (!((Rect)(ref val)).Contains(PlayerBehavior.LocalPlayer.CurrentTile.ToVector2()))
		{
			_artifact = null;
			return false;
		}
		SelectAreaUI.AreaStruct areaStruct = default(SelectAreaUI.AreaStruct);
		areaStruct.Pos = new Point2(((Rect)(ref val)).position);
		areaStruct.Size = new Point2(((Rect)(ref val)).size);
		areaStruct.Color = PresetColor.UIYellow;
		SelectAreaUI.AreaStruct areaStruct2 = areaStruct;
		KSingleton<SelectAreaUI>.Instance().Show(new SelectAreaUI.AreaStruct[1] { areaStruct2 });
		_artifactAddonController.SetArtifact(_artifact);
		_artifact.Artifact.ArtifactDisplayUpdated += OnUpdateArtifactDisplay;
		OnUpdateArtifactDisplay();
		_artifactAddonSelector.ResetAddonList();
		SetSelectedAddonName(null);
		return base.OnOpen();
	}

	protected override bool OnClose()
	{
		_artifact.Artifact.ArtifactDisplayUpdated -= OnUpdateArtifactDisplay;
		if (_artifactAddonController.IsModified)
		{
			BuildSystem.PlaceAddons(_artifact, _artifactAddonController.ModifyAddons);
		}
		else
		{
			_artifact.UpdateWalls(_artifact.WallModel, _artifact.GetAddons());
		}
		_artifact.Models.GetCategory("Roof").SetActive(active: true);
		_artifact = null;
		KSingleton<SelectAreaUI>.Instance().Hide();
		return base.OnClose();
	}

	private void OnPlayerTileChange(Point2 prev, Point2 current)
	{
		if (_artifact != null)
		{
			Point2 worldTile = _artifact.WorldTile;
			Point2 point = worldTile + _artifact.Size;
			if (current.x < worldTile.x - 2 || current.x > point.x + 2 || current.y < worldTile.y - 2 || current.y > point.y + 2)
			{
				ForceClose();
			}
		}
	}

	private void OnUpdateArtifactDisplay()
	{
		_artifact.UpdateWalls(_artifact.WallModel, _artifactAddonController.ModifyAddons);
		_artifact.Models.GetCategory("Roof").SetActive(active: false);
	}

	private void OnAddonPlaced(ItemData item)
	{
		_artifactAddonSelector.PlacedAddon(item);
	}

	private void OnPreAddonRemoved(ItemData item)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
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
		_artifactAddonSelector.PlacedAddon(addon.Item);
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
			((Component)((Component)_selectedAddonNameLabel).transform.parent).gameObject.SetActive(false);
			return;
		}
		_selectedAddonNameLabel.text = item.Name;
		((Component)((Component)_selectedAddonNameLabel).transform.parent).gameObject.SetActive(true);
	}
}
