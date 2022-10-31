using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Darkrell.Pro.Blazor.Extensions;

public abstract class OnRenderReference<T>
{
    private T _ref;
    private bool _isInitialized;
    private ConcurrentQueue<Func<T, Task>> _delayedAsyncActions = new();
    private TaskCompletionSource _pipeTask = new TaskCompletionSource();

    public T Ref { get => _ref; set => Set(value); }
    private void Set(T value)
    {
        _ref = value;
        _isInitialized = true;
        var taskQueue = Task.CompletedTask;
        while (_delayedAsyncActions.TryDequeue(out var action))
            taskQueue = taskQueue.ContinueWith(_ => action(_ref)).Unwrap();
        Task.Run(() =>
        {
            Task.WaitAny(taskQueue);
            if (taskQueue.IsCompletedSuccessfully)
            {
                _pipeTask.SetResult();
                return;
            }
            _pipeTask.SetException(taskQueue.Exception);
        });
    }
    public OnRenderReference<T> OnAvailable(Action<T> action)
    {
        if (_isInitialized)
            action(_ref);
        else
            _delayedAsyncActions.Enqueue(e => { action(e); return Task.CompletedTask; });
        return this;
    }
    public OnRenderReference<T> OnAvailable(Func<T, Task> action)
    {
        if (_isInitialized)
            action(_ref);
        else
            _delayedAsyncActions.Enqueue(action);
        return this;
    }
    public OnRenderReference<T> OnAvailable(Func<T, ValueTask> action)
    {
        if (_isInitialized)
            action(_ref);
        else
            _delayedAsyncActions.Enqueue(e => action(e).AsTask());
        return this;
    }
    public Task<T2> OnAvailablePipe<T2>(Func<T, Task<T2>> func)
    {
        if (!_isInitialized)
        {
            var tcs = new TaskCompletionSource<T2>();
            _delayedAsyncActions.Enqueue(async e => tcs.SetResult(await func(e)));
            return tcs.Task;
        }
        return func(_ref);
    }
    public System.Runtime.CompilerServices.TaskAwaiter GetAwaiter()
        => _pipeTask.Task.GetAwaiter();
}
public class OnStockedRef : OnRenderReference<ElementReference>
{ }
