using Core.Game;
using Core.Game.Enums;
using Core.Services;
using HealthSystem;
using HealthSystem.Runtime;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;
using VarelaAloisio.Core;

namespace UI
{
    /// <summary>
    /// The visual representation for the players health points 
    /// Author: Juan Pablo Varela Aloisio
    /// email: juampyvarela@gmail.com
    /// </summary>
    public class LifeBar : MonoBehaviour
    {
        [SerializeField] private Image graphic;
        [SerializeField] private Color fullHealthColor = Color.darkGreen;
        [SerializeField] private Color zeroHealthColor = Color.darkRed;
        [SerializeField] private float barAnimationDuration = .1f;
        private Health _health;

        private void Start()
        {
            if (!Service.TryGet(out IUnitsRepository unitsRepository))
            {
                Debug.LogError($"{name} <color=grey>({nameof(LifeBar)})</color>: Units Repository not found. Deactivating");
                gameObject.SetActive(false);
                return;
            }

            if (unitsRepository.TryGetShipsOfType(ShipType.Player, out var ships)
                && ships.Length > 0)
                HookToHealth(ships[0]);
            else
            {
                unitsRepository.OnShipSpawned += HandleShipSpawned;
                Debug.Log($"{name}: Player not found. Deactivating until spawned");
                gameObject.SetActive(false);
            }
        }

        private void HandleShipSpawned(IShip ship, Team team)
        {
            if (team is Team.Player)
            {
                HookToHealth(ship);
                gameObject.SetActive(true);
                if (Service.TryGet(out IUnitsRepository unitsRepository))
                    unitsRepository.OnShipSpawned -= HandleShipSpawned;
            }
        }

        private async void HookToHealth(IShip ship)
        {
            ship.OnKill += UnhookHealth;
            if (ship.gameObject.TryGetComponent(out IHealthComponent healthComponent))
            {
                await Awaitable.NextFrameAsync();
                _health = healthComponent.Health;
                _health.OnDamage += HandleLifeChanged;
                _health.OnHeal += HandleLifeChanged;
                HandleLifeChanged(_health.HP, _health.HP);
            }
            else
                Debug.LogError($"{name} <color=grey>({nameof(LifeBar)})</color>: Player ship doesn't have health.");
        }

        private void UnhookHealth(IShip _)
        {
            _health = null;
            
            if (!Service.TryGet(out IUnitsRepository unitsRepository))
            {
                Debug.LogError($"{name} <color=grey>({nameof(LifeBar)})</color>: Units Repository not found. Deactivating");
                gameObject.SetActive(false);
                return;
            }
            unitsRepository.OnShipSpawned -= HandleShipSpawned;
            unitsRepository.OnShipSpawned += HandleShipSpawned;
        }

        private void HandleLifeChanged(int before, int after)
        {
            if (_health is null)
                return;
            float healthLerp = (float)after / _health.MaxHP;
            LMotion.Create(graphic.fillAmount, healthLerp, barAnimationDuration)
                   .WithEase(Ease.InOutQuad)
                   .BindToFillAmount(graphic);
            graphic.CrossFadeColor(Color.Lerp(zeroHealthColor, fullHealthColor, healthLerp), 0.1f, false, false);
        }

        private void OnDestroy()
        {
            if (_health is not null)
            {
                _health.OnDamage -= HandleLifeChanged;
                _health.OnHeal -= HandleLifeChanged;
            }
        }
    }
}
