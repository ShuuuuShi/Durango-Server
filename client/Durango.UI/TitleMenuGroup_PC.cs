using System;
using Durango.System;
using L10N;

namespace Durango.UI;

public class TitleMenuGroup_PC : TitleMenuGroup
{
	protected override void RedirectToDownloadUrl(string downloadUrl)
	{
		TitleMenuUserControlBase userControl = UserControl;
		string title = T._("업데이트");
		string explain = T._("업데이트를 다운로드하고 다시 접속해주세요.");
		Action okAction = Platform.Instance.Quit;
		string okButtonLabel = T._("종료");
		userControl.ShowMessageBox(title, explain, okAction, null, okButtonLabel);
	}
}
