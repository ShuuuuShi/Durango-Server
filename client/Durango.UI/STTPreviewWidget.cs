using UnityEngine;

namespace Durango.UI;

public class STTPreviewWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject _speakButton;

	[SerializeField]
	private UISprite _speakSprite;

	[SerializeField]
	private UISprite _sttWaveSprite1;

	[SerializeField]
	private UISprite _sttWaveSprite2;

	[SerializeField]
	private UILabel _sttPreviewLine;

	[SerializeField]
	private UISprite _sttPreviewBg;

	[SerializeField]
	private GameObject _sttAlertIcon;

	[SerializeField]
	private STTWaveWidget _sttWaveWidget;

	private bool _sttButtonPressed;

	private int _sttPreviewBgPadding;

	private Color _sttNoticeColor = new Color(1f, 0.85f, 0.36f);

	private Color _sttPreviewColor = new Color(1f, 0.83f, 0.61f);

	private int _sttPreviewLeftMargin;

	private int _sttPreviewRightMargin;

	private string _text;

	private bool _isInitLayout;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void Awake()
	{
		InitLayout();
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
	}

	private void OnEnable()
	{
		SetLineText(_text);
	}

	private void InitLayout()
	{
		if (!_isInitLayout)
		{
			_isInitLayout = true;
			_sttPreviewBg.SetAnchor((Transform)null);
			_sttPreviewBgPadding = _sttPreviewBg.height - _sttPreviewLine.fontSize;
			_sttPreviewLeftMargin = (int)(_sttPreviewLine.GetPosition(0f, 0f).x - Widget.localCorners[0].x);
			_sttPreviewRightMargin = (int)(Widget.localCorners[3].x - _sttPreviewLine.GetPosition(1f, 0f).x);
		}
	}

	private void SetLineText(string text)
	{
		_text = text;
		if (base.enabled)
		{
			Vector3 pos = Widget.localCorners[0] + Vector3.right * _sttPreviewLeftMargin;
			pos.y += (float)_sttPreviewBgPadding * 0.5f;
			_sttPreviewLine.SetPosition(pos, 0f, 0f);
			_sttPreviewLine.width = Widget.width - _sttPreviewLeftMargin - _sttPreviewRightMargin;
			_sttPreviewLine.text = text;
			int height = _sttPreviewLine.height + _sttPreviewBgPadding;
			_sttPreviewBg.width = Widget.width;
			_sttPreviewBg.height = height;
		}
	}

	private void StopAllTweens(GameObject targetObject)
	{
		UITweener[] components = targetObject.GetComponents<UITweener>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = false;
		}
	}
}
