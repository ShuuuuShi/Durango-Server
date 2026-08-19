using System;
using System.IO;
using Durango.Render.Camera;
using Durango.Render.PersonalMaps;
using Durango.Render.Screen;
using Durango.System;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

[Uri("Capture")]
public class ScreenCaptureGroup : UIBase
{
	private const string HideKey = "ScreenCapture";

	[SerializeField]
	private GameObject _closeButton;

	[SerializeField]
	private GameObject _labelScreenshot;

	[SerializeField]
	private SelectableWidget _captureButton;

	[SerializeField]
	private SelectableWidget _personalMapsButton;

	[SerializeField]
	private UILabel _textPersonalMapsTooltip;

	[SerializeField]
	private UITexture _renderTexture;

	[SerializeField]
	private GameObject _waitingForPersonalMaps;

	[SerializeField]
	private GameObject _top;

	[SerializeField]
	private GameObject _controller;

	[SerializeField]
	private GameObject _captureProgress;

	[SerializeField]
	private UILabel _textProgress;

	[SerializeField]
	private TweenFill _tweenProgress;

	[SerializeField]
	private UILabel _textRemainTimeTooltip;

	[SerializeField]
	private GameObject _personalMapsCancelButton;

	[SerializeField]
	private SoundEventType _cameraSound;

	[SerializeField]
	private ScreenCapture.EffectEnum _defaultEffects;

	private ScreenCapture.EffectEnum _effect;

	private UIPanel _inGameUI;

	private string _estateIdForHideLines;

	private bool _zoomOut;

	private float _prevZoom;

	private float _captureStartTime;

	protected override bool IsSoundOcclusion => false;

	private void Awake()
	{
		UIEventListener.Get(_closeButton).onClick = delegate
		{
			Close();
		};
		UIEventListener.Get(_renderTexture.gameObject).onClick = delegate
		{
			SetReadyToCapture();
		};
		UIEventListener.Get(_captureButton.gameObject).onClick = delegate
		{
			CaptureScreen();
		};
		UIEventListener.Get(_personalMapsButton.gameObject).onClick = delegate
		{
			CapturePersonalMaps();
		};
		UIEventListener.Get(_personalMapsCancelButton).onClick = OnCancelPersonalMaps;
		SoundManager.PrepareEvent(_cameraSound);
		ResetScreenEffects();
		base.OnOpenSucceed += OnOpenSucceeded;
		base.OnCloseSucceed += OnCloseSucceeded;
		SetChildrenActive(activated: false);
		GameObject gameObject = GameObject.Find("InGameUI");
		if ((bool)gameObject)
		{
			_inGameUI = gameObject.GetComponent<UIPanel>();
		}
	}

	private void Start()
	{
		GameSystem<InputSystem>.Instance().On(InputCommand.ScreenCaptureUIOnlyForEditor, CaptureUIScreen);
		GameSystem<InputSystem>.Instance().On(InputCommand.ScreenCaptureForEditor, CaptureScreenForEditor);
		GameSystem<InputSystem>.Instance().On(InputCommand.ScreenCaptureNoUIForEditor, CaptureScreenNoUIForEditor);
		GameSystem<InputSystem>.Instance().On(InputCommand.ScreenCapture, delegate
		{
			Open(zoomOut: false);
			CaptureScreen();
		});
	}

	protected override bool TryClose()
	{
		if (Singleton<PersonalMaps>.Instance().IsWorking)
		{
			OnCancelPersonalMaps(null);
			return false;
		}
		return base.TryClose();
	}

	public bool Open(bool zoomOut)
	{
		_zoomOut = zoomOut;
		return base.Open();
	}

	public override bool Open()
	{
		bool zoomOut = !PlayerBehavior.LocalPlayer.Driver.IsHovering;
		return Open(zoomOut);
	}

	public void ToggleScreenEffect(ScreenCapture.EffectEnum effect, bool on)
	{
		if (on)
		{
			_effect |= effect;
		}
		else
		{
			_effect &= ~effect;
		}
	}

	public void ResetScreenEffects()
	{
		_effect = _defaultEffects;
	}

