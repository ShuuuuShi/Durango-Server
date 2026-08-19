using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Durango.Logic.Social;
using Durango.Player.Animation;
using Durango.Terrain;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Chat;
using UnityEngine;

public class RescueTarget : NaturalPrefabObject
{
	public enum ActType
	{
		Exausted,
		Dead
	}

	[Serializable]
	private class ActPair
	{
		public int EntityType;

		public ActType ActType;
	}

	[CompilerGenerated]
	private sealed class _003CCoRescued_003Ed__11 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public RescueTarget _003C_003E4__this;

		private float _003Cyaw_003E5__2;

		private Vector3 _003Cdir_003E5__3;

		private float _003CelapsedTime_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoRescued_003Ed__11(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			RescueTarget rescueTarget = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				PlayerAnimationClipInfo playerAnimationClipInfo = Singleton<PlayerAnimationClipManager>.Instance().GetPlayerAnimationClipInfo("Barehand_Stand_Tired");
				rescueTarget._humanBehavior.CrossFade(playerAnimationClipInfo.Clip, -1f, loop: false);
				_003C_003E2__current = new WaitForSeconds(playerAnimationClipInfo.Length - playerAnimationClipInfo.FadeOutTime);
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
			{
				_003C_003E1__state = -1;
				rescueTarget.ShowMessage();
				rescueTarget._humanBehavior.CrossFade("Barehand_Walk_Tired");
				int randomHash = KUtility.GetRandomHash(rescueTarget.WorldTile.x, rescueTarget.WorldTile.y);
				_003Cyaw_003E5__2 = KUtility.GetRandomHashRange(0, 360, randomHash);
				_003Cdir_003E5__3 = Quaternion.Euler(0f, _003Cyaw_003E5__2, 0f) * Vector3.forward;
				_003CelapsedTime_003E5__4 = 0f;
				break;
			}
			case 2:
				_003C_003E1__state = -1;
				break;
			}
			if (_003CelapsedTime_003E5__4 < 10f)
			{
				rescueTarget._humanBehavior.TurnToYaw(_003Cyaw_003E5__2, bSnap: false);
				rescueTarget._humanBehavior.CurrentPosition += _003Cdir_003E5__3.normalized * Time.deltaTime * 180f;
				_003CelapsedTime_003E5__4 += Time.deltaTime;
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
			rescueTarget.ReturnToPoolAndDeactive();
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[SerializeField]
	private List<ActPair> _actTypes;

	private ActType _actType;

	private HumanBehavior _humanBehavior;

	private readonly string[] _fallDownMotions = new string[6] { "Barehand_Die", "Onehand_Die_A", "Onehand_Die_B", "Onehand_Die_C", "Onehand_Die_D", "Onehand_Die_E" };

	private void Awake()
	{
		_humanBehavior = GetComponent<HumanBehavior>();
	}

	[ExposedInEditor(null)]
	protected override void OnSetEntity()
	{
		_actType = _actTypes.FirstOrDefault((ActPair o) => o.EntityType == base.EntityType)?.ActType ?? ActType.Dead;
		int randomHash = KUtility.GetRandomHash(base.WorldTile.x, base.WorldTile.y);
		_humanBehavior.EntityTypeId = base.EntityType;
		_humanBehavior.LoadCostume(randomHash);
		FallDown(randomHash);
	}

	protected override void OnUpdateEntityId()
	{
		_humanBehavior.EntityId = base.EntityId;
	}

	private void FallDown(int randomKey)
	{
		int num = Maths.Mod(randomKey, _fallDownMotions.Length);
		_humanBehavior.PlayToLast(_fallDownMotions[num]);
	}

	[ExposedInEditor(null)]
	public override void OnRemoved(TerrainChunkBase chunk, bool fastRemove)
	{
		if (fastRemove)
		{
			ReturnToPoolAndDeactive();
			return;
		}
		switch (_actType)
		{
		case ActType.Dead:
			ReturnToPoolAndDeactive();
			break;
		case ActType.Exausted:
			StartCoroutine(CoRescued());
			break;
		}
	}

	private IEnumerator CoRescued()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoRescued_003Ed__11(0)
		{
			_003C_003E4__this = this
		};
	}

	private void ShowMessage()
	{
		if (_humanBehavior.ChatableBase is ChatableHuman chatableHuman)
		{
			chatableHuman.RefreshPortrait(KUtility.GetRandomHash(base.WorldTile.x, base.WorldTile.y));
		}
		ChatStruct chat = new ChatStruct
		{
			EntityId = _humanBehavior.EntityId,
			Chatter = _humanBehavior.ChatableBase,
			Body = new RadioNotice
			{
				Text = T._("고맙습니다. 살 것 같아요.")
			},
			Name = GetName(),
			Emotion = PortraitEmotion.Smile,
			Type = ChannelType.System,
			Duration = 3f,
			IsVolatile = true
		};
		GameSystem<SocialSystem>.Instance().AddChat(chat);
	}
}
