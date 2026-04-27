using System;

namespace Core.Utils
{
    public readonly struct Factory<T>
    {
        private readonly Func<T> _spawn;
        private readonly Action<T> _destroy;

        public Factory(Func<T> spawn,
                       Action<T> destroy)
        {
            _spawn = spawn;
            _destroy = destroy;
        }
        public T Get() => _spawn();
        public void Destroy() => _destroy(_spawn());
    }
}