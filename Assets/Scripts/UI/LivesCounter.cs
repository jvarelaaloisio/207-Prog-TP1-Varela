using Core.Services;
using TMPro;
using UnityEngine;
using VarelaAloisio.Core;

namespace UI
{
    /// <summary>
    /// Simple visual representation for the lives the player has left.
    /// It only updates in OnEnable because it's meant to be deactivated all the time, except when the player dies.
    /// Author: Juan Pablo Varela Aloisio
    /// email: juampyvarela@gmail.com
    /// </summary>
    public class LivesCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        private void OnEnable()
        {
            if (!Service.TryGet(out IGameManager gameManager))
            {
                Debug.LogError($"{name} <color=grey>({nameof(LivesCounter)})</color>: {nameof(IGameManager)} not found.", this);
                return;
            }
            label?.SetText(gameManager.LivesLeft.ToString());
        }
    }
}