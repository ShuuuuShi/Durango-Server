using System;

namespace MusicData;

public class MusicController
{
	public Action OnPlay;

	public Action OnStop;

	public Action OnTick;

	public Action OnPlayNote;

	public static MusicController Current;

	private bool _isPlay;

	private float _timer;

	private int _currentTick;

	private Note _lastNote;

	public bool IsPlay
	{
		get
		{
			return _isPlay;
		}
		set
		{
			if (_isPlay == value)
			{
				return;
			}
			_isPlay = value;
			if (_isPlay)
			{
				if (OnPlay != null)
				{
					Current = this;
					OnPlay();
					Current = null;
				}
			}
			else if (OnStop != null)
			{
				Current = this;
				OnStop();
				Current = null;
			}
		}
	}

	public float Timer
	{
		get
		{
			return _timer;
		}
		set
		{
			_timer = value;
		}
	}

	public int CurrentTick
	{
		get
		{
			return _currentTick;
		}
		set
		{
			if (_currentTick != value)
			{
				_currentTick = value;
				if (OnTick != null)
				{
					Current = this;
					OnTick();
					Current = null;
				}
			}
		}
	}

	public Note LastNote
	{
		get
		{
			return _lastNote;
		}
		set
		{
			_lastNote = value;
			if (_lastNote.Midi != 0 && OnPlayNote != null)
			{
				Current = this;
				OnPlayNote();
				Current = null;
			}
		}
	}

	public void Reset()
	{
		IsPlay = false;
		Timer = 0f;
		CurrentTick = 0;
		LastNote = default(Note);
	}
}
