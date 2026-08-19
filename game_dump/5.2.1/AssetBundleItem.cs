using JetBrains.Annotations;
using UnityEngine;

public class AssetBundleItem
{
	[NotNull]
	public readonly string Name;

	[NotNull]
	public readonly AssetBundleFile Parent;

	public AssetBundleRequest Request;

	public AssetBundleItem([NotNull] string name, [NotNull] AssetBundleFile parent)
	{
		Name = name;
		Parent = parent;
	}
}
