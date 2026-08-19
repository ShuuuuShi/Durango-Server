using System;
using System.Collections;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class MiniGameDanceNote : MonoBehaviour
{
	private const float FadeOutTime = 0.5f;

	private const float NoteSpawnDistance = 4f;

	[SerializeField]
	private UISprite _arrowSprite;

	[SerializeField]
	private UISprite _dotSprite;

	private ICoroutineBinder _sequence;

	public void Set(float startTime, MiniGameDanceAsset.DanceNoteData danceNoteData, UIWidget target, Action<float, MiniGameDanceNote, bool> destroyCallback)
	{
		base.gameObject.SetActive(value: true);
		this.StartCoroutine(ref _sequence, FlyingSequence(startTime, danceNoteData, target, destroyCallback));
	}

	private IEnumerator FlyingSequence(float startTime, MiniGameDanceAsset.DanceNoteData data, UIWidget target, Action<float, MiniGameDanceNote, bool> destroyCallback)
	{
		SetIcon(data);
		float radian = (float)Math.PI;
		Vector3 spawnPos = target.transform.position + new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0f) * 4f;
		base.transform.position = spawnPos;
		float reachAt = startTime + data.TimeKey;
		for (float cur = MiniGameDanceHelper.ElapsedTime; cur < reachAt; cur = MiniGameDanceHelper.ElapsedTime)
		{
			float alpha2 = (reachAt - cur) / data.TransitionTime;
			base.transform.position = Vector3.Lerp(spawnPos, target.transform.position, 1f - alpha2);
			yield return null;
		}
		float fadeSince = MiniGameDanceHelper.ElapsedTime;
		Vector3 previousVelocity = (target.transform.position - spawnPos) / data.TransitionTime;
		for (float i = fadeSince; i < fadeSince + 0.5f; i += MiniGameDanceHelper.DeltaTime)
		{
			float dt = MiniGameDanceHelper.ElapsedTime - fadeSince;
			float alpha = dt / 0.5f;
			base.transform.Translate(previousVelocity * MiniGameDanceHelper.DeltaTime);
			_arrowSprite.alpha = 1f - alpha;
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		destroyCallback(data.TimeKey, this, arg3: true);
	}

	private void SetIcon(MiniGameDanceAsset.DanceNoteData data)
	{
		if ((data.Pattern & MiniGameDanceAsset.DanceNoteData.Type.Dot) != 0)
		{
			_dotSprite.enabled = true;
			_arrowSprite.enabled = false;
			_dotSprite.color = GetColorByArrow(data);
			_dotSprite.alpha = 1f;
		}
		else if ((data.Pattern & MiniGameDanceAsset.DanceNoteData.ArrowPattern) != 0)
		{
			_dotSprite.enabled = false;
			_arrowSprite.enabled = true;
			_arrowSprite.color = GetColorByArrow(data);
			_arrowSprite.alpha = 1f;
			float rotation = MiniGameDanceHelper.GetRotation(data.Pattern);
			_arrowSprite.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);
		}
	}

	private Color GetColorByArrow(MiniGameDanceAsset.DanceNoteData danceNoteData)
	{
		return danceNoteData.Pattern switch
		{
			MiniGameDanceAsset.DanceNoteData.Type.Left => PresetColor.Aqua, 
			MiniGameDanceAsset.DanceNoteData.Type.Right => Color.yellow, 
			MiniGameDanceAsset.DanceNoteData.Type.Up => PresetColor.Lima, 
			MiniGameDanceAsset.DanceNoteData.Type.Down => PresetColor.WildStrawberry, 
			MiniGameDanceAsset.DanceNoteData.Type.Dot => PresetColor.SpringGreen, 
			_ => Color.white, 
		};
	}

	public void HitAndKillObject(float timeKey, Action<float, MiniGameDanceNote, bool> destroyCallback)
	{
		this.StopCoroutine(_sequence);
		destroyCallback(timeKey, this, arg3: false);
	}
}
