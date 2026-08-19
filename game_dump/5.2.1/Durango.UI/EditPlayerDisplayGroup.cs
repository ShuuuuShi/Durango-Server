using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Player;
using UnityEngine;

namespace Durango.UI;

public class EditPlayerDisplayGroup : UIBase
{
	private enum State
	{
		Region,
		Preset,
		Costume,
		Name
	}

	private struct Page
	{
		public State State;

		public string Title;

		public bool CanBack;
	}

	[SerializeField]
	private UITitle _title;

	[SerializeField]
	private EditPlayerPresetPage _presetPage;

	[SerializeField]
	private EditPlayerCostumePage _costumePage;

	[SerializeField]
	private EditPlayerNamePage _namePage;

	[SerializeField]
	private SelectPersonalRegion _selectRegionPage;

	[SerializeField]
	private UITexture _textureViewer;

	[SerializeField]
	private UITexture _shadowDrawer;

	[SerializeField]
	private TweenTransform _playerPreviewPanelUIAnimation;

	private Action<string, string, EditPlayerDisplayProxy, Action> _createCharacter;

	private Action<EditPlayerDisplayProxy> _changeDisplay;

	private IEditPlayerDisplayPage[] _pageViewers;

	private EditPlayerDisplayProxy _display;

	private UIModelRender _modelRender;

	private int? _page;

	private readonly List<Page> _pages = new List<Page>();

	private void Start()
	{
		_display = new EditPlayerDisplayProxy();
		_pageViewers = new IEditPlayerDisplayPage[4] { _selectRegionPage, _presetPage, _costumePage, _namePage };
		for (int i = 0; i < _pageViewers.Length; i++)
		{
			_pageViewers[i].Initialize(_display);
			_pageViewers[i].Confirmed += MoveToNext;
		}
		Observable<PlayerBehavior> preview = _display.Preview;
		preview.Changed = (Action<PlayerBehavior>)Delegate.Combine(preview.Changed, new Action<PlayerBehavior>(PreviewModelChanged));
		UIEventListener.Get(_textureViewer.gameObject).onDrag = delegate(GameObject go, Vector2 delta)
		{
			if ((bool)_display.Preview.Value)
			{
				Transform mainTransform = _display.Preview.Value.MainTransform;
				mainTransform.Rotate(mainTransform.up, 0f - delta.x, Space.World);
			}
		};
		base.OnOpenSucceed += delegate
		{
			for (int j = 0; j < _pageViewers.Length; j++)
			{
				_pageViewers[j].Hide(instant: true);
			}
			_display.MakePreview();
		};
		base.OnCloseSucceed += delegate
		{
			_display.ReleasePreview();
			_page = null;
		};
		_title.ShowCloseButton(!GameManager.IsPrologueMode);
		SetChildrenActive(activated: false);
	}

	protected override bool TryClose()
	{
		if (_page.HasValue && _page.Value > 0)
		{
			SetPage(_page.Value - 1, instant: false);
			return false;
		}
		if (GameManager.IsPrologueMode)
		{
			return false;
		}
		return base.TryClose();
	}

	private void Update()
	{
		if (base.IsOpened)
		{
			_display.UpdatePreview();
		}
	}

	private void PreviewModelChanged(PlayerBehavior model)
	{
		if (model == null)
		{
			UIModelRenderBuilder.Release(_modelRender);
			return;
		}
		model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
		_modelRender = UIModelRenderBuilder.Make();
		_modelRender.SetModel(model.gameObject, 35f, 0.75f);
		_modelRender.FillTexture(_textureViewer);
		_modelRender.FillTexture(_shadowDrawer);
	}

	private void MoveToNext()
	{
		if (_pages.Count != 0)
		{
			int num = ((!_page.HasValue) ? (-1) : _page.Value);
			num++;
			if (num >= _pages.Count)
			{
				OnConfirmed();
			}
			else
			{
				SetPage(num, instant: false);
			}
		}
	}

	private void SetPage(int page, bool instant)
	{
		int? page2 = _page;
		_page = page;
		HasBack.Value = page > 0;
		Page page3 = _pages[page];
		_title.Object.SetTitle(page3.Title);
		_title.ShowBackButton(page3.CanBack);
		for (int i = 0; i < _pageViewers.Length; i++)
		{
			if (i == (int)page3.State)
			{
				_pageViewers[i].Show(instant);
			}
			else
			{
				_pageViewers[i].Hide(instant);
			}
		}
		PlayTransformTweener(page2.HasValue ? _pages[page2.Value].State : page3.State, page3.State);
		IEditPlayerDisplayPage obj = _pageViewers[(int)page3.State];
		obj.WaitForLoading(loading: false);
		obj.SetConfirmText((page >= _pages.Count - 1) ? T._("확인") : T._("다음"));
	}

	public override bool Open()
	{
		return false;
	}

