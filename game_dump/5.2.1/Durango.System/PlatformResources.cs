using Durango.Utils;
using UnityEngine;

namespace Durango.System;

public class PlatformResources : Singleton<PlatformResources>
{
	[SerializeField]
	private GameObject _femaleReference;

	[SerializeField]
	private GameObject _maleReference;

	public GameObject FemaleReference => _femaleReference;

	public GameObject MaleReference => _maleReference;
}
