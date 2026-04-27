using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Navigator : MonoBehaviour
{
    [SerializeField] private Button[] buttons;

    private void Reset()
        => buttons = GetComponentsInChildren<Button>();

    private void Awake()
    {
        foreach (Button button in buttons)
        {
            
        }
    }
}