	public void OpenCreateCharacter(bool? gender, PlayerDisplay? display, Job? job, Action<string, string, EditPlayerDisplayProxy, Action> result)
	{
		_createCharacter = result;
		_changeDisplay = null;
		if (!gender.HasValue)
		{
			gender = UnityEngine.Random.value > 0.5f;
		}
		if (!job.HasValue)
		{
			Job[] array = Enums<Job>.Greater(Job.Invalid);
			job = array[UnityEngine.Random.Range(0, array.Length)];
		}
		PlayerDisplay display2 = default(PlayerDisplay);
		EditPlayerDisplayProxy.FillRandomPlayerDisplayData(gender.Value, job.Value, ref display2);
		EditPlayerDisplayProxy.FillRandomPortrait(gender.Value, ref display2);
		if (display.HasValue)
		{
			display2.HeadColor = display.Value.HeadColor;
			display2.BodyColor = display.Value.BodyColor;
			display2.HairColor = display.Value.HairColor;
			display2.SkinColor = display.Value.SkinColor;
			display2.Hair = display.Value.Hair;
			display2.Beard = display.Value.Beard;
		}
		_display.Set(gender.Value, display2);
		_display.Job.Value = job.Value;
		_display.RandomVoice();
		_costumePage.CanEditCostumeColor = true;
		_costumePage.CanEditGender = false;
		base.Open();
		_pages.Clear();
		_pages.Add(new Page
		{
			State = State.Region,
			Title = T._("지역"),
			CanBack = false
		});
		_pages.Add(new Page
		{
			State = State.Preset,
			Title = T._("성별과 직업"),
			CanBack = true
		});
		_pages.Add(new Page
		{
			State = State.Costume,
			Title = T._("캐릭터 생성"),
			CanBack = true
		});
		_pages.Add(new Page
		{
			State = State.Name,
			Title = T._("이름"),
			CanBack = true
		});
		int page = 0;
		for (int i = 0; i < _pages.Count; i++)
		{
			if (_pages[i].State == State.Region)
			{
				page = i;
				break;
			}
		}
		SetPage(page, instant: true);
	}

	public void OpenEditPlayerCostume(ItemData ticket)
	{
		if (ticket == null)
		{
			return;
		}
		bool editGender = ticket.HasTag("gender_changes");
		if (!editGender && !ticket.HasTag("display_changes"))
		{
			return;
		}
		_createCharacter = null;
		_changeDisplay = delegate(EditPlayerDisplayProxy d)
		{
			MessageBox messageBox = UIManager.MessageBox;
			ChangePlayerDisplay changed = d.MakeChangePlayerDisplay();
			changed.ItemId = ticket.Id;
			string subText = ((!editGender) ? T._("[icon=icon_make_alert] 캐릭터의 외형 변경을 완료하면 인벤토리의 <em>{0}</em>{0:-이} 소모됩니다.", ticket.Name) : string.Format("{0}\n{1}\n{2}", T._("[icon=icon_make_alert] 캐릭터의 성별과 외형 변경을 완료하면 인벤토리의 <em>{0}</em>{0:-이} 소모됩니다.", ticket.Name), T._("[icon=icon_make_alert] 캐릭터의 성별을 변경하기 위해 장착한 장비를 모두 해제하고 재접속합니다."), T._("[icon=icon_make_alert] 착용 중이었던 장비는 가방에서 확인하실 수 있습니다.")));
			messageBox.Show((!editGender) ? T._("<em>캐릭터의 외형</em>을 변경하시겠습니까?") : T._("<em>캐릭터의 성별과 외형</em>을 변경하시겠습니까?"), subText, delegate(bool ok)
			{
				if (ok)
				{
					IEditPlayerDisplayPage p = null;
					if (_page.HasValue)
					{
						p = _pageViewers[(int)_pages[_page.Value].State];
						p.WaitForLoading(loading: true);
					}
					InventorySystem.ChangeDisplay(changed, delegate(bool success)
					{
						if (p != null)
						{
							p.WaitForLoading(loading: false);
						}
						if (success)
						{
							ForceClose();
						}
					});
				}
			});
		};
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		bool isMale = localPlayer.IsMale;
		PlayerDisplay display = localPlayer.Display;
		_display.Set(isMale, display);
		_display.SetDefaultHead((!isMale) ? string.Empty : display.Head, isMale ? string.Empty : display.Head);
		_display.SetDefaultBody((!isMale) ? "Models/PC/Male/Body/m_body_nothing.FBX" : display.Body, isMale ? "Models/PC/Female/Body/f_body_nothing.FBX" : display.Body);
		_display.SetNudeBody((!isMale) ? "Models/PC/Male/Inner/m_inner_basic.FBX" : display.DefaultInner, isMale ? "Models/PC/Female/Inner/f_inner_basic.FBX" : display.DefaultInner);
		_costumePage.CanEditCostumeColor = false;
		_costumePage.CanEditGender = editGender;
		base.Open();
		_pages.Clear();
		_pages.Add(new Page
		{
			State = State.Costume,
			Title = ((!editGender) ? T._("외모 변경") : T._("성별과 외모 변경")),
			CanBack = true
		});
		SetPage(0, instant: true);
	}

	private void PlayTransformTweener(State prev, State next)
	{
		Transform modelPosition = _pageViewers[(int)prev].GetModelPosition();
		Transform modelPosition2 = _pageViewers[(int)next].GetModelPosition();
		_playerPreviewPanelUIAnimation.gameObject.SetActive(modelPosition2 != null);
		if (modelPosition2 != null)
		{
			_playerPreviewPanelUIAnimation.from = ((!(modelPosition != null)) ? modelPosition2 : modelPosition);
			_playerPreviewPanelUIAnimation.to = modelPosition2;
			_playerPreviewPanelUIAnimation.ResetToBeginning();
			_playerPreviewPanelUIAnimation.PlayForward();
		}
	}

	private void OnConfirmed()
	{
		if (_createCharacter != null)
		{
			string arg = _namePage.Name;
			IEditPlayerDisplayPage p = null;
			if (_page.HasValue)
			{
				p = _pageViewers[(int)_pages[_page.Value].State];
				p.WaitForLoading(loading: true);
			}
			string selectedRegionid = _selectRegionPage.SelectedRegionid;
			_createCharacter(arg, selectedRegionid, _display, delegate
			{
				if (p != null)
				{
					p.WaitForLoading(loading: false);
				}
			});
		}
		else if (_changeDisplay != null)
		{
			_changeDisplay(_display);
		}
	}
}
