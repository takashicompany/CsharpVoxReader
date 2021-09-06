namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class VoxelSkinnedMeshHumanoid : VoxelHumanoid<SimpleVoxel>
	{
		[SerializeField]
		protected SkinnedMeshRenderer _renderer;

		[SerializeField]
		private Mesh _mesh;

		private bool _updateMeshRequested = false;

		private void Update()
		{
			if (_updateMeshRequested)
			{
				UpdateMesh();
				_updateMeshRequested = false;
			}
		}
		
		private void UpdateMesh()
		{
			var tris = new List<int>();
			
			foreach (var v in _vertexGenerator.voxels)
			{
				if (IsActiveVoxel(v.x, v.y, v.z))
				{
					tris.AddRange(v.GetTriangleIndices());
				}
			}

			_renderer.sharedMesh.triangles = tris.ToArray();
		}

		[ContextMenu("generate all")]
		private void　Generate()
		{
			_vertexGenerator.GenerateVoxel(true);

			GenerateBones();

			var boneIndexDict = new Dictionary<HumanBodyBones, int>();

			Debug.Assert(Common.humanBoneNames.Length == _bones.Length, "ボーンの数が不正です。" + Common.humanBoneNames.Length + "/" + _bones.Length);

			for (int i = 0; i < Common.humanBoneNames.Length; i++)
			{
				var bn = Common.humanBoneNames[i];
				boneIndexDict[bn] = i;
			}

			_mesh = _vertexGenerator.GenerateMesh();

			var boneWeights = new List<BoneWeight>();

			foreach (var v in _vertexGenerator.voxels)
			{
				foreach (var p in v.GetVertexPoints())
				{
					var index = boneIndexDict[v.bone];
					var bw = new BoneWeight();
					bw.boneIndex0 = index;
					bw.weight0 = 1f;
					boneWeights.Add(bw);
				}
			}

			_mesh.boneWeights = boneWeights.ToArray();

			Matrix4x4[] bindPoses = new Matrix4x4[_bones.Length];

			for (int i = 0; i < bindPoses.Length; i++)
			{
				var bone = _bones[i];
				bindPoses[i] = bone.worldToLocalMatrix * transform.localToWorldMatrix;
			}

			_mesh.bindposes = bindPoses;

			_mesh.RecalculateNormals();
			_mesh.RecalculateBounds();
			_mesh.RecalculateTangents();

			_renderer.bones = _bones;
			_renderer.sharedMesh = _mesh;

			SetUpAvatar();
		}

		public override void Damage(Vector3 position, float radius)
		{
			base.Damage(position, radius);
			UpdateMesh();
		}

		public override void RequestUpdateMesh()
		{
			_updateMeshRequested = true;
		}

		private void OnDrawGizmos()
		{
			// if (_voxelPositionMap != null)
			// {
			// 	Gizmos.color = Color.red;

			// 	foreach (var v in _vertexGenerator.voxels)
			// 	{
			// 		var p = _voxelPositionMap[v.x, v.y, v.z];

			// 		if (_boneDict.TryGetValue(v.bone, out var bone))
			// 		{
			// 			var wp = bone.TransformPoint(p);

			// 			Gizmos.DrawWireSphere(wp, _vertexGenerator.voxelUnitScale / 2);
			// 		}
			// 	}
			// }
		}

	}
}