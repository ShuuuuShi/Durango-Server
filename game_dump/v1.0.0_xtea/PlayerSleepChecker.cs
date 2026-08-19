using System;
using Messages;
using UnityEngine;

public class PlayerSleepChecker : MonoBehaviour
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
			Object.Destroy((Object)(object)this);
			return;
		}
		PlayerController playerController = KSingleton<PlayerController>.Instance();
		playerController.MoveStarted += OnMoveStarted;
		playerController.MoveEnded += OnMoveEnded;
		AccelerationChecker accelerationChecker = KSingleton<AccelerationChecker>.Instance();
		if ((Object)(object)accelerationChecker != (Object)null)
		{
			accelerationChecker.BrokenEquilibrium += OnBrokenEquilibrium;
			accelerationChecker.ComebackEquilibrium += OnComebackEquilibrium;
		}
		UIBase.OnOpenCloseableUI += OnOnOpenCloseableUI;
		UIBase.OnCloseCloseableUI += OnOnCloseCloseableUI;
		GameSystem<TimerSystem>.Instance().StartSubjectProgress += OnStartSubjectProgress;
		GameSystem<TimerSystem>.Instance().FinishedSubjectProgress += OnFinishedSubjectProgress;
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += CombatSystemOnChangedCombatMode;
		LoadingCurtainGroup loadingCurtainGroup = UIManager.FindScript<LoadingCurtainGroup>();
		if ((Object)(object)loadingCurtainGroup != (Object)null && ((Behaviour)loadingCurtainGroup).enabled)
		{
			StopTimer(WhyCannotSleep.Loading);
			EventDelegate.Add(loadingCurtainGroup.FadeOutFinished, LoadingFinished, oneShot: true);
		}
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
		if (!_isSleep && _whyCannotSleep == WhyCannotSleep.None && KSingleton<PlayerController>.Instance().TouchCount() == 0 && !GameManager.IsPrologueMode)
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
			PlayerController.TouchEvent currentTouchEvent = PlayerController.CurrentTouchEvent;
			if (currentTouchEvent != null && currentTouchEvent.Used == PlayerController.TouchEvent.UsedBy.None)
			{
				currentTouchEvent.Used = PlayerController.TouchEvent.UsedBy.Joystick;
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

	private void OnOnCloseCloseableUI()
	{
		if (!UIBase.IsOpenUI)
		{
			ResumeTimer(WhyCannotSleep.UIOpen);
		}
	}

	private void OnOnOpenCloseableUI()
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
		KSingleton<UIManager>.Instance().SetSleepMode(isSleep: false);
		PlayerBehavior.LocalPlayer.IsOutlineEnabled = true;
		KSingleton<PlayerController>.Instance().MotionParam("IsSleep", 0);
		KSingleton<PlayerController>.Instance().RefreshMotion(string.Empty);
		Connections.Frontend.Send(new ToggleStatusEffect
		{
			Id = "away_from_keyboard",
			Toggle = false
		});
	}

	private void Sleep()
	{
		_isSleep = true;
		KSingleton<UIManager>.Instance().SetSleepMode(isSleep: true);
		PlayerBehavior.LocalPlayer.IsOutlineEnabled = false;
		KSingleton<PlayerController>.Instance().MotionParam("IsSleep", 1);
		if (PlayerBehavior.LocalPlayer.IsCurrentAnimState("Stand") || PlayerBehavior.LocalPlayer.IsCurrentAnimState("Rest"))
		{
			KSingleton<PlayerController>.Instance().RefreshMotion(string.Empty);
		}
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
