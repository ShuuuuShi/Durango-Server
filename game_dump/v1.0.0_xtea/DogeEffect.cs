using System.Collections.Generic;
using UnityEngine;

public class DogeEffect : UIEffect
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private Vector2 _makePeriod;

	[SerializeField]
	private float _scrollSpeed;

	[SerializeField]
	private Vector2 _durationRange;

	[SerializeField]
	private Rect _startRect;

	[TextArea(3, 10)]
	[SerializeField]
	private string _labelTexts;

	private Stack<UILabel> _pool = new Stack<UILabel>();

	private List<KeyValuePair<Transform, float>> _labels = new List<KeyValuePair<Transform, float>>();

	private string[] _textPool;

	private float _labelMakeTimer;

	protected override void OnAwake()
	{
		List<string> list = new List<string>(_labelTexts.Split('\n'));
		for (int num = list.Count - 1; num >= 0; num--)
		{
			string text = list[num].Trim();
			if (string.IsNullOrEmpty(text))
			{
				list.RemoveAt(num);
			}
			list[num] = LocalizeSystem.Get(text);
		}
		_textPool = list.ToArray();
		((Component)_label).gameObject.SetActive(false);
	}

	protected override void OnUpdate()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		for (int num = _labels.Count - 1; num >= 0; num--)
		{
			KeyValuePair<Transform, float> pair = _labels[num];
			if (pair.Value < time)
			{
				TweenAlpha tweenAlpha = TweenAlpha.Begin(((Component)pair.Key).gameObject, 0.3f, 0f);
				EventDelegate.Add(tweenAlpha.onFinished, delegate
				{
					((Component)pair.Key).gameObject.SetActive(false);
					_pool.Push(((Component)pair.Key).GetComponent<UILabel>());
				}, oneShot: true);
				_labels.RemoveAt(num);
			}
			else
			{
				Transform key = pair.Key;
				key.localPosition += Vector3.up * _scrollSpeed * Time.deltaTime;
			}
		}
		if (_labelMakeTimer > 0f)
		{
			_labelMakeTimer -= Time.deltaTime;
			return;
		}
		Make();
		_labelMakeTimer = Random.Range(_makePeriod.x, _makePeriod.y);
	}

	private void Make()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		UILabel uILabel = null;
		uILabel = ((_pool.Count <= 0) ? ((Component)((Component)_label).transform.parent).gameObject.AddChild(((Component)_label).gameObject).GetComponent<UILabel>() : _pool.Pop());
		((Component)uILabel).gameObject.SetActive(true);
		uILabel.color = new Color(Random.value, Random.value, Random.value);
		((Component)uILabel).transform.localPosition = new Vector3(-1f, 0f, 1f) * Random.Range(((Rect)(ref _startRect)).xMin, ((Rect)(ref _startRect)).xMax) + Vector3.up * Random.Range(((Rect)(ref _startRect)).yMin, ((Rect)(ref _startRect)).yMax);
		((Component)uILabel).transform.rotation = ((Component)_label).transform.rotation;
		uILabel.text = _textPool[Random.Range(0, _textPool.Length)];
		uILabel.alpha = 0f;
		TweenAlpha.Begin(((Component)uILabel).gameObject, 0.3f, 1f);
		KeyValuePair<Transform, float> item = new KeyValuePair<Transform, float>(((Component)uILabel).transform, Time.time + Random.Range(_durationRange.x, _durationRange.y));
		_labels.Add(item);
	}

	public void SetTextPools(params string[] words)
	{
		_textPool = words;
	}
}
