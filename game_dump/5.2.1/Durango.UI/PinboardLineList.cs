using System;
using Durango.UI.Popup;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class PinboardLineList : MonoBehaviour
{
	public class ReadPinboard
	{
		public PinboardContent[] contents;
	}

	public class PinboardContent
	{
		public string content;

		public PinboardRadioId radio_id;

		public string id;

		public double at;
	}

	public class PinboardRadioId
	{
		public int freq;

		public string name;
	}

	[SerializeField]
	private UIWidget _scrollViewContainer;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private PinboardLine _pinboardLine;

	[SerializeField]
	private Color[] _lineColors;

	private bool _initialized;

	private UIWidget _invisibleWidget;

	private readonly ListObjectPool<PinboardLine> _pinboardLines = new ListObjectPool<PinboardLine>();

	private int _lineColorIndex;

	private bool _updatePosition;

	public int Count => _pinboardLines.Count;

	private void OnEnable()
	{
		_invisibleWidget = UIUtility.SetScrollViewInvisibleBox(_scrollView, _invisibleWidget);
		KeyboardHeightChecker.KeyboardHeightUpdated += OnKeyboardHeightUpdated;
		OnKeyboardHeightUpdated(KeyboardHeightChecker.Height);
	}

	private void OnDisable()
	{
		KeyboardHeightChecker.KeyboardHeightUpdated -= OnKeyboardHeightUpdated;
	}

	private void LateUpdate()
	{
		if (_updatePosition)
		{
			Vector3 zero = Vector3.zero;
			for (int num = _pinboardLines.Count - 1; num >= 0; num--)
			{
				PinboardLine pinboardLine = _pinboardLines[num];
				zero += Vector3.up * pinboardLine.Height;
				pinboardLine.Position = zero;
			}
			_updatePosition = false;
		}
	}

	public void Init()
	{
		if (_initialized)
		{
			return;
		}
		_pinboardLines.BaseObject = _pinboardLine;
		_pinboardLines.Init(delegate(PinboardLine line)
		{
			line.Init();
			line.NameLabelClicked = (Action<string>)Delegate.Combine(line.NameLabelClicked, (Action<string>)delegate(string entityId)
			{
				PlayerInfoPopup.RequestShow(entityId);
			});
			line.HeightChanged = (Action)Delegate.Combine(line.HeightChanged, (Action)delegate
			{
				_updatePosition = true;
			});
		});
		_initialized = true;
	}

	public void Clear()
	{
		_lineColorIndex = 0;
		_scrollView.ResetPosition();
		_pinboardLines.Clear();
	}

	public void Refresh([CanBeNull] ReadPinboard readPinboard)
	{
		_pinboardLines.BeginLoad();
		if (readPinboard != null)
		{
			PinboardLine pinboardLine = null;
			for (int i = 0; i < readPinboard.contents.Length; i++)
			{
				PinboardContent pinboardContent = readPinboard.contents[i];
				if (!string.IsNullOrEmpty(pinboardContent.content))
				{
					PinboardLine obj = ((!(pinboardLine != null) || !(pinboardLine.EntityId == pinboardContent.id)) ? AddNewLine() : pinboardLine);
					obj.AddContent(pinboardContent);
					pinboardLine = obj;
				}
			}
		}
		_pinboardLines.EndLoad();
	}

	private PinboardLine AddNewLine()
	{
		PinboardLine next = _pinboardLines.GetNext();
		next.Clear((int)_scrollView.GetComponent<UIPanel>().width, _lineColors[_lineColorIndex]);
		_lineColorIndex = (_lineColorIndex + 1) % _lineColors.Length;
		return next;
	}

	private void OnKeyboardHeightUpdated(int height)
	{
		if (height > 0)
		{
			int num = (int)UIUtility.ToRootPosition(_scrollViewContainer.gameObject).y + UIManager.ScreenHeight / 2;
			_scrollViewContainer.bottomAnchor.absolute = Mathf.Max(0, height - num);
		}
		else
		{
			_scrollViewContainer.bottomAnchor.absolute = 0;
		}
		_scrollViewContainer.UpdateAnchors();
		_scrollView.panel.UpdateAnchors();
		_scrollView.ResetPosition();
	}
}
