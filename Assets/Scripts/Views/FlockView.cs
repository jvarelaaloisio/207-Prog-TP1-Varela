using System.Collections.Generic;
using System.Linq;
using Core.Game;
using Core.Steering;
using Unity.Mathematics;
using UnityEngine;
using VarelaAloisio.Core;

namespace Views
{
    public class FlockView : MacacoBehaviour
    {
        [SerializeField] private Ref<IFlockController> controller;
        [SerializeField] private Mesh mesh;
        [SerializeField] private Mesh destinationMesh;
        [SerializeField] private Material material;
        [SerializeField] private Material destinationMaterial;
        [SerializeField] private float unitSize = 0.05f;

        private void LateUpdate()
        {
            if (controller.HasValue)
                DrawFlock(controller.Value.Flock);
        }

        private void DrawFlock(List<Boid> flock)
        {
            if (mesh && material)
            {
                Graphics.RenderMeshInstanced(new RenderParams(material), mesh, 0,
                                             flock.Select(boid => Matrix4x4.TRS(boid.Position,
                                                                                Quaternion.LookRotation(math.normalize(boid.Velocity), Vector3.back),
                                                                                Vector3.one * unitSize))
                                                  .ToArray());
            }
            if (destinationMesh && destinationMaterial)
            {
                Graphics.RenderMesh(new RenderParams(destinationMaterial), destinationMesh, 0,
                                    Matrix4x4.Translate(controller.Value.Destination));
            }
        }
    }
}
