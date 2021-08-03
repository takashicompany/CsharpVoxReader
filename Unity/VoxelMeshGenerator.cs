namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class VoxelMeshGenerator : MonoBehaviour
	{
		[SerializeField]
		private VoxelVertexGenerator _vertexGenerator;

		[SerializeField]
		private MeshFilter _meshFilter;

		[ContextMenu("generate")]
		private void Generate()
		{
			_vertexGenerator.LoadVoxFile();
			var mesh = _vertexGenerator.GenerateMesh();

			_meshFilter.sharedMesh = mesh;
		}

	}
}