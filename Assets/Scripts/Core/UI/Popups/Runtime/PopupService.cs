using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.UI.Popups.Contracts;
using Core.UI.Popups.Runtime.Handlers;
using Core.UI.Popups.Runtime.Handlers.Core;
using Cysharp.Threading.Tasks;

namespace Core.UI.Popups.Runtime
{
    public sealed class PopupService : IPopupService, IDisposable
    {
        private readonly IReadOnlyList<IPopupHandler> _handlers;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private bool _isDisposed;

        public PopupService(IEnumerable<IPopupHandler> handlers)
        {
            _handlers = handlers?.ToArray()
                        ?? throw new ArgumentNullException(nameof(handlers));
        }

        public async UniTask<TResult> ShowAsync<TResult>(
            IPopupRequest<TResult> request,
            CancellationToken token = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PopupService));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            await _semaphore.WaitAsync(token);

            try
            {
                IPopupHandler handler = FindHandler(request.GetType());

                object result = await handler.HandleAsync(request, token);

                return result is TResult typedResult
                    ? typedResult
                    : default;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private IPopupHandler FindHandler(Type requestType)
        {
            IPopupHandler handler = _handlers.FirstOrDefault(x => x.RequestType == requestType);

            if (handler != null)
                return handler;

            handler = _handlers.FirstOrDefault(x => x.RequestType.IsAssignableFrom(requestType));

            if (handler != null)
                return handler;

            throw new InvalidOperationException(
                $"No popup handler registered for request type '{requestType.Name}'.");
        }

        public void Dispose()
        {
            _isDisposed = true;
            _semaphore.Dispose();
        }
    }
}