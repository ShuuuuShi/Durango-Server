using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		PlayerAnimationClipInfo standClip = Singleton<PlayerAnimationClipManager>.Instance().GetPlayerAnimationClipInfo("Barehand_Stand_Tired");
		_humanBehavior.CrossFade(standClip.Clip, -1f, loop: false);
		yield return new WaitForSeconds(standClip.Length - standClip.FadeOutTime);
		ShowMessage();
		_humanBehavior.CrossFade("Barehand_Walk_Tired");
		int randKey = KUtility.GetRandomHash(base.WorldTile.x, base.WorldTile.y);
		float yaw = KUtility.GetRandomHashRange(0, 360, randKey);
		Vector3 dir = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
		float elapsedTime = 0f;
		while (elapsedTime < 10f)
		{
			_humanBehavior.TurnToYaw(yaw, bSnap: false);
			_humanBehavior.CurrentPosition += dir.normalized * Time.deltaTime * 180f;
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		ReturnToPoolAndDeactive();
	}

	private void ShowMessage()
	{
		if (_humanBehavior.ChatableBase is ChatableHuman chatableHuman)
		{
			chatableHuman.RefreshPortrait(KUtility.GetRandomHash(base.WorldTile.x, base.WorldTile.y));
		}
		ChatStruct chatStruct = new ChatStruct();
		chatStruct.EntityId = _humanBehavior.EntityId;
		chatStruct.Chatter = _humanBehavior.ChatableBase;
		chatStruct.Body = new RadioNotice
		{
			Text = T._("고맙습니다. 살 것 같아요.")
		};
		chatStruct.Name = GetName();
		chatStruct.Emotion = PortraitEmotion.Smile;
		chatStruct.Type = ChannelType.System;
		chatStruct.Duration = 3f;
		chatStruct.IsVolatile = true;
		ChatStruct chat = chatStruct;
		GameSystem<SocialSystem>.Instance().AddChat(chat);
	}
}
