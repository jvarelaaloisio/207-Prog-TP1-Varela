using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Intermediary class to allow for this object to be pointed at. 
    /// Author: Juan Pablo Varela Aloisio
    /// email: juampyvarela@gmail.com
    /// </summary>
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
            RequestPointer?.Invoke(transform);
        }
    }
}