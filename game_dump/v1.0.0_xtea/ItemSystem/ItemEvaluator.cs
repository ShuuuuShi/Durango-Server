using System;
using System.Collections.Generic;

namespace ItemSystem;

public class ItemEvaluator : IItemEvaluator
{
	private static Stack<bool> _commonEvalStack;

	private static Stack<string> _commonParseStack;

	private readonly string _value;

	private readonly Func<ItemData, string, bool> _predicate;

	private List<string> _expressonList;

	public ItemEvaluator(string text, Func<ItemData, string, bool> predicate)
	{
		_value = text;
		_predicate = predicate;
		if (!text.Contains("&") && !text.Contains("|"))
		{
			return;
		}
		if (_commonParseStack == null)
		{
			_commonParseStack = new Stack<string>();
		}
		else
		{
			_commonParseStack.Clear();
		}
		text = text.Replace(" ", string.Empty);
		text = text.Replace("\t", string.Empty);
		string text2 = string.Empty;
		_expressonList = new List<string>();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			switch (c)
			{
			case '(':
				_commonParseStack.Push(c.ToString());
				break;
			case ')':
			{
				if (!string.IsNullOrEmpty(text2))
				{
					_expressonList.Add(text2);
					num++;
					text2 = string.Empty;
				}
				bool flag = false;
				while (_commonParseStack.Count > 0)
				{
					flag = _commonParseStack.Peek().Equals("(");
					if (flag)
					{
						break;
					}
					_expressonList.Add(_commonParseStack.Pop());
				}
				if (!flag)
				{
					Error("Parenthesis is not matched - " + text);
					return;
				}
				_commonParseStack.Pop();
				break;
			}
			case '!':
			case '&':
			case '|':
			{
				if (!string.IsNullOrEmpty(text2))
				{
					_expressonList.Add(text2);
					num++;
					text2 = string.Empty;
				}
				string text3;
				switch (c)
				{
				case '!':
					text3 = "!";
					break;
				case '&':
					text3 = "&";
					num2++;
					break;
				default:
					text3 = "|";
					num2++;
					break;
				}
				if (num2 > num)
				{
					Error("Invalid Operator Count - " + text);
					return;
				}
				while (_commonParseStack.Count > 0 && Priority(_commonParseStack.Peek()) > Priority(text3))
				{
					_expressonList.Add(_commonParseStack.Pop());
				}
				_commonParseStack.Push(text3);
				break;
			}
			default:
				text2 += c;
				break;
			}
		}
		if (!string.IsNullOrEmpty(text2))
		{
			_expressonList.Add(text2);
		}
		while (_commonParseStack.Count > 0)
		{
			string text4 = _commonParseStack.Pop();
			if (text4 == "(")
			{
				Error("Parenthesis is not matched - " + text);
				break;
			}
			_expressonList.Add(text4);
		}
	}

	public virtual bool Evaluate(ItemData data)
	{
		if (data == null)
		{
			return false;
		}
		if (_expressonList == null)
		{
			if (_value.StartsWith("!"))
			{
				return !_predicate(data, _value);
			}
			return string.IsNullOrEmpty(_value) || _predicate(data, _value);
		}
		if (_commonEvalStack == null)
		{
			_commonEvalStack = new Stack<bool>();
		}
		else
		{
			_commonEvalStack.Clear();
		}
		for (int i = 0; i < _expressonList.Count; i++)
		{
			string text = _expressonList[i];
			bool item;
			switch (text)
			{
			case "|":
			{
				bool flag3 = _commonEvalStack.Pop();
				bool flag4 = _commonEvalStack.Pop();
				item = flag3 || flag4;
				break;
			}
			case "&":
			{
				bool flag = _commonEvalStack.Pop();
				bool flag2 = _commonEvalStack.Pop();
				item = flag && flag2;
				break;
			}
			case "!":
				item = !_commonEvalStack.Pop();
				break;
			default:
				item = _predicate(data, text);
				break;
			}
			_commonEvalStack.Push(item);
		}
		return _commonEvalStack.Pop();
	}

	private void Error(string msg)
	{
		_expressonList = null;
	}

	private static int Priority(string op)
	{
		return op switch
		{
			"|" => 0, 
			"&" => 1, 
			"!" => 2, 
			"(" => -1, 
			_ => throw new NotImplementedException(op + " is not implemented"), 
		};
	}
}
