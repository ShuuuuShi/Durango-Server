using System.Collections.Generic;
using Durango.Prologue;
using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueGuideMaskGroup : MonoBehaviour
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
	private GameObject _virtualStickDemo;

	private List<UISprite> _bgSprites;

	public Vector3 TargetPos => _circleSprites[0].transform.localPosition;

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

	protected virtual void Awake()
	{
		base.gameObject.SetActive(value: false);
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
		if (_circleSprites.Count > 0)
		{
			Vector3 targetPos = TargetPos;
			targetPos.x = Mathf.Round(targetPos.x);
			targetPos.y = Mathf.Round(targetPos.y);
			UpdateCirclePos(targetPos);
			UpdateBgPos(targetPos);
		}
	}

	private void UpdateCirclePos(Vector3 pos)
	{
		int count = _circleSprites.Count;
		for (int i = 0; i < count; i++)
		{
			_circleSprites[i].transform.localPosition = pos;
		}
	}

	public void SetTouchPos(Vector3 pos)
	{
		pos.x = Mathf.Round(pos.x);
		pos.y = Mathf.Round(pos.y);
		_circleSprites[0].transform.localPosition = pos;
	}

	public virtual void SetType(string type)
	{
		EnableTouchHand(type == "Click" || type == "Select");
		EnableVirtualStick(type == "VirtualStick");
	}

	public void EnableTouchHand(bool show)
	{
		_touchHand.SetActive(show);
		if (show)
		{
			_touchHand.transform.localPosition = TargetPos;
			PrologueClickTargetLocator locator = new PrologueClickTargetLocator(_touchHand.transform);
			Singleton<PrologueManager>.Instance().PlayGuideHelper.EnableClickTarget(locator);
		}
		else
		{
			Singleton<PrologueManager>.Instance().PlayGuideHelper.DisableClickTarget();
		}
	}

	public void EnableVirtualStick(bool show)
	{
		_virtualStickDemo.SetActive(show);
	}

	private void UpdateBgPos(Vector3 pos)
	{
		float num = (float)Screen.width * MainCamera.NGUIScale();
		float num2 = (float)Screen.height * MainCamera.NGUIScale();
		_bgSpritesLT.transform.localPosition = Vector3.zero;
		_bgSpritesLT.width = (int)(pos.x + _circleSize);
		_bgSpritesLT.height = (int)(0f - pos.y - _circleSize + 0.5f);
		_bgSpritesRT.transform.localPosition = new Vector3(pos.x + _circleSize, 0f, 0f);
		_bgSpritesRT.width = (int)(num - pos.x - _circleSize);
		_bgSpritesRT.height = (int)(0f - pos.y + _circleSize + 0.5f);
		_bgSpritesRB.transform.localPosition = new Vector3(pos.x - _circleSize, pos.y - _circleSize, 0f);
		_bgSpritesRB.width = (int)(num - pos.x + _circleSize);
		_bgSpritesRB.height = (int)(num2 + pos.y - _circleSize);
		_bgSpritesLB.transform.localPosition = new Vector3(0f, pos.y + _circleSize, 0f);
		_bgSpritesLB.width = (int)(pos.x - _circleSize);
		_bgSpritesLB.height = (int)(num2 + pos.y + _circleSize);
	}

	public void HelperOnly(bool helperOnly)
	{
		for (int i = 0; i < BgSprites.Count; i++)
		{
			BgSprites[i].gameObject.SetActive(!helperOnly);
		}
	}

	public static void Show()
	{
		PrologueGuideMaskGroup prologueGuideMaskGroup = UIManager.FindScript<PrologueGuideMaskGroup>();
		if (prologueGuideMaskGroup != null)
		{
			prologueGuideMaskGroup.gameObject.SetActive(value: true);
		}
	}

	public static void Hide()
	{
		PrologueGuideMaskGroup prologueGuideMaskGroup = UIManager.FindScript<PrologueGuideMaskGroup>();
		if (prologueGuideMaskGroup != null)
		{
			prologueGuideMaskGroup.gameObject.SetActive(value: false);
		}
	}
}
