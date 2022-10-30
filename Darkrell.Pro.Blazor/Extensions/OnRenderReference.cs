using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Darkrell.Pro.Blazor.Extensions;

public abstract class OnRenderReference<T>
{
    private T _ref;
    private bool isInitialized;
    private ConcurrentQueue<Func<T, Task>> delayedAsyncActions = new();

    public T Ref { get => _ref; set => Set(value); }
    private void Set(T value)
    {
        _ref = value;
        isInitialized = true;
        var taskQueue = Task.CompletedTask;
        while (delayedAsyncActions.TryDequeue(out var action))
            taskQueue = taskQueue.ContinueWith(_ => action(_ref)).Unwrap();
    }
    public OnRenderReference<T> OnAvailable(Action<T> action)
    {
        if (isInitialized)
            action(_ref);
        else
            delayedAsyncActions.Enqueue(e => { action(e); return Task.CompletedTask; });
        return this;
    }
    public OnRenderReference<T> OnAvailable(Func<T, Task> action)
    {
        if (isInitialized)
            action(_ref);
        else
            delayedAsyncActions.Enqueue(action);
        return this;
    }
    public OnRenderReference<T> OnAvailable(Func<T, ValueTask> action)
    {
        if (isInitialized)
            action(_ref);
        else
            delayedAsyncActions.Enqueue(e => action(e).AsTask());
        return this;
    }
    public Task<T2> OnAvailablePipe<T2>(Func<T, Task<T2>> func)
    {
        if (!isInitialized)
        {
            var tcs = new TaskCompletionSource<T2>();
            delayedAsyncActions.Enqueue(async e => tcs.SetResult(await func(e)));
            return tcs.Task;
        }
        return func(_ref);
    }
}
public class OnStockedRef : OnRenderReference<ElementReference>
{ }
