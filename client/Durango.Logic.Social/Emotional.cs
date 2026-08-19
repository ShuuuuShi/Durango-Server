using System;
using System.Collections.Generic;
using Durango.Logic.Notification;
using Durango.Network;
using Durango.Utils;
using Durango.Utils.Extensions;
using Messages;
using Yaml;

namespace Durango.Logic.Social;

public class Emotional
{
	private const string StorageKey = "Emotional";

	public readonly Durango.Logic.Notification.Container EmoticonNotification = new Durango.Logic.Notification.Container();

	public readonly Durango.Logic.Notification.Container MotionNotification = new Durango.Logic.Notification.Container();

	private readonly List<Emoticon> _emoticons = new List<Emoticon>();

	private readonly List<Motion> _motions = new List<Motion>();

	private AvailableEmotions? _availableEmotions;

	private bool _saveDirty;

	public List<Emoticon> Emoticons => _emoticons;

	public List<Motion> Motions => _motions;

	public bool HasNewNotifiaction => EmoticonNotification.On || MotionNotification.On;

	public event Action Changed;

	public void Init(Emotions yaml)
	{
		_emoticons.Clear();
		_motions.Clear();
		EmoticonNotification.ClearChild();
		MotionNotification.ClearChild();
		EmoticonNotification.BeginSetting();
		Yaml.Emoticon[] emoticons = yaml.Emoticons;
		foreach (Yaml.Emoticon value in emoticons)
		{
			Emoticon emoticon = new Emoticon(value);
			_emoticons.Add(emoticon);
			EmoticonNotification.AddChild(emoticon);
		}
		EmoticonNotification.EndSetting();
		MotionNotification.BeginSetting();
		foreach (KeyValuePair<string, Yaml.Motion> motion2 in yaml.Motions)
		{
			Motion motion = new Motion(motion2.Key, motion2.Value);
			_motions.Add(motion);
			MotionNotification.AddChild(motion);
		}
		MotionNotification.EndSetting();
		_emoticons.Sort();
		_motions.Sort();
	}

	public void Set(AvailableEmotions msg)
	{
		bool hasValue = _availableEmotions.HasValue;
		_availableEmotions = msg;
		SetAvailables(msg.Emoticons, _emoticons, hasValue);
		SetAvailables(msg.Motions, _motions, hasValue);
		if (this.Changed != null)
		{
			this.Changed();
		}
		SaveFavorites();
	}

	private void SetAvailables<T>(string[] availables, List<T> emotions, bool update) where T : EmotionBase
	{
		bool flag = false;
		int? num = null;
		for (int i = 0; i < emotions.Count; i++)
		{
			EmotionBase emotionBase = emotions[i];
			if (availables == null || !availables.Contains(emotionBase.Key))
			{
				_saveDirty |= emotionBase.Available != emotionBase.Free;
				emotionBase.Available = emotionBase.Free;
				continue;
			}
			bool available = emotionBase.Available;
			emotionBase.Available = true;
			if (!update || available)
			{
				continue;
			}
			if (!num.HasValue)
			{
				for (int j = 0; j < emotions.Count; j++)
				{
					T val = emotions[j];
					if (val != emotionBase && val.FavoriteIndex.HasValue && (!num.HasValue || num.Value > val.FavoriteIndex.Value))
					{
						num = val.FavoriteIndex;
					}
				}
			}
			emotionBase.Notification.On = true;
			emotionBase.SetFavorite(favorite: true, --num);
			flag = true;
		}
		emotions.Sort();
		int num2 = 0;
		for (int k = 0; k < emotions.Count; k++)
		{
			T val2 = emotions[k];
			if (val2.Favorite)
			{
				T val3 = emotions[k];
				val3.SetFavorite(favorite: true, num2++);
			}
		}
		_saveDirty |= flag;
	}

	public void SetMotionFavorite(string key, bool isFavorite)
	{
		Motion motion = GetMotion(key);
		SetFavorite(motion, Motions, isFavorite);
	}

	public void SetEmoticonFavorite(string key, bool isFavorite)
	{
		Emoticon emoticon = GetEmoticon(key);
		SetFavorite(emoticon, Emoticons, isFavorite);
	}

	private void SetFavorite<T>(T emotion, List<T> emotions, bool isFavorite) where T : EmotionBase
	{
		if (emotion == null || !emotion.Available || emotion.Favorite == isFavorite)
		{
			return;
		}
		if (isFavorite)
		{
			int num = -1;
			for (int i = 0; i < emotions.Count; i++)
			{
				T val = emotions[i];
				if (val.FavoriteIndex.HasValue)
				{
					T val2 = emotions[i];
					num = val2.FavoriteIndex.Value;
				}
			}
			emotion.SetFavorite(favorite: true, num + 1);
		}
		else
		{
			emotion.SetFavorite(favorite: false);
		}
		emotions.Sort();
		_saveDirty = true;
		if (this.Changed != null)
		{
			this.Changed();
		}
	}

	private int EmoticonIndexOf(string key)
	{
		for (int i = 0; i < _emoticons.Count; i++)
		{
			if (_emoticons[i].Key == key)
			{
				return i;
			}
		}
		return -1;
	}

	public Emoticon GetEmoticon(string key)
	{
		int num = EmoticonIndexOf(key);
		return (num != -1) ? _emoticons[num] : null;
	}

	private int MotionIndexOf(string key)
	{
		for (int i = 0; i < _motions.Count; i++)
		{
			if (_motions[i].Key == key)
			{
				return i;
			}
		}
		return -1;
	}

	public Motion GetMotion(string key)
	{
		int num = MotionIndexOf(key);
		return (num != -1) ? _motions[num] : null;
	}

	public void SaveFavorites()
	{
		if (_saveDirty)
		{
			Singleton<GameManager>.Instance().AddOnReady(delegate
			{
				List<List<string>> data = new List<List<string>>
				{
					ToFavorites(_motions),
					ToFavorites(_emoticons)
				};
				SetStorageItem msg = default(SetStorageItem);
				msg.Key = "Emotional";
				msg.Value = Json.WriteToBytes(data);
				Connections.Frontend.Send(msg);
			});
			_saveDirty = false;
		}
	}

	private static List<string> ToFavorites<T>(List<T> emotions) where T : EmotionBase
	{
		List<string> list = new List<string>();
		foreach (T emotion in emotions)
		{
			T current = emotion;
			if (current.Favorite)
			{
				list.Add(current.Key);
			}
		}
		return list;
	}

	public void LoadFavorites(Dictionary<string, byte[]> storage)
	{
		byte[] data = storage?.Get("Emotional");
		List<List<string>> list = Json.Read<List<List<string>>>(data);
		if (list != null && list.Count >= 2)
		{
			FromFavorites(list[0], _motions);
			FromFavorites(list[1], _emoticons);
		}
	}

	private static void FromFavorites<T>(List<string> favorites, List<T> emotions) where T : EmotionBase
	{
		foreach (T emotion in emotions)
		{
			T current = emotion;
			int num = favorites.IndexOf(current.Key);
			if (num == -1)
			{
				current.SetFavorite(favorite: false);
			}
			else
			{
				current.SetFavorite(favorite: true, num);
			}
		}
		emotions.Sort();
	}
}
