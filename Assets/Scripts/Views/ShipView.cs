using System.Linq;
using Units;
using UnityEngine;

namespace Views
{
    public class ShipView : MonoBehaviour
    {
        [SerializeField] private Ship ship;
        [ContextMenuItem("Populate", nameof(FetchThrusters))]
        [SerializeField] private ParticleSystem[] thrusterParticles;

        private void Reset()
            => ship = GetComponent<Ship>()
                      ?? gameObject.AddComponent<Ship>();

        private void Update()
        {
            bool isMoving = ship.MoveDirection.magnitude > 0;
            foreach (ParticleSystem particle in thrusterParticles)
            {
                if (isMoving)
                    particle?.Play();
                else
                    particle?.Stop();
            }
        }
        private void FetchThrusters()
        {
            thrusterParticles = transform.GetComponentsInChildren<ParticleSystem>()
                                         .Where(tr => tr.CompareTag("Thruster"))
                                         .ToArray();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
