using System;

namespace Durango.UI;

public class TitleMenuUserControl : TitleMenuUserControlBase
{
	public override void ShowCluster(Action onConfirm, Action onPlayerSelection, Action onLogout, bool autoConfirm)
	{
		base.ShowCluster(onConfirm, onPlayerSelection, onLogout, autoConfirm);
		_logoutButton.Clicked = delegate
		{
			Clusters.Clear();
			onLogout();
		};
	}
}
