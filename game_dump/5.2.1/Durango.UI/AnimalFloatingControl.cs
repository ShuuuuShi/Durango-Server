using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class AnimalFloatingControl : MonoBehaviour
{
	[SerializeField]
	private UISprite _statusIcon;

	private bool _hideStatusIcon;

	public AnimalBehavior Animal { get; private set; }

	public PetAI Pet { get; private set; }

	public void Initialize([NotNull] AnimalBehavior animal)
	{
		Animal = animal;
		Pet = animal.GetComponent<PetAI>();
		_hideStatusIcon = animal is HumanBehavior;
	}

	public void SetStatusIcon(SpriteData spriteData)
	{
		if (_hideStatusIcon || string.IsNullOrEmpty(spriteData.sprite))
		{
			_statusIcon.gameObject.SetActive(value: false);
			return;
		}
		_statusIcon.gameObject.SetActive(value: true);
		spriteData.Set(_statusIcon);
	}
}
