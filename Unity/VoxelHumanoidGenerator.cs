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

			var mesh = _vertexGenerator.GenerateMesh();

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

			mesh.boneWeights = boneWeights.ToArray();

			Matrix4x4[] bindPoses = new Matrix4x4[_bones.Length];

			for (int i = 0; i < bindPoses.Length; i++)
			{
				var bone = _bones[i];
				bindPoses[i] = bone.worldToLocalMatrix * transform.localToWorldMatrix;
			}

			mesh.bindposes = bindPoses;

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			mesh.RecalculateTangents();

			_renderer.bones = _bones;
			_renderer.sharedMesh = mesh;

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

			var v2m = _vertexGenerator;

			var boundsDict = _vertexGenerator.BuildBoundsDict();

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

	}
}