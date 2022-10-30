using System;

namespace Darkrell.Pro.Blazor.Extensions;

public class BindWrapper<T>
{

    public BindWrapper(Func<T> getter, Action<T> setter)
    {
        _getter = getter;
        _setter = setter;
    }

    private Func<T> _getter;
    private Action<T> _setter;
    public T V { get => _getter(); set => _setter(value); }
}