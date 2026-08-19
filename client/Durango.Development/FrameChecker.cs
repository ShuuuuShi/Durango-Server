using UnityEngine;

namespace Durango.Development;

public class FrameChecker : MonoBehaviour
{
	private const int MinNormalFrame = 30;

	[SerializeField]
	private bool _showFrameRateOnConsole = true;

	[SerializeField]
	private float _consoleMsgInterval = 30f;

	[SerializeField]
	private UILabel _fpsLabel;

	private int _frameCount;

	private float _lastCheckTime;

	private int _frameCountConsole;

	private float _lastCheckTimeConsole;

	private void Start()
	{
		_lastCheckTime = Time.realtimeSinceStartup;
		_lastCheckTimeConsole = _lastCheckTime;
	}

	private void Update()
	{
		_frameCount++;
		_frameCountConsole++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = realtimeSinceStartup - _lastCheckTime;
		if (num >= 1f)
		{
			int num2 = Mathf.RoundToInt((float)_frameCount / num * 100f) / 100;
			_lastCheckTime = realtimeSinceStartup;
			_frameCount = 0;
			_fpsLabel.color = ((num2 < 30) ? Color.red : Color.green);
			_fpsLabel.text = "FPS: " + num2;
		}
		if (_showFrameRateOnConsole)
		{
			num = realtimeSinceStartup - _lastCheckTimeConsole;
			if (num >= _consoleMsgInterval)
			{
				int num3 = Mathf.RoundToInt((float)_frameCountConsole / num * 100f) / 100;
				_lastCheckTimeConsole = realtimeSinceStartup;
				_frameCountConsole = 0;
			}
		}
	}
}