	private void OnPreScreenCapture()
	{
		EstateSystem estateSystem = GameSystem<EstateSystem>.Instance();
		if ((bool)_inGameUI)
		{
			_inGameUI.alpha = 0f;
		}
		if (estateSystem.CurrentEstate != null && estateSystem.CurrentEstate.IsLocalPlayers())
		{
			_estateIdForHideLines = estateSystem.CurrentEstate.Id;
			estateSystem.SetVisibleEstateLines(_estateIdForHideLines, visible: false);
		}
		else
		{
			_estateIdForHideLines = null;
		}
		estateSystem.DisableEstateOwnerPopup = true;
	}

	private void OnPostScreenCapture()
	{
		EstateSystem estateSystem = GameSystem<EstateSystem>.Instance();
		if ((bool)_inGameUI)
		{
			_inGameUI.alpha = 1f;
		}
		if (_estateIdForHideLines != null)
		{
			estateSystem.SetVisibleEstateLines(_estateIdForHideLines, visible: true);
			_estateIdForHideLines = null;
		}
		estateSystem.DisableEstateOwnerPopup = false;
	}

	private void CaptureScreen()
	{
		Platform.Instance.RequestPermission("WRITE_EXTERNAL_STORAGE", RequestCaptureScreenPermissionResult);
	}

	private void CapturePersonalMaps()
	{
		UIManager.MessageBox.Show(T._("개인섬 사유지를 촬영하시겠습니까?"), T._("[icon=icon_make_alert] 선언한 사유지 기반으로 섬을 촬영합니다.\n[icon=icon_make_alert] 같은 크기의 사유지여도 선언된 모양에 따라 촬영되는 범위가 달라집니다."), delegate(bool ok)
		{
			if (ok)
			{
				Platform.Instance.RequestPermission("WRITE_EXTERNAL_STORAGE", RequestCaptureEstatePermissionResult);
			}
		});
	}

	private void OnOpenSucceeded()
	{
		SetReadyToCapture();
		if (_zoomOut)
		{
			_prevZoom = Singleton<MainCamera>.Instance().Zoom;
			Singleton<CameraController>.Instance().ZoomRange(0.42f, 2.2f, 0.3f).Zoom(0.42f, 0.3f);
			VisibleController.Hide(HideUIFunc, hide: true, "ScreenCapture", 0.3f);
		}
	}

	private void OnCloseSucceeded()
	{
		if (_zoomOut)
		{
			Singleton<CameraController>.Instance().ZoomRange(0.42f, 2.2f, 0.3f).Zoom(_prevZoom, 0.3f);
		}
		_renderTexture.mainTexture = null;
		VisibleController.Hide(HideUIFunc, hide: false, "ScreenCapture", 0.3f);
	}

	private bool HideUIFunc(VisibleController script)
	{
		if (script != base.VisibleController && script.GetComponent<AlarmGroup>() == null)
		{
			return script.GetComponent<MessageBox>() == null;
		}
		return false;
	}

	private void SetReadyToCapture()
	{
		_renderTexture.gameObject.SetActive(value: false);
		_waitingForPersonalMaps.SetActive(value: false);
		_top.SetActive(value: true);
		_controller.SetActive(value: true);
		_labelScreenshot.SetActive(value: true);
		_captureButton.gameObject.SetActive(value: true);
		SetActivePersonalMapsButton(active: true);
	}

	private void SetInstagramShot(Texture2D tex)
	{
		UIManager.SystemMsg(T._("스크린샷이 저장되었습니다."));
		_renderTexture.gameObject.SetActive(value: true);
		_labelScreenshot.SetActive(value: false);
		_captureButton.gameObject.SetActive(value: false);
		SetActivePersonalMapsButton(active: false);
		_renderTexture.mainTexture = tex;
		ScreenshotManager.SaveImage(tex);
		OnPostScreenCapture();
	}

