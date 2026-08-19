using L10N;
using NPA;

public class ToySNSConnector : INPListenerType, INPListener
{
	private static ToySNSConnector _instance;

	private static string _prevNPSN;

	private static string _prevToken;

	private static string _prevNPA;

	public static ToySNSConnector Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new ToySNSConnector();
			}
			return _instance;
		}
	}

	public static void ShowAccountMenu()
	{
		_prevNPSN = ToyLoginHelper.NPSN;
		NPAccount.Instance.ShowAccountMenu(Instance);
	}

	public void OnResult(NPResult npResult)
	{
		int asInt = npResult.resultJson["errorCode"].AsInt;
		switch (npResult.requestTag)
		{
		case NPRequestTypeTag.NPRequestTypeLoginWithNX:
		case NPRequestTypeTag.NPRequestTypeLogin:
		case NPRequestTypeTag.NPRequestTypeLoginWithGameCenter:
		case NPRequestTypeTag.NPRequestTypeLoginWithTwitter:
		case NPRequestTypeTag.NPRequestTypeLoginWithGPlus:
		case NPRequestTypeTag.NPRequestTypeLoginWithFB:
		case NPRequestTypeTag.NPRequestTypeLoginWithGuest:
			if (asInt == 0)
			{
				string text = npResult.resultJson["result"]["npSN"];
				if (_prevNPSN != text)
				{
					KSingleton<GameManager>.Instance().MoveToTitle();
				}
			}
			else
			{
				UIManager.MessageBox.Show(T._("계정 연동에 실패했습니다. 에러코드: {0}", asInt));
			}
			break;
		case NPRequestTypeTag.NPRequestTypeMigrationForGcid:
			if (asInt == 0)
			{
				KSingleton<GameManager>.Instance().MoveToTitle();
				break;
			}
			UIManager.MessageBox.Show(T._("기기 연동에 실패했습니다. 에러코드: {0}", asInt));
			break;
		case NPRequestTypeTag.NPRequestTypeShowAccountMenu:
			if (asInt != 99999)
			{
			}
			break;
		}
	}
}
