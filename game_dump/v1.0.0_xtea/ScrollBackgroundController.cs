using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using UnityEngine;

public class ScrollBackgroundController : KSingleton<ScrollBackgroundController>
{
	private List<GameObject> _objects = new List<GameObject>();

	[SerializeField]
	private float _speed = 300000f;

	[SerializeField]
	private float _blockSize = 2000f;

	public Color _curBGColor = Color.white;

	public Color _curTreeColor = Color.white;

	public Color _curGodRayColor = Color.white;

	[SerializeField]
	private Color _dayBGColor = Color.white;

	[SerializeField]
	private Color _daytTreeColor = new Color(0.39f, 0.39f, 0.66f, 1f);

	[SerializeField]
	private Color _nightBGColor = new Color(0.12f, 0.12f, 0.2f, 1f);

	[SerializeField]
	private Color _nightTreeColor = new Color(0.12f, 0.12f, 0.2f, 1f);

	[SerializeField]
	private List<GameObject> tree_groups_normal = new List<GameObject>();

	[SerializeField]
	private List<GameObject> tree_groups_thunder = new List<GameObject>();

	[SerializeField]
	private List<GameObject> _godRays = new List<GameObject>();

	private IEnumerator Start()
	{
		_curBGColor = _dayBGColor;
		_curTreeColor = _daytTreeColor;
		SetTreeVisible(bNormal: true, bThunder: false);
		int count = ((Component)this).transform.GetChild(0).childCount;
		for (int l = 0; l < count; l++)
		{
			_objects.Add(((Component)((Component)this).transform.GetChild(0).GetChild(l)).gameObject);
		}
		_objects.Sort((GameObject v1, GameObject v2) => (int)(v2.transform.localPosition.z - v1.transform.localPosition.z));
		float bound = _objects[count - 1].transform.localPosition.z - _blockSize;
		float prevTime = Time.time;
		int godRayCount = _godRays.Count;
		while (true)
		{
			float dt = Time.time - prevTime;
			prevTime = Time.time;
			for (int k = 0; k < count; k++)
			{
				Vector3 curPos = _objects[k].transform.localPosition;
				curPos.z = Mathf.Repeat(curPos.z - dt * _speed, bound);
				_objects[k].GetComponent<Renderer>().material.color = _curBGColor;
				_objects[k].transform.localPosition = curPos;
			}
			for (int j = 0; j < godRayCount; j++)
			{
				if (Object.op_Implicit((Object)(object)_godRays[j]))
				{
					_godRays[j].GetComponent<Renderer>().material.color = _curGodRayColor;
				}
			}
			int countTreeNormal = tree_groups_normal.Count;
			for (int i = 0; i < countTreeNormal; i++)
			{
				Renderer[] materials = tree_groups_normal[i].GetComponentsInChildren<Renderer>();
				int countMaterial = materials.Length;
				for (int m = 0; m < countMaterial; m++)
				{
					materials[m].material.color = _curTreeColor;
				}
			}
			yield return (object)new WaitForSeconds(0.03f);
		}
	}

	public void SetTreeVisible(bool bNormal, bool bThunder)
	{
		int count = tree_groups_normal.Count;
		for (int i = 0; i < count; i++)
		{
			if (Object.op_Implicit((Object)(object)tree_groups_normal[i]))
			{
				tree_groups_normal[i].SetActive(bNormal);
			}
		}
		count = tree_groups_thunder.Count;
		for (int j = 0; j < count; j++)
		{
			if (Object.op_Implicit((Object)(object)tree_groups_thunder[j]))
			{
				tree_groups_thunder[j].SetActive(bThunder);
			}
		}
	}

	public void PlayTunnelEffect(float _BG_TunnelDelay, float _BG_TunnelFadeTime, float _BG_TunnelDuration)
	{
		((MonoBehaviour)this).StartCoroutine(coBG_TunnelEffect(_BG_TunnelDelay, _BG_TunnelFadeTime, _BG_TunnelDuration));
	}

	private IEnumerator coBG_TunnelEffect(float _BG_TunnelDelay, float _BG_TunnelFadeTime, float _BG_TunnelDuration)
	{
		_curBGColor = _dayBGColor;
		_curTreeColor = _daytTreeColor;
		_curGodRayColor = Color.white;
		yield return (object)new WaitForSeconds(_BG_TunnelDelay);
		TweenParms parms = new TweenParms();
		parms.Prop("_curBGColor", (object)new Color(0f, 0f, 0f, 0f));
		parms.Ease((EaseType)5);
		HOTween.To((object)this, _BG_TunnelFadeTime, parms);
		TweenParms parmsGodray = new TweenParms();
		parmsGodray.Prop("_curGodRayColor", (object)new Color(0f, 0f, 0f, 0f));
		parmsGodray.Ease((EaseType)5);
		HOTween.To((object)this, _BG_TunnelFadeTime, parmsGodray);
		SetTreeVisible(bNormal: false, bThunder: false);
		yield return (object)new WaitForSeconds(_BG_TunnelDuration);
		TweenParms parms2 = new TweenParms();
		parms2.Prop("_curBGColor", (object)_nightBGColor);
		parms2.Ease((EaseType)5);
		HOTween.To((object)this, _BG_TunnelFadeTime, parms2);
		TweenParms parms3 = new TweenParms();
		parms3.Prop("_curTreeColor", (object)_nightTreeColor);
		parms3.Ease((EaseType)5);
		HOTween.To((object)this, _BG_TunnelFadeTime, parms3);
		SetTreeVisible(bNormal: true, bThunder: false);
	}
}
