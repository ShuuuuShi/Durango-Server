using System;
using JetBrains.Annotations;
using Messages;
using Shared.System;
using UnityEngine;

public class CPRSystem : GameSystem<CPRSystem>
{
	public const string CPRBGM = "Sound/Effect/BGM/BGM_CPR_01.mp3";

	private PlayerBehavior _target;

	private Vector3 _position = default(Vector3);

	private int _soundSeq;

	public event Action CPRStarted;

	public event Action CPRInterrupted;

	private void Awake()
	{
		Connections.Frontend.RegisterRelayHandler(delegate(PlayerCPR msg, float timePassed)
		{
			PlayerBehavior playerIncludeLocalPlayer = KSingleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(msg.RescuerId);
			PlayerBehavior playerIncludeLocalPlayer2 = KSingleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(msg.TargetId);
			if ((Object)(object)playerIncludeLocalPlayer != (Object)null && (Object)(object)playerIncludeLocalPlayer2 != (Object)null)
			{
				HandleCPR(playerIncludeLocalPlayer, playerIncludeLocalPlayer2, msg.State);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Resurrect, delegate(InteractionObject target)
		{
			PlayerBehavior targetComponent = target.GetTargetComponent<PlayerBehavior>();
			if ((Object)(object)targetComponent != (Object)null)
			{
				ReadyCPR(targetComponent);
			}
		});
		KSingleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			KSingleton<PlayerController>.Instance().MoveStarted += PlayerController_MoveStarted;
			PlayerBehavior.LocalPlayer.DamageTaken += LocalPlayer_DamageTaken;
			PlayerBehavior.LocalPlayer.Died += LocalPlayer_Died;
			KSingleton<PlayerManager>.Instance().PlayerDisappeared += OnDisappearPlayer;
			SoundManager.Cache("Sound/Effect/BGM/BGM_CPR_01.mp3");
		};
	}

	private void Update()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_target == (Object)null))
		{
			if (((Vector3)(ref _position)).sqrMagnitude > 0f && KSingleton<PlayerController>.HasInstance())
			{
				KSingleton<PlayerController>.Instance().MoveToTarget(_position, MoveToCPRPosition, 10f, ((Component)_target).gameObject);
				_position = default(Vector3);
			}
			if (_target.IsReceivingCPR && _target.IsAlive)
			{
				Interrupt(refreshMotion: true);
			}
		}
	}

	private void OnDisappearPlayer(PlayerBehavior player)
	{
		if ((Object)(object)_target != (Object)null && (Object)(object)((Component)_target).gameObject == (Object)(object)((Component)player).gameObject)
		{
			Interrupt(refreshMotion: true);
		}
	}

	public void CPR(ulong targetId, string state)
	{
		PlayerCPR msg = default(PlayerCPR);
		msg.RescuerId = PlayerBehavior.LocalPlayer.EntityId;
		msg.TargetId = targetId;
		msg.State = state;
		Connections.Frontend.Send(msg);
	}

	public void ReadyCPR([NotNull] PlayerBehavior target)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!target.IsReceivingCPR && !target.IsAlive)
		{
			_target = target;
			_position = FindPlayerSide(target, left: true);
		}
	}

	private static Vector3 FindPlayerSide([NotNull] PlayerBehavior target, bool left)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		float yawDeg = target.TargetYaw + ((!left) ? 90f : (-90f));
		Vector3 currentPosition = target.CurrentPosition;
		Vector3 val = KMathUtil.CalcDirectionFromYaw(yawDeg);
		return currentPosition + ((Vector3)(ref val)).normalized * target.XRadius;
	}

	private void MoveToCPRPosition([CanBeNull] GameObject target)
	{
		if (!((Object)(object)target == (Object)null))
		{
			CharacterBehavior component = target.GetComponent<CharacterBehavior>();
			if ((Object)(object)component != (Object)null)
			{
				CPR(component.EntityId, "Run");
			}
		}
	}

	private void HandleCPR([NotNull] PlayerBehavior rescuer, [NotNull] PlayerBehavior target, string state)
	{
		switch (state)
		{
		case "Run":
			RunCPR(rescuer, target);
			break;
		case "Interrupt":
			InterruptedCPR(target);
			break;
		case "End":
			EndCPR(target);
			break;
		}
	}

	private void RunCPR([NotNull] PlayerBehavior rescuer, [NotNull] PlayerBehavior target)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (target.IsReceivingCPR || target.IsAlive)
		{
			return;
		}
		if (target.IsLocalPlayer)
		{
			UIManager.SystemMsg(LocalizeSystem.Format("#someone_giving_cpr_msg_text", rescuer.PlayerName));
		}
		target.IsReceivingCPR = true;
		Vector3 val = FindPlayerSide(target, left: true);
		if (rescuer.CurrentPosition != val)
		{
			rescuer.Teleport(val);
			rescuer.RotateToTarget(((Component)target).gameObject);
		}
		target.PlayAnimation("Barehand_CPR_Dead");
		if (rescuer.IsLocalPlayer)
		{
			_target = target;
			_soundSeq = SoundManager.Play("Sound/Effect/BGM/BGM_CPR_01.mp3");
			KSingleton<PlayerController>.Instance().Motion("Barehand_CPR");
			if (this.CPRStarted != null)
			{
				this.CPRStarted();
			}
		}
	}

	private void InterruptedCPR([NotNull] PlayerBehavior target)
	{
		if (target.IsLocalPlayer && !target.IsAlive)
		{
			target.PlayAnimation("Barehand_CPR_Stop");
		}
		target.IsReceivingCPR = false;
	}

	private void EndCPR([NotNull] PlayerBehavior target)
	{
		target.IsReceivingCPR = false;
	}

	public void CPRResult(float score)
	{
		KSingleton<PlayerController>.Instance().RefreshMotion(string.Empty);
		if (Object.op_Implicit((Object)(object)_target))
		{
			Connections.Frontend.Send(new Resurrect
			{
				EntityId = _target.EntityId,
				Score = score
			});
			CPR(_target.EntityId, "End");
		}
		_target = null;
	}

	private void Interrupt(bool refreshMotion = false)
	{
		if (Object.op_Implicit((Object)(object)_target))
		{
			CPR(_target.EntityId, "Interrupt");
		}
		_target = null;
		SoundManager.Stop(_soundSeq);
		if (refreshMotion)
		{
			KSingleton<PlayerController>.Instance().RefreshMotion(string.Empty);
		}
		if (this.CPRInterrupted != null)
		{
			this.CPRInterrupted();
		}
	}

	private void PlayerController_MoveStarted()
	{
		Interrupt();
	}

	private void LocalPlayer_DamageTaken(CharacterBehavior attacker, Damage damage)
	{
		Interrupt();
	}

	private void LocalPlayer_Died(PlayerBehavior player)
	{
		Interrupt();
	}
}
