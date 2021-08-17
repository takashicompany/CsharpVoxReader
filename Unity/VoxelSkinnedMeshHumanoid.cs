namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class VoxelSkinnedMeshHumanoid : VoxelHumanoid
	{
		[SerializeField]
		protected SkinnedMeshRenderer _renderer;

		[SerializeField]
		private Mesh _mesh;

		[SerializeField]
		private ParticleSystem _damage;

		private void UpdateMesh()
		{
			var tris = new List<int>();
			
			foreach (var v in _vertexGenerator.voxels)
			{
				var active = _voxelActive[v.x, v.y, v.z];

				if (active)
				{
					tris.AddRange(v.GetTriangleIndices());
				}
			}

			_mesh.triangles = tris.ToArray();
		}

		[ContextMenu("generate all")]
		private void　Generate()
		{
			_vertexGenerator.GenerateVoxel(true);

			GenerateBones();

			var boneIndexDict = new Dictionary<HumanBodyBones, int>();

			Debug.Assert(VoxToMesh.boneNames.Length == _bones.Length, "ボーンの数が不正です。" + VoxToMesh.boneNames.Length + "/" + _bones.Length);

			for (int i = 0; i < VoxToMesh.boneNames.Length; i++)
			{
				var bn = VoxToMesh.boneNames[i];
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

		protected override void OnDestroyVoxel(IVoxel voxel, Vector3 position)
		{
			base.OnDestroyVoxel(voxel, position);

			var p = GetVoxelWorldPosition(voxel.x, voxel.y, voxel.z);
			var ep = new ParticleSystem.EmitParams();
			ep.position = p;
			ep.velocity = (p - position).normalized * Random.Range(1, 2);
			_damage.Emit(ep, 1);
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