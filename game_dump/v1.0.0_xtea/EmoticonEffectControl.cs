using System.Collections.Generic;
using UnityEngine;

public class EmoticonEffectControl : KSingleton<EmoticonEffectControl>
{
	[SerializeField]
	private EmoticonEffect _emoticon;

	private List<EmoticonEffect> _emoticons = new List<EmoticonEffect>();

	private Stack<EmoticonEffect> _pool = new Stack<EmoticonEffect>();

	private readonly string[] _emoticonSound = new string[6] { "Sound/Effect/emo_smile.wav", "Sound/Effect/emo_wink.wav", "Sound/Effect/emo_surprise.wav", "Sound/Effect/emo_love.wav", "Sound/Effect/emo_angry.wav", "Sound/Effect/emo_angry.wav" };

	protected override void OnAwake()
	{
		((Component)_emoticon).gameObject.SetActive(false);
		for (int i = 0; i < _emoticonSound.Length; i++)
		{
			SoundManager.Cache(_emoticonSound[i]);
		}
	}

	public void Show(ulong entityId, uint type, float power, bool findLocalPlayer = false)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		PlayerBehavior playerBehavior = ((!findLocalPlayer) ? KSingleton<PlayerManager>.Instance().GetPlayer(entityId) : KSingleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(entityId));
		if (!((Object)(object)playerBehavior == (Object)null) && playerBehavior.GetRenderEnabled() && !GameSystem<SocialSystem>.Instance().BlockList.Contains(entityId))
		{
			Show(((Component)playerBehavior).transform, new Vector3(-30f, 130f, -30f), $"emoticon_{type + 1}", _emoticonSound[type]);
		}
	}

	public void Show(Transform target, Vector3 offset, string emoticon, string sound)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		EmoticonEffect emoticonEffect = Get(target);
		emoticonEffect.Set(target, offset, emoticon, sound);
		emoticonEffect.Show(3f);
	}

	private EmoticonEffect Get(Transform target)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int count = _emoticons.Count; i < count; i++)
		{
			if ((Object)(object)_emoticons[i].Target == (Object)(object)target)
			{
				return _emoticons[i];
			}
		}
		EmoticonEffect emoticonEffect = null;
		if (_pool.Count > 0)
		{
			emoticonEffect = _pool.Pop();
		}
		else
		{
			emoticonEffect = ((Component)((Component)_emoticon).transform.parent).gameObject.AddChild(((Component)_emoticon).gameObject).GetComponent<EmoticonEffect>();
			((Component)emoticonEffect).transform.rotation = ((Component)_emoticon).transform.rotation;
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
