using System;
using System.Collections.Generic;
using Durango.Utils;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// Keeps one authoritative weather state per island process and broadcasts
/// transitions to every connected player. The state is deliberately server
/// owned so reconnecting players receive the current weather immediately.
/// </summary>
public sealed class ServerWeather
{
    private static readonly HashSet<string> KnownWeather = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "sunny",
        "cloudy",
        "rainy",
        "heavy_rainy",
        "snowy",
        "heavy_snowy"
    };

    private readonly ServerWorld _world;
    private string _current = "sunny";
    private int _sequenceIndex;
    private double _nextChangeAt;
    private bool _initialized;

    public ServerWeather(ServerWorld world)
    {
        _world = world;
    }

    public string Current
    {
        get
        {
            if (!_initialized)
            {
                Initialize(Times.UnixTimeNow());
            }
            return _current;
        }
    }

    public void SendCurrent(ServerPlayer player)
    {
        if (player == null)
        {
            return;
        }
        player.Send(new Weather { _Weather = Current, WeatherRatio = 1f });
    }

    public void Process(double now, ServerPlayer[] players)
    {
        WeatherConfig config = ServerConfig.Current.Weather ?? WeatherConfig.Defaults();
        List<string> sequence = GetSafeSequence(config.Sequence);
        double cycleSeconds = Math.Max(5.0, config.CycleSeconds);

        if (!config.Enabled)
        {
            if (!_initialized || !string.Equals(_current, "sunny", StringComparison.OrdinalIgnoreCase))
            {
                _initialized = true;
                _current = "sunny";
                _nextChangeAt = now + cycleSeconds;
                Broadcast(players);
            }
            return;
        }

        if (!_initialized)
        {
            _sequenceIndex = 0;
            _current = sequence[0];
            _initialized = true;
            _nextChangeAt = now + cycleSeconds;
            Broadcast(players);
            return;
        }

        if (now < _nextChangeAt)
        {
            return;
        }

        _sequenceIndex = (_sequenceIndex + 1) % sequence.Count;
        _current = sequence[_sequenceIndex];
        _nextChangeAt = now + cycleSeconds;
        Console.WriteLine("[weather] island={0} weather={1} next_in={2:F0}s",
            IslandRegistry.Current?.Id ?? "single", _current, cycleSeconds);
        Broadcast(players);
    }

    public static bool IsKnown(string weather)
    {
        return !string.IsNullOrWhiteSpace(weather) && KnownWeather.Contains(weather.Trim());
    }

    private void Initialize(double now)
    {
        WeatherConfig config = ServerConfig.Current.Weather ?? WeatherConfig.Defaults();
        List<string> sequence = GetSafeSequence(config.Sequence);
        _sequenceIndex = 0;
        _current = config.Enabled ? sequence[0] : "sunny";
        _initialized = true;
        _nextChangeAt = now + Math.Max(5.0, config.CycleSeconds);
    }

    private void Broadcast(ServerPlayer[] players)
    {
        if (players == null)
        {
            return;
        }
        Weather message = new Weather { _Weather = _current, WeatherRatio = 1f };
        for (int i = 0; i < players.Length; i++)
        {
            try
            {
                players[i].Send(message);
            }
            catch (Exception e)
            {
                Console.WriteLine("[weather] send failed for {0}: {1}", players[i].EntityId, e.Message);
            }
        }
        Console.WriteLine("[weather] island={0} weather={1}", IslandRegistry.Current?.Id ?? "single", _current);
    }

    private static List<string> GetSafeSequence(List<string> configured)
    {
        List<string> safe = new List<string>();
        if (configured != null)
        {
            for (int i = 0; i < configured.Count; i++)
            {
                string value = configured[i]?.Trim().ToLowerInvariant();
                if (IsKnown(value))
                {
                    safe.Add(value);
                }
            }
        }
        if (safe.Count == 0)
        {
            safe.Add("sunny");
        }
        return safe;
    }
}
