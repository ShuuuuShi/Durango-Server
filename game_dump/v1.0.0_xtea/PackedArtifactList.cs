using System;
using Building_;
using InteractionData;
using ItemSystem;
using L10N;
using Shared.Item;
using TimerData;
using UnityEngine;

public class PackedArtifactList : MonoBehaviour
{
	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private KScrollView _list;

	[SerializeField]
	private UISprite _packageIcon;

	[SerializeField]
	private UILabel _sizeLabel;

	[SerializeField]
	private DefaultSelectableButton _confirmButton;

	[SerializeField]
	private ParticleType _packParticle;

	[SerializeField]
	private AudioClipType _packAudio;

	private ItemData _package;

	private bool _enableFlag;

	public event Action Closed;

	private void Awake()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.PackArtifact, OnPackArtifact);
		DefaultSelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(OnConfirm));
		_list.Nodes.Init(delegate(GameObject o)
		{
			Selectable component = o.GetComponent<Selectable>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnSelectItem));
		});
	}

	private void OnEnable()
	{
		_enableFlag = true;
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnUpdateInventory;
		GameSystem<InteractionSystem>.Instance().PreTouchTarget += OnPreTouchTarget;
		if ((Object)(object)PlayerBehavior.LocalPlayer != (Object)null)
		{
			PlayerBehavior.LocalPlayer.TileChanged += OnPlayerTileChanged;
		}
		OnUpdateInventory();
		GameSystem<InventorySystem>.Instance().PlayerInventory.UpdateIfNeeded();
		UIBase.HideUI(UIBase.UIFlag.Base, hide: true, "PackArtifact");
		UIManager.FindScript<InteractionGroup>().SetVisible(visible: true, "PackArtifact");
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdateInventory;
		GameSystem<InteractionSystem>.Instance().PreTouchTarget -= OnPreTouchTarget;
		if ((Object)(object)PlayerBehavior.LocalPlayer != (Object)null)
		{
			PlayerBehavior.LocalPlayer.TileChanged -= OnPlayerTileChanged;
		}
		_list.ResetPosition();
		UIBase.HideUI(UIBase.UIFlag.Base, hide: false, "PackArtifact");
	}

	private void OnUpdateInventory()
	{
		_package = PackArtifactSystem.GetPackage();
		Refresh();
	}

	private void Refresh()
	{
		ArtifactPackage artifactPackage = ((_package != null) ? _package.ArtifactPackage : null);
		int num = ((artifactPackage != null) ? KUtility.GetSize(artifactPackage.Artifacts) : 0);
		PackageStatus packageStatus = artifactPackage?.Status ?? PackageStatus.Invalid;
		if (!_enableFlag && packageStatus != 0 && num == 0)
		{
			Close();
			return;
		}
		_enableFlag = false;
		ListObjectPool nodes = _list.Nodes;
		nodes.Set(num);
		for (int i = 0; i < num; i++)
		{
			PackedArtifactItem component = nodes[i].GetComponent<PackedArtifactItem>();
			component.Set(artifactPackage.Artifacts[i]);
			component.Disable = packageStatus == PackageStatus.Packing;
			component.Select = false;
		}
		_list.Reposition();
		_sizeLabel.text = ((artifactPackage != null) ? $"{num} / {artifactPackage.Size}" : string.Empty);
		int num2 = (int)((float)_titleLabel.Label.fontSize * 0.8f);
		switch (packageStatus)
		{
		case PackageStatus.Packing:
			_titleLabel.text = string.Format("{0}\n[size={2}][icon=icon_make_alert] {1}[/size]", T._("짐 쌀 가구를 선택하세요"), T._("한 번 상자에 넣은 아이템은 <alert>포장을 풀기 전까지 꺼낼 수 없으니</alert> 신중하게 넣어주세요!"), num2);
			_confirmButton.Text = T._("확인");
			_packageIcon.spriteName = "relocate_box_close";
			break;
		case PackageStatus.Sealed:
		case PackageStatus.Unpacking:
			_titleLabel.text = string.Format("{0}\n[size={2}][icon=icon_make_alert] {1}[/size]", T._("가구 배치 모드"), T._("한 번 배치한 아이템은 <alert>다시 이동이 불가능</alert>합니다"), num2);
			_confirmButton.Text = T._("이삿짐 풀기");
			_packageIcon.spriteName = "relocate_box_open";
			break;
		}
	}

	private void OnPreTouchTarget(InteractionObject obj, ref bool result)
	{
		Artifact targetComponent = obj.GetTargetComponent<Artifact>();
		if ((Object)(object)targetComponent == (Object)null)
		{
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		}
		else
		{
			InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
			menuList.Reset();
			menuList.Add(new InteractionMenuData(Interaction.PackArtifact));
			menuList.Apply();
		}
		result = true;
	}

	private void OnConfirm()
	{
		if (_package != null)
		{
			switch (_package.ArtifactPackage.Status)
			{
			case PackageStatus.Packing:
				PackingFinish();
				break;
			case PackageStatus.Sealed:
			case PackageStatus.Unpacking:
				UnpackSelectedArtifact();
				break;
			}
		}
	}

	private void OnPackArtifact(InteractionObject obj)
	{
		Artifact artifact = obj.GetTargetComponent<Artifact>();
		if ((Object)(object)artifact == (Object)null)
		{
			return;
		}
		MotionMap.Instance().GetBuildMotion("Capsulate", null, out var motion, out var equip);
		Timer timer = TimerSystem.SetGaugeAndPlayMotion(3f, artifact.Blueprint.Icon, motion, equip);
		timer.Finished += delegate(Timer t)
		{
			if (!t.IsInterrupt)
			{
				PackArtifactSystem.PackArtifact(artifact, delegate
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_000b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0022: Unknown result type (might be due to invalid IL or missing references)
					//IL_0027: Unknown result type (might be due to invalid IL or missing references)
					//IL_0046: Unknown result type (might be due to invalid IL or missing references)
					Vector3 center = artifact.Center;
					ParticleManager.Emit(_packParticle.Path, artifact.Center, Quaternion.identity);
					SoundManager.Play(_packAudio.Path, center);
					KSingleton<StaticObjectManager>.Instance().RemoveImmovable(artifact.WorldTile, artifact.EntityId, -1.0);
				});
			}
		};
	}

	private void PackingFinish()
	{
		if (_package == null || KUtility.GetSize(_package.ArtifactPackage.Artifacts) == 0)
		{
			UIManager.SystemMsg(T._("빈 상자로는 완료할 수 없습니다"));
			return;
		}
		UIManager.MessageBox.Show(T._("완료하면 물건 꺼내기만 가능합니다\n정말 완료하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				PackArtifactSystem.FinishPacking();
				Close();
			}
		});
	}

	private void UnpackSelectedArtifact()
	{
		int index = -1;
		for (int i = 0; i < _list.Nodes.Count; i++)
		{
			Selectable component = _list.Nodes[i].GetComponent<Selectable>();
			if (component.Select)
			{
				index = i;
				break;
			}
		}
		if (index == -1)
		{
			return;
		}
		Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(_package.ArtifactPackage.Artifacts[index].BlueprintId);
		if (blueprint != null)
		{
			BuildGridGroup buildGridGroup = UIManager.FindScript<BuildGridGroup>();
			buildGridGroup.Open(blueprint, delegate
			{
				BuildManager buildManager = KSingleton<BuildManager>.Instance();
				PackArtifactSystem.UnpackArtifact(_package.ArtifactPackage.Artifacts[index], buildManager.WorldTilePos, buildManager.Rotated);
			});
		}
	}

	private void OnSelectItem()
	{
		int num = _list.Nodes.IndexOf(((Component)Selectable.Current).gameObject);
		for (int i = 0; i < _list.Nodes.Count; i++)
		{
			Selectable component = _list.Nodes[i].GetComponent<Selectable>();
			component.Select = i == num;
		}
		if (num != -1)
		{
			ArtifactInfoTooltip.Show(UIManager.Popup.Tooltip<InfoTooltip>(), _package.ArtifactPackage.Artifacts[num]);
		}
	}

	private void OnPlayerTileChanged(Point2 prev, Point2 current)
	{
		TileObject tileObject = TerrainA6.GetTileObject(prev, warning: false);
		TileObject tileObject2 = TerrainA6.GetTileObject(current, warning: false);
		ulong num = tileObject?.EstateId ?? 0;
		ulong num2 = tileObject2?.EstateId ?? 0;
		if (num2 == 0L || num != num2)
		{
			Close();
		}
	}

	private void Close()
	{
		if (this.Closed != null)
		{
			this.Closed();
		}
	}
}
