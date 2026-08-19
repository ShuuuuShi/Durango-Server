using System.Collections.Generic;
using UnityEngine;

public class AndroidTouch
{
	public class Touch
	{
		public int fingerId;

		public TouchPhase m_phase;

		public Vector2 position;

		public int tapCount;

		public TouchPhase phase
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return m_phase;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Expected I4, but got Unknown
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Invalid comparison between Unknown and I4
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_0036: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_002f: Invalid comparison between Unknown and I4
				TouchPhase val = m_phase;
				switch ((int)val)
				{
				case 0:
				case 3:
				case 4:
					if ((int)value == 4 || (int)value == 3)
					{
						m_phase = value;
					}
					break;
				default:
					m_phase = value;
					break;
				}
			}
		}

		public void BeganChecked()
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_phase = (TouchPhase)1;
		}
	}

	private static AndroidTouch m_instance;

	private Dictionary<int, Touch> dicTouch = new Dictionary<int, Touch>();

	public static AndroidTouch instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new AndroidTouch();
			}
			return m_instance;
		}
	}

	public int touchCount => dicTouch.Count;

	public Touch GetTouch(int index)
	{
		List<Touch> list = new List<Touch>(dicTouch.Values);
		if (list.Count <= index)
		{
			Debug.LogError((object)string.Empty);
			return null;
		}
		return list[index];
	}

	public void OnTouch(int pointerIndex, int fingerId, int tapCount, int phase, float x, float y)
	{
		if (!dicTouch.ContainsKey(fingerId))
		{
			dicTouch.Add(fingerId, new Touch());
		}
		Touch touch = dicTouch[fingerId];
		touch.fingerId = fingerId;
		touch.phase = (TouchPhase)phase;
		touch.position.x = x;
		touch.position.y = (float)Screen.height - y;
		touch.tapCount = tapCount;
	}

	public void LateUpdate()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, Touch> item in dicTouch)
		{
			if ((int)item.Value.phase == 0)
			{
				item.Value.BeganChecked();
			}
			if ((int)item.Value.phase >= 3)
			{
				list.Add(item.Key);
			}
		}
		foreach (int item2 in list)
		{
			dicTouch.Remove(item2);
		}
	}
}
