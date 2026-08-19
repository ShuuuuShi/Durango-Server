using UnityEngine;

namespace Durango.UI.InGame;

public class EnemySelectorArrow : MonoBehaviour
{
	[SerializeField]
	private float _selectTime;

	[SerializeField]
	private float _endDelay = 2f;

	[SerializeField]
	private float _arrowTailChaseDelay = 0.7f;

	private float _enableTime;

	private EnemySelector.Target _start;

	private EnemySelector.Target _end;

	private void Update()
	{
		if (!_start.IsValid() || !_end.IsValid())
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		float num = _enableTime + _selectTime;
		float num2;
		if (Time.time < num)
		{
			num2 = (Time.time - _enableTime) / _selectTime;
		}
		else
		{
			num2 = 1f;
			if (Time.time >= num + _endDelay)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
		}
		Vector3 vector = _start.GetPosition();
		vector.y = 0f;
		Vector3 position = _end.GetPosition();
		position.y = 0f;
		if (_arrowTailChaseDelay > 0f)
		{
			float b = num + _arrowTailChaseDelay - 0.1f;
			float t = Mathf.Clamp01((Mathf.Min(Time.time, b) - num) / _selectTime);
			vector = Vector3.Lerp(vector, position, t);
		}
		Vector3 vector2 = position - vector;
		UISprite component = GetComponent<UISprite>();
		Transform transform = base.transform;
		float num3 = vector2.magnitude / transform.localScale.x;
		transform.position = vector;
		component.width = (int)(num3 * num2);
		float z = Mathf.Atan2(vector2.z, vector2.x) * 57.29578f;
		transform.eulerAngles = new Vector3(90f, 0f, z);
	}

	public void Show(EnemySelector.Target start, EnemySelector.Target end)
	{
		_enableTime = Time.time;
		_start = start;
		_end = end;
		if (_start.Transform != null)
		{
			Artifact component = _start.Transform.GetComponent<Artifact>();
			if (component != null)
			{
				_start = component.InteractionPosition;
			}
		}
		if (_end.Transform != null)
		{
			Artifact component2 = _end.Transform.GetComponent<Artifact>();
			if (component2 != null)
			{
				_end = component2.InteractionPosition;
			}
		}
		base.gameObject.SetActive(value: true);
		Update();
	}
}
