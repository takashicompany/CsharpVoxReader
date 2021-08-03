namespace TakashiCompany.Unity.VoxReader
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using CsharpVoxReader;
	using CsharpVoxReader.Chunks;

	public class VoxLoader : CsharpVoxReader.IVoxLoader
	{
		private byte[,,] _voxelMap;

		public byte[,,] voxelMap => _voxelMap;

		private int[,,] _modelIndexMap;

		private HumanBodyBones?[,,] _voxelBoneMap;

		public HumanBodyBones?[,,] voxelBoneMap => _voxelBoneMap;

		private int _modelIndex = 1;
		private int _transformIndex = 1;

		private Vector3Int _size;

		void IVoxLoader.LoadModel(int sizeX, int sizeY, int sizeZ, byte[,,] data)
		{
			if (_voxelMap == null)
			{
				_size = new Vector3Int(sizeX, sizeY, sizeZ);
				_voxelMap = new byte[sizeX, sizeY, sizeZ];
				_modelIndexMap = new int[sizeX, sizeY, sizeZ];
				_voxelBoneMap = new HumanBodyBones?[sizeX, sizeY, sizeZ];

				_modelIndex = 1;
				_transformIndex = 1;
			}

			data.Foreach((voxelIndex, d) =>
			{
				if (d > 0)
				{
					_voxelMap[voxelIndex.x, voxelIndex.y, voxelIndex.z] = d;
					_modelIndexMap[voxelIndex.x, voxelIndex.y, voxelIndex.z] = _modelIndex;
				}
			});

			_modelIndex++;
		}

		void IVoxLoader.LoadPalette(uint[] palette)
		{
			
		}

		void IVoxLoader.NewGroupNode(int id, Dictionary<string, byte[]> attributes, int[] childrenIds)
		{
			
		}

		void IVoxLoader.NewLayer(int id, string name, Dictionary<string, byte[]> attributes)
		{
			
		}

		void IVoxLoader.NewMaterial(int id, Dictionary<string, byte[]> attributes)
		{
			
		}

		void IVoxLoader.NewShapeNode(int id, Dictionary<string, byte[]> attributes, int[] modelIds, Dictionary<string, byte[]>[] modelsAttributes)
		{
			
		}

		void IVoxLoader.NewTransformNode(int id, int childNodeId, int layerId, string name, Dictionary<string, byte[]>[] framesAttributes)
		{
			// nameが入ってない場合は多分root
			if (name == null)
			{
				return;
			}

			_modelIndexMap.Foreach((x, y, z, index) =>
			{
				if (_transformIndex == index)
				{
					if (name.TryGetHumanBodyBones(out var bone))
					{
						_voxelBoneMap[x, y, z] = bone;
					}
					else
					{
						Debug.LogError("名前が不正です:" + name);
					}
				}
			});

			_transformIndex++;
		}

		void IVoxLoader.SetMaterialOld(int paletteId, MaterialOld.MaterialTypes type, float weight, MaterialOld.PropertyBits property, float normalized)
		{
			
		}

		void IVoxLoader.SetModelCount(int count)
		{
			
		}
	}
}