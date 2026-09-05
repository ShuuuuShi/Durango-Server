using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class AlarmMessageWidget : MonoBehaviour
{
	public const float FadeIn = 0.3f;

	public const float FadeOut = 0.3f;

	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private UISpriteLabel _textLabel;

	private bool _init;

	public UIWidget Widget => _widget;

	public string Key { get; private set; }

	public string Text { get; private set; }

	public int Index { get; set; }

	public Vector3 TargetPosition { get; set; }

	public float Since { get; private set; }

	public float Until { get; private set; }

	private void OnDisable()
	{
		_init = false;
	}

	public void Set(string key, string text, float duration, float scale = 1f)
	{
		Key = key;
		Text = text;
		_textLabel.text = text;
		// [3 ก.ย. 2026] ขนาดข้อความบรอดแคสต์ — สเกล transform ทั้ง widget (NGUI ไม่มีแท็กขนาดในข้อความ)
		//   scale=1 = ของเดิมทุกอย่างไม่เปลี่ยน · บรอดแคสต์แอดมินส่ง scale > 1 มาเพื่อให้ตัวใหญ่ขึ้น
		base.transform.localScale = (scale > 0f) ? Vector3.one * scale : Vector3.one;
		_textLabel.overflowWidth = UIManager.SafeWidth - 100;
		if (!_init)
		{
			Since = Time.time;
		}
		Until = Since + duration;
		Index = -1;
		Point2 point = new Point2(_textLabel.printedSize);
		_widget.SetDimensions(Mathf.Max(400, point.x + 100), point.y + 60);
		UIUtility.UpdateAnchors(base.transform);
		_init = true;
	}

	public void UpdatePosition(float speed)
	{
		Vector3 vector = TargetPosition - base.transform.localPosition;
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude < speed * speed)
		{
			base.transform.localPosition = TargetPosition;
		}
		else
		{
			base.transform.localPosition += vector.normalized * speed;
		}
	}
}
