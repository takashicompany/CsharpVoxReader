namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class VoxelMeshHumanoid : VoxelHumanoid
	{
		[SerializeField]
		private Material _material;

		[SerializeField]
		private MeshFilter[] _meshFilters;

		[ContextMenu("generate")]
		private void Generate()
		{
			_vertexGenerator.GenerateVoxel(false);

			GenerateBones();
			SetUpAvatar();

			var dict = _vertexGenerator.GenerateMeshDict();

			var meshFilters = new List<MeshFilter>();

			foreach (var kvp in dict)
			{
				var boneName = kvp.Key;
				var mesh = kvp.Value;
				var bone = GetBone(boneName);
				
				if (!bone.transform.TryGetComponent<MeshFilter>(out var mf))
				{
					mf = bone.gameObject.AddComponent<MeshFilter>();
				}

				if (!bone.TryGetComponent<MeshRenderer>(out var renderer))
				{
					renderer = bone.gameObject.AddComponent<MeshRenderer>();
				}

				renderer.sharedMaterial = _material;

				mf.sharedMesh = mesh;
				meshFilters.Add(mf);
			}

			_meshFilters = meshFilters.ToArray();
		}
	}
}