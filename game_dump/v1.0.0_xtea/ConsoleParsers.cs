using System.Collections;
using UnityEngine;

internal class ConsoleParsers : MonoBehaviour
{
	private void OnEnable()
	{
		((MonoBehaviour)this).StartCoroutine(InitParsers());
	}

	private IEnumerator InitParsers()
	{
		yield return null;
		Console.Instance.RegisterParser(typeof(Vector3), new ParserCallback(parseVector3));
	}

	private bool parseVector3(string v, out object obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		string[] array = v.Split(',');
		if (!float.TryParse(array[0], out var result))
		{
			Console.Instance.Print("Invalid Vector3: " + array[0] + " is not a float");
			obj = null;
			return false;
		}
		val.x = result;
		if (!float.TryParse(array[1], out result))
		{
			Console.Instance.Print("Invalid Vector3: " + array[1] + " is not a float");
			obj = null;
			return false;
		}
		val.y = result;
		if (!float.TryParse(array[2], out result))
		{
			Console.Instance.Print("Invalid Vector3: " + array[2] + " is not a float");
			obj = null;
			return false;
		}
		val.z = result;
		obj = val;
		return true;
	}

	private void vector3ParseTest(Vector3 vector)
	{
		Console.Instance.Print(vector.x.ToString());
		Console.Instance.Print(vector.y.ToString());
		Console.Instance.Print(vector.z.ToString());
	}
}
