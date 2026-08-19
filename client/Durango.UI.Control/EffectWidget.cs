using UnityEngine;

namespace Durango.UI.Control;

public abstract class EffectWidget : UIWidget
{
	[SerializeField]
	private bool _isLoop = true;

	[SerializeField]
	private float _duration = 1f;

	[SerializeField]
	private float _delay;

	private float _time;

	private float _delta;

	private bool _isEditorPlay;

	private float _timer;

	public bool IsLoop
	{
		get
		{
			return _isLoop;
		}
		set
		{
			_isLoop = value;
		}
	}

	public float Duration
	{
		get
		{
			return _duration;
		}
		set
		{
			_duration = value;
		}
	}

	public float Delay
	{
		get
		{
			return _delay;
		}
		set
		{
			_delay = value;
		}
	}

	public void Play()
	{
		Play(0f);
	}

	public void Play(float ratio)
	{
		_timer = ratio * Duration;
		mChanged = true;
		base.enabled = true;
	}

	public void Stop()
	{
		base.enabled = false;
	}

	protected abstract void Sample(float ratio, UIGeometry.Arguments arguments);

	protected override void OnUpdate()
	{
		base.OnUpdate();
		bool isPlaying = Application.isPlaying;
		if (!isPlaying && !_isEditorPlay)
		{
			return;
		}
		if (isPlaying)
		{
			_delta = Time.deltaTime;
			_time = Time.time;
		}
		else
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			_delta = realtimeSinceStartup - _time;
			_time = realtimeSinceStartup;
		}
		mChanged = true;
		_timer += _delta;
		if (_timer > Duration + Delay)
		{
			if (IsLoop)
			{
				_timer -= Duration + Delay;
			}
			else
			{
				Stop();
			}
		}
		if (!isPlaying && mChanged)
		{
			MarkAsChanged();
		}
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		if (Application.isPlaying || _isEditorPlay)
		{
			Sample(Mathf.Clamp01(_timer / Duration), arguments);
		}
	}

	public void EditorPlay(bool enable)
	{
		if (enable)
		{
			_isEditorPlay = true;
			_time = Time.realtimeSinceStartup;
			_timer = 0f;
		}
		else
		{
			_isEditorPlay = false;
			geometry.Clear();
			MarkAsChanged();
		}
	}
}
