using UnityEngine;

public class PathDrawLine : MonoBehaviour
{
	[SerializeField]
	private UIWidget _widget;

	private TweenWidth _tweener;

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Component)this).transform.position = value;
		}
	}

	public Vector3 Angle
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.eulerAngles;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Component)this).transform.eulerAngles = value;
		}
	}

	public int Length
	{
		get
		{
			return _widget.width;
		}
		set
		{
			_widget.width = value;
		}
	}

	private void Awake()
	{
		_tweener = ((!((Object)(object)((Component)_widget).GetComponent<TweenWidth>() != (Object)null)) ? ((Component)_widget).gameObject.AddComponent<TweenWidth>() : ((Component)_widget).GetComponent<TweenWidth>());
		_tweener.SetOnFinished(OnTweenWidthFinish);
		((Behaviour)_tweener).enabled = false;
	}

	private void OnTweenWidthFinish()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void TweenLength(float delay, float duration)
	{
		_tweener.from = _widget.width;
		_tweener.to = 0;
		_tweener.delay = delay;
		_tweener.duration = duration;
		_tweener.ResetToBeginning();
		_tweener.PlayForward();
	}
}
