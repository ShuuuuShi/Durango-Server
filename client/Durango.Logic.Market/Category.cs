namespace Durango.Logic.Market;

public class Category
{
	public class Sub
	{
		private string _name;

		public string Id { get; private set; }

		public string Name => (_name != null) ? _name : (_name = LocalizeSystem.Get($"#prototype_sub_category_{Id}"));

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

		private string Key => (_key != null) ? _key : (_key = $"#prototype_category_{Id}");

		public string Name => (_name != null) ? _name : (_name = LocalizeSystem.Get(Key));

		public string Icon => (_icon != null) ? _icon : (_icon = IconMap.Get(Key));

		public Main(string id)
		{
			Id = id;
		}
	}

	public Main MainCategory;

	public Sub[] Subs;
}
