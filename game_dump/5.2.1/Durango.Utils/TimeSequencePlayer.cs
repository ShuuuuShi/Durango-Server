using System.Collections.Generic;
using UnityEngine;

namespace Durango.Utils;

public class TimeSequencePlayer : ITimeSequencePlayer
{
	private readonly List<ITimeSequencePlayer> _players = new List<ITimeSequencePlayer>();

	public float Now => Time.time;

	public bool IsPlaying()
	{
		foreach (ITimeSequencePlayer player in _players)
		{
			if (player.IsPlaying())
			{
				return true;
			}
		}
		return false;
	}

	public float? NextAt()
	{
		if (TryGetNext(out var _, out var at))
		{
			return at;
		}
		return null;
	}

	private bool TryGetNext(out ITimeSequencePlayer target, out float at)
	{
		target = null;
		float num = Now;
		foreach (ITimeSequencePlayer player in _players)
		{
			if (player.IsPlaying())
			{
				target = null;
				break;
			}
			float? num2 = player.NextAt();
			if (num2.HasValue && !(num <= num2.Value))
			{
				target = player;
				num = num2.Value;
			}
		}
		if (target == null)
		{
			at = 0f;
			return false;
		}
		at = num;
		return true;
	}

	public void Play()
	{
		if (TryGetNext(out var target, out var _))
		{
			target.Play();
		}
	}

	public void Stop()
	{
		foreach (ITimeSequencePlayer player in _players)
		{
			player.Stop();
		}
	}

	public void AddPlayer(ITimeSequencePlayer player)
	{
		_players.Add(player);
	}

	public void Update()
	{
		Play();
	}
}
