using System;
using System.Collections.Generic;
using UnityEngine;
public class App : MonoBehaviour, IDisposable
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Main()
    {
        var go = new GameObject(nameof(App));
        _instance = go.AddComponent<App>();
        DontDestroyOnLoad(go);
        var services = Resources.LoadAll<GameObject>("Services");
        foreach (var service in services)
        {
            try
            {
                var instance = Instantiate(service, go.transform);
                instance.name = service.name;
            }catch(Exception e)
            {
                Debug.LogError("Failed to load service: " + service.name);
                Debug.LogException(e);
            }
        }
    }

    public static bool TryGet<T>(out T service)
    {
        if (_instance._services.TryGetValue(typeof(T), out var serviceObj))
        {
            service = (T)serviceObj;
            return true;
        }
        service = default;
        return false;
    }
    public static T Get<T>()
    {
        if(_instance._services.TryGetValue(typeof(T), out var service))
            return (T)service;
        return _instance.gameObject.GetComponentInChildren<T>();
    }
    
    public static void Add<T>() => _instance._services.Add(typeof(T), Activator.CreateInstance<T>());
    public static void Add<T>(T service) => _instance._services.Add(typeof(T), service);

    readonly Dictionary<Type, object> _services = new();
    static App _instance;
    public void Dispose()
    {
        if (!_instance) return;
        if(_services == null) return;
        foreach (var service in _services.Values)
        {
            if (service is IDisposable disposable)
                disposable.Dispose();
        }
        _services.Clear();
        _instance = null;
    }
    
    void OnDestroy() => Dispose();
}
