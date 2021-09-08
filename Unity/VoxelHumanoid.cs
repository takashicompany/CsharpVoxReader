namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public abstract class VoxelHumanoid : MonoBehaviour, IVoxelObject
	{
		[SerializeField]
		protected Animator _animator;

		[SerializeField]
		protected Transform _rootBone;

		[SerializeField]
		protected Transform[] _bones;

		[SerializeField]
		protected bool _withCollider;
		
		private bool[,,] _voxelActive;

		private IVoxel[,,] _voxelMap;

		private Vector3[,,] _voxelPositionMap;

		private Dictionary<HumanBodyBones, List<IVoxel>> _voxelBoneDict;

		private Dictionary<HumanBodyBones, Transform> _boneDict;

		private Transform[,,] _boneMap;

		public event IVoxelObject.VoxelDestroyDelegate onVoxelDestroyEvent;

		protected abstract Vector3Int _voxelSize { get; }
		public Vector3Int voxelSize => _voxelSize;

		protected abstract IVoxel[] _voxels { get; }
		public IVoxel[] voxels => _voxels;

		protected virtual void Awake()
		{
			Init();
		}

		private void Init()
		{
			var size = _voxelSize;
			_voxelActive = new bool[size.x, size.y, size.z];
			_voxelMap = new IVoxel[size.x, size.y, size.z];
			_voxelPositionMap = new Vector3[size.x, size.y, size.z];
			_boneMap = new Transform[size.x, size.y, size.z];

			_voxelBoneDict = new Dictionary<HumanBodyBones, List<IVoxel>>();
			_boneDict = new Dictionary<HumanBodyBones, Transform>();

			foreach (var v in _voxels)
			{
				if (v == null)
				{
					Debug.LogError("nullです！");
					continue;
				}
				_voxelActive[v.x, v.y, v.z] = true;
				_voxelMap[v.x, v.y, v.z] = v;

				if (v.bone != HumanBodyBones.LastBone)
				{
					// if (!_voxelBoneDict.ContainsKey(v.bone))
					// {
					// 	_voxelBoneDict.Add(v.bone, new List<IVoxel>() { v });
					// }
					// else
					// {
					// 	_voxelBoneDict[v.bone].Add(v);
					// }

					_voxelBoneDict.GetOrNew(v.bone).Add(v);

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

						_boneMap[v.x, v.y, v.z] = myBone;
					}
				}
			}
		}

		protected void GenerateBones()
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

			var boundsDict = _voxels.BuildBoundsDict();

			var boneDict = new Dictionary<HumanBodyBones, Transform>();
			var bones = new List<Transform>();

			boneDict.Add(HumanBodyBones.LastBone, _rootBone);

			foreach (var boneName in Common.humanBoneNames)
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

				if (boneName == "Hips")
				{
					sb.position = Vector3.zero;
				}

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

		protected List<int> GenerateTriangleIndices(HumanBodyBones bone, bool onlyActiveVoxel)
		{
			var triangleIndices = new List<int>();
			if (_voxelBoneDict.TryGetValue(bone, out var voxels))
			{
				foreach (var v in voxels)
				{
					if (onlyActiveVoxel && !IsActiveVoxel(v.voxelPosition))
					{
						continue;
					}

					triangleIndices.AddRange(v.GetTriangleIndices());
				}
			}

			return triangleIndices;
		}

		protected void SetUpAvatar()
		{
			_animator.avatar = GenerateAvatar();
		}

		protected Transform GetBone(HumanBodyBones bone)
		{
			return _animator.GetBoneTransform(bone);
		}

		public bool IsActiveVoxel(Vector3Int voxelPosition)
		{
			return IsActiveVoxel(voxelPosition.x, voxelPosition.y, voxelPosition.z);
		}

		public bool IsActiveVoxel(int x, int y, int z)
		{
			return _voxelActive[x, y, z];
		}

		public Vector3 GetVoxelWorldPosition(Vector3Int voxelPosition)
		{
			var pos = _voxelPositionMap[voxelPosition.x, voxelPosition.y, voxelPosition.z];

			var bone = _boneMap[voxelPosition.x, voxelPosition.y, voxelPosition.z];

			var worldPos = bone.TransformPoint(pos);

			return worldPos;
		}

		public virtual void Damage(Vector3 center, float radius)	// List<IVoxel>をreturnした方がコスト安いかも
		{
			foreach (var v in _voxels)
			{
				if (IsActiveVoxel(v.x, v.y, v.z))
				{
					var pos = GetVoxelWorldPosition(v.voxelPosition);

					if (Vector3.Distance(center, pos) <= radius)
					{
						ChangeVoxelActive(v.voxelPosition, false);
						OnDestroyVoxel(v, pos, center);
					}
				}
			}
		}

		public void ChangeVoxelActiveAll(bool active)
		{
			foreach (var v in _voxels)
			{
				ChangeVoxelActive(v.voxelPosition, active);
			}
		}

		public void ChangeVoxelActive(Vector3Int voxelPosition, bool active)
		{
			if (_voxelActive[voxelPosition.x, voxelPosition.y, voxelPosition.z] != active)
			{
				_voxelActive[voxelPosition.x, voxelPosition.y, voxelPosition.z] = active;
				OnChangeVoxelActive(voxelPosition, active);
			}
		}

		protected virtual void OnChangeVoxelActive(Vector3Int voxelPosition, bool active)
		{

		}

		protected virtual void OnDestroyVoxel(IVoxel voxel, Vector3 point, Vector3 center)
		{
			onVoxelDestroyEvent?.Invoke(voxel, point, center);
		}

		public IVoxel GetVoxel(int x, int y, int z)
		{
			return _voxelMap[x, y, z];
		}

		public virtual void RequestUpdateMesh()
		{

		}
	}

	public abstract class VoxelHumanoid<T> : VoxelHumanoid where T : IVoxel, new()
	{
		[SerializeField]
		protected VoxelMeshGenerator<T> _vertexGenerator;
		
		protected override Vector3Int _voxelSize => _vertexGenerator.voxelSize;
		protected override IVoxel[] _voxels => _vertexGenerator.voxels;
	}
}