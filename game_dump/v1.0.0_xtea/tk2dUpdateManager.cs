using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("")]
public class tk2dUpdateManager : MonoBehaviour
{
	private static tk2dUpdateManager inst;

	[SerializeField]
	private List<tk2dTextMesh> textMeshes = new List<tk2dTextMesh>(64);

	private static tk2dUpdateManager Instance
	{
		get
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			if ((Object)(object)inst == (Object)null)
			{
				inst = Object.FindObjectOfType(typeof(tk2dUpdateManager)) as tk2dUpdateManager;
				if ((Object)(object)inst == (Object)null)
				{
					GameObject val = new GameObject("@tk2dUpdateManager");
					((Object)val).hideFlags = (HideFlags)3;
					inst = val.AddComponent<tk2dUpdateManager>();
					Object.DontDestroyOnLoad((Object)(object)val);
				}
			}
			return inst;
		}
	}

	public static void QueueCommit(tk2dTextMesh textMesh)
	{
		Instance.QueueCommitInternal(textMesh);
	}

	public static void FlushQueues()
	{
		Instance.FlushQueuesInternal();
	}

	private void OnEnable()
	{
		((MonoBehaviour)this).StartCoroutine(coSuperLateUpdate());
	}

	private void LateUpdate()
	{
		FlushQueuesInternal();
	}

	private IEnumerator coSuperLateUpdate()
	{
		FlushQueuesInternal();
		yield break;
	}

	private void QueueCommitInternal(tk2dTextMesh textMesh)
	{
		textMeshes.Add(textMesh);
	}

	private void FlushQueuesInternal()
	{
		int count = textMeshes.Count;
		for (int i = 0; i < count; i++)
		{
			tk2dTextMesh tk2dTextMesh2 = textMeshes[i];
			if ((Object)(object)tk2dTextMesh2 != (Object)null)
			{
				tk2dTextMesh2.DoNotUse__CommitInternal();
			}
		}
		textMeshes.Clear();
	}
}
