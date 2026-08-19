namespace Durango.Logic.Market;

public class Category
{
	public class Sub
	{
		private string _name;

		public string Id { get; private set; }

		public string Name
		{
			get
			{
				if (_name == null)
				{
					return _name = LocalizeSystem.Get("#prototype_sub_category_" + Id);
				}
				return _name;
			}
		}

		public Sub(string id)
		{
			Id = id;
		}
	}

	public class Main
	{
		private string _key;

		private string _name;

		private string _icon;

		public string Id { get; private set; }

		private string Key
		{
			get
			{
				if (_key == null)
				{
					return _key = "#prototype_category_" + Id;
				}
				return _key;
			}
		}

		public string Name
		{
			get
			{
				if (_name == null)
				{
					return _name = LocalizeSystem.Get(Key);
				}
				return _name;
			}
		}

		public string Icon
		{
			get
			{
				if (_icon == null)
				{
					return _icon = IconMap.Get(Key);
				}
				return _icon;
			}
		}

		public Main(string id)
		{
			Id = id;
		}
	}

	public Main MainCategory;

	public Sub[] Subs;
}
