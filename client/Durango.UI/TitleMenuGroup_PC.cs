using System;
using Durango.System;
using L10N;

namespace Durango.UI;

public class TitleMenuGroup_PC : TitleMenuGroup
{
	protected override void RedirectToDownloadUrl(string downloadUrl, string patchNotes = null)
	{
		LaunchInGamePatcher(downloadUrl, required: true, patchNotes);
	}
}
