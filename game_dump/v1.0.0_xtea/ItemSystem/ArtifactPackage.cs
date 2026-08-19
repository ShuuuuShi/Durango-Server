using Messages;
using Shared.Item;

namespace ItemSystem;

public class ArtifactPackage
{
	public int Size;

	public PackageStatus Status;

	public ArtifactCapsule[] Artifacts;

	public ArtifactPackage(Messages.ArtifactPackage package)
	{
		Size = package.Size;
		Status = package.Status;
		int size = KUtility.GetSize(package.Artifacts);
		if (size > 0)
		{
			Artifacts = new ArtifactCapsule[size];
			for (int i = 0; i < size; i++)
			{
				Artifacts[i] = new ArtifactCapsule(package.Artifacts[i]);
			}
		}
	}
}
