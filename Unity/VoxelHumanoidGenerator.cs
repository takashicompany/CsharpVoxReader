namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class VoxelHumanoidGenerator : MonoBehaviour
	{
		[SerializeField]
		private VoxelVertexGenerator _vertexGenerator;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private SkinnedMeshRenderer _renderer;

		[SerializeField]
		private Transform _rootBone;

		[SerializeField]
		private Transform[] _bones;

		[SerializeField]
		private Mesh _mesh;

		[SerializeField]
		private bool _withCollider;

		private bool[,,] _voxelActive;

		private VoxelVertexGenerator.Voxel[,,] _voxels;

		private Vector3[,,] _voxelPositionMap;

		private Dictionary<HumanBodyBones, List<VoxelVertexGenerator.Voxel>> _voxelBoneDict;

		private Dictionary<HumanBodyBones, Transform> _boneDict;

		private void Awake()
		{
			var size = _vertexGenerator.voxelSize;
			_voxelActive = new bool[size.x, size.y, size.z];
			_voxels = new VoxelVertexGenerator.Voxel[size.x, size.y, size.z];
			_voxelPositionMap = new Vector3[size.x, size.y, size.z];

			_voxelBoneDict = new Dictionary<HumanBodyBones, List<VoxelVertexGenerator.Voxel>>();
			_boneDict = new Dictionary<HumanBodyBones, Transform>();

			foreach (var v in _vertexGenerator.voxels)
			{
				_voxelActive[v.x, v.y, v.z] = true;
				_voxels[v.x, v.y, v.z] = v;

				if (v.bone != HumanBodyBones.LastBone)
				{
					if (!_voxelBoneDict.ContainsKey(v.bone))
					{
						_voxelBoneDict.Add(v.bone, new List<VoxelVertexGenerator.Voxel>() { v });
					}
					else
					{
						_voxelBoneDict[v.bone].Add(v);
					}

					if (!_boneDict.ContainsKey(v.bone))
					{
						var bone = _animator.GetBoneTransform(v.bone);

						if (bone == null)
						{
							Debug.LogError(v.bone + "が見つかりませんでした");
						}
						else
						{
							_boneDict.Add(v.bone, bone);
						}
					}

					if (_boneDict.TryGetValue(v.bone, out var myBone))
					{
						var wp = _rootBone.TransformPoint(v.positionFromCenter);
						var lp = myBone.InverseTransformPoint(wp);

						_voxelPositionMap[v.x, v.y, v.z] = lp;
					}
				}
			}
		}

		private void Start()
		{
			_vertexGenerator.voxelSize.Foreach((x, y, z) =>
			{
				if (Random.Range(0, 4) == 0)
				{
					_voxelActive[x, y, z] = false;
				}
			});

			UpdateMesh();
		}

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
			_vertexGenerator.LoadVoxFile();

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
				foreach (var p in v.GetVertex())
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

			var avatar = GenerateAvatar();

			_animator.avatar = avatar;
		}

		[ContextMenu("Generate Bones")]
		private void GenerateBones()
		{
			if (_bones != null)
			{
				foreach (var bone in _bones)
				{
					if (bone == null || bone.gameObject == null)
					{
						continue;
					}

					DestroyImmediate(bone.gameObject);
				}
			}

			_bones = null;

			var boundsDict = this._vertexGenerator.BuildBoundsDict();

			var boneDict = new Dictionary<HumanBodyBones, Transform>();
			var bones = new List<Transform>();

			boneDict.Add(HumanBodyBones.LastBone, _rootBone);

			foreach (var boneName in VoxToMesh.boneNames)
			{
				var parentBoneName = boneName.GetParentBone();

				var parent = boneDict[parentBoneName];

				var go = new GameObject(boneName.ToString());

				if (boundsDict.TryGetValue(boneName, out var bounds))
				{
					go.transform.position = _rootBone.TransformPoint(bounds.GetBoneConnectionPoint(boneName));
					go.transform.SetParent(parent);
					
					if (_withCollider)
					{
						var collider = go.AddComponent<BoxCollider>();
						var cb = new Bounds(go.transform.InverseTransformPoint(bounds.center), bounds.size);
						collider.center = cb.center;
						collider.size = cb.size;
					}
				}
				else
				{
					Debug.Log(boneName + "はありません。");
					go.transform.SetParent(parent);
					go.transform.localPosition = Vector3.zero;
				}
				
				boneDict.Add(boneName, go.transform);
				bones.Add(go.transform);
			}

			_bones = bones.ToArray();
		}

		private Avatar GenerateAvatar()
		{
			var bones = _rootBone.GetComponentsInChildren<Transform>();

			var humanBones = new List<HumanBone>();
			var skeletonBones = new List<SkeletonBone>();

			foreach (var boneName in HumanTrait.BoneName)
			{
				var bone = bones.FirstOrDefault(b => b.name == boneName);

				if (bone == null)
				{
					continue;
				}

				// Debug.Assert(bone != null, boneName + "が見つかりませんでした。");

				var hb = new HumanBone();
				hb.boneName = boneName;
				hb.humanName = boneName;
				hb.limit.useDefaultValues = true;

				humanBones.Add(hb);

				var sb = new SkeletonBone();
				sb.name = bone.name;
				sb.position = bone.localPosition;
				sb.rotation = bone.localRotation;
				sb.scale = bone.localScale;

				skeletonBones.Add(sb);
			}

			var rootSb = new SkeletonBone();
			rootSb.name = _rootBone.name;
			rootSb.position = _rootBone.localPosition;
			rootSb.rotation = _rootBone.localRotation;
			rootSb.scale = _rootBone.localScale;

			skeletonBones.Add(rootSb);
		
			var hd = new HumanDescription();
			hd.human = humanBones.ToArray();
			hd.skeleton = skeletonBones.ToArray();

			hd.upperLegTwist = 0.5f;
			hd.lowerLegTwist = 0.5f;
			hd.upperArmTwist = 0.5f;
			hd.lowerArmTwist = 0.5f;

			hd.legStretch = 0.05f;
			hd.armStretch = 0.05f;
			hd.feetSpacing = 0f;
			hd.hasTranslationDoF = false;

			return AvatarBuilder.BuildHumanAvatar(_rootBone.gameObject, hd);
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