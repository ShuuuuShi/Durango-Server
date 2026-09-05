using System.Collections.Generic;
using MsgPack;
using Shared.Player;

namespace Messages;

public struct Social
{
	public const uint TypeCode = 2403u;

	public string[] FollowingEntityIds;

	public Dictionary<string, Shared.Player.FriendType> FriendEntities;

	public string[] ReceivedFriendRequests;

	public string[] SentFriendRequests;

	public string[] BlockedEntityIds;

	public string[] FavoriteRegionOwners;

	public static void Pack(Packer packer, Social val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(2403u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		if (val.FollowingEntityIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.FollowingEntityIds.Length);
			for (int i = 0; i < val.FollowingEntityIds.Length; i++)
			{
				if (val.FollowingEntityIds[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.FollowingEntityIds[i]);
				}
			}
		}
		if (val.FriendEntities == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.FriendEntities.Count);
			foreach (KeyValuePair<string, Shared.Player.FriendType> friendEntity in val.FriendEntities)
			{
				if (friendEntity.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(friendEntity.Key);
				}
				packer.Pack((int)friendEntity.Value);
			}
		}
		if (val.ReceivedFriendRequests == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.ReceivedFriendRequests.Length);
			for (int j = 0; j < val.ReceivedFriendRequests.Length; j++)
			{
				if (val.ReceivedFriendRequests[j] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.ReceivedFriendRequests[j]);
				}
			}
		}
		if (val.SentFriendRequests == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.SentFriendRequests.Length);
			for (int k = 0; k < val.SentFriendRequests.Length; k++)
			{
				if (val.SentFriendRequests[k] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.SentFriendRequests[k]);
				}
			}
		}
		if (val.BlockedEntityIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.BlockedEntityIds.Length);
			for (int l = 0; l < val.BlockedEntityIds.Length; l++)
			{
				if (val.BlockedEntityIds[l] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.BlockedEntityIds[l]);
				}
			}
		}
		if (val.FavoriteRegionOwners == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.FavoriteRegionOwners.Length);
		for (int m = 0; m < val.FavoriteRegionOwners.Length; m++)
		{
			if (val.FavoriteRegionOwners[m] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.FavoriteRegionOwners[m]);
			}
		}
	}

	public static Social Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Social result = default(Social);
		result.FollowingEntityIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.FollowingEntityIds[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.FriendEntities = new Dictionary<string, Shared.Player.FriendType>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int num3 = unpacker.LastReadData.AsInt32();
			Shared.Player.FriendType value = ((num3 >= 0 && 1 >= num3) ? ((Shared.Player.FriendType)num3) : Shared.Player.FriendType.Invalid);
			result.FriendEntities.Add(key, value);
		}
		unpacker.Read();
		int num4 = unpacker.LastReadData.AsInt32();
		result.ReceivedFriendRequests = new string[num4];
		for (int k = 0; k < num4; k++)
		{
			unpacker.Read();
			result.ReceivedFriendRequests[k] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num5 = unpacker.LastReadData.AsInt32();
		result.SentFriendRequests = new string[num5];
		for (int l = 0; l < num5; l++)
		{
			unpacker.Read();
			result.SentFriendRequests[l] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num6 = unpacker.LastReadData.AsInt32();
		result.BlockedEntityIds = new string[num6];
		for (int m = 0; m < num6; m++)
		{
			unpacker.Read();
			result.BlockedEntityIds[m] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num7 = unpacker.LastReadData.AsInt32();
		result.FavoriteRegionOwners = new string[num7];
		for (int n = 0; n < num7; n++)
		{
			unpacker.Read();
			result.FavoriteRegionOwners[n] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Social FollowingEntityIds={FollowingEntityIds} FriendEntities={FriendEntities} ReceivedFriendRequests={ReceivedFriendRequests} SentFriendRequests={SentFriendRequests} BlockedEntityIds={BlockedEntityIds} FavoriteRegionOwners={FavoriteRegionOwners}>";
	}
}
