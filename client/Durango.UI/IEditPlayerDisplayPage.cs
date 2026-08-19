using System;
using UnityEngine;

namespace Durango.UI;

internal interface IEditPlayerDisplayPage
{
	event Action Confirmed;

	void Initialize(EditPlayerDisplayProxy display);

	void Show(bool instant);

	void Hide(bool instant);

	Transform GetModelPosition();

	void SetConfirmText(string text);

	void WaitForLoading(bool loading);
}
