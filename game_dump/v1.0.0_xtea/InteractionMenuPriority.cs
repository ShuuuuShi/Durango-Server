using System;
using System.Collections.Generic;
using InteractionData;
using Shared.System;
using UnityEngine;

[ResourcePath("interaction_menu_priority")]
public class InteractionMenuPriority : ResourceSingleton<InteractionMenuPriority>
{
	[Serializable]
	[EnumType(typeof(Shared.System.Interaction))]
	private class ServerList : EnumKeyList
	{
		[SerializeField]
		private List<int> _values;

		public int Get(Shared.System.Interaction val)
		{
			int num = IndexOf((int)val);
			if (num == -1)
			{
				return -1;
			}
			return _values[num];
		}
	}

	[Serializable]
	[EnumType(typeof(InteractionData.Interaction))]
	private class ClientList : EnumKeyList
	{
		[SerializeField]
		private List<int> _values;

		public int Get(InteractionData.Interaction val)
		{
			int num = IndexOf((int)val);
			if (num == -1)
			{
				return -1;
			}
			return _values[num];
		}
	}

	[SerializeField]
	private int _major;

	[SerializeField]
	private ServerList _serverList;

	[SerializeField]
	private ClientList _clientList;

	public static int Major => ResourceSingleton<InteractionMenuPriority>.Instance()._major;

	public static int Priority(Shared.System.Interaction val)
	{
		return ResourceSingleton<InteractionMenuPriority>.Instance()._serverList.Get(val);
	}

	public static int Priority(InteractionData.Interaction val)
	{
		return ResourceSingleton<InteractionMenuPriority>.Instance()._clientList.Get(val);
	}
}
