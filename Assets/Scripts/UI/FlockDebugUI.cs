using System;
using Core.Steering;
using TMPro;
using UnityEngine;
using VarelaAloisio.Core;

namespace UI
{
    public class FlockDebugUI : MacacoBehaviour
    {
        [SerializeField] private Ref<IFlockController> controller;
        [SerializeField] private TMP_Text count;

        private void LateUpdate()
        {
            count?.SetText(controller.HasValue ? controller.Value.Flock.Count.ToString() : "null");
        }
    }
}
