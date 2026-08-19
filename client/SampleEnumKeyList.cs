using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[EnumType(typeof(SampleEnum))]
public class SampleEnumKeyList : EnumKeyList
{
	[SerializeField]
	private List<SampleClass> _values;
}
