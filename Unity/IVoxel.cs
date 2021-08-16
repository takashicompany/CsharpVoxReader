namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public interface IVoxel
	{
		Vector3Int voxelPosition { get; }
		Vector3 positionFromCenter { get; }
		HumanBodyBones bone { get; }

		int x { get; }
		int y { get; }
		int z { get; }

		void Init(Vector3Int position, Vector3 positionFromCenter, float unitPerSize, HumanBodyBones bone, int startIndex);

		void CreateCache();

		IEnumerable<Vector3> GetVertexPoints();

		IEnumerable<int> GetTriangleIndices();
	}

	public static partial class Extension
	{

	}
}