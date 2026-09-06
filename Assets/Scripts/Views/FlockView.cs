using Core.Steering;
using Unity.Collections;
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

        private void DrawFlock(NativeList<Boid> flock)
        {
            if (mesh && material)
            {
                var matrices = new Matrix4x4[flock.Length];
                for (int i = 0; i < flock.Length; i++)
                {
                    Boid boid = flock[i];
                    matrices[i] = Matrix4x4.TRS(boid.Position,
                                                Quaternion.LookRotation(math.normalize(boid.Velocity), Vector3.back),
                                                Vector3.one * unitSize);
                }

                Graphics.RenderMeshInstanced(new RenderParams(material), mesh, 0,
                                             matrices);
            }
            if (destinationMesh && destinationMaterial)
            {
                Graphics.RenderMesh(new RenderParams(destinationMaterial), destinationMesh, 0,
                                    Matrix4x4.Translate(controller.Value.Destination));
            }
        }
    }
}
