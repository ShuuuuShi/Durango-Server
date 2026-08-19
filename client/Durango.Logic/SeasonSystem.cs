using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;
using UnityEngine;

namespace Durango.Logic;

public class SeasonSystem : GameSystem<SeasonSystem>
{
	public enum Period
	{
		Invalid,
		Before,
		During,
		After
	}

	private float _seasonsValidAt;

	private readonly Dictionary<string, Season> _seasons = new Dictionary<string, Season>();

	public bool Initialized { get; private set; }

	public event Action SeasonUpdated;

	private void Start()
	{
		Singleton<GameManager>.Instance().WelcomeReceived += delegate(Welcome welcome)
		{
			OnSeasons(welcome.Seasons);
		};
	}

	private void Update()
	{
		if (_seasonsValidAt > 0f && Time.time > _seasonsValidAt)
		{
			_seasonsValidAt = 0f;
			Connections.Frontend.Send(default(GetSeasons)).On<Seasons>(OnSeasons);
		}
	}

	public void OnSeasons(Seasons msg, PacketHeader header = default(PacketHeader))
	{
		_seasons.Clear();
		if (msg._Seasons == null)
		{
			return;
		}
		double? num = null;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		Season[] seasons = msg._Seasons;
		for (int i = 0; i < seasons.Length; i++)
		{
			Season value = seasons[i];
			_seasons.Add(value.Id, value);
			double until = value.Until;
			if (predictedServerTime < until && (!num.HasValue || until < num.Value))
			{
				num = until;
			}
		}
		_seasonsValidAt = ((!num.HasValue) ? 0f : (Times.UnixTimeToUnityTime(num.Value) + UnityEngine.Random.value * 300f));
		Initialized = true;
		if (this.SeasonUpdated != null)
		{
			this.SeasonUpdated();
		}
	}

	public Season? GetSeason(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		if (_seasons.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public Period GetSeasonStatus(string key)
	{
		Season? season = GetSeason(key);
		if (!season.HasValue)
		{
			return Period.Invalid;
		}
		Season value = season.Value;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (predictedServerTime < value.Since)
		{
			return Period.Before;
		}
		if (predictedServerTime >= value.Until)
		{
			return Period.After;
		}
		return Period.During;
	}
}
