using System;
using Shared.Etc;
using UnityEngine;

public static class ArtifactUtil
{
	public static string GetAssetDirectory(string resource)
	{
		return "Models/" + resource;
	}

	public static Direction RotationToDirection(Rotation rotation)
	{
		return rotation switch
		{
			Rotation.None => Direction.SouthWest, 
			Rotation.Quarter => Direction.SouthEast, 
			Rotation.Half => Direction.NorthEast, 
			Rotation.ThreeQuarter => Direction.NorthWest, 
			_ => throw new ArgumentException("Invalid Rotation - " + rotation), 
		};
	}

	public static Vector3 DirectionToAngle(Direction dir)
	{
		return dir switch
		{
			Direction.SouthWest => Vector3.up * 0f, 
			Direction.SouthEast => Vector3.up * -90f, 
			Direction.NorthWest => Vector3.up * 90f, 
			Direction.NorthEast => Vector3.up * 180f, 
			Direction.West => Vector3.up * 45f, 
			Direction.South => Vector3.up * -45f, 
			Direction.East => Vector3.up * -135f, 
			Direction.North => Vector3.up * 135f, 
			_ => throw new ArgumentException("Invalid Direction - " + dir), 
		};
	}

	public static Vector3 DirectionToForwardAngle(Direction dir)
	{
		switch (dir)
		{
		case Direction.SouthWest:
		case Direction.NorthEast:
			return Vector3.up * 0f;
		case Direction.SouthEast:
		case Direction.NorthWest:
			return Vector3.up * -90f;
		case Direction.West:
			return Vector3.up * 45f;
		case Direction.South:
			return Vector3.up * -45f;
		case Direction.East:
			return Vector3.up * -135f;
		case Direction.North:
			return Vector3.up * 135f;
		default:
			throw new ArgumentException("Invalid Direction - " + dir);
		}
	}

	public static Vector2 GetDirectionPivot(Direction dir)
	{
		return dir switch
		{
			Direction.SouthWest => new Vector2(0f, 0.5f), 
			Direction.SouthEast => new Vector2(0.5f, 0f), 
			Direction.NorthWest => new Vector2(0.5f, 1f), 
			Direction.NorthEast => new Vector2(1f, 0.5f), 
			Direction.West => new Vector2(0f, 1f), 
			Direction.South => new Vector2(0f, 0f), 
			Direction.East => new Vector2(1f, 0f), 
			Direction.North => new Vector2(1f, 1f), 
			_ => throw new ArgumentException("Invalid Direction - " + dir), 
		};
	}
}
