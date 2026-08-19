using System.Collections.Generic;
using Durango.Logic.Social;
using Durango.Utils;
using Shared.Battle;
using UnityEngine;

namespace Durango.UI.InGame;

public class EmoticonEffectControl : Singleton<EmoticonEffectControl>
{
	[SerializeField]
	private EmoticonEffect _emoticon;

	private readonly List<EmoticonEffect> _emoticons = new List<EmoticonEffect>();

	private readonly Stack<EmoticonEffect> _pool = new Stack<EmoticonEffect>();

	protected override void OnAwake()
	{
		_emoticon.gameObject.SetActive(value: false);
	}

	public void Show(string entityId, Emoticon emoticon, bool findLocalPlayer = false)
	{
		if (emoticon != null)
		{
			PlayerBehavior playerBehavior = ((!findLocalPlayer) ? Singleton<PlayerManager>.Instance().GetPlayer(entityId) : Singleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(entityId));
			if (!(playerBehavior == null) && playerBehavior.GetVisible() && !GameSystem<SocialSystem>.Instance().IsBlocked(entityId))
			{
				Transform bodyPartTransform = playerBehavior.GetBodyPartTransform(BodyPart.Body);
				Vector3 offset = new Vector3(-30f, 40f, -30f);
				Show(bodyPartTransform, offset, emoticon.Icon, emoticon.Key);
			}
		}
	}

	public void Show(Transform target, Vector3 offset, string emoticon, string soundSwitchName)
	{
		EmoticonEffect emoticonEffect = Get(target);
		emoticonEffect.Set(target, offset, emoticon, soundSwitchName);
		emoticonEffect.Show(3f);
	}

	private EmoticonEffect Get(Transform target)
	{
		int i = 0;
		for (int count = _emoticons.Count; i < count; i++)
		{
			if (_emoticons[i].Target == target)
			{
				return _emoticons[i];
			}
		}
		EmoticonEffect emoticonEffect;
		if (_pool.Count > 0)
		{
			emoticonEffect = _pool.Pop();
		}
		else
		{
			emoticonEffect = _emoticon.transform.parent.gameObject.AddChild(_emoticon.gameObject).GetComponent<EmoticonEffect>();
			emoticonEffect.transform.rotation = _emoticon.transform.rotation;
			emoticonEffect.Disabled = Release;
		}
		_emoticons.Add(emoticonEffect);
		return emoticonEffect;
	}

	private void Release(EmoticonEffect emoticon)
	{
		_emoticons.Remove(emoticon);
		_pool.Push(emoticon);
	}
}
