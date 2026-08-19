using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Durango.Logic.Item;
using InteractionData;
using UnityEngine;

namespace Durango.Logic.Interactions;

public class ReservationQueue
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct InteractionQueueData
	{
		private static int _idCounter;

		public int QueueId { get; private set; }

		public InteractionMenuData Data { get; private set; }

		public InteractionQueueData(InteractionMenuData data)
		{
			this = default(InteractionQueueData);
			QueueId = _idCounter++;
			Data = data;
		}
	}

	private readonly List<InteractionQueueData> _interactionQueue = new List<InteractionQueueData>();

	public int Count => _interactionQueue.Count;

	public event Action Updated;

	public void Init()
	{
		GameSystem<GatheringSystem>.Instance().TargetRunOut += GatheringSystem_TargetRunOut;
		GameSystem<GatheringSystem>.Instance().GatheringFailed += GatheringSystem_GatheringFailed;
		GameSystem<InteractionSystem>.Instance().MenuList.Updated += InteractionSystem_MenuListUpdated;
	}

	private void GatheringSystem_TargetRunOut()
	{
		Clear();
	}

	private void GatheringSystem_GatheringFailed()
	{
		Clear();
	}

	private void InteractionSystem_MenuListUpdated()
	{
		for (int i = 0; i < _interactionQueue.Count; i++)
		{
			InteractionMenuData data = _interactionQueue[i].Data;
			if (IsFull(data.Action, data.Id, data.Count, out var overCount))
			{
				for (int j = 0; j < overCount; j++)
				{
					RemoveLast(data.Action, data.Id);
				}
			}
		}
	}

	public bool Any()
	{
		return _interactionQueue.Count > 0;
	}

	public void Push(InteractionMenuData data, int iterateCount)
	{
		if (iterateCount <= 0)
		{
			return;
		}
		for (int i = 0; i < iterateCount; i++)
		{
			if (IsFull(data.Action, data.Id, data.Count, out var overCount))
			{
				for (int j = 0; j < overCount; j++)
				{
					RemoveLast(data.Action, data.Id);
				}
				break;
			}
			if (data.Action != Interaction.Collect || data.GatheringData.BestPerformance != 0)
			{
				_interactionQueue.Add(new InteractionQueueData(data));
			}
		}
		OnQueueUpdated();
	}

	private bool IsFull(Interaction type, string id, int totalCount, out int overCount)
	{
		int num = 0;
		if (InteractionSystem.CurrentMenu.Action == type && InteractionSystem.CurrentMenu.Id == id)
		{
			num++;
		}
		for (int i = 0; i < _interactionQueue.Count; i++)
		{
			InteractionQueueData interactionQueueData = _interactionQueue[i];
			if (interactionQueueData.Data.Action == type && interactionQueueData.Data.Id == id)
			{
				num++;
			}
		}
		overCount = Mathf.Max(0, num - totalCount);
		return num >= totalCount;
	}

	public InteractionMenuData Pop()
	{
		if (_interactionQueue.Count > 0)
		{
			InteractionMenuData data = _interactionQueue[0].Data;
			Remove(0);
			return data;
		}
		return default(InteractionMenuData);
	}

	public void Clear()
	{
		_interactionQueue.Clear();
		OnQueueUpdated();
	}

	public void RemoveFirst(Interaction action, string id)
	{
		for (int i = 0; i < _interactionQueue.Count; i++)
		{
			if (_interactionQueue[i].Data.Action == action && _interactionQueue[i].Data.Id == id)
			{
				Remove(i);
				break;
			}
		}
	}

	public void RemoveLast(Interaction action, string id)
	{
		for (int num = _interactionQueue.Count - 1; num >= 0; num--)
		{
			if (_interactionQueue[num].Data.Action == action && _interactionQueue[num].Data.Id == id)
			{
				Remove(num);
				break;
			}
		}
	}

	private void Remove(int index)
	{
		_interactionQueue.RemoveAt(index);
		OnQueueUpdated();
	}

	public bool TryGetQueueItems(Interaction type, string id, out List<Pair<int, ItemIcon>> items)
	{
		items = null;
		for (int i = 0; i < _interactionQueue.Count; i++)
		{
			InteractionQueueData interactionQueueData = _interactionQueue[i];
			if (interactionQueueData.Data.Action == type && interactionQueueData.Data.Id == id)
			{
				if (items == null)
				{
					items = new List<Pair<int, ItemIcon>>();
				}
				items.Add(new Pair<int, ItemIcon>(interactionQueueData.QueueId, interactionQueueData.Data.Icon));
			}
		}
		return items != null;
	}

	private void OnQueueUpdated()
	{
		if (this.Updated != null)
		{
			this.Updated();
		}
	}
}
