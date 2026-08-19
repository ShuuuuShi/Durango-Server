using System;
using InteractionData;
using ItemSystem;
using L10N;
using Shared.Economy;
using UnityEngine;

public class PackArtifactGroup : UIBase
{
	[SerializeField]
	private SizeSelector _packBoxSizeSelector;

	[SerializeField]
	private Selectable _closeButton;

	[SerializeField]
	private PackedArtifactList _packedList;

	private void Awake()
	{
		((Component)_packBoxSizeSelector).gameObject.SetActive(false);
		OnClose();
	}

	private void Start()
	{
		Selectable closeButton = _closeButton;
		closeButton.Clicked = (Action)Delegate.Combine(closeButton.Clicked, new Action(Close));
		_packedList.Closed += Close;
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.StartPakingArtifact, ArtifactPackingAction);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.PackingArtifact, ArtifactPackingAction);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.UnpackArtifact, ArtifactPackingAction);
	}

	protected override bool OnOpen()
	{
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if ((Object)(object)localPlayer == (Object)null)
		{
			return false;
		}
		TileObject currentTileObject = localPlayer.CurrentTileObject;
		if (currentTileObject.EstateId == 0L || currentTileObject.OwnerId != localPlayer.EntityId)
		{
			UIManager.SystemMsg(T._("내 사유지가 아닙니다"));
			return false;
		}
		return base.OnOpen();
	}

	private void OnBoxSizeChange(int size)
	{
		UIManager.MessageBox.SetButtonText(0, Inventory.CurrencyFormat(size * size * 10, Currency.TStone));
	}

	private void ArtifactPackingAction(InteractionObject o)
	{
		ItemData package = PackArtifactSystem.GetPackage();
		if (package == null)
		{
			ShowBoxSizeSelector();
		}
		else
		{
			Open();
		}
	}

	private void ShowBoxSizeSelector()
	{
		((Component)_packBoxSizeSelector).gameObject.SetActive(true);
		_packBoxSizeSelector.ValueChanged = OnBoxSizeChange;
		UIManager.MessageBox.Show(T._("이사에 사용할 상자의 크기를 선택하세요"), ((Component)_packBoxSizeSelector).GetComponent<UIWidget>(), OnSelectBoxSize, string.Empty, T._("취소"));
		_packBoxSizeSelector.Set(0, 10, 10, 1000);
	}

	private void OnSelectBoxSize(int index)
	{
		if (index == 0)
		{
			PackArtifactSystem.StartPack(_packBoxSizeSelector.Value, Open);
		}
	}
}
