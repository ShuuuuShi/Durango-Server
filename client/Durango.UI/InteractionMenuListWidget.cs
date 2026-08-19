using System;
using System.Collections.Generic;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class InteractionMenuListWidget : InteractionMenuListWidgetBase
{
	[SerializeField]
	private UISprite _linkLineBase;

	private GameObject _linkLineContainer;

	private readonly List<UISprite> _linkLines = new List<UISprite>();

	public override void Init()
	{
		if (!IsInit)
		{
			base.Init();
			PrevArrow = NextArrow.transform.parent.gameObject.AddChild(NextArrow.gameObject);
			for (int i = 0; i < PrevArrow.transform.childCount; i++)
			{
				PrevArrow.transform.GetChild(i).localEulerAngles += Vector3.forward * 180f;
			}
			UIEventListener.Get(NextArrow).onClick = OnClickArrow;
			UIEventListener.Get(PrevArrow).onClick = OnClickArrow;
			_linkLineBase.gameObject.SetActive(value: true);
			_linkLines.Add(_linkLineBase);
			for (int j = 1; j < base.VisibleCountPerPage; j++)
			{
				GameObject gameObject = _linkLineBase.transform.parent.gameObject.AddChild(_linkLineBase.gameObject);
				gameObject.SetActive(value: true);
				_linkLines.Add(gameObject.GetComponent<UISprite>());
			}
			_linkLineContainer = _linkLineBase.transform.parent.gameObject;
		}
	}

	private void OnClickArrow(GameObject go)
	{
		if (go == PrevArrow)
		{
			VisiblePage--;
		}
		else if (go == NextArrow)
		{
			VisiblePage++;
		}
		for (int i = 0; i < Menus.Count; i++)
		{
			Menus[i].NeedInitAnimation = true;
		}
		ClearSubMenus();
		Reposition(instant: true);
	}

	protected override void Reposition(bool instant)
	{
		RepositionMenuItems();
		RepositionInteractionMenuLines(instant);
	}

	private void RepositionMenuItems()
	{
		float radius = Radius;
		int count = Menus.Count;
		int num = VisiblePage * base.VisibleCountPerPage;
		int num2 = (VisiblePage + 1) * base.VisibleCountPerPage;
		float num3 = 360f / (float)base.VisibleCountPerPage;
		while (true)
		{
			int num4 = 0;
			for (int i = 0; i < count; i++)
			{
				InteractionMenuWidgetBase interactionMenuWidgetBase = Menus[i];
				if (interactionMenuWidgetBase.Index < num || interactionMenuWidgetBase.Index >= num2)
				{
					interactionMenuWidgetBase.Index = -1;
				}
				else
				{
					num4++;
				}
			}
			if (num4 == 0 && VisiblePage > 0)
			{
				VisiblePage--;
				num -= base.VisibleCountPerPage;
				num2 -= base.VisibleCountPerPage;
				continue;
			}
			break;
		}
		for (int j = 0; j < count; j++)
		{
			InteractionMenuWidgetBase interactionMenuWidgetBase2 = Menus[j];
			if (interactionMenuWidgetBase2.Index == -1)
			{
				interactionMenuWidgetBase2.Index = FindEmptyIndex();
				interactionMenuWidgetBase2.NeedInitAnimation = true;
			}
		}
		VisibleStartIndex = 0;
		int num5 = 0;
		Vector2 vector = default(Vector2);
		for (int k = 0; k < count; k++)
		{
			InteractionMenuWidgetBase interactionMenuWidgetBase3 = Menus[k];
			if (interactionMenuWidgetBase3.Index < num || interactionMenuWidgetBase3.Index >= num2)
			{
				interactionMenuWidgetBase3.gameObject.SetActive(value: false);
				continue;
			}
			interactionMenuWidgetBase3.gameObject.SetActive(value: true);
			int num6 = interactionMenuWidgetBase3.Index % base.VisibleCountPerPage;
			float num7 = ((interactionMenuWidgetBase3.Type != 0) ? InteractionMenuListWidgetBase.MinorScale : InteractionMenuListWidgetBase.MajorScale);
			int num8 = VisibleOrder[(num6 + VisibleStartIndex) % base.VisibleCountPerPage];
			float t = VisibleStartDegree + (float)num8 * num3;
			float num9 = Mathf.Repeat(t, 360f) * ((float)Math.PI / 180f);
			float num10 = radius + (float)interactionMenuWidgetBase3.Widget.width * 0.5f * (num7 - 1f);
			vector.x = Mathf.Cos(num9) * num10;
			vector.y = Mathf.Sin(num9) * num10;
			interactionMenuWidgetBase3.MenuRadian = num9;
			if (interactionMenuWidgetBase3.NeedInitAnimation)
			{
				interactionMenuWidgetBase3.transform.localPosition = Vector3.Lerp(Vector3.zero, vector, 0.5f);
				interactionMenuWidgetBase3.Widget.alpha = 0f;
				float delay = (float)num5++ * 0.05f + 0.1f;
				TweenPosition positionTweener = interactionMenuWidgetBase3.PositionTweener;
				positionTweener.from = interactionMenuWidgetBase3.transform.localPosition;
				positionTweener.to = vector;
				positionTweener.delay = delay;
				positionTweener.tweenFactor = 0f;
				positionTweener.PlayForward();
				TweenAlpha alphaTweener = interactionMenuWidgetBase3.AlphaTweener;
				alphaTweener.from = interactionMenuWidgetBase3.Widget.alpha;
				alphaTweener.to = interactionMenuWidgetBase3.Alpha;
				alphaTweener.delay = delay;
				alphaTweener.tweenFactor = 0f;
				alphaTweener.PlayForward();
				interactionMenuWidgetBase3.NeedInitAnimation = false;
			}
			else
			{
				TweenPosition positionTweener2 = interactionMenuWidgetBase3.PositionTweener;
				if (positionTweener2.enabled)
				{
					positionTweener2.to = vector;
				}
				else
				{
					interactionMenuWidgetBase3.transform.localPosition = vector;
				}
			}
			interactionMenuWidgetBase3.UpdateUIPosition();
		}
		if (VisiblePage > 0)
		{
			PrevArrow.SetActive(value: true);
			PrevArrow.transform.localPosition = Vector3.left * radius * 1.7f + Vector3.down * 50f;
		}
		else
		{
			PrevArrow.SetActive(value: false);
		}
		if ((VisiblePage + 1) * base.VisibleCountPerPage < count)
		{
			NextArrow.SetActive(value: true);
			NextArrow.transform.localPosition = Vector3.right * radius * 1.7f + Vector3.down * 50f;
		}
		else
		{
			NextArrow.SetActive(value: false);
		}
	}

	private void RepositionInteractionMenuLines(bool instant)
	{
		int count = Menus.Count;
		float radius = Radius - 30f;
		if (count > 1)
		{
			RepositionMenuMultipleLines(radius, 37);
		}
		else if (count == 1)
		{
			RepositionMenuOneLine(radius, 37);
		}
		else
		{
			_linkLineContainer.SetActive(value: false);
		}
		TweenAlpha component = _linkLineContainer.GetComponent<TweenAlpha>();
		if (!instant && !component.enabled && _linkLineContainer.activeSelf)
		{
			_linkLineContainer.GetComponent<UIWidget>().alpha = 0f;
			component.delay = (float)count * 0.05f + 0.1f;
			component.tweenFactor = 0f;
			component.PlayForward();
		}
	}

	private void RepositionMenuMultipleLines(float radius, int eraseWidth)
	{
		_linkLineContainer.SetActive(value: true);
		float num = 360f / (float)base.VisibleCountPerPage;
		int i = 0;
		for (int visibleCountPerPage = base.VisibleCountPerPage; i < visibleCountPerPage; i++)
		{
			int num2 = VisibleOrder[(i + VisibleStartIndex) % base.VisibleCountPerPage];
			UISprite uISprite = _linkLines[i];
			uISprite.gameObject.SetActive(value: true);
			float num3 = VisibleStartDegree + (float)num2 * num;
			Vector3 vector = radius * new Vector3(Mathf.Cos(num3 * ((float)Math.PI / 180f)), Mathf.Sin(num3 * ((float)Math.PI / 180f)));
			Vector3 vector2 = radius * new Vector3(Mathf.Cos((num3 + num) * ((float)Math.PI / 180f)), Mathf.Sin((num3 + num) * ((float)Math.PI / 180f)));
			Vector3 normalized = (vector2 - vector).normalized;
			int visibleIndex = i + VisibleStartIndex;
			int visibleIndex2 = (VisibleOrder.IndexOf((num2 + 1) % base.VisibleCountPerPage) + (base.VisibleCountPerPage - VisibleStartIndex)) % base.VisibleCountPerPage;
			int num4 = VisibleIndexToMenuIndex(visibleIndex);
			if (num4 != -1)
			{
				InteractionMenuWidgetBase interactionMenuWidgetBase = Menus[num4];
				float num5 = ((interactionMenuWidgetBase.Type != 0) ? InteractionMenuListWidgetBase.MinorScale : InteractionMenuListWidgetBase.MajorScale);
				float num6 = (float)eraseWidth * (1f - (1f - num5) * 0.2f);
				vector += normalized * num6;
			}
			num4 = VisibleIndexToMenuIndex(visibleIndex2);
			if (num4 != -1)
			{
				InteractionMenuWidgetBase interactionMenuWidgetBase2 = Menus[num4];
				float num7 = ((interactionMenuWidgetBase2.Type != 0) ? InteractionMenuListWidgetBase.MinorScale : InteractionMenuListWidgetBase.MajorScale);
				float num8 = (float)eraseWidth * (1f - (1f - num7) * 0.2f);
				vector2 -= normalized * num8;
			}
			uISprite.transform.localPosition = Vector3.Lerp(vector, vector2, 0.5f);
			uISprite.transform.localEulerAngles = Vector3.forward * Mathf.Atan2(normalized.y, normalized.x) * 57.29578f;
			uISprite.width = (int)(vector2 - vector).magnitude;
			uISprite.color = Color.black;
			uISprite.alpha = 0.45f;
			uISprite.height = 4;
		}
	}

	private void RepositionMenuOneLine(float radius, int eraseWidth)
	{
		InteractionMenuWidgetBase interactionMenuWidgetBase = Menus[0];
		float num = ((interactionMenuWidgetBase.Type != 0) ? InteractionMenuListWidgetBase.MinorScale : InteractionMenuListWidgetBase.MajorScale);
		_linkLineContainer.SetActive(value: true);
		for (int i = 1; i < _linkLines.Count; i++)
		{
			_linkLines[i].gameObject.SetActive(value: false);
		}
		UISprite uISprite = _linkLines[0];
		uISprite.gameObject.SetActive(value: true);
		float menuRadian = interactionMenuWidgetBase.MenuRadian;
		uISprite.transform.localEulerAngles = Vector3.forward * menuRadian * 57.29578f;
		uISprite.width = (int)(radius - (float)eraseWidth * num - 30f);
		uISprite.transform.localPosition = (Vector3.right * Mathf.Cos(menuRadian) + Vector3.up * Mathf.Sin(menuRadian)) * (30f + (float)uISprite.width * 0.5f);
		uISprite.color = Color.white;
		uISprite.alpha = 0.45f;
		uISprite.height = 2;
	}

	protected int VisibleIndexToMenuIndex(int visibleIndex)
	{
		int i = 0;
		for (int count = Menus.Count; i < count; i++)
		{
			if (Menus[i].Index - VisiblePage * base.VisibleCountPerPage == visibleIndex)
			{
				return i;
			}
		}
		return -1;
	}
}
