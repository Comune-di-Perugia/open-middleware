using System;
using System.Collections.Generic;
using System.Reflection;
using FIWARE.Orion.Client.Models;

namespace Wisetown.ContextBroker.Worker;

public static class Extensions
{
  public static ContextElement ToContextElement<T>(this T item, Func<T, string> gettype, Func<T, string> getid)
  {
    var attributes = new List<ContextAttribute>();

    foreach (PropertyInfo p in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty))
    {
      attributes.Add(new ContextAttribute()
      {
        Name = p.Name,
        Type = p.PropertyType.Name,
        Value = p.GetValue(item)
      });
    }

    return new ContextElement()
    {
      Id = getid(item),
      Type = gettype(item),
      Attributes = attributes
    };
  }
}