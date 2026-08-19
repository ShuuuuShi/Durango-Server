using System;
using Durango.Network;
using Durango.Player;
using Durango.UI;
using Durango.Utils;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;

public class CPRSystem : GameSystem<CPRSystem>
{
	public const int ReadySecond = 3;

	private PlayerBehavior _target;

	private Vector3 _position = Vector3.zero;

	public event Action CPRStarted;

	public event Action CPRInterrupted;

	private void Awake()
	{
		Connections.Frontend.RegisterRelayHandler(delegate(PlayerCPR msg, float timePassed)
		{
			PlayerBehavior playerIncludeLocalPlayer = Singleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(msg.RescuerId);
			PlayerBehavior playerIncludeLocalPlayer2 = Singleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(msg.TargetId);
			if (playerIncludeLocalPlayer != null && playerIncludeLocalPlayer2 != null)
			{
				HandleCPR(playerIncludeLocalPlayer, playerIncludeLocalPlayer2, msg.State);
			}
		});
		Connections.Frontend.On<ResurrectionReady>(OnResurrectionReady);
		Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			Singleton<PlayerController>.Instance().MoveStarted += delegate
			{
				Interrupt();
			};
			PlayerBehavior.LocalPlayer.TakenDamage += delegate
			{
				Interrupt();
			};
			PlayerBehavior.LocalPlayer.Died += delegate
			{
				Interrupt();
			};
			Singleton<PlayerManager>.Instance().PlayerDisappeared += delegate(PlayerBehavior player)
			{
				if (_target != null && _target.gameObject == player.gameObject)
				{
					Interrupt(refreshMotion: true);
				}
			};
		};
	}

	private void Start()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Resurrect, delegate(InteractionObject target)
		{
			PlayerBehavior targetComponent = target.GetTargetComponent<PlayerBehavior>();
			if (targetComponent != null)
			{
				ReadyCPR(targetComponent);
			}
		});
	}

	private void Update()
	{
		if (!(_target == null))
		{
			if (_position != Vector3.zero && Singleton<PlayerController>.HasInstance())
			{
				Singleton<PlayerController>.Instance().MoveToPosition(_position, OnMoveToCPRPosition);
				_position = Vector3.zero;
			}
			if (_target.IsReceivingCPR && _target.IsAlive)
			{
				Interrupt(refreshMotion: true);
			}
		}
	}

	private void SendCPRMsg(string targetId, string state)
	{
		PlayerCPR msg = default(PlayerCPR);
		msg.RescuerId = PlayerBehavior.LocalPlayer.EntityId;
		msg.TargetId = targetId;
		msg.State = state;
		Connections.Frontend.Send(msg);
	}

	public void ReadyCPR([NotNull] PlayerBehavior target)
	{
		if (!target.IsReceivingCPR && !target.IsAlive)
		{
			_target = target;
			_position = target.GetSidePos(left: true);
		}
	}

	private void OnMoveToCPRPosition()
	{
		if (!(_target == null))
		{
			CharacterBehavior component = _target.GetComponent<CharacterBehavior>();
			if (component != null)
			{
				SendCPRMsg(component.EntityId, "Run");
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
		if (target.IsReceivingCPR || target.IsAlive)
		{
			return;
		}
		if (target.IsLocalPlayer)
		{
			UIManager.Alarm.ShowNotify(T._("<{0}>님이 당신을 구조하려고 합니다.", rescuer.PlayerName), "act_Resurrect", major: true);
		}
		target.IsReceivingCPR = true;
		target.PlayMotionForcely("Barehand_CPR_Dead");
		if (rescuer.IsLocalPlayer)
		{
			_target = target;
			Singleton<PlayerController>.Instance().PrepareCPR(target);
			if (this.CPRStarted != null)
			{
				this.CPRStarted();
			}
		}
	}

	private void InterruptedCPR([NotNull] PlayerBehavior target)
	{
		if (!target.IsAlive)
		{
			target.PlayMotionForcely("Barehand_CPR_Stop");
		}
		target.IsReceivingCPR = false;
	}

	private void EndCPR([NotNull] PlayerBehavior target)
	{
		if (!target.IsAlive)
		{
			target.PlayMotionForcely("Barehand_CPR_Stop");
		}
		target.IsReceivingCPR = false;
	}

	public void CPRResult(float score)
	{
		PlayerController.MotionUpdater.RefreshMotion();
		if ((bool)_target)
		{
			Connections.Frontend.Send(new Resurrect
			{
				EntityId = _target.EntityId,
				Score = score
			});
			SendCPRMsg(_target.EntityId, "End");
		}
		_target = null;
	}

	private void Interrupt(bool refreshMotion = false)
	{
		if ((bool)_target)
		{
			SendCPRMsg(_target.EntityId, "Interrupt");
		}
		_target = null;
		if (refreshMotion)
		{
			PlayerController.MotionUpdater.RefreshMotion();
		}
		if (this.CPRInterrupted != null)
		{
			this.CPRInterrupted();
		}
	}

	private void OnResurrectionReady(ResurrectionReady msg, PacketHeader header)
	{
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(msg.HelperEntityId, delegate(Durango.Player.PlayerInfo info)
		{
			float num = Times.UnixTimeToUnityTime(msg.ValidUntil);
			float time = Time.time;
			if (!(num < time))
			{
				MessageBox messageBox = UIManager.MessageBox;
				messageBox.Show((info != null && !string.IsNullOrEmpty(info.EntityId)) ? T._("{0}님이 당신을 일으켜 세웁니다.\n부활하시겠습니까?", info.Name) : T._("누군가가 당신을 일으켜 세웁니다.\n부활하시겠습니까?"), delegate(bool ok)
				{
					if (ok)
					{
						ConfirmResurrection(msg.HelperEntityId);
					}
				});
				messageBox.SetHideTimer(num);
			}
		});
	}

	public static void ConfirmResurrection(string helperEntityId)
	{
		Connections.Frontend.Send(new ConfirmResurrection
		{
			HelperEntityId = helperEntityId
		});
	}
}
