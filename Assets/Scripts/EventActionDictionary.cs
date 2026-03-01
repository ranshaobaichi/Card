using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventActionDictionary<TKey> : IEnumerable<KeyValuePair<TKey, Action>> where TKey : struct, Enum {
  private sealed class TickBucket {
    public event Action OnTick;

    public void Add(Action action) {
      OnTick += action;
    }

    public void Remove(Action action) {
      OnTick -= action;
    }

    public void Invoke() {
      OnTick?.Invoke();
    }

    public bool IsEmpty() {
      return OnTick == null;
    }

    public Action GetAction() {
      return OnTick;
    }

    public void SetAction(Action action) {
      OnTick = action;
    }
  }

  private readonly Dictionary<TKey, TickBucket> _ticks = new Dictionary<TKey, TickBucket>();

  public Action this[TKey key] {
    get {
      return _ticks.TryGetValue(key, out var bucket) ? bucket.GetAction() : null;
    }
    set {
      if (_ticks.TryGetValue(key, out var bucket)) {
        bucket.SetAction(value);
        return;
      }

      if (value == null) {
        Debug.LogError("[TickGroup] You should not set a null action to a key. " +
                       "Use Clear or ClearAll instead.");
        return;
      }

      bucket = new TickBucket();
      bucket.SetAction(value);
      _ticks[key] = bucket;
    }
  }

  public void Add(TKey key, Action action) {
    if (action == null) {
      return;
    }

    if (!_ticks.TryGetValue(key, out var bucket)) {
      bucket = new TickBucket();
      _ticks[key] = bucket;
    }

    bucket.Add(action);
  }

  public void Remove(TKey key, Action action) {
    if (action == null) {
      return;
    }

    if (_ticks.TryGetValue(key, out var bucket)) {
      bucket.Remove(action);
    }
  }

  public void Invoke(TKey key) {
    if (_ticks.TryGetValue(key, out var bucket)) {
      bucket.Invoke();
    }
  }

  public bool ContainsKey(TKey key) {
    return _ticks.ContainsKey(key);
  }

  public bool HasHandlers(TKey key) {
    return _ticks.TryGetValue(key, out var bucket) && !bucket.IsEmpty();
  }

  public void ClearHandlers(TKey key) {
    if (_ticks.TryGetValue(key, out var bucket)) {
      bucket.SetAction(null);
    }
  }

  public void ClearAllHandlers() {
    foreach (var bucket in _ticks.Values) {
      bucket.SetAction(null);
    }
  }

  public void Clear(TKey key) {
    _ticks.Remove(key);
  }

  public void ClearAll() {
    _ticks.Clear();
  }

  public IEnumerator<KeyValuePair<TKey, Action>> GetEnumerator() {
    foreach (var pair in _ticks) {
      yield return new KeyValuePair<TKey, Action>(pair.Key, pair.Value.GetAction());
    }
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }
}
