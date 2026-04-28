using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class CustomButton : Button
    {
        public Action<Transform> RequestPointer;
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            RequestPointer?.Invoke(transform);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            RequestPointer(transform);
        }
    }
}