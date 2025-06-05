using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T inst;
    public static T ins;
    //{
    //    get
    //    {
    //        if (inst == null)
    //        {
    //            inst = GameObject.FindObjectOfType<T>();

    //            if (inst == null)
    //            {
    //                inst = new GameObject().AddComponent<T>();
    //            }
    //        }

    //        return inst;
    //    }
    //}

    public bool isDontDestroy;

    public virtual void Awake()
    {
        if (ins == null)
        {
            ins = this as T;
            if (isDontDestroy)
            {
                DontDestroyOnLoad(this);
            }
        }
        else
        {
            Destroy(gameObject);
        }

    }



    public static bool Exists()
    {
        return (ins != null);
    }
}