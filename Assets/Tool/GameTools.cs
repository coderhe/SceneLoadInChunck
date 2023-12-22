using UnityEngine;
using System.Collections.Generic;

public class GameTools
{
    public static T[] GetComponentsInChildren<T>(GameObject parent, bool includeInactive = false, bool includeParent = true) where T : Component
    {
        if (parent == null)
            return null;

        T[] result = null;
        T[] componets = parent.GetComponentsInChildren<T>(includeInactive);
        if (includeParent)
        {
            result = componets;
        }
        else
        {
            if (componets != null)
            {
                if (componets.Length == 1 && componets[0].gameObject != parent)
                {
                    result = componets;
                }
                else if (componets.Length > 1)
                {
                    List<T> compList = new List<T>();
                    for (int i = 0; i < componets.Length; i++)
                    {
                        if (componets[i].gameObject == parent)
                            continue;
                        
                        compList.Add(componets[i]);
                    }

                    result = compList.ToArray();
                }
            }
        }

        return result;
    }
}