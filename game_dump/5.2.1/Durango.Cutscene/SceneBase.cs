using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Cutscene;

public abstract class SceneBase : MonoBehaviour
{
	public abstract void Play([NotNull] Action callback, params object[] args);
}
