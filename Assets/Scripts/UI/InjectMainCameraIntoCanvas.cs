using UnityEngine;

namespace UIp
{
    /// <summary>
    /// This is just a simple utils class to hook the main camera into a canvas. 
    /// Author: Juan Pablo Varela Aloisio
    /// email: juampyvarela@gmail.com
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class InjectMainCameraIntoCanvas : MonoBehaviour
    {
        private void Awake()
            => GetComponent<Canvas>().worldCamera = Camera.main;
    }
}
