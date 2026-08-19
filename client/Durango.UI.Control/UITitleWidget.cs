using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Durango.UI.Control;

public class UITitleWidget : UIWidget
{
	[SerializeField]
	protected UISprite Background;

	[FormerlySerializedAs("_layout")]
	[SerializeField]
	protected RectLayout Layout;

	[SerializeField]
	private UIWidget _closeButton;

	[SerializeField]
	private UIWidget _backButton;

	[FormerlySerializedAs("_titleLabel")]
	[SerializeField]
	protected UILabel TitleLabel;

	private Transform _titleNextContainer;

	private Vector3 _titleNextOffset;

	private bool? _isCloneArea;

	public UIBase Parent { get; protected set; }

	public bool IsCloseButtonVisible => _closeButton != null && _closeButton.gameObject.activeInHierarchy;

	public bool IsBackButtonVisible => _backButton != null && _backButton.gameObject.activeInHierarchy;

	public event Action OnClose;

	public event Action OnBack;

	public event Action<bool> CloseButtonVisibilityChanged;

	public event Action<bool> BackButtonVisibilityChanged;

	public void SetTitleLabelPivot(Pivot newPivot)
	{
		if (!(TitleLabel == null))
		{
			TitleLabel.pivot = newPivot;
		}
	}


	/// <summary>
	/// [แก้เอง] หา UIWidget ลูกที่ชื่อ "มีคำนี้อยู่" (ไม่สนตัวพิมพ์) — ใช้กู้กรณี SerializeField หลุด
	/// </summary>
	private UIWidget FindWidgetByName(params string[] names)
	{
		UIWidget[] all = GetComponentsInChildren<UIWidget>(includeInactive: true);
		for (int i = 0; i < all.Length; i++)
		{
			if (all[i] == null || all[i].gameObject == base.gameObject)
			{
				continue;
			}
			string n = all[i].name.ToLowerInvariant();
			for (int j = 0; j < names.Length; j++)
			{
				if (n.Contains(names[j]))
				{
					return all[i];
				}
			}
		}
		return null;
	}

	protected override void OnStart()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Parent = UIUtility.FindComponentInParent<UIBase>(base.gameObject);

		// [แก้เอง] อาการ "กดกากบาทแล้วไม่ปิด" — กดแล้วไม่มีอะไรเกิดขึ้นและไม่มี error ใน log เลย
		// แปลว่า onClick ไม่เคยถูกผูก ⇒ _closeButton (SerializeField) เป็น null
		// ซึ่งเข้ากับอาการ asset พังของบิลด์นี้ (resources.assets แจ้ง "is corrupted!" ตั้งแต่บูต
		// และ FatigueGaugeScrollSprite ก็เจอ sprite เป็น null แบบเดียวกัน)
		//
		// หาปุ่มจากชื่อลูกใน hierarchy แทน แล้วผูกให้เอง
		Debug.LogWarning("[ตรวจ] UITitleWidget.OnStart '" + base.name + "' parent=" + (Parent == null ? "null" : Parent.name)
			+ " closeButton=" + (_closeButton == null ? "null" : _closeButton.name));
		if (_closeButton == null)
		{
			_closeButton = FindWidgetByName("close", "btn_close", "closebutton", "button_close");
			Debug.LogWarning(_closeButton == null
				? "[แก้เอง] UITitleWidget '" + base.name + "': ไม่มีปุ่มปิด และหาจากชื่อลูกก็ไม่เจอ — ปุ่มกากบาทจะกดไม่ได้"
				: "[แก้เอง] UITitleWidget '" + base.name + "': _closeButton เป็น null — ผูกกับ '" + _closeButton.name + "' ที่หาเจอจากชื่อแทน");
		}
		if (_closeButton != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(_closeButton.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
			{
				Debug.LogWarning("[ตรวจ] กดกากบาทของ '" + base.name + "' แล้ว · OnClose="
					+ (this.OnClose == null ? "null(จะเรียก CloseAllUI)" : "มีตัวรับ") );
				if (this.OnClose == null)
				{
					if (Parent != null)
					{
						UIBase.CloseAllUI();
					}
				}
				else
				{
					this.OnClose();
				}
			});
		}
		if (_backButton != null)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(_backButton.gameObject);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
			{
				if (this.OnBack == null)
				{
					if (Parent != null)
					{
						Parent.Close();
					}
				}
				else
				{
					this.OnBack();
				}
				if (Parent == null || Parent.IsOpened)
				{
					UISound.PlayClick(UISound.ClickType.ButtonDefault);
				}
			});
		}
		UIManager.AddOnScreenResized(OnScreenResize);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			UpdateLayout();
		}
	}

	private void OnScreenResize()
	{
		UpdateLayout();
	}

	protected virtual void UpdateLayout()
	{
		bool flag = UIManager.IsPortraitScreen && Parent != null && Parent.Anchor == UIBase.AnchorType.CloneFullscreen;
		bool? isCloneArea = _isCloneArea;
		if (!isCloneArea.HasValue || _isCloneArea.Value != flag)
		{
			_isCloneArea = flag;
			if (flag)
			{
				Background.leftAnchor.Set(base.transform, 0f, 0f);
				Background.rightAnchor.Set(base.transform, 1f, 0f);
			}
			else
			{
				Background.leftAnchor.SetScreen(0f, 0f);
				Background.rightAnchor.SetScreen(1f, 0f);
			}
			Background.ResetAnchors();
			UIUtility.UpdateAnchors(Background.transform);
		}
		Layout.UpdateLayout();
		RefreshTitleNextContainer();
	}

	public void SetTitle(string text)
	{
		TitleLabel.text = text;
		RefreshTitleNextContainer();
	}

	public void ShowCloseButton(bool show)
	{
		if (_closeButton != null)
		{
			bool activeInHierarchy = _closeButton.gameObject.activeInHierarchy;
			if (activeInHierarchy != show)
			{
				_closeButton.gameObject.SetActive(show);
				Layout.UpdateLayout();
				UIUtility.UpdateAnchors(base.transform);
			}
		}
		if (this.CloseButtonVisibilityChanged != null)
		{
			this.CloseButtonVisibilityChanged(show);
		}
	}

	public void ShowBackButton(bool show)
	{
		if (_backButton != null)
		{
			bool activeInHierarchy = _backButton.gameObject.activeInHierarchy;
			if (activeInHierarchy != show)
			{
				_backButton.gameObject.SetActive(show);
				Layout.UpdateLayout();
				UIUtility.UpdateAnchors(base.transform);
			}
		}
		if (this.BackButtonVisibilityChanged != null)
		{
			this.BackButtonVisibilityChanged(show);
		}
	}

	public void SetTitleNext(Transform container, Vector2 offset)
	{
		if (!(container == null))
		{
			_titleNextContainer = container;
			_titleNextOffset = offset;
			_titleNextContainer.transform.parent = TitleLabel.transform.parent;
			RefreshTitleNextContainer();
		}
	}

	protected void RefreshTitleNextContainer()
	{
		if (!(_titleNextContainer == null))
		{
			Vector3 vector = TitleLabel.GetPosition(0f, 0.5f) + TitleLabel.printedSize.x * Vector3.right;
			_titleNextContainer.transform.localPosition = vector + _titleNextOffset;
		}
	}
}
