using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class DialogsManager : Singleton<DialogsManager>
{
	public GameObject _colliderWithKid;

	public GameObject _triggerDialogAfterEvent;

	public GameObject _triggerDialogActingSitDown;

	public GameObject _npcKid;

	public TextAsset _dialoglocalizedTextFile;

	public void AfterKidFindDropItem()
	{
	}
}
