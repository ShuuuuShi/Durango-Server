using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartFormat.Core.Parsing;

public class ParsingErrors : Exception
{
	public class ParsingIssue
	{
		public int Index { get; private set; }

		public int Length { get; private set; }

		public string Issue { get; private set; }

		public ParsingIssue(string issue, int index, int length)
		{
			Issue = issue;
			Index = index;
			Length = length;
		}
	}

	private readonly Format result;

	public List<ParsingIssue> Issues { get; private set; }

	public bool HasIssues => Issues.Count > 0;

	public string MessageShort => string.Format("The format string has {0} issue{1}: {2}", Issues.Count, (Issues.Count != 1) ? "s" : string.Empty, string.Join(", ", Issues.Select((ParsingIssue i) => i.Issue).ToArray()));

	public override string Message
	{
		get
		{
			string text = string.Empty;
			int num = 0;
			foreach (ParsingIssue issue in Issues)
			{
				text += new string('-', issue.Index - num);
				if (issue.Length > 0)
				{
					text += new string('^', Math.Max(issue.Length, 1));
					num = issue.Index + issue.Length;
				}
				else
				{
					text += '^';
					num = issue.Index + 1;
				}
			}
			return string.Format("The format string has {0} issue{1}:\n{2}\nIn: \"{3}\"\nAt:  {4} ", Issues.Count, (Issues.Count != 1) ? "s" : string.Empty, string.Join(", ", Issues.Select((ParsingIssue i) => i.Issue).ToArray()), result.baseString, text);
		}
	}

	public ParsingErrors(Format result)
	{
		this.result = result;
		Issues = new List<ParsingIssue>();
	}

	public void AddIssue(Format parent, string issue, int startIndex, int endIndex)
	{
		Issues.Add(new ParsingIssue(issue, startIndex, endIndex - startIndex));
	}
}
