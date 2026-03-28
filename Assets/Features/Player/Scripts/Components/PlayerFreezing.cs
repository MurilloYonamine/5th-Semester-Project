// Autor: Murillo Gomes Yonamine
// Data: 28/03/2026

using UnityEngine;
using FifthSemester.Enemy;

namespace FifthSemester.Player.Components {
    [RequireComponent(typeof(PlayerController))]
    public class PlayerFreezing : MonoBehaviour {
        [SerializeField] private LayerMask _enemyLayer;
        [SerializeField] private Camera _playerCamera;

        private void LateUpdate() {
            FreezeEnemy();
        }

        private void FreezeEnemy() {
            EnemyController[] enemies = EnemyController.AllEnemies.ToArray();

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_playerCamera);

            EnemyController frozenEnemy = null;

            foreach (var enemy in enemies) {
                var renderer = enemy.GetComponent<Renderer>();
                if (renderer != null && GeometryUtility.TestPlanesAABB(planes, renderer.bounds)) {
                    enemy.Freeze(true);
                    frozenEnemy = enemy;
                }
                else {
                    enemy.Freeze(false);
                }
            }
        }
    }
}
