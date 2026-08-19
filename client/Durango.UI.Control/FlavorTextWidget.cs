using L10N;
using UnityEngine;

namespace Durango.UI.Control;

public class FlavorTextWidget : UIWidget
{
	[SerializeField]
	private UILabel _flavorTextLabel;

	[SerializeField]
	private float _flavorTextPeriod;

	[Tooltip("한 글자가 나오는 시간")]
	[SerializeField]
	private float _flavorTextSpeed;

	[LocalizableString]
	[SerializeField]
	private string[] _flavorTexts;

	private bool _isFlavorTextTyping;

	private float _nextTimeToFlavorTextChange;

	private int _flavorTextIndex = -1;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			_flavorTextIndex = -1;
			ShowNextFlavorText();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying && _nextTimeToFlavorTextChange > 0f && _nextTimeToFlavorTextChange < Time.time)
		{
			ShowNextFlavorText();
		}
	}

	private void ShowNextFlavorText()
	{
		if (_flavorTextIndex == -1)
		{
			_flavorTextIndex = Random.Range(0, _flavorTexts.Length);
		}
		else
		{
			_flavorTextIndex = (_flavorTextIndex + Random.Range(1, _flavorTexts.Length)) % _flavorTexts.Length;
		}
		ShowFlavorText(_flavorTextIndex);
		if (_flavorTextPeriod > 0f)
		{
			_nextTimeToFlavorTextChange = Time.time + _flavorTextPeriod;
		}
		else
		{
			_nextTimeToFlavorTextChange = 0f;
		}
	}

	private void ShowFlavorText(int index)
	{
		_flavorTextIndex = index;
		string text = _flavorTexts[index];
		_flavorTextLabel.text = T._(text);
		_isFlavorTextTyping = true;
		TypeWriterEffect.Begin(_flavorTextLabel, _flavorTextSpeed, OnFinishTypeWriterEffect);
	}

	private void OnFinishTypeWriterEffect()
	{
		_isFlavorTextTyping = false;
	}

	private void OnClick()
	{
		if (!_isFlavorTextTyping)
		{
			ShowNextFlavorText();
		}
	}
}
