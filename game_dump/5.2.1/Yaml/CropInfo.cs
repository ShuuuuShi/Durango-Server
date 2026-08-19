using System;
using Durango.Utils;
using NCalc;
using Newtonsoft.Json;

namespace Yaml;

public class CropInfo
{
	private class CropExpressionInfo
	{
		private readonly CropInfo _parent;

		private Expression _expression;

		private float? _minValue;

		private float? _maxValue;

		public CropExpressionInfo(CropInfo parent, string value)
		{
			_parent = parent;
			_expression = ExpressionParser.Parse(value);
		}

		private float GetValue(int level)
		{
			if (_expression == null)
			{
				return 0f;
			}
			_expression.Parameters["level"] = level;
			return Maths.ToFloat(_expression.Evaluate());
		}

		public float GetMinValue()
		{
			float? minValue = _minValue;
			if (!minValue.HasValue)
			{
				_minValue = GetValue(1);
			}
			return _minValue.Value;
		}

		public float GetMaxValue()
		{
			float? maxValue = _maxValue;
			if (!maxValue.HasValue)
			{
				_maxValue = GetValue(Math.Max(1, (_parent == null) ? 1 : _parent.MaxLevel));
			}
			return _maxValue.Value;
		}
	}

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "color_r")]
	public string ColorR;

	[JsonProperty(PropertyName = "color_g")]
	public string ColorG;

	[JsonProperty(PropertyName = "color_b")]
	public string ColorB;

	[JsonProperty(PropertyName = "max_level")]
	public int MaxLevel;

	private CropExpressionInfo _growsUntill;

	private CropExpressionInfo _requiredWater;

	private CropExpressionInfo _requiredFertilizer;

	[JsonProperty(PropertyName = "grows_until", Required = Required.Always)]
	public string GrowsUntill
	{
		set
		{
			_growsUntill = new CropExpressionInfo(this, value);
		}
	}

	[JsonProperty(PropertyName = "required_water", Required = Required.Always)]
	public string RequiredWater
	{
		set
		{
			_requiredWater = new CropExpressionInfo(this, value);
		}
	}

	[JsonProperty(PropertyName = "required_fertilizer", Required = Required.Always)]
	public string RequiredFertilizer
	{
		set
		{
			_requiredFertilizer = new CropExpressionInfo(this, value);
		}
	}

	public bool TryGetGrowsUntillRange(out float min, out float max)
	{
		if (_growsUntill == null)
		{
			min = 0f;
			max = 0f;
			return false;
		}
		min = _growsUntill.GetMinValue();
		max = _growsUntill.GetMaxValue();
		return true;
	}

	public bool TryGetRequiredWaterRange(out float min, out float max)
	{
		if (_requiredWater == null)
		{
			min = 0f;
			max = 0f;
			return false;
		}
		min = _requiredWater.GetMinValue();
		max = _requiredWater.GetMaxValue();
		return true;
	}

	public bool TryGetRequiredFertilizerRange(out float min, out float max)
	{
		if (_requiredFertilizer == null)
		{
			min = 0f;
			max = 0f;
			return false;
		}
		min = _requiredFertilizer.GetMinValue();
		max = _requiredFertilizer.GetMaxValue();
		return true;
	}
}
