using System;
using UnityEngine;

namespace Durango.UI.Control;

[RequireComponent(typeof(UILabel))]
public class TypeWriterEffect : MonoBehaviour
{
	private UILabel _label;

	private bool _volatility;

	private int _commentTypingCount;

	private int _maxTypingCount;

	private float _typeAt;

	private bool _enabled;

	private bool _fastFoward;

	private float _interval = 0.03f;

	public event Action Finished;

	public void Reset()
	{
		_commentTypingCount = 0;
		_maxTypingCount = -1;
		_typeAt = Time.realtimeSinceStartup;
		_fastFoward = false;
		_interval = 0.03f;
		if (_enabled)
		{
			_label.MarkAsChanged();
		}
	}

	public void SetFastFoward(bool fastFoward)
	{
		_fastFoward = fastFoward;
	}

	public void SetInterval(float interval)
	{
		_interval = interval;
	}

	private void OnEnable()
	{
		_enabled = true;
		_label = GetComponent<UILabel>();
		_label.onPostFill = OnPostFill;
		Reset();
	}

	private void OnDisable()
	{
		_enabled = false;
		_label.onPostFill = null;
		_label.MarkAsChanged();
		if (this.Finished != null)
		{
			this.Finished();
		}
		if (_volatility)
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (_maxTypingCount < 0)
		{
			return;
		}
		if (_commentTypingCount < _maxTypingCount)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float num = realtimeSinceStartup - _typeAt;
			float num2 = _interval;
			if (LocalizeSystem.IsLengthyLocale(LocalizeSystem.Locale))
			{
				num2 *= 0.66f;
			}
			if (_fastFoward)
			{
				num2 *= 0.33f;
			}
			if (num > num2)
			{
				if (num2 > 0f)
				{
					_commentTypingCount += (int)(num / num2);
					_typeAt = realtimeSinceStartup - num % num2;
				}
				else
				{
					_commentTypingCount = _maxTypingCount;
				}
				_label.MarkAsChanged();
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnPostFill(UIWidget widget, int bufferOffset, UIGeometry.Arguments arguments)
	{
		BetterList<Vector3> verts = arguments.verts;
		BetterList<Color> cols = arguments.cols;
		_maxTypingCount = (verts.size - bufferOffset) / 4;
		for (int i = _commentTypingCount; i < _maxTypingCount; i++)
		{
			for (int j = i * 4; j < cols.size; j += _maxTypingCount * 4)
			{
				for (int k = 0; k < 4; k++)
				{
					Color value = cols[j + k];
					value.a = 0f;
					cols[j + k] = value;
				}
			}
		}
	}

	public static TypeWriterEffect Begin(UILabel label, float interval = 0.1f, Action onFinish = null)
	{
		label.SetEnable<TypeWriterEffect>(enable: false);
		TypeWriterEffect typeWriterEffect = label.GetComponent<TypeWriterEffect>();
		if (typeWriterEffect == null)
		{
			typeWriterEffect = label.gameObject.AddComponent<TypeWriterEffect>();
			typeWriterEffect._volatility = true;
		}
		typeWriterEffect.SetInterval(interval);
		typeWriterEffect.Finished = onFinish;
		return typeWriterEffect;
	}
}
