using UnityEngine;

namespace Management
{
    [RequireComponent(typeof(Canvas))]
    public class InjectMainCameraIntoCanvas : MonoBehaviour
    {
        private void Awake()
            => GetComponent<Canvas>().worldCamera = Camera.main;
    }
}
