using System.Text.RegularExpressions;

namespace SharpRaven.Logging.Filters;

public class CreditCardFilter : IFilter
{
	public string Filter(string input)
	{
		Regex regex = new Regex("\\b(?:\\d[ -]*?){13,16}\\b", RegexOptions.IgnoreCase);
		regex.Replace(input, (Match m) => IsValidCreditCardNumber(m.Value) ? "####-CC-TRUNCATED-####" : m.Value);
		return input;
	}

	private bool IsValidCreditCardNumber(string number)
	{
		number.Replace("-", string.Empty);
		number.Replace(" ", string.Empty);
		int[] array = new int[10] { 0, 1, 2, 3, 4, -4, -3, -2, -1, 0 };
		int num = 0;
		char[] array2 = number.ToCharArray();
		for (int num2 = array2.Length - 1; num2 > -1; num2--)
		{
			int num3 = array2[num2] - 48;
			num += num3;
			if ((num2 - array2.Length) % 2 == 0)
			{
				num += array[num3];
			}
		}
		return num % 10 == 0;
	}
}
