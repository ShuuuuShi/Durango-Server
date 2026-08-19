using System.Collections.Generic;
using UnityEngine;

public static class ShaderLibrary
{
	private static readonly Dictionary<string, string> OpaqueToTransparentDict = new Dictionary<string, string>
	{
		{ "LitSphere/Diffuse", "LitSphere/Transparent" },
		{ "LitSphere/DiffuseNoCull", "LitSphere/Transparent" },
		{ "LitSphere/AlphaTest", "LitSphere/Transparent" },
		{ "LitSphere/AlphaTestNoCull", "LitSphere/Transparent" },
		{ "LitSphere/Specular", "LitSphere/Specular/Transparent" },
		{ "LitSphere/Specular/AlphaTest", "LitSphere/Specular/Transparent" },
		{ "LitSphere/Diffuse2X", "LitSphere/Transparent2X" },
		{ "LitSphere/AlphaTest2X", "LitSphere/Transparent2X" },
		{ "LitSphere/Specular2X", "LitSphere/Specular/Transparent2X" },
		{ "LitSphere/ThreeColor/Diffuse", "LitSphere/ThreeColor/Transparent" },
		{ "LitSphere/ThreeColor/Alphatest", "LitSphere/ThreeColor/Transparent" },
		{ "LitSphere/ThreeColor/DualMode/Alphatest", "LitSphere/ThreeColor/DualMode/Transparent" },
		{ "LitSphere/ThreeColor/DualMode/Fixed/Diffuse", "LitSphere/ThreeColor/DualMode/Fixed/Transparent" },
		{ "LitSphere/ThreeColor/DualMode/Fixed/Alphatest", "LitSphere/ThreeColor/DualMode/Fixed/Transparent" },
		{ "LitSphere/DualMode/Diffuse", "LitSphere/DualMode/Transparent" },
		{ "LitSphere/DualMode/AlphaTest", "LitSphere/DualMode/Transparent" },
		{ "LitSphere/DualMode/Diffuse2X", "LitSphere/DualMode/Transparent2X" },
		{ "LitSphere/DualMode/AlphaTest2X", "LitSphere/DualMode/Transparent2X" },
		{ "Custom/Unlit/Opaque", "Custom/Unlit/Transparent" }
	};

	public static Shader GetTransparent(Shader shader)
	{
		if ((Object)(object)shader == (Object)null)
		{
			return null;
		}
		string text = OpaqueToTransparentDict.Get(((Object)shader).name);
		if (string.IsNullOrEmpty(text))
		{
			return shader;
		}
		Shader val = Shader.Find(text);
		if ((Object)(object)val == (Object)null)
		{
			return shader;
		}
		return val;
	}
}
