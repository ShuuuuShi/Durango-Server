using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Utils;
using Messages;
using NCalc;
using Newtonsoft.Json;
using Yaml.Util;

namespace Yaml;

public struct ConstantPet
{
	[JsonProperty(PropertyName = "battle", Required = Required.Always)]
	public Battle Battle;

	private Expression _domesticateProbabilityExpression;

	[JsonProperty(PropertyName = "domesticate_decrease_limit", Required = Required.Always)]
	public float DomesticateDecreaseLimit;

	[JsonProperty(PropertyName = "performance_reference", Required = Required.Always)]
	public Dictionary<string, string[]> PerformanceReference;

	[JsonProperty(PropertyName = "reinify_tags", Required = Required.Always)]
	public string[] ReinifyTags;

	[JsonProperty(PropertyName = "milestone_level", Required = Required.Always)]
	public Dictionary<int, int[][]> MilestoneLevel;

	[JsonProperty(PropertyName = "default_feed_energy", Required = Required.Always)]
	public float DefaultFeedEnergy;

	private Expression _domesticateTimeExpression;

	private Expression _taskTimeExpression;

	private List<string> _domesticationParameters;

	[JsonProperty(PropertyName = "domesticate_probability", Required = Required.Always)]
	public string DomesticateProbability
	{
		set
		{
			_domesticateProbabilityExpression = ExpressionParser.Parse(value);
		}
	}

	[JsonProperty(PropertyName = "domesticate_time", Required = Required.Always)]
	public string DomesticateTime
	{
		set
		{
			_domesticateTimeExpression = ExpressionParser.Parse(value);
		}
	}

	[JsonProperty(PropertyName = "task_time", Required = Required.Always)]
	public string TaskTime
	{
		set
		{
			_taskTimeExpression = ExpressionParser.Parse(value);
		}
	}

	public float GetDomesticationProbability(float maxProb, float currentProb, IEnumerable<ItemData> items)
	{
		if (_domesticateProbabilityExpression == null)
		{
			return 0f;
		}
		_domesticateProbabilityExpression.Parameters.Clear();
		_domesticateProbabilityExpression.Parameters["max_prob"] = maxProb;
		_domesticateProbabilityExpression.Parameters["current_prob"] = currentProb;
		if (PerformanceReference != null && items != null)
		{
			foreach (KeyValuePair<string, string[]> item in PerformanceReference)
			{
				double num = 0.0;
				string[] value = item.Value;
				foreach (string key in value)
				{
					foreach (ItemData item2 in items)
					{
						num += (double)item2.GetFloatAttribute(key);
					}
				}
				_domesticateProbabilityExpression.Parameters[item.Key] = num;
			}
		}
		return _domesticateProbabilityExpression.ToSingle();
	}

	public bool IsDomesticationTimeFullyModified(DomesticationInfo target)
	{
		return IsDomesticationTimeFullyModified(target, target.DomesticateUntil);
	}

	public bool IsDomesticationTimeFullyModified(DomesticationInfo target, double predictedTime)
	{
		if (!target.DomesticationInProgress)
		{
			return false;
		}
		return (double)DomesticateDecreaseLimit >= (predictedTime - target.DomesticateSince) / target.TotalTime;
	}

	public double? GetDomesticationEndTime(DomesticationInfo target, IEnumerable<ItemData> items)
	{
		if (_domesticateTimeExpression == null)
		{
			return null;
		}
		_domesticateTimeExpression.Parameters.Clear();
		_domesticateTimeExpression.Parameters["starts_at"] = target.DomesticateSince;
		_domesticateTimeExpression.Parameters["total_time"] = target.TotalTime;
		_domesticateTimeExpression.Parameters["ends_at"] = target.DomesticateUntil;
		_domesticateTimeExpression.Parameters["decrease_limit"] = DomesticateDecreaseLimit;
		if (PerformanceReference != null && items != null)
		{
			foreach (KeyValuePair<string, string[]> item in PerformanceReference)
			{
				double num = 0.0;
				string[] value = item.Value;
				foreach (string key in value)
				{
					foreach (ItemData item2 in items)
					{
						num += (double)item2.GetFloatAttribute(key);
					}
				}
				_domesticateTimeExpression.Parameters[item.Key] = num;
			}
		}
		return _domesticateTimeExpression.ToDouble();
	}

	public double? GetTaskEndTime(TaskStatus taskStatus, IEnumerable<ItemData> items)
	{
		if (_taskTimeExpression == null)
		{
			return null;
		}
		PetTask petTask = ((!string.IsNullOrEmpty(taskStatus.TaskId)) ? SingletonDict<string, PetTask>.Get(taskStatus.TaskId) : null);
		if (petTask == null)
		{
			return null;
		}
		_taskTimeExpression.Parameters.Clear();
		_taskTimeExpression.Parameters["since"] = taskStatus.Since;
		_taskTimeExpression.Parameters["total_time"] = petTask.Duration;
		_taskTimeExpression.Parameters["until"] = taskStatus.Until;
		if (PerformanceReference != null && items != null)
		{
			foreach (KeyValuePair<string, string[]> item in PerformanceReference)
			{
				double num = 0.0;
				string[] value = item.Value;
				foreach (string key in value)
				{
					foreach (ItemData item2 in items)
					{
						num += (double)item2.GetFloatAttribute(key);
					}
				}
				_taskTimeExpression.Parameters[item.Key] = num;
			}
		}
		return _taskTimeExpression.ToDouble();
	}

	public List<string> GetDomesticationParameters()
	{
		if (_domesticationParameters == null && PerformanceReference != null)
		{
			HashSet<string> set = new HashSet<string>();
			Dictionary<string, string[]> references = PerformanceReference;
			EvaluateParameterHandler value = delegate(string n, ParameterArgs args)
			{
				if (references.ContainsKey(n))
				{
					set.Add(n);
				}
				args.Result = 0;
			};
			Expression[] array = new Expression[2] { _domesticateTimeExpression, _domesticateProbabilityExpression };
			Expression[] array2 = array;
			foreach (Expression expression in array2)
			{
				expression.EvaluateParameter += value;
				expression.Evaluate();
				expression.EvaluateParameter -= value;
			}
			_domesticationParameters = new List<string>(set);
		}
		return _domesticationParameters;
	}
}
