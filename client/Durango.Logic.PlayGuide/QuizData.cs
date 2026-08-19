using System.Collections.Generic;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Durango.Logic.PlayGuide;

public class QuizData
{
	private class Event
	{
		public string[] Conditions;

		public string[] Messages;

		public int Index;
	}

	public string[] Choices;

	public string[] Solutions;

	public string Message;

	private readonly List<Event> _events = new List<Event>();

	[NotNull]
	public string[] GetMessages(List<string> selected, out int index)
	{
		for (int i = 0; i < _events.Count; i++)
		{
			Event @event = _events[i];
			if (@event.Conditions.Length > selected.Count || KUtility.GetSize(@event.Messages) <= 0)
			{
				continue;
			}
			bool flag = true;
			int j = 0;
			for (int num = @event.Conditions.Length; j < num; j++)
			{
				if (@event.Conditions[j] != selected[j] && @event.Conditions[j] != "*")
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				index = @event.Index;
				return @event.Messages;
			}
		}
		index = -1;
		return new string[0];
	}

	public static QuizData Parse(JObject obj)
	{
		QuizData quizData = new QuizData();
		quizData.Message = obj.Get<string>("message");
		quizData.Solutions = obj.GetArray<string>("solutions");
		quizData.Choices = obj.GetArray<string>("choices");
		int num = 0;
		foreach (KeyValuePair<string, JToken> item in obj)
		{
			if (item.Key == "message" || item.Key == "solutions" || item.Key == "choices")
			{
				continue;
			}
			Event @event = new Event();
			@event.Conditions = item.Key.SplitAndTrim('|');
			if (@event.Conditions.Length == 0)
			{
				continue;
			}
			if (item.Value.Type == JTokenType.String)
			{
				@event.Messages = new string[1] { item.Value.GetString() };
			}
			else
			{
				if (item.Value.Type != JTokenType.Array)
				{
					continue;
				}
				@event.Messages = item.Value.ToObject<string[]>();
			}
			@event.Index = num;
			num++;
			quizData._events.Add(@event);
		}
		quizData._events.Sort(Comparison);
		return quizData;
	}

	private static int Comparison(Event event1, Event event2)
	{
		int size = KUtility.GetSize(event1.Conditions);
		int size2 = KUtility.GetSize(event2.Conditions);
		if (size != size2)
		{
			return (size < size2) ? 1 : (-1);
		}
		for (int num = size - 1; num >= 0; num--)
		{
			bool flag = event1.Conditions[num] == "*";
			bool flag2 = event2.Conditions[num] == "*";
			if (flag != flag2)
			{
				return flag ? 1 : (-1);
			}
		}
		return 0;
	}
}
