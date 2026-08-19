using System;
using Durango.Network;
using Durango.Utils;
using Messages;
using UnityEngine;

public class SleepChecker : MonoBehaviour
{
	[Flags]
	public enum WhyCannotSleep
	{
		None = 0,
		Loading = 1,
		Move = 4,
		Acceleraction = 8,
		UIOpen = 0x10,
		DoSomething = 0x20,
		Battle = 0x40,
		GuideOfK = 0x80
	}

	[SerializeField]
	private float _sleepCheckTime = 60f;

	private float _sleepTimer;

	private WhyCannotSleep _whyCannotSleep;

	private bool _isSleep;

	private void Start()
	{
		if (GameManager.IsPrologueMode)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		PlayerController playerController = Singleton<PlayerController>.Instance();
		playerController.MoveStarted += OnMoveStarted;
		playerController.MoveEnded += OnMoveEnded;
		AccelerationChecker accelerationChecker = Singleton<AccelerationChecker>.Instance();
		if (accelerationChecker != null)
		{
			accelerationChecker.BrokenEquilibrium += OnBrokenEquilibrium;
			accelerationChecker.ComebackEquilibrium += OnComebackEquilibrium;
		}
		UIBase.UIOpened += OnUIOpened;
		UIBase.UIClosed += OnUIClosed;
		GameSystem<TimerSystem>.Instance().StartSubjectProgress += OnStartSubjectProgress;
		GameSystem<TimerSystem>.Instance().FinishedSubjectProgress += OnFinishedSubjectProgress;
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += CombatSystemOnChangedCombatMode;
		UIManager.OnLoadingCurtainHidden(LoadingFinished);
		if (GameSystem<PlayGuideSystem>.HasInstance())
		{
			GameSystem<PlayGuideSystem>.Instance().Command.GuideOfKBegin += PlayGuideSystem_GuideOfKBegin;
			GameSystem<PlayGuideSystem>.Instance().Command.GuideOfKEnd += PlayGuideSystem_GuideOfKEnd;
		}
	}

	private void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
	}

	private void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
	}

	private void Update()
	{
		if (!_isSleep && _whyCannotSleep == WhyCannotSleep.None && GameSystem<InputSystem>.Instance().Touch.TouchCount() == 0)
		{
			if (_sleepTimer < _sleepCheckTime)
			{
				_sleepTimer += Time.deltaTime;
			}
			else
			{
				Sleep();
			}
		}
	}

	private void OnTouch(GameObject obj, bool touch)
	{
		if (_isSleep)
		{
			InputTouch.TouchEvent currentTouchEvent = GameSystem<InputSystem>.Instance().Touch.CurrentTouchEvent;
			if (currentTouchEvent != null && currentTouchEvent.Used == InputTouch.TouchEvent.UsedBy.None)
			{
				currentTouchEvent.Used = InputTouch.TouchEvent.UsedBy.Joystick;
			}
		}
		ResumeTimer();
	}

	private void CombatSystemOnChangedCombatMode(bool isCombat)
	{
		if (isCombat)
		{
			StopTimer(WhyCannotSleep.Battle);
		}
		else
		{
			ResumeTimer(WhyCannotSleep.Battle);
		}
	}

	private void OnFinishedSubjectProgress(string subject, bool isInterrupt)
	{
		ResumeTimer(WhyCannotSleep.DoSomething);
	}

	private void OnStartSubjectProgress(string subject)
	{
		StopTimer(WhyCannotSleep.DoSomething);
	}

	private void OnUIClosed()
	{
		if (!UIBase.HasOpenedUI)
		{
			ResumeTimer(WhyCannotSleep.UIOpen);
		}
	}

	private void OnUIOpened()
	{
		StopTimer(WhyCannotSleep.UIOpen);
	}

	private void LoadingFinished()
	{
		ResumeTimer(WhyCannotSleep.Loading);
	}

	private void OnComebackEquilibrium()
	{
		ResumeTimer(WhyCannotSleep.Acceleraction);
	}

	private void OnBrokenEquilibrium()
	{
		StopTimer(WhyCannotSleep.Acceleraction);
	}

	private void OnMoveEnded()
	{
		ResumeTimer(WhyCannotSleep.Move);
	}

	private void OnMoveStarted()
	{
		StopTimer(WhyCannotSleep.Move);
	}

	private void PlayGuideSystem_GuideOfKBegin()
	{
		StopTimer(WhyCannotSleep.GuideOfK);
	}

	private void PlayGuideSystem_GuideOfKEnd()
	{
		ResumeTimer(WhyCannotSleep.GuideOfK);
	}

	private void WakeUp()
	{
		_isSleep = false;
		Singleton<PlayerController>.Instance().Sleep(sleep: false);
		Connections.Frontend.Send(new ToggleStatusEffect
		{
			Id = "away_from_keyboard",
			Toggle = false
		});
	}

	private void Sleep()
	{
		_isSleep = true;
		Singleton<PlayerController>.Instance().Sleep(sleep: true);
		Connections.Frontend.Send(new ToggleStatusEffect
		{
			Id = "away_from_keyboard",
			Toggle = true
		});
	}

	private void StopTimer(WhyCannotSleep reason)
	{
		ResumeTimer();
		_whyCannotSleep |= reason;
	}

	private void ResumeTimer(WhyCannotSleep reason = WhyCannotSleep.None)
	{
		if (_isSleep)
		{
			WakeUp();
		}
		_whyCannotSleep &= ~reason;
		_sleepTimer = 0f;
	}
}
