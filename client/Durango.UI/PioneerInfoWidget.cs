using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PioneerInfoWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _grade;

	[SerializeField]
	private UILabel _point;

	[SerializeField]
	private UISprite _pointBar;

	[SerializeField]
	private GameObject _anchor;

	[SerializeField]
	private UILabel _mutiplier;

	private readonly Dictionary<float, float> _exchangedPoints = new Dictionary<float, float>();

	private float _curItemPoint;

	private float _nextItemPoint;

	private float _minSpeed = 200f;

	public float LastRate { get; private set; }

	public event Action Clicked;

	public event Action RateChanged;

	private void OnEnable()
	{
		GameSystem<EstateSystem>.Instance().PioneerGradeInfoUpdated += Refresh;
	}

	private void OnDisable()
	{
		GameSystem<EstateSystem>.Instance().PioneerGradeInfoUpdated -= Refresh;
	}

	private void Update()
	{
		if (!Mathf.Approximately(_curItemPoint, _nextItemPoint))
		{
			float b = Mathf.Abs(_curItemPoint - _nextItemPoint) / 3f;
			float num = Time.deltaTime * Mathf.Max(_minSpeed, b);
			if (LastRate > 0f)
			{
				num /= LastRate;
			}
			_curItemPoint = Mathf.MoveTowards(_curItemPoint, _nextItemPoint, num);
			Refresh();
		}
	}

	public void Refresh()
	{
		Refresh(GameSystem<EstateSystem>.Instance().PioneerGradeInfo);
	}

	public void Refresh(PioneerGradeInfo info)
	{
		bool paid = info.IsPaid();
		_exchangedPoints.Clear();
		_exchangedPoints.AddRange(info.DailyExchangedPoints);
		int grade = info.Grade;
		float curGradePoint = info.Point;
		float lastRate = LastRate;
		LastRate = PioneerPointCalculator.Run(_exchangedPoints, ref grade, ref curGradePoint, paid, _curItemPoint);
		_minSpeed = Mathf.Max(150f, (float)Singleton<Pioneer>.Instance.GetNextGradePoint(grade) * 0.2f);
		if (!Mathf.Approximately(lastRate, LastRate) && this.RateChanged != null)
		{
			this.RateChanged();
		}
		Set(grade, curGradePoint, _exchangedPoints, paid);
	}

	[ExposedInEditor(null)]
	public void SetNextItemPoints(float points, bool immediately = false)
	{
		_nextItemPoint = points;
		if (immediately)
		{
			_curItemPoint = _nextItemPoint;
			Refresh();
		}
	}

	private void Set(int grade, float curPoint, Dictionary<float, float> exchangedPoints, bool paid)
	{
		PioneerCostExchangeRate pioneerCostExchangeRate = Singleton<Pioneer>.Instance.GetPioneerCostExchangeRate(grade);
		if (pioneerCostExchangeRate == null)
		{
			return;
		}
		_grade.text = grade.ToString();
		int nextGradePoint = Singleton<Pioneer>.Instance.GetNextGradePoint(grade);
		_point.text = ((nextGradePoint <= 0) ? string.Empty : $"<em> {curPoint:0.#} </em> <weak>/ {nextGradePoint} P </weak>");
		_pointBar.fillAmount = ((nextGradePoint <= 0) ? 0f : (curPoint / (float)nextGradePoint));
		_mutiplier.text = T._("x{0}", LastRate);
		float num = 0f;
		PioneerRate[] rates = pioneerCostExchangeRate.Rates;
		foreach (PioneerRate pioneerRate in rates)
		{
			if (!pioneerRate.Paid || paid)
			{
				num = pioneerRate.GetRemainPoint(exchangedPoints.Get(pioneerRate.Rate, 0f));
				if (num > 0f)
				{
					break;
				}
			}
		}
		float num2 = Mathf.Clamp01((nextGradePoint <= 0) ? 1f : ((curPoint + num) / (float)nextGradePoint));
		float num3 = (float)_pointBar.width * num2;
		if (num2 >= 1f)
		{
			_anchor.gameObject.SetActive(value: false);
		}
		else
		{
			_anchor.gameObject.SetActive(value: true);
			_anchor.transform.localPosition = new Vector3(num3, 0f, 0f);
		}
		float num4 = num3 + 10f;
		if (num4 + 5f + (float)_mutiplier.width > (float)_pointBar.width)
		{
			num4 = num3 - 10f - (float)_mutiplier.width;
		}
		_mutiplier.transform.localPosition = new Vector3(num4, 0f, 0f);
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (this.Clicked != null)
		{
			this.Clicked();
		}
	}
}
