using System;
using Durango.Logic;

namespace Durango.UI;

public interface IMenuList
{
	event Action<MenuType> MenuClicked;

	void Refresh();

	bool TryGetMenuItem(MenuType type, out MenuWidget comp);

	void Show(bool instant);

	void Hide();
}
