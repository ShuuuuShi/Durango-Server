using L10N;
using Messages;
using Player;
using Shared.Region;
using UnityEngine;

public class LeaveTutorialIslandGroup : UIBase
{
	[SerializeField]
	private GameObject _findPlayerButton;

	[SerializeField]
	private GameObject _exploreButton;

	[SerializeField]
	private GameObject _closeButton;

	private Artifact _portArtifact;

	private void Start()
	{
		UIEventListener.Get(_findPlayerButton).onClick = OnClickFindPlayerButton;
		UIEventListener.Get(_exploreButton).onClick = OnClickExploreButton;
		UIEventListener.Get(_closeButton).onClick = delegate
		{
			ForceClose();
		};
		GameSystem<TutorialIslandSystem>.Instance().ReadyToDepartBootcamp += OnReadyDepartTutorial;
		base.OnClose();
	}

	private void OnClickFindPlayerButton(GameObject obj)
	{
		UIManager.Popup.PlayerSearch.Show(PlayerSelected, T._("친구 캐릭터의 이름을 입력하세요"));
	}

	private void PlayerSelected(Player.PlayerInfo playerInfo)
	{
		if (playerInfo.ReturningRegion == null || playerInfo.ReturningRegion.Template == null || playerInfo.ReturningRegion.Template.role != Role.Safe)
		{
			UIManager.MessageBox.Show(T._("아직 정착한 곳이 없는 사람이라 따라갈 수 없습니다.\n다른 사람을 선택해주세요."));
		}
		else
		{
			GameSystem<TutorialIslandSystem>.Instance().DepartTutorialFor(playerInfo);
		}
	}

	private void OnClickExploreButton(GameObject obj)
	{
		Connections.Frontend.Send(new DepartTutorial
		{
			EntityId = _portArtifact.EntityId,
			Tile = _portArtifact.WorldTile
		});
	}

	private void OnReadyDepartTutorial(Artifact port)
	{
		_portArtifact = port;
		if (!base.IsOpen)
		{
			Open();
		}
	}
}
