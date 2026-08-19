using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI.InGame;

public class FillBorderAlerts : AreaOfEffectAlert
{
	[SerializeField]
	private AnimationCurve _borderAlpha;

	[SerializeField]
	private Color _backgroundColor;

	[SerializeField]
	private Color _borderColor;

	private Texture2D _borderTexture;

	private readonly Stack<FillBorderAlert> _pool = new Stack<FillBorderAlert>();

	private readonly Dictionary<int, FillBorderAlert> _alerts = new Dictionary<int, FillBorderAlert>();

	public override int ShowCircle(Vector3 position, float radius, float startAt, float finishAt, float showAt, float hideAt)
	{
		return ShowArc(position, radius, 0f, 360f, startAt, finishAt, showAt, hideAt);
	}

	public override int ShowArc(Vector3 position, float radius, float startAngle, float endAngle, float startAt, float finishAt, float showAt, float hideAt)
	{
		FillBorderAlert viewer = GetViewer();
		viewer.SetArc(position, radius, startAngle, endAngle);
		viewer.Show(startAt, finishAt, showAt, hideAt);
		int num = (viewer.Id = AreaOfEffectVisualizer.GetNextId());
		_alerts.Add(num, viewer);
		return num;
	}

	public override int ShowRect(Vector3 position, float width, float height, float angle, float startAt, float finishAt, float showAt, float hideAt)
	{
		FillBorderAlert viewer = GetViewer();
		viewer.SetRect(position, width, height, angle);
		viewer.Show(startAt, finishAt, showAt, hideAt);
		int num = (viewer.Id = AreaOfEffectVisualizer.GetNextId());
		_alerts.Add(num, viewer);
		return num;
	}

	public override void Stop(int id, float delay)
	{
		if (_alerts.TryGetValue(id, out var value))
		{
			value.Stop(delay);
		}
	}

	public override void Move(int id, Vector3 position)
	{
		if (_alerts.TryGetValue(id, out var value))
		{
			value.transform.position = position;
		}
	}

	private Texture2D GetBorderTexture()
	{
		if (_borderTexture == null)
		{
			if (_borderAlpha.length > 0)
			{
				_borderTexture = new Texture2D(16, 1, TextureFormat.RGBA32, mipmap: false);
				float time = _borderAlpha.keys[_borderAlpha.length - 1].time;
				for (int i = 0; i < _borderTexture.width; i++)
				{
					float num = ((float)i + 0.5f) / (float)_borderTexture.width;
					Color white = Color.white;
					white.a = _borderAlpha.Evaluate(num * time);
					_borderTexture.SetPixel(i, 1, white);
				}
				_borderTexture.Apply();
			}
			else
			{
				_borderTexture = Texture2D.whiteTexture;
			}
		}
		return _borderTexture;
	}

	private FillBorderAlert GetViewer()
	{
		if (_pool.Count > 0)
		{
			return _pool.Pop();
		}
		FillBorderAlert fillBorderAlert = base.gameObject.AddChild<FillBorderAlert>();
		fillBorderAlert.Init(_backgroundColor, _borderColor, GetBorderTexture());
		fillBorderAlert.gameObject.SetActive(value: false);
		fillBorderAlert.Finished = OnFinishViewer;
		fillBorderAlert.gameObject.hideFlags = HideFlags.DontSave;
		return fillBorderAlert;
	}

	private void OnFinishViewer(FillBorderAlert viewer)
	{
		int id = viewer.Id;
		_alerts.Remove(id);
		if (Application.isPlaying)
		{
			_pool.Push(viewer);
		}
		else
		{
			Object.DestroyImmediate(viewer.gameObject);
		}
	}
}