	private void SetActivePersonalMapsButton(bool active)
	{
		if (active)
		{
			bool disabled = true;
			if (GameSystem<EstateSystem>.Instance().PersonalIslandInfo.HasValue)
			{
				if (HasPersonalIslandEstateArea())
				{
					_textPersonalMapsTooltip.text = T._("개인섬 전체");
					disabled = false;
				}
				else
				{
					_textPersonalMapsTooltip.text = T._("개인섬 사유지 선언 필요");
				}
			}
			else
			{
				_textPersonalMapsTooltip.text = T._("개인섬 전체");
				disabled = false;
			}
			UIUtility.UpdateAnchors(_textPersonalMapsTooltip.transform);
			_personalMapsButton.Disabled = disabled;
			_personalMapsButton.gameObject.SetActive(value: true);
		}
		else
		{
			_personalMapsButton.gameObject.SetActive(value: false);
		}
	}

	private void RequestCaptureScreenPermissionResult(bool granted)
	{
		if (granted)
		{
			OnPreScreenCapture();
			ScreenCapture.CaptureOption option = default(ScreenCapture.CaptureOption);
			option.Effect = _effect;
			option.Logo = true;
			option.NoUI = true;
			option.OnResult = SetInstagramShot;
			ScreenCapture.Capture(option);
			SoundManager.PlayEvent(_cameraSound);
		}
		else
		{
			Close();
		}
	}

	private void RequestCaptureEstatePermissionResult(bool granted)
	{
		if (granted)
		{
			Point2 down = Point2.down;
			Point2 up = Point2.up;
			OnPreScreenCapture();
			_waitingForPersonalMaps.SetActive(value: true);
			_top.SetActive(value: false);
			_controller.SetActive(value: false);
			SetPersonalMapsProgress(0f);
			Singleton<PersonalMaps>.Instance().Capture(down, up, delegate(float? percentage)
			{
				SetPersonalMapsProgress(percentage);
			}, delegate(MemoryStream memoryStream)
			{
				UIManager.MessageBox.Hide();
				if (memoryStream != null)
				{
					UIManager.SystemMsg(T._("사유지 스크린샷이 저장되었습니다."));
					string path = Directory.GetCurrentDirectory() + "\\AppData\\Screenshots\\" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
					DirectoryInfo directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\AppData\\Screenshots");
					if (!directoryInfo.Exists)
					{
						directoryInfo.Create();
					}
					ScreenshotManager.SaveImage(memoryStream, path, ".jpeg");
					memoryStream.Dispose();
				}
				else if (!Singleton<PersonalMaps>.Instance().IsCanceled)
				{
					UIManager.SystemMsg(T._("사유지 스크린샷 저장에 실패했습니다."));
				}
				SetReadyToCapture();
				OnPostScreenCapture();
			});
		}
		else
		{
			Close();
		}
	}

	private void SetPersonalMapsProgress(float? percentage)
	{
		if (percentage.HasValue)
		{
			percentage = Mathf.Clamp01(percentage.Value);
			float num2;
			if (percentage.Value > 0f)
			{
				float num = Time.realtimeSinceStartup - _captureStartTime;
				num2 = num / percentage.Value - num;
				_tweenProgress.from = _tweenProgress.value;
				_tweenProgress.to = percentage.Value;
				_tweenProgress.ResetToBeginning();
				_tweenProgress.PlayForward();
			}
			else
			{
				_captureStartTime = Time.realtimeSinceStartup;
				num2 = 0f;
				_tweenProgress.from = 0f;
				_tweenProgress.to = 0f;
				_tweenProgress.ResetToBeginning();
			}
			_textProgress.text = $"{percentage.Value:P0}";
			if (num2 > 0f && percentage.Value < 1f)
			{
				_textRemainTimeTooltip.text = T._("<em>{0}</em> 뒤 완료 예상", TimedeltaFormatter.Format(num2));
				UIUtility.UpdateAnchors(_textRemainTimeTooltip.transform);
				_textRemainTimeTooltip.gameObject.SetActive(value: true);
			}
			else
			{
				_textRemainTimeTooltip.gameObject.SetActive(value: false);
			}
			_captureProgress.SetActive(value: true);
		}
		else
		{
			_captureProgress.SetActive(value: false);
		}
	}

