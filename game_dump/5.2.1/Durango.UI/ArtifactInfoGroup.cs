using System;
using Durango.Logic.Clusters;
using Durango.Logic.Estate;
using Durango.Logic.Item;
using Durango.UI.InGame;
using Durango.UI.Popup;
using Durango.Utils;
using InteractionData;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ArtifactInfoGroup : UIBase
{
	[Serializable]
	private struct Background
	{
		public UIWidget Widget;

		public int SizeOffset;
	}

	private enum Phase
	{
		Main,
		ManageRights,
		InventoryAccess,
		StatsInfo
	}

	private class PhaseChange
	{
		public bool On;

		public UIWidget Prev;

		public UIWidget Next;

		public float Since;

		public float Until;

		public void Reset()
		{
			Prev = null;
			Next = null;
			Since = 0f;
			Until = 0f;
		}
	}

	[SerializeField]
	private UIWidget _container;

	[SerializeField]
	private ArtifactInfoMainWidget _artifactInfoMainWidget;

	[SerializeField]
	private ArtifactInfoManageRights _manageRights;

	[SerializeField]
	private ArtifactInventoryAccessWidget _inventoryAccess;

	[SerializeField]
	private ArtifactStatsInfo _statsInfo;

	[SerializeField]
	private Background[] _backgrounds;

	private Phase _phase;

	private readonly PhaseChange _phaseChange = new PhaseChange();

	private Artifact _artifact;

	private bool _isDirty;

	protected override bool IsSoundOcclusion => false;

	private void Start()
	{
		_manageRights.Closed = delegate
		{
			SetPhase(Phase.Main, instant: false);
		};
		ArtifactInventoryAccessWidget inventoryAccess = _inventoryAccess;
		inventoryAccess.Closed = (Action)Delegate.Combine(inventoryAccess.Closed, (Action)delegate
		{
			SetPhase(Phase.ManageRights, instant: false);
		});
		_statsInfo.Closed = delegate
		{
			SetPhase(Phase.Main, instant: false);
		};
		ArtifactInfoManageRights manageRights = _manageRights;
		manageRights.InventoryAccessEdited = (Action<string, int, Action<int>>)Delegate.Combine(manageRights.InventoryAccessEdited, (Action<string, int, Action<int>>)delegate(string text, int accessCount, Action<int> onChanged)
		{
			_inventoryAccess.Set(text, accessCount, onChanged);
			SetPhase(Phase.InventoryAccess, instant: false);
		});
		base.OnOpenSucceed += OpenSucceed;
		base.OnCloseSucceed += CloseSucceed;
		if (GameManager.ClusterMode == Mode.Online)
		{
			GameSystem<InteractionSystem>.Instance().InteractionTargetSelected += InteractionSystem_InteractionTargetSelected;
			GameSystem<InteractionSystem>.Instance().PostTouched += OnPostTouched;
		}
		_artifactInfoMainWidget.ManageButtonClicked += ArtifactInfoMainWidget_ManageButtonClicked;
		_artifactInfoMainWidget.LayoutUpdated += ArtifactInfoMainWidget_LayoutUpdated;
		_artifactInfoMainWidget.ArtifactStatsInfoClicked += ArtifactInfoMainWidget_ArtifactStatsInfoClicked;
		GameSystem<InventorySystem>.Instance().TrackingInventoryUpdated += delegate
		{
			if (!(_artifact == null))
			{
				Durango.Logic.Item.Inventory trackingInventory = GameSystem<InventorySystem>.Instance().TrackingInventory;
				if (trackingInventory.Type == Durango.Logic.Item.Inventory.InventoryType.Warehouse || !(trackingInventory.OwnerId != _artifact.EntityId))
				{
					SetDirty();
				}
			}
		};
		SetChildrenActive(activated: false);
	}

	private void Update()
	{
		if (base.IsOpened)
		{
			if (_isDirty)
			{
				Refresh();
			}
			if (_phaseChange.On)
			{
				PhaseChangeImpl();
			}
			if (_phase == Phase.Main)
			{
				_artifactInfoMainWidget.UpdateScrollOffset();
			}
		}
	}

	private void OpenSucceed()
	{
		SetPhase(Phase.Main, instant: true);
	}

	private void CloseSucceed()
	{
		_inventoryAccess.InvokeChanged();
		CheckRightsChanged();
		_phaseChange.On = false;
		_phaseChange.Reset();
		if (_artifact != null)
		{
			Singleton<GridAreaViewer>.Instance().Hide();
		}
	}

	private void SetPhase(Phase phase, bool instant)
	{
		if (_phase != phase)
		{
			Phase phase2 = _phase;
			_phase = phase;
			if (instant)
			{
				_phaseChange.On = false;
			}
			else
			{
				_phaseChange.On = true;
				_phaseChange.Reset();
				_phaseChange.Since = Time.time;
				_phaseChange.Until = Time.time + 0.3f;
			}
			_phaseChange.Prev = GetPhaseWidget(phase2);
			_phaseChange.Next = GetPhaseWidget(phase);
			RefreshLayout();
			OnPhaseClosed(phase2);
		}
	}

	private void OnPhaseClosed(Phase phase)
	{
		switch (phase)
		{
		case Phase.ManageRights:
			if (_phase != Phase.InventoryAccess)
			{
				CheckRightsChanged();
			}
			break;
		case Phase.InventoryAccess:
			_inventoryAccess.InvokeChanged();
			if (_phase != Phase.ManageRights)
			{
				CheckRightsChanged();
			}
			break;
		}
	}

	private UIWidget GetPhaseWidget(Phase phase)
	{
		return phase switch
		{
			Phase.Main => _artifactInfoMainWidget.Widget, 
			Phase.ManageRights => _manageRights, 
			Phase.InventoryAccess => _inventoryAccess, 
			Phase.StatsInfo => _statsInfo, 
			_ => null, 
		};
	}

	private void SetManageRights()
	{
		if (_artifact != null)
		{
			EstateInfo estateInfo = _artifact.GetEstateInfo();
			if (estateInfo != null && _artifactInfoMainWidget.Access.HasValue && EstateSystem.IsAdmin(estateInfo))
			{
				_manageRights.Set(_artifactInfoMainWidget.Access.Value, estateInfo);
			}
		}
	}

	private void CheckRightsChanged()
	{
		if (!_manageRights.TryGetChangedAccess(out var access))
		{
			return;
		}
		string entityId = _artifact.EntityId;
		EstateSystem.SetArtifactAccess(_artifact, access, delegate(bool result)
		{
			if (result && !(entityId != _artifact.EntityId))
			{
				_artifactInfoMainWidget.SetArtifactAccess(access);
				SetDirty();
			}
		});
	}

	private void Show(Artifact artifact)
	{
		SetPhase(Phase.Main, instant: true);
		_artifact = artifact;
		_artifactInfoMainWidget.SetArtifact(artifact);
		_artifactInfoMainWidget.SetArtifactAccess(artifact.ArtifactState.Access);
		Open();
		Refresh();
	}

	private void SetDirty()
	{
		_isDirty = true;
	}

	private void Refresh()
	{
		_isDirty = false;
		_artifactInfoMainWidget.Refresh();
		SetManageRights();
		RefreshLayout();
		ShowGridArea();
	}

	private void ShowGridArea()
	{
		if (_artifact == null)
		{
			return;
		}
		if (_artifact.ArtifactState.Sprinkler.HasValue)
		{
			SimpleTileEdgeArea simpleTileEdgeArea = new SimpleTileEdgeArea
			{
				Tile = _artifact.WorldTile,
				Offsets = _artifact.Blueprint.GetEffectTiles(_artifact.Rotation),
				TileColorFunc = delegate(Point2 tile, out Color color)
				{
					color = Sprinklable.WateringTileColor;
					return true;
				}
			};
			Singleton<GridAreaViewer>.Instance().Show(new GridAreaBase[1] { simpleTileEdgeArea }, GridAreaViewer.LayerType.Upper);
		}
		else
		{
			Singleton<GridAreaViewer>.Instance().Hide();
		}
	}

	private void PhaseChangeImpl()
	{
		int num = -1;
		float num2 = 0f;
		if (_phaseChange.On)
		{
			float num3 = (Time.time - _phaseChange.Since) / (_phaseChange.Until - _phaseChange.Since);
			num = (int)(num3 * 3f);
			num2 = num3 * 3f % 1f;
		}
		int num4;
		switch (num)
		{
		case 0:
			_phaseChange.Prev.alpha = 1f - num2;
			_phaseChange.Next.alpha = 0f;
			num4 = _phaseChange.Prev.height;
			break;
		case 1:
			_phaseChange.Prev.alpha = 0f;
			_phaseChange.Next.alpha = 0f;
			num4 = (int)Mathf.Lerp(_phaseChange.Prev.height, _container.height, num2);
			break;
		case 2:
			_phaseChange.Prev.alpha = 0f;
			_phaseChange.Next.alpha = num2;
			num4 = _container.height;
			break;
		default:
			if ((bool)_phaseChange.Prev)
			{
				_phaseChange.Prev.alpha = 0f;
			}
			if ((bool)_phaseChange.Next)
			{
				_phaseChange.Next.alpha = 1f;
			}
			num4 = _container.height;
			_phaseChange.On = false;
			break;
		}
		for (int i = 0; i < _backgrounds.Length; i++)
		{
			_backgrounds[i].Widget.height = num4 + _backgrounds[i].SizeOffset;
		}
	}

	public void RefreshLayout(bool keepScrollOffset = false)
	{
		switch (_phase)
		{
		case Phase.Main:
			_artifactInfoMainWidget.Widget.alpha = 1f;
			_manageRights.alpha = 0f;
			_statsInfo.alpha = 0f;
			_inventoryAccess.alpha = 0f;
			_container.height = _artifactInfoMainWidget.UpdateHeight(keepScrollOffset);
			_artifactInfoMainWidget.Widget.SetPosition(_container.localCenter, 0.5f, 0.5f);
			break;
		case Phase.ManageRights:
			_artifactInfoMainWidget.Widget.alpha = 0f;
			_manageRights.alpha = 1f;
			_statsInfo.alpha = 0f;
			_inventoryAccess.alpha = 0f;
			_container.height = _manageRights.height;
			_manageRights.SetPosition(_container.localCenter, 0.5f, 0.5f);
			break;
		case Phase.StatsInfo:
			_artifactInfoMainWidget.Widget.alpha = 0f;
			_manageRights.alpha = 0f;
			_statsInfo.alpha = 1f;
			_inventoryAccess.alpha = 0f;
			_container.height = _statsInfo.height;
			_statsInfo.SetPosition(_container.localCenter, 0.5f, 0.5f);
			break;
		case Phase.InventoryAccess:
			_artifactInfoMainWidget.Widget.alpha = 0f;
			_manageRights.alpha = 0f;
			_statsInfo.alpha = 0f;
			_inventoryAccess.alpha = 1f;
			_container.height = _inventoryAccess.height;
			_inventoryAccess.SetPosition(_container.localCenter, 0.5f, 0.5f);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		UIWidget rootAnchor = UIRootAnchor.GetRootAnchor(AnchorType.Base);
		if (base.IsPortrait)
		{
			Vector3 position = rootAnchor.GetPosition(0f, 1f);
			position.x += 10f;
			position.y -= 120f;
			_container.SetPosition(position, 0f, 1f);
		}
		else
		{
			Vector3 position2 = rootAnchor.GetPosition(0f, 0.5f);
			position2.x += 10f;
			_container.SetPosition(position2, 0f, 0.5f);
		}
		PhaseChangeImpl();
		UIUtility.UpdateAnchors(base.transform);
		_artifactInfoMainWidget.UpdateLayout(keepScrollOffset);
	}

	private void OnPostTouched(InteractionMenuList menuList, InteractionObject obj)
	{
		if (_artifact == null || obj == null || _artifact.EntityId != obj.EntityId)
		{
			return;
		}
		foreach (InteractionMenuData menu in menuList)
		{
			_artifactInfoMainWidget.Interactions[menu.Action] = menu;
		}
		SetDirty();
	}

	private void InteractionSystem_InteractionTargetSelected(InteractionObject obj)
	{
		if (GameManager.Region.IsPvpIsland())
		{
			return;
		}
		Artifact artifact = obj?.GetTargetComponent<Artifact>();
		if (artifact == null)
		{
			if (base.IsOpened)
			{
				Close();
			}
		}
		else
		{
			Show(artifact);
		}
	}

	private void ArtifactInfoMainWidget_ManageButtonClicked()
	{
		SetPhase(Phase.ManageRights, instant: false);
	}

	private void ArtifactInfoMainWidget_LayoutUpdated(bool keepScrollOffset)
	{
		RefreshLayout(keepScrollOffset);
	}

	private void ArtifactInfoMainWidget_ArtifactStatsInfoClicked(ArtifactInfoMainWidget.StatsType type)
	{
		Artifact artifact = _artifact;
		if (artifact == null)
		{
			return;
		}
		string title;
		string text;
		switch (type)
		{
		case ArtifactInfoMainWidget.StatsType.Comfort:
			title = T._("안락함");
			text = T._("조립식 건축물 실내에 해당 가구를 배치하면 휴식시 피로도 추가 감소 효과를 얻을 수 있습니다.");
			_statsInfo.Set(artifact, title, T._("총 안락함"), (ArtifactStats stats) => stats.Comfort.Factor, (ArtifactStats stats) => stats.Comfort.Complexity);
			break;
		case ArtifactInfoMainWidget.StatsType.Antibacterial:
			title = T._("항균력");
			text = T._("조립식 건축물 실내에 해당 가구를 배치하면 휴식시 건강 추가 회복 효과를 얻을 수 있습니다.");
			_statsInfo.Set(artifact, title, T._("총 항균력"), (ArtifactStats stats) => stats.Antibacterial.Factor, (ArtifactStats stats) => stats.Antibacterial.Complexity);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (artifact.IsEnterable)
		{
			SetPhase(Phase.StatsInfo, instant: false);
			return;
		}
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(title, text, 400);
		widgetTooltipControl.Show();
	}
}
