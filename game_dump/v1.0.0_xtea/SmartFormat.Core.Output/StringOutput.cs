using System.Text;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Core.Output;

public class StringOutput : IOutput
{
	private readonly StringBuilder output;

	public StringOutput()
	{
		output = new StringBuilder();
	}

	public StringOutput(int capacity)
	{
		output = new StringBuilder(capacity);
	}

	public StringOutput(StringBuilder output)
	{
		this.output = output;
	}

	public void Write(string text, IFormattingInfo formattingInfo)
	{
		output.Append(text);
	}

	public void Write(string text, int startIndex, int length, IFormattingInfo formattingInfo)
	{
		output.Append(text, startIndex, length);
	}

	public override string ToString()
	{
		return output.ToString();
	}
}
