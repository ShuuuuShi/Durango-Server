using System;
using UnityEngine;

namespace Durango.UI.Control;

public abstract class SortableColumnWidget<T> : MonoBehaviour
{
	public enum State
	{
		None,
		Ascending,
		Descending
	}

	public Action<T> Clicked;

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UISprite _upSprite;

	[SerializeField]
	private UISprite _downSprite;

	private State _state;

	private bool _clickEnabled = true;

	public virtual T Value { get; set; }

	public bool ClickEnabled
	{
		get
		{
			return _clickEnabled;
		}
		set
		{
			_clickEnabled = value;
			_upSprite.gameObject.SetActive(value);
			_downSprite.gameObject.SetActive(value);
		}
	}

	protected abstract void GetStateColor(out Color normal, out Color selected);

	private void Start()
	{
		UpdateColor();
		UpdateLayout();
	}

	public void SetState(State state)
	{
		_state = state;
		UpdateColor();
	}

	public void SetText(string text)
	{
		_label.text = text;
		UpdateLayout();
	}

	public State NextState()
	{
		_state++;
		if (_state > State.Descending)
		{
			_state = State.Ascending;
		}
		UpdateColor();
		return _state;
	}

	private void UpdateLayout()
	{
		Transform obj = _upSprite.transform;
		Vector3 localPosition = obj.localPosition;
		localPosition.x = _label.printedSize.x / 2f + 10f;
		obj.localPosition = localPosition;
		Transform obj2 = _downSprite.transform;
		localPosition = obj2.localPosition;
		localPosition.x = _label.printedSize.x / 2f + 10f;
		obj2.localPosition = localPosition;
	}

	private void UpdateColor()
	{
		GetStateColor(out var normal, out var selected);
		_label.color = ((ClickEnabled && _state != 0) ? selected : normal);
		_upSprite.color = ((_state == State.Ascending) ? selected : normal);
		_downSprite.color = ((_state == State.Descending) ? selected : normal);
	}

	private void OnClick()
	{
		if (ClickEnabled && Clicked != null)
		{
			Clicked(Value);
		}
	}
}
