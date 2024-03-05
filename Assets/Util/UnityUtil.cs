using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable] public class BoolEvent : UnityEvent<bool> { }

public static class UnityUtil {
    public static bool TryGetComponentInChildren<T>(this Component o, out T component, bool includeInactive = false) where T : Component {
        return TryGetComponentInChildren(o.gameObject, out component, includeInactive);
    }
    public static bool TryGetComponentInChildren<T>(this GameObject o, out T component, bool includeInactive = false) where T : Component {
        component = o.GetComponentInChildren<T>(includeInactive);
        return component != null;
    }
    public static bool TryGetComponentInParent<T>(this Component o, out T component) where T : Component {
        component = o.GetComponentInParent<T>();
        return component != null;
    }
    public static bool TryGetComponentInParent<T>(this GameObject o, out T component) where T : Component {
        component = o.GetComponentInParent<T>();
        return component != null;
    }

    public static bool TryFindObjectOfType<T>(out T obj, bool includeDisabled = false) where T : UnityEngine.Object {
        UnityEngine.Object[] array = includeDisabled ? Resources.FindObjectsOfTypeAll(typeof(T)) : UnityEngine.Object.FindObjectsOfType(typeof(T));
        if (array.Length != 0) {
            if (includeDisabled && typeof(T).IsSubclassOf(typeof(Component))) {
                for (int i = 0; i < array.Length; i++) {
                    if (((Component)array[i]).gameObject.scene.name != null) { obj = (T)array[i]; return true; }
                }
            }

            obj = (T)array[0];
            return true;
        }

        obj = null;
        return false;
    }

    public static void DestroyChildren(this Transform parent) {
        while (parent.childCount > 0) UnityEngine.Object.DestroyImmediate(parent.GetChild(0).gameObject);
    }

    public static void Validate<T>(this GameObject gameObject, string name, ref T variable, bool includeDisabled = false) where T : UnityEngine.Object {
        if (string.IsNullOrEmpty(gameObject.scene.name)) return;// Don't run for prefabs

        if (variable == null) {
            if (TryFindObjectOfType(out variable, includeDisabled)) Debug.LogWarning($"Auto found {name}", gameObject);
            else Debug.LogError($"No {name} found!", gameObject);
        }
    }

    public static void ValidateListeners(this UnityEventBase unityEvent, GameObject o = null) {
        if (unityEvent != null) {
            int c = unityEvent.GetPersistentEventCount();
            for (int i = 0; i < c; i++) {
                if (unityEvent.GetPersistentTarget(i) == null || string.IsNullOrEmpty(unityEvent.GetPersistentMethodName(i)))
                    Debug.LogError("Invalid Event Listener!", o);
            }
        }
    }
    public static void ValidateListeners(GameObject o = null, params UnityEventBase[] unityEvents) {
        foreach (var ev in unityEvents)
            ev.ValidateListeners(o);
    }

    public static bool TryAndSetActive(this GameObject o, bool active) {
        if (o.activeSelf != active) {
            o.SetActive(active);
            return true;
        }
        else return false;
    }

    public static AnimationCurve LinearCurve(params Keyframe[] keys) {
        for (int i = 0; i < keys.Length; i++) {
            keys[i].inWeight = 0f;
            keys[i].outWeight = 0f;
            keys[i].weightedMode = WeightedMode.Both;
        }

        return new AnimationCurve(keys);
    }

    /*public static void AddOnChanged(this InputAction action, Action<InputAction.CallbackContext> callback) {
        action.performed += callback;
        action.canceled += callback;
    }
    public static void RemoveOnChanged(this InputAction action, Action<InputAction.CallbackContext> callback) {
        action.performed -= callback;
        action.canceled -= callback;
    }*/
}