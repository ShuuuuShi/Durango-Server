using L10N;
using Shared.System;
using UnityEngine;

public class TimelineLogGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private TimelineLogContainer _container;

	private void Awake()
	{
		_container.Init();
		OnClose();
	}

	private void Start()
	{
		_titleWidget.OnClose += base.ForceClose;
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.GetTimeline, delegate(InteractionObject target)
		{
			if (InteractionSystem.CurrentMenu.Disabled)
			{
				UIManager.SystemMsg(T._("이력을 볼 수 있는 권한이 없습니다."));
			}
			else
			{
				Artifact targetComponent = target.GetTargetComponent<Artifact>();
				if ((Object)(object)targetComponent != (Object)null)
				{
					_container.SetTimeline(targetComponent.EntityId, TimelineLogSystem.TimelineType.Entity);
					Open();
				}
			}
		});
	}
}
