using System.Threading;
using UnityEngine;
using VarelaAloisio.Core.Utils;

namespace Core
{
    /// <summary>
    /// Simple intermediary class to add a disable cancellation token to MonoBehaviours. 
    /// Author: Juan Pablo Varela Aloisio
    /// email: juampyvarela@gmail.com
    /// </summary>
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