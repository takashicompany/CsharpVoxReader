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

		private Dictionary<HumanBodyBones, MeshFilter> _meshFilterDict;
		

		protected override void Awake()
		{
			base.Awake();

			_meshFilterDict = new Dictionary<HumanBodyBones, MeshFilter>();

			foreach(var boneName in Common.humanBoneNames)
			{
				var bone = GetBone(boneName);
				
				_meshFilterDict.Add(boneName, bone.GetComponent<MeshFilter>());
			}
		}

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
		}

		private void UpdateMesh()
		{
			// 一旦シンプルに
			foreach (var boneName in Common.humanBoneNames)
			{
				var tri = GenerateTriangleIndices(boneName, true).ToArray();
				var mesh = GetMesh(boneName);
				mesh.triangles = tri;
			}
		}

		private Mesh GetMesh(HumanBodyBones bone)
		{
			return _meshFilterDict[bone].sharedMesh;	// shared mesh
		}

		public override void Damage(Vector3 position, float radius)
		{
			base.Damage(position, radius);
			UpdateMesh();
		}
	}
}