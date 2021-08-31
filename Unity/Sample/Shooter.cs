namespace TakashiCompany.Unity.VoxReader.Sample
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;

	public class Shooter : MonoBehaviour
	{
		[SerializeField]
		private VoxelHumanoid _target;

		[SerializeField]
		private Bullet _prefab;

		[SerializeField]
		private VoxelParticle _voxelParticle;

		private bool _isRecover;

		private void Awake()
		{
			Application.targetFrameRate = 60;
		}

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (!_isRecover)
				{
					var bullet = Instantiate(_prefab, transform);

					bullet.Shoot(ray);
				}
				else
				{
					var count = 0;

					foreach (var v in _target.voxels)
					{
						if (!_target.IsActiveVoxel(v.voxelPosition))
						{
							_voxelParticle.Repair(_target, v, ray.origin + ray.direction * 2, 5, voxel =>
							{
								_target.ChangeVoxelActive(voxel.voxelPosition, true);
								_target.RequestUpdateMesh();
							});

							count++;

							if (count >= 30)
							{
								break;
							}
							
						}
					}
				}
			}

			if (Input.GetMouseButton(0) && _isRecover)
			{
				
			}
		}

		private void OnGUI()
		{
			if (GUI.Button(new Rect(0, Screen.height - 100, 100 ,100), "Recover\n" + _isRecover))
			{
				_isRecover = !_isRecover;
			}
		}
	}
}