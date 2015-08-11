using UnityEngine;
using System;
using System.Collections;

public class Category : MonoBehaviour
{
    public GameObject GetObjectByID(string name)
    {
        int childCount = transform.childCount;

        if (childCount == 0)
            throw new MissingComponentException("Categorie has no Objects");

        for (int i = 0; i < childCount; i++)
        {
            var childTransform = transform.GetChild(i);

            if (childTransform.name.Equals(name))
                return childTransform.gameObject;
        }

        throw new ArgumentException(string.Format("Object \"{0}\" not found!", name));
    }
}
