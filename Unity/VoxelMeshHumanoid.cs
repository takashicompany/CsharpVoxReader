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


			foreach (var boneName in Common.humanBoneNames)
			{
				var mesh = dict[boneName];
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

		private void UpdateMesh()
		{
			// 一旦シンプルに
			foreach (var boneName in Common.humanBoneNames)
			{
				
			}
		}

		public override void Damage(Vector3 position, float radius)
		{
			base.Damage(position, radius);
			UpdateMesh();
		}
	}
}