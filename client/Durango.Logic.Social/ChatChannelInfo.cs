using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Messages;
using Shared.Chat;

namespace Durango.Logic.Social;

public class ChatChannelInfo
{
	private class ChannelInfo
	{
		public bool Hidden;

		public double ReadAt;

		public string CustomName;
	}

	private const string StorageKey = "ChatChannelInfo";

	private readonly Dictionary<string, ChannelInfo> _channelInfos = new Dictionary<string, ChannelInfo>();

	private bool _changed;

	public bool IsHidden(ChannelType channelType)
	{
		return IsHidden(channelType.ToString());
	}

	public bool IsHidden(ChatFilterType filterType)
	{
		return IsHidden(SocialSystem.ConvertToChannelType(filterType));
	}

	public bool IsHidden(Conversation conv)
	{
		return IsHidden(conv.Id);
	}

	public bool ToggleHide(ChannelType channelType)
	{
		return ToggleHide(channelType.ToString());
	}

	public bool ToggleHide(ChatFilterType filterType)
	{
		return ToggleHide(SocialSystem.ConvertToChannelType(filterType));
	}

	public bool ToggleHide(Conversation conv)
	{
		return ToggleHide(conv.Id);
	}

	public static bool IsHideable(ChannelType channelType)
	{
		return channelType != ChannelType.Invalid && channelType != ChannelType.System;
	}

	public double GetReadAt(string id)
	{
		return _channelInfos.Get(id)?.ReadAt ?? 0.0;
	}

	public void SetReadAt(string id, double readAt)
	{
		ChannelInfo orAdd = GetOrAdd(id);
		if (orAdd.ReadAt != readAt)
		{
			_changed = true;
			orAdd.ReadAt = readAt;
		}
	}

	[CanBeNull]
	public string GetCustomName(string id)
	{
		return _channelInfos.Get(id)?.CustomName;
	}

	public void SetCustomName(string id, string name)
	{
		ChannelInfo orAdd = GetOrAdd(id);
		if (orAdd.CustomName != name)
		{
			_changed = true;
			orAdd.CustomName = name;
		}
	}

	public void LoadStorage(Dictionary<string, byte[]> storage)
	{
		_channelInfos.Clear();
		_changed = false;
		byte[] data = storage?.Get("ChatChannelInfo");
		Dictionary<string, ChannelInfo> dictionary = Json.Read<Dictionary<string, ChannelInfo>>(data);
		if (dictionary != null)
		{
			_channelInfos.AddRange(dictionary);
		}
	}

	public void SaveStorage(Dictionary<string, Conversation> conversations)
	{
		if (!_changed)
		{
			return;
		}
		Dictionary<string, ChannelInfo>.Enumerator enumerator = _channelInfos.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key = enumerator.Current.Key;
			bool flag = !conversations.ContainsKey(key);
			if (flag)
			{
				flag = !key.TryEnum<ChannelType>(out var _);
			}
			if (flag)
			{
				_channelInfos.Remove(key);
				enumerator.Dispose();
				enumerator = _channelInfos.GetEnumerator();
			}
		}
		Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			SetStorageItem msg = default(SetStorageItem);
			msg.Key = "ChatChannelInfo";
			msg.Value = Json.WriteToBytes(_channelInfos);
			Connections.Frontend.Send(msg);
		});
		_changed = false;
	}

	private bool IsHidden(string id)
	{
		return _channelInfos.Get(id)?.Hidden ?? false;
	}

	private bool ToggleHide(string id)
	{
		_changed = true;
		ChannelInfo orAdd = GetOrAdd(id);
		orAdd.Hidden = !orAdd.Hidden;
		return orAdd.Hidden;
	}

	[NotNull]
	private ChannelInfo GetOrAdd(string id)
	{
		ChannelInfo channelInfo = _channelInfos.Get(id);
		if (channelInfo == null)
		{
			channelInfo = new ChannelInfo();
			_channelInfos[id] = channelInfo;
		}
		return channelInfo;
	}
}
