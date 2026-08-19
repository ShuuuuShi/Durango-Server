using System.Collections.Generic;
using System.IO;
using System.Text;
using APNGLib;
using Messages;
using Shared.MessageBoard;
using UnityEngine;

public class MessageBoard : ArtifactComponent
{
	public const string TextContainerName = "text_board";

	public const string CanvasContainerName = "drawing_board";

	private byte[] _data;

	private string _text;

	private APNG _image;

	private RectTransform _textContainer;

	private bool _textContainerFlag;

	private MeshRenderer _drawContainer;

	private bool _drawContainerFlag;

	public Drawing DrawType { get; private set; }

	private RectTransform TextContainer
	{
		get
		{
			if (!_textContainerFlag)
			{
				_textContainerFlag = true;
				GameObject val = KUtility.FindObjectByName(((Component)base.Artifact).gameObject, "text_board", includeInactive: true);
				if ((Object)(object)val != (Object)null)
				{
					_textContainer = val.GetComponent<RectTransform>();
				}
			}
			return _textContainer;
		}
	}

	private MeshRenderer DrawContainer
	{
		get
		{
			if (!_drawContainerFlag)
			{
				_drawContainerFlag = true;
				GameObject val = KUtility.FindObjectByName(((Component)base.Artifact).gameObject, "drawing_board", includeInactive: true);
				if ((Object)(object)val != (Object)null)
				{
					_drawContainer = val.GetComponent<MeshRenderer>();
				}
			}
			return _drawContainer;
		}
	}

	public UISpriteLabel TextBoard { get; private set; }

	public ApngTexture PixelBoard { get; private set; }

	public override void ResourcesLoadCompleted()
	{
		MakeComponent();
		UpdateBoard();
	}

	private void MakeComponent()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		if (!KSingleton<UIManager>.HasInstance())
		{
			return;
		}
		RectTransform textContainer = TextContainer;
		MeshRenderer drawContainer = DrawContainer;
		if ((Object)(object)textContainer != (Object)null)
		{
			UILabel uILabel = ((Component)textContainer).gameObject.AddChild<UILabel>();
			uILabel.bitmapFont = KSingleton<UIManager>.Instance().Font;
			uILabel.minFontSize = 30;
			Rect rect = textContainer.rect;
			uILabel.fontSize = (int)(((Rect)(ref rect)).height * 0.8f);
			uILabel.overflowMethod = UILabel.Overflow.ShrinkContent;
			uILabel.pivot = UIWidget.Pivot.Center;
			TextBoard = ((Component)uILabel).gameObject.AddComponent<UISpriteLabel>();
			TextBoard.Atlases.Add(KSingleton<UIManager>.Instance().UIAtlas);
			TextBoard.Atlases.Add(KSingleton<UIManager>.Instance().IconAtlas);
			TextBoard.Label = uILabel;
		}
		if ((Object)(object)drawContainer != (Object)null)
		{
			PixelBoard = ((Component)drawContainer).gameObject.AddComponent<ApngTexture>();
		}
		else if ((Object)(object)textContainer != (Object)null)
		{
			UITexture uITexture = ((Component)textContainer).gameObject.AddChild<UITexture>();
			PixelBoard = ((Component)uITexture).gameObject.AddComponent<ApngTexture>();
		}
		if ((Object)(object)PixelBoard != (Object)null)
		{
			PixelBoard.TextureWidth = base.Artifact.Blueprint.Scribblable.CanvasSize.x;
			PixelBoard.TextureHeight = base.Artifact.Blueprint.Scribblable.CanvasSize.y;
			((Behaviour)PixelBoard).enabled = false;
		}
		if (!((Object)(object)textContainer != (Object)null))
		{
			return;
		}
		int i = 0;
		for (int childCount = ((Component)textContainer).transform.childCount; i < childCount; i++)
		{
			UIWidget component = ((Component)((Component)textContainer).transform.GetChild(i)).GetComponent<UIWidget>();
			if ((Object)(object)component != (Object)null)
			{
				Rect rect2 = textContainer.rect;
				component.width = Mathf.RoundToInt(((Rect)(ref rect2)).width);
				Rect rect3 = textContainer.rect;
				component.height = Mathf.RoundToInt(((Rect)(ref rect3)).height);
				component.SetPosition(Vector3.zero, 0.5f, 0.5f);
			}
		}
	}

	public override bool OnSelectArtifact(bool isSelect)
	{
		if (isSelect)
		{
			UpdateBoard();
			if (DrawType == Drawing.Text && !string.IsNullOrEmpty(_text))
			{
				KUtility.DelayedCall((MonoBehaviour)(object)base.Artifact, MessageTooltip, 0.5f);
			}
		}
		return false;
	}

	private void MessageTooltip()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		Vector3 val = MainCamera.WorldToNGUIPos(base.Artifact.InteractionPosition) + Vector3.left * 50f;
		widgetTooltipControl.Sign = -1;
		widgetTooltipControl.Set(null, _text, 300);
		widgetTooltipControl.Show(Vector2.op_Implicit(val), 3600f);
	}

	private void UpdateBoard()
	{
		if (((Component)base.Artifact).transform.childCount != 0)
		{
			if ((Object)(object)TextBoard != (Object)null)
			{
				((Component)TextBoard).gameObject.SetActive(false);
			}
			if ((Object)(object)PixelBoard != (Object)null)
			{
				((Component)PixelBoard).gameObject.SetActive(false);
			}
			switch (DrawType)
			{
			case Drawing.Text:
				SetText(_text);
				break;
			case Drawing.Canvas:
				SetCanvas(_image);
				break;
			}
		}
	}

	public override bool OnUpdateState(double eventAt)
	{
		if (!base.Artifact.ArtifactState.Scribble.HasValue)
		{
			_image = null;
			_text = null;
			return true;
		}
		ScribbleContent value = base.Artifact.ArtifactState.Scribble.Value;
		if (IsEqualArray(_data, value.Data))
		{
			return true;
		}
		_data = value.Data;
		DrawType = value.Type;
		switch (DrawType)
		{
		case Drawing.Text:
			_text = Encoding.UTF8.GetString(_data).Trim();
			_image = null;
			break;
		case Drawing.Canvas:
			_image = new APNG();
			_image.Load(new MemoryStream(_data));
			_text = null;
			break;
		}
		UpdateBoard();
		return true;
	}

	public void SetText(string text)
	{
		if ((Object)(object)TextBoard != (Object)null)
		{
			if (string.IsNullOrEmpty(text))
			{
				((Component)TextBoard).gameObject.SetActive(false);
				return;
			}
			((Component)TextBoard).gameObject.SetActive(true);
			TextBoard.text = $"[000000]{text}[-]";
		}
	}

	public void SetCanvas(APNG apng)
	{
		if ((Object)(object)PixelBoard != (Object)null)
		{
			if (apng == null)
			{
				((Component)PixelBoard).gameObject.SetActive(false);
				return;
			}
			((Component)PixelBoard).gameObject.SetActive(true);
			PixelBoard.Set(apng);
			LenticularViewer.Enable(PixelBoard, PixelBoard.FrameLength > 0);
		}
	}

	private static bool IsEqualArray(IList<byte> b1, IList<byte> b2)
	{
		if (b1 == null && b2 == null)
		{
			return true;
		}
		if (b1 == null || b2 == null)
		{
			return false;
		}
		if (b1.Count != b2.Count)
		{
			return false;
		}
		int i = 0;
		for (int count = b1.Count; i < count; i++)
		{
			if (b1[i] != b2[i])
			{
				return false;
			}
		}
		return true;
	}
}
