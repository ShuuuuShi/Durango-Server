using Durango.Logic.Shop;
using Newtonsoft.Json;

namespace Yaml;

public class ShopCategory
{
	[JsonProperty(PropertyName = "key")]
	public string Key;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "icon")]
	public Gettext Icon;

	[JsonProperty(PropertyName = "commodities")]
	public ShopCategoryCondition[] Conditions;

	private ShopCategory[] _childs;

	private bool? _isShowPromotion;

	private string _viewType;

	[JsonProperty(PropertyName = "show_promotion")]
	public bool? ShowPromotion
	{
		set
		{
			_isShowPromotion = value;
		}
	}

	[JsonProperty(PropertyName = "view_type")]
	public string ViewType
	{
		get
		{
			if (string.IsNullOrEmpty(_viewType) && Parent != null)
			{
				return Parent.ViewType;
			}
			return _viewType;
		}
		set
		{
			_viewType = value;
		}
	}

	[JsonProperty(PropertyName = "childs")]
	public ShopCategory[] Childs
	{
		get
		{
			return _childs;
		}
		set
		{
			_childs = value;
			if (_childs != null)
			{
				ShopCategory[] childs = _childs;
				for (int i = 0; i < childs.Length; i++)
				{
					childs[i].Parent = this;
				}
			}
		}
	}

	public ShopCategory Parent { get; private set; }

	public ShopCategory FindChild(string key)
	{
		if (Childs == null)
		{
			return null;
		}
		ShopCategory[] childs = Childs;
		foreach (ShopCategory shopCategory in childs)
		{
			if (shopCategory.Key == key)
			{
				return shopCategory;
			}
		}
		return null;
	}

	public bool IsValidCommodity(Durango.Logic.Shop.Commodity commodity)
	{
		if (Parent != null && !Parent.IsValidCommodity(commodity))
		{
			return false;
		}
		if (KUtility.GetSize(Conditions) == 0)
		{
			return true;
		}
		ShopCategoryCondition[] conditions = Conditions;
		for (int i = 0; i < conditions.Length; i++)
		{
			if (conditions[i].IsValidCommodity(commodity))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsShowPromotion()
	{
		if (_isShowPromotion.HasValue)
		{
			return _isShowPromotion.Value;
		}
		if (Parent != null)
		{
			return Parent.IsShowPromotion();
		}
		return false;
	}
}
