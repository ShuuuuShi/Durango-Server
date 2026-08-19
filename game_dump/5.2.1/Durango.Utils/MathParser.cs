using System;
using System.Collections;
using System.Collections.Generic;

namespace Durango.Utils;

public class MathParser
{
	private Dictionary<Parameters, decimal> _Parameters = new Dictionary<Parameters, decimal>();

	private List<string> OperationOrder = new List<string>();

	public Dictionary<Parameters, decimal> Parameters
	{
		get
		{
			return _Parameters;
		}
		set
		{
			_Parameters = value;
		}
	}

	public MathParser()
	{
		OperationOrder.Add("/");
		OperationOrder.Add("*");
		OperationOrder.Add("-");
		OperationOrder.Add("+");
	}

	public decimal Calculate(string Formula)
	{
		try
		{
			string[] array = Formula.Split("/+-*()".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			foreach (KeyValuePair<Parameters, decimal> parameter in _Parameters)
			{
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (text != parameter.Key.ToString() && text.EndsWith(parameter.Key.ToString()))
					{
						Formula = Formula.Replace(text, (Convert.ToDecimal(text.Replace(parameter.Key.ToString(), "")) * parameter.Value).ToString());
					}
				}
				Formula = Formula.Replace(parameter.Key.ToString(), parameter.Value.ToString());
			}
			while (Formula.LastIndexOf("(") > -1)
			{
				int num = Formula.LastIndexOf("(");
				int num2 = Formula.IndexOf(")", num);
				decimal num3 = ProcessOperation(Formula.Substring(num + 1, num2 - num - 1));
				bool flag = false;
				if (num > 0 && Formula.Substring(num - 1, 1) != "(" && !OperationOrder.Contains(Formula.Substring(num - 1, 1)))
				{
					flag = true;
				}
				Formula = Formula.Substring(0, num) + (flag ? "*" : "") + num3 + Formula.Substring(num2 + 1);
			}
			return ProcessOperation(Formula);
		}
		catch (Exception innerException)
		{
			throw new Exception("Error Occured While Calculating. Check Syntax", innerException);
		}
	}

	private decimal ProcessOperation(string operation)
	{
		ArrayList arrayList = new ArrayList();
		string text = "";
		for (int i = 0; i < operation.Length; i++)
		{
			string text2 = operation.Substring(i, 1);
			if (OperationOrder.IndexOf(text2) > -1)
			{
				if (text != "")
				{
					arrayList.Add(text);
				}
				arrayList.Add(text2);
				text = "";
			}
			else
			{
				text += text2;
			}
		}
		arrayList.Add(text);
		text = "";
		foreach (string item in OperationOrder)
		{
			while (arrayList.IndexOf(item) > -1)
			{
				int num = arrayList.IndexOf(item);
				decimal number = Convert.ToDecimal(arrayList[num - 1]);
				decimal num2 = default(decimal);
				if (arrayList[num + 1].ToString() == "-")
				{
					arrayList.RemoveAt(num + 1);
					num2 = Convert.ToDecimal(arrayList[num + 1]) * -1m;
				}
				else
				{
					num2 = Convert.ToDecimal(arrayList[num + 1]);
				}
				arrayList[num] = CalculateByOperator(number, num2, item);
				arrayList.RemoveAt(num - 1);
				arrayList.RemoveAt(num);
			}
		}
		return Convert.ToDecimal(arrayList[0]);
	}

	private decimal CalculateByOperator(decimal number1, decimal number2, string op)
	{
		return op switch
		{
			"/" => number1 / number2, 
			"*" => number1 * number2, 
			"-" => number1 - number2, 
			"+" => number1 + number2, 
			_ => 0m, 
		};
	}
}
