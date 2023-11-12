using UnityEngine;
using System;
using System.Collections.Generic;

public class Reactive<T>
{
  private T val;
  private Dictionary<Func<T, bool>, List<Action<T, T>>> predicates = new Dictionary<Func<T, bool>, List<Action<T, T>>>();

  public Reactive(T _val)
  {
    val = _val;
  }

  public void Set(T to)
  {
    T prevVal = val;
    val = to;

    if (prevVal != null && prevVal.Equals(val))
    {
      return;
    }

    foreach (var pair in predicates)
    {
      Func<T, bool> predicate = pair.Key;
      List<Action<T, T>> cbs = pair.Value;

      if (!predicate(val))
      {
        continue;
      }

      cbs.ForEach(cb => cb(prevVal, val));
    }
  }

  public void When(Func<T, bool> predicate, Action<T, T> cb)
  {
    AddPredicate(predicate, cb);
  }

  public void OnChange(Action<T, T> cb)
  {
    Func<T, bool> predicate = val => true;
    AddPredicate(predicate, cb);
  }

  public T Get()
  {
    return val;
  }

  private void AddPredicate(Func<T, bool> predicate, Action<T, T> cb)
  {
    if (predicates.ContainsKey(predicate))
    {
      predicates[predicate].Add(cb);
    }
    else
    {
      predicates[predicate] = new List<Action<T, T>>() { cb };
    }
  }
}