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

		private VoxelRepairer _repairer;

		private void Awake()
		{
			Application.targetFrameRate = 60;
		}

		private void Start()
		{
			_repairer = _target.GetComponent<VoxelRepairer>();
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
					_repairer.RepairRandom(50, Vector3.up * 2, 4, 10);
				}
			}

			_target.transform.Rotate(Vector3.up * 90 * Time.deltaTime);
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