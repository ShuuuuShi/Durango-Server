using UnityEngine;

namespace Durango.UI;

public class PointTargetController : MonoBehaviour, IUIInitializable
{
	public struct Arguments
	{
		public Vector3? Position;

		public Transform Target;

		public string Icon;

		public int? IconSize;

		public Color? IconColor;

		public Color? BorderColor;

		public bool ShowBg;

		public string Season;

		public bool HideInScreen;

		public bool TryGetPosition(out Vector3 pos)
		{
			Vector3? position = Position;
			if (!position.HasValue)
			{
				if (Target == null)
				{
					pos = Vector3.zero;
					return false;
				}
				pos = Target.position;
				return true;
			}
			pos = Position.Value;
			return true;
		}
	}

	[SerializeField]
	private PointTargetWidget _baseObject;

	private readonly ListObjectPool<PointTargetWidget> _navigateListPool = new ListObjectPool<PointTargetWidget>();

	private int _baseDepth;

	void IUIInitializable.Init()
	{
		_navigateListPool.BaseObject = _baseObject;
		_navigateListPool.Clear();
		_baseDepth = _baseObject.GetComponent<UIPanel>().depth;
	}

	private void LateUpdate()
	{
		bool flag = false;
		for (int i = 0; i < _navigateListPool.Count; i++)
		{
			if (!_navigateListPool[i].Tick())
			{
				flag = true;
				_navigateListPool.Remove(i);
				i--;
			}
		}
		if (flag)
		{
			RefreshDepth();
		}
	}

	public void SetTarget(string key, Arguments args)
	{
		GetOrAddTarget(key).SetTarget(key, args);
	}

	public void UpdateGauge(string key, float value, bool warning)
	{
		PointTargetWidget target = GetTarget(key);
		if (!(target == null))
		{
			target.UpdateGauge(value, warning);
		}
	}

	public void ClearTarget(string key)
	{
		PointTargetWidget target = GetTarget(key);
		if (!(target == null))
		{
			target.Clear();
			ClearObject(key);
		}
	}

	public void Select(string key, bool selected)
	{
		PointTargetWidget target = GetTarget(key);
		if (!(target == null))
		{
			target.Select(selected);
		}
	}

	public bool Has(string key)
	{
		for (int i = 0; i < _navigateListPool.Count; i++)
		{
			if (_navigateListPool[i].Key == key)
			{
				return true;
			}
		}
		return false;
	}

	private PointTargetWidget GetOrAddTarget(string key)
	{
		for (int i = 0; i < _navigateListPool.Count; i++)
		{
			if (_navigateListPool[i].Key == key)
			{
				return _navigateListPool[i];
			}
		}
		PointTargetWidget pointTargetWidget = _navigateListPool.Add();
		pointTargetWidget.SetDepth(_baseDepth + _navigateListPool.Count - 1);
		return pointTargetWidget;
	}

	private PointTargetWidget GetTarget(string key)
	{
		for (int i = 0; i < _navigateListPool.Count; i++)
		{
			if (_navigateListPool[i].Key == key)
			{
				return _navigateListPool[i];
			}
		}
		return null;
	}

	private void ClearObject(string key)
	{
		int num = -1;
		for (int i = 0; i < _navigateListPool.Count; i++)
		{
			if (_navigateListPool[i].Key == key)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			_navigateListPool.Remove(num);
			RefreshDepth();
		}
	}

	private void RefreshDepth()
	{
		int num = 0;
		foreach (PointTargetWidget item in _navigateListPool)
		{
			item.SetDepth(_baseDepth + num++);
		}
	}
}
