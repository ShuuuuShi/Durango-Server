using System.Collections;
using UnityEngine;

public class IntroCutSceneGroup : MonoBehaviour
{
	public GameObject[] _cutScenes;

	public AuthMenuGroup _authMenuGroup;

	public AudioClip _prologueBGM;

	private AudioSource _bgmAudioSource;

	public float _bgmBeginDelay;

	private int _curScene = -1;

	private bool _finished;

	private float _lastSkipTime;

	private void Start()
	{
		_bgmAudioSource = GameObject.Find("BGMSound").GetComponent<AudioSource>();
		((MonoBehaviour)this).StartCoroutine(ChangeTunes(_prologueBGM, _bgmBeginDelay, 2f));
		GameObject[] cutScenes = _cutScenes;
		foreach (GameObject val in cutScenes)
		{
			val.SetActive(false);
		}
		ShowNextScene();
	}

	public void ShowNextScene()
	{
		if (_cutScenes.Length <= _curScene + 1)
		{
			if (!_finished)
			{
				OnFinishIntro();
			}
			return;
		}
		_curScene++;
		if (_curScene >= 1)
		{
			for (int i = 0; i < _curScene; i++)
			{
				CutSceneDirector component = _cutScenes[i].GetComponent<CutSceneDirector>();
				component.ForceEndNarrative();
				_cutScenes[i].SetActive(false);
			}
		}
		_cutScenes[_curScene].SetActive(true);
	}

	private void TrySkipCurrentScene()
	{
		if (!(Time.time - _lastSkipTime < 0.25f))
		{
			_lastSkipTime = Time.time;
			CutSceneDirector component = _cutScenes[_curScene].GetComponent<CutSceneDirector>();
			if (!component.SkipCurrentNarrative())
			{
				ShowNextScene();
			}
		}
	}

	private void OnFinishIntro()
	{
		_finished = true;
		GameObject[] cutScenes = _cutScenes;
		foreach (GameObject val in cutScenes)
		{
			val.SetActive(false);
		}
		_authMenuGroup.OnFinishIntro();
		((MonoBehaviour)this).StartCoroutine(FadeOut(1f));
	}

	private IEnumerator ChangeTunes(AudioClip chart, float bgmBeginDelay, float fadeRate)
	{
		if (!((Object)(object)chart == (Object)null))
		{
			yield return ((MonoBehaviour)this).StartCoroutine(FadeOut(fadeRate));
			yield return (object)new WaitForSeconds(bgmBeginDelay);
			_bgmAudioSource.clip = chart;
			_bgmAudioSource.Play();
			yield return ((MonoBehaviour)this).StartCoroutine(FadeIn(fadeRate));
		}
	}

	private IEnumerator FadeOut(float fadeRate)
	{
		while (_bgmAudioSource.volume > 0.1f)
		{
			_bgmAudioSource.volume = Mathf.Lerp(_bgmAudioSource.volume, 0f, fadeRate * Time.deltaTime);
			yield return null;
		}
		_bgmAudioSource.volume = 0f;
	}

	private IEnumerator FadeIn(float fadeRate)
	{
		while (_bgmAudioSource.volume < 0.9f)
		{
			_bgmAudioSource.volume = Mathf.Lerp(_bgmAudioSource.volume, 1f, fadeRate * Time.deltaTime);
			yield return null;
		}
		_bgmAudioSource.volume = 1f;
	}

	private void Update()
	{
		if (!_finished && Input.GetMouseButtonUp(0))
		{
			TrySkipCurrentScene();
		}
	}
}
