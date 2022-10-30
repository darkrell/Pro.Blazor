using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Darkrell.Pro.Blazor.Extensions;

public static class BindExtentions
{
    public static BindWrapper<TProperty> Trace<TSource, TProperty>(this TSource source, Expression<Func<TSource, TProperty>> propertyLambda, Action<TProperty> OnChanged)
    {
        Type type = typeof(TSource);

        MemberExpression member = propertyLambda.Body as MemberExpression;
        if (member == null)
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a method, not a property.",
                propertyLambda.ToString()));

        PropertyInfo propInfo = member.Member as PropertyInfo;
        if (propInfo == null)
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a field, not a property.",
                propertyLambda.ToString()));

        if (type != propInfo.ReflectedType &&
            !type.IsSubclassOf(propInfo.ReflectedType))
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a property that is not from type {1}.",
                propertyLambda.ToString(),
                type));
        return new BindWrapper<TProperty>(() => (TProperty)propInfo.GetValue(source), v => { propInfo.SetValue(source, v); OnChanged(v); });
    }
}