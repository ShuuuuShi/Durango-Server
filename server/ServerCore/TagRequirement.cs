namespace DurangoServer.Core;

public readonly struct TagRequirement
{
    public readonly string Id;
    public readonly int Level;

    public TagRequirement(string id, int level)
    {
        Id = id;
        Level = level;
    }

    public override string ToString() => Id + " " + Level;
}
