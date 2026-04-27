using System.Threading;
using UnityEngine;
using VarelaAloisio.Core.Utils;

namespace Units
{
    public class MonoBehaviourAsync : MonoBehaviour
    {
        private CancellationTokenSource _disableCancellationTokenSource;
        public CancellationToken disableCancellationToken
            => (_disableCancellationTokenSource ??= new CancellationTokenSource()).Token;

        protected virtual void OnEnable()
            => _disableCancellationTokenSource ??= new CancellationTokenSource();

        protected virtual void OnDisable()
            => TokenUtils.CancelAndDispose(ref _disableCancellationTokenSource);
    }
}