using System;
using System.Collections;
using ItemSystem;
using UnityEngine;

public class LootingItemGroup : MonoBehaviour
{
	[SerializeField]
	private ItemIconTex _icon;

	[SerializeField]
	private float _delayInMiliseconds;

	[SerializeField]
	private float[] _times;

	[SerializeField]
	private Vector3[] _positions;

	[SerializeField]
	private float[] _scales;

	[SerializeField]
	private float[] _alphas;

	[SerializeField]
	private bool[] _sqrts;

	[SerializeField]
	private AudioClipType _lootingAudio;

	private bool _playSound;

	private int _cursor;

	private float _initTime;

	private float _elapsedTime;

	private float _animationNormal;

	private bool _isCoroutinePlay;

	private void Awake()
	{
		SoundManager.Cache(_lootingAudio);
		((Component)_icon).gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		GameSystem<InventorySystem>.Instance().OnCollectItem += OnCollectItem;
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Combine(UICamera.onScreenResize, new UICamera.OnScreenResize(OnScreenResize));
		OnScreenResize();
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().OnCollectItem -= OnCollectItem;
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Remove(UICamera.onScreenResize, new UICamera.OnScreenResize(OnScreenResize));
	}

	private void OnScreenResize()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector((float)Screen.width, (float)Screen.height);
		ref Vector3 reference = ref _positions[_positions.Length - 1];
		reference = -((Vector3)(ref val)).normalized * 150f;
	}

	private void OnCollectItem(ItemData item)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Begin(item, Vector3.zero);
	}

	private IEnumerator coMove()
	{
		if (_isCoroutinePlay)
		{
			yield break;
		}
		_isCoroutinePlay = true;
		while (true)
		{
			if (Time.time < _initTime)
			{
				yield return null;
			}
			_animationNormal = Mathf.Sin((_elapsedTime - _times[_cursor]) * (float)Math.PI / 2f / (_times[_cursor + 1] - _times[_cursor]));
			if (_sqrts[_cursor])
			{
				_animationNormal = Mathf.Sqrt(_animationNormal);
			}
			Vector3 pos = _positions[_cursor] + (_positions[_cursor + 1] - _positions[_cursor]) * _animationNormal;
			Vector3 scale = (_scales[_cursor] + (_scales[_cursor + 1] - _scales[_cursor]) * _animationNormal) * new Vector3(1f, 1f, 0f);
			_icon.UITexture.alpha = _alphas[_cursor] + (_alphas[_cursor + 1] - _alphas[_cursor]) * _animationNormal;
			((Component)_icon).transform.localPosition = pos;
			((Component)_icon).transform.localScale = scale;
			if (_elapsedTime > _times[_cursor + 1])
			{
				_cursor++;
				if (!_playSound)
				{
					_playSound = true;
					SoundManager.Play((string)_lootingAudio, loop: false, default(SoundManager.PitchRange));
				}
				if (_cursor > _times.Length - 2)
				{
					break;
				}
			}
			_elapsedTime = Time.time - _initTime;
			yield return null;
		}
		((Component)_icon).gameObject.SetActive(false);
		_isCoroutinePlay = false;
	}

	public void Begin(ItemData item, Vector3 beginPos)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		((Component)_icon).gameObject.SetActive(true);
		_playSound = false;
		_cursor = 0;
		_elapsedTime = 0f;
		_initTime = Time.time + _delayInMiliseconds / 1000f;
		((Component)_icon).transform.localPosition = beginPos;
		_icon.SetIcon(item);
		if (!_isCoroutinePlay)
		{
			((MonoBehaviour)this).StartCoroutine(coMove());
		}
	}
}
