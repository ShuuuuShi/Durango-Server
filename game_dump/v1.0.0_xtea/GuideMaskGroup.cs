using System.Collections.Generic;
using UnityEngine;

public class GuideMaskGroup : MonoBehaviour
{
	private float _circleSize = 80f;

	[SerializeField]
	private List<UISprite> _circleSprites;

	[SerializeField]
	private UISprite _bgSpritesLT;

	[SerializeField]
	private UISprite _bgSpritesRT;

	[SerializeField]
	private UISprite _bgSpritesRB;

	[SerializeField]
	private UISprite _bgSpritesLB;

	[SerializeField]
	private GameObject _touchHand;

	[SerializeField]
	private GameObject _holdHand;

	[SerializeField]
	private GameObject _virtualStickDemo;

	private List<UISprite> _bgSprites;

	private List<UISprite> BgSprites
	{
		get
		{
			if (_bgSprites == null)
			{
				_bgSprites = new List<UISprite>();
				_bgSprites.AddRange(_circleSprites);
				_bgSprites.Add(_bgSpritesLT);
				_bgSprites.Add(_bgSpritesRT);
				_bgSprites.Add(_bgSpritesRB);
				_bgSprites.Add(_bgSpritesLB);
			}
			return _bgSprites;
		}
	}

	private void Awake()
	{
	}

	private void Start()
	{
		if (_circleSprites.Count > 0)
		{
			_circleSize = _circleSprites[0].GetAtlasSprite().width;
		}
	}

	private void Update()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (_circleSprites.Count > 0)
		{
			Vector3 localPosition = ((Component)_circleSprites[0]).transform.localPosition;
			localPosition.x = Mathf.Round(localPosition.x);
			localPosition.y = Mathf.Round(localPosition.y);
			UpdateCirclePos(localPosition);
			UpdateBgPos(localPosition);
		}
	}

	private void UpdateCirclePos(Vector3 pos)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		int count = _circleSprites.Count;
		for (int i = 0; i < count; i++)
		{
			((Component)_circleSprites[i]).transform.localPosition = pos;
		}
	}

	public void SetTouchPos(Vector3 pos)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		pos.x = Mathf.Round(pos.x);
		pos.y = Mathf.Round(pos.y);
		((Component)_circleSprites[0]).transform.localPosition = pos;
	}

	public void SetTouchHandShow(bool show)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		_touchHand.SetActive(show);
		if (show)
		{
			_touchHand.transform.localPosition = ((Component)_circleSprites[0]).transform.localPosition;
		}
	}

	public void SetHoldHandShow(bool show)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		_holdHand.SetActive(show);
		if (show)
		{
			_holdHand.transform.localPosition = ((Component)_circleSprites[0]).transform.localPosition;
		}
	}

	public void SetVirtualStickDemoShow(bool show)
	{
		_virtualStickDemo.SetActive(show);
	}

	private void UpdateBgPos(Vector3 pos)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Screen.width * MainCamera.NGUIScale();
		float num2 = (float)Screen.height * MainCamera.NGUIScale();
		((Component)_bgSpritesLT).transform.localPosition = Vector3.zero;
		_bgSpritesLT.width = (int)(pos.x + _circleSize);
		_bgSpritesLT.height = (int)(0f - pos.y - _circleSize + 0.5f);
		((Component)_bgSpritesRT).transform.localPosition = new Vector3(pos.x + _circleSize, 0f, 0f);
		_bgSpritesRT.width = (int)(num - pos.x - _circleSize);
		_bgSpritesRT.height = (int)(0f - pos.y + _circleSize + 0.5f);
		((Component)_bgSpritesRB).transform.localPosition = new Vector3(pos.x - _circleSize, pos.y - _circleSize, 0f);
		_bgSpritesRB.width = (int)(num - pos.x + _circleSize);
		_bgSpritesRB.height = (int)(num2 + pos.y - _circleSize);
		((Component)_bgSpritesLB).transform.localPosition = new Vector3(0f, pos.y + _circleSize, 0f);
		_bgSpritesLB.width = (int)(pos.x - _circleSize);
		_bgSpritesLB.height = (int)(num2 + pos.y + _circleSize);
	}

	public void HelperOnly(bool helperOnly)
	{
		for (int i = 0; i < BgSprites.Count; i++)
		{
			((Component)BgSprites[i]).gameObject.SetActive(!helperOnly);
		}
	}
}
