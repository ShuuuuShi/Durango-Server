using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI.Popup;

[CreateAssetMenu]
public class CardNewsAsset : ScriptableObject
{
	[Serializable]
	public class Card
	{
		[SerializeField]
		public Texture Texture;

		[LocalizableString]
		[SerializeField]
		public string Subject;

		[TextArea]
		[LocalizableString]
		[SerializeField]
		public string Explain;
	}

	[SerializeField]
	public List<Card> Cards = new List<Card>();
}
