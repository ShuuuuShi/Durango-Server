using UnityEngine;

public class RootMotionExporter : MonoBehaviour
{
	public float _deadband = 5f;

	public float _deadbandYaw = 5f;

	public string _rootMotionBoneName = "Bip001";

	public Vector3 _rootMotionForward = Vector3.forward;

	public bool _doesImportAnimEventFilesFolder;
}