	private static bool HasPersonalIslandEstateArea()
	{
		if (GameSystem<EstateSystem>.Instance().PersonalIslandInfo.HasValue)
		{
			Pair<Point2, Point2> estateCellArea = GameSystem<EstateSystem>.Instance().PersonalIslandInfo.Value.EstateCellArea;
			if (!(estateCellArea.Item1 != Point2.zero))
			{
				return estateCellArea.Item2 != Point2.zero;
			}
			return true;
		}
		return false;
	}

	private static bool TryGetPersonalIslandEstateArea(out Point2 minTile, out Point2 maxTile)
	{
		if (GameSystem<EstateSystem>.Instance().PersonalIslandInfo.HasValue)
		{
			Pair<Point2, Point2> estateCellArea = GameSystem<EstateSystem>.Instance().PersonalIslandInfo.Value.EstateCellArea;
			if (estateCellArea.Item1 != Point2.zero || estateCellArea.Item2 != Point2.zero)
			{
				minTile = estateCellArea.Item1 * 4;
				maxTile = (estateCellArea.Item2 + Point2.one) * 4;
				return true;
			}
		}
		minTile = Point2.zero;
		maxTile = Point2.zero;
		return false;
	}

	private void OnCancelPersonalMaps(GameObject go)
	{
		if (Singleton<PersonalMaps>.Instance().IsCanceled)
		{
			return;
		}
		UIManager.MessageBox.Show(T._("개인섬 촬영을 취소하시겠습니까?"), T._("[icon=icon_make_alert] 촬영 중간 취소 시 사진은 저장되지 않습니다."), delegate(bool ok)
		{
			if (ok)
			{
				Singleton<PersonalMaps>.Instance().Cancel();
			}
		});
	}

	private void CaptureUIScreen(InputCommandMessage message)
	{
		if (Application.isEditor)
		{
			string fileName = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
			CaptureUIScreen(fileName);
		}
	}

	private void CaptureScreenForEditor(InputCommandMessage message)
	{
		CaptureScreenForEditor(noUI: false);
	}

	private void CaptureScreenNoUIForEditor(InputCommandMessage message)
	{
		CaptureScreenForEditor(noUI: true);
	}

	private void CaptureScreenForEditor(bool noUI)
	{
		if (Application.isEditor)
		{
			string file = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
			ScreenCapture.CaptureOption option = default(ScreenCapture.CaptureOption);
			option.NoUI = noUI;
			option.OnResult = delegate(Texture2D origin)
			{
				ScreenshotManager.SaveImage(origin, file);
			};
			ScreenCapture.Capture(option);
		}
	}

	private void CaptureUIScreen(string fileName)
	{
		UICamera[] array = UICamera.list.ToArray();
		float[] array2 = new float[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i].cachedCamera.depth;
		}
		Array.Sort(array2, array);
		RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
		Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, mipmap: false);
		Texture2D texture2D2 = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, mipmap: false);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = renderTexture;
		for (int j = 0; j < array.Length; j++)
		{
			Camera cachedCamera = array[j].cachedCamera;
			RenderTexture targetTexture = cachedCamera.targetTexture;
			cachedCamera.targetTexture = renderTexture;
			cachedCamera.Render();
			cachedCamera.targetTexture = targetTexture;
		}
		texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
		texture2D.Apply();
		for (int k = 0; k < array.Length; k++)
		{
			Camera cachedCamera2 = array[k].cachedCamera;
			RenderTexture targetTexture2 = cachedCamera2.targetTexture;
			cachedCamera2.targetTexture = renderTexture;
			cachedCamera2.RenderWithShader(Shader.Find("Durango/NGUI/AlphaCalculator"), null);
			cachedCamera2.targetTexture = targetTexture2;
		}
		texture2D2.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
		texture2D2.Apply();
		RenderTexture.active = active;
		for (int l = 0; l < texture2D.width; l++)
		{
			for (int m = 0; m < texture2D.height; m++)
			{
				Color pixel = texture2D.GetPixel(l, m);
				pixel.a = texture2D2.GetPixel(l, m).a;
				texture2D.SetPixel(l, m, pixel);
			}
		}
		texture2D.Apply();
		ScreenshotManager.SaveImage(texture2D, fileName);
	}
}
