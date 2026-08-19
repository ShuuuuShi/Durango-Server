using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Durango.UI.Popup;

public class PopupGroup_PC : PopupGroup
{
	protected override IEnumerable<GameObject> GetPopupList()
	{
		GameObject[] popupArray = Resources.LoadAll<GameObject>("Popup");
		return popupArray.Where((GameObject x) => !popupArray.Any((GameObject y) => y.name.Equals(x.name + "_PC", StringComparison.OrdinalIgnoreCase)));
	}
}
