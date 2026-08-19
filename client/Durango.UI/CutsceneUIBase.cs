using System;
using UnityEngine;

namespace Durango.UI;

public abstract class CutsceneUIBase : MonoBehaviour
{
	public abstract void Open(Action callback);

	public abstract void Close(Action callback);
}
