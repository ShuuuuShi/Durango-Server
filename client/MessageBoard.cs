using System.Collections.Generic;
using System.IO;
using System.Text;
using APNGLib;
using Durango.Render.Camera;
using Durango.UI.Popup;
using Durango.Utils;
using Messages;
using Shared.Building;
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
				GameObject gameObject = KUtility.FindObjectByName(base.Artifact.gameObject, "text_board", includeInactive: true);
				if (gameObject != null)
				{
					_textContainer = gameObject.GetComponent<RectTransform>();
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
				GameObject gameObject = KUtility.FindObjectByName(base.Artifact.gameObject, "drawing_board", includeInactive: true);
				if (gameObject != null)
				{
					_drawContainer = gameObject.GetComponent<MeshRenderer>();
				}
			}
			return _drawContainer;
		}
	}

	public UILabel TextBoard { get; private set; }

	public ApngTexture PixelBoard { get; private set; }

	private float GetContentsAlpha()
	{
		return base.Artifact.Condition switch
		{
			Shared.Building.Condition.Worn => 0.6f, 
			Shared.Building.Condition.Broken => 0.3f, 
			_ => 1f, 
		};
	}

	public override void ResourcesLoadCompleted()
	{
		MakeComponent();
		UpdateBoard();
	}

	private void MakeComponent()
	{
		if (!Singleton<UIManager>.HasInstance())
		{
			return;
		}
		RectTransform textContainer = TextContainer;
		MeshRenderer drawContainer = DrawContainer;
		if (textContainer != null)
		{
			UILabel uILabel = textContainer.gameObject.AddChild<UILabel>();
			uILabel.transform.localScale = Vector3.one * 3f;
			uILabel.bitmapFont = Singleton<UIManager>.Instance().Font;
			uILabel.minFontSize = 10;
			uILabel.fontSize = Mathf.Min(50, (int)(textContainer.rect.height * 0.8f / 3f));
			uILabel.overflowMethod = UILabel.Overflow.ShrinkContent;
			uILabel.pivot = UIWidget.Pivot.Center;
			uILabel.wrapAlways = true;
			TextBoard = uILabel;
		}
		if (drawContainer != null)
		{
			PixelBoard = drawContainer.gameObject.AddComponent<ApngTexture>();
		}
		else if (textContainer != null)
		{
			UITexture uITexture = textContainer.gameObject.AddChild<UITexture>();
			PixelBoard = uITexture.gameObject.AddComponent<ApngTexture>();
		}
		if (PixelBoard != null)
		{
			PixelBoard.TextureWidth = base.Artifact.Blueprint.Scribblable.CanvasSize.x;
			PixelBoard.TextureHeight = base.Artifact.Blueprint.Scribblable.CanvasSize.y;
			PixelBoard.enabled = false;
		}
		if (!(textContainer != null))
		{
			return;
		}
		int i = 0;
		for (int childCount = textContainer.transform.childCount; i < childCount; i++)
		{
			UIWidget component = textContainer.transform.GetChild(i).GetComponent<UIWidget>();
			if (component != null)
			{
				Vector3 localScale = component.transform.localScale;
				component.width = Mathf.RoundToInt(textContainer.rect.width / localScale.x);
				component.height = Mathf.RoundToInt(textContainer.rect.height / localScale.y);
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
				KUtility.DelayedCall(base.Artifact, MessageTooltip, 0.5f);
			}
		}
		return false;
	}

	private void MessageTooltip()
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		Vector3 vector = MainCamera.WorldToNGUIPos(base.Artifact.InteractionPosition) + Vector3.left * 50f;
		widgetTooltipControl.Sign = -1;
		widgetTooltipControl.Set(null, _text, 300);
		widgetTooltipControl.Show(vector, 3600f);
	}

	private void UpdateBoard()
	{
		if (base.Artifact.transform.childCount != 0)
		{
			if (TextBoard != null)
			{
				TextBoard.gameObject.SetActive(value: false);
			}
			if (PixelBoard != null)
			{
				PixelBoard.gameObject.SetActive(value: false);
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
		{
			_image = new APNG();
			using (MemoryStream stream = new MemoryStream(_data))
			{
				_image.Load(stream);
			}
			_text = null;
			break;
		}
		}
		UpdateBoard();
		return true;
	}

	public void SetText(string text)
	{
		if (TextBoard != null)
		{
			if (string.IsNullOrEmpty(text))
			{
				TextBoard.gameObject.SetActive(value: false);
				return;
			}
			TextBoard.gameObject.SetActive(value: true);
			TextBoard.text = $"[000000]{text}[-]";
			TextBoard.alpha = GetContentsAlpha();
		}
	}

	public void SetCanvas(APNG apng)
	{
		if (PixelBoard != null)
		{
			if (apng == null)
			{
				PixelBoard.gameObject.SetActive(value: false);
				return;
			}
			PixelBoard.gameObject.SetActive(value: true);
			PixelBoard.Set(apng);
			PixelBoard.Color = new Color(1f, 1f, 1f, GetContentsAlpha());
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
