using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class ObjectSocket : MonoBehaviour {

    private BoxCollider boxToFit;

    public bool AutoRescaleToFitTheBox = true;

    void Awake()
    {
        boxToFit = GetComponent<BoxCollider>();
    }

    public void PutIn(GameObject objectToPresent)
    {
        objectToPresent.transform.SetParent(this.transform);
        objectToPresent.transform.localPosition = Vector3.zero;

        if (AutoRescaleToFitTheBox) { 

            var meshRenderers = objectToPresent.GetComponentsInChildren<MeshRenderer>();

            var objectsOriginalBounds = new Bounds();

            foreach (var meshRenderer in meshRenderers)
            {
                objectsOriginalBounds.Encapsulate(meshRenderer.bounds);
            }

            float x_fac = boxToFit.bounds.size.x / objectsOriginalBounds.size.x ;
            float y_fac = boxToFit.bounds.size.y / objectsOriginalBounds.size.y ;
            float z_fac = boxToFit.bounds.size.z / objectsOriginalBounds.size.z ;

            objectToPresent.transform.localScale = new Vector3(1-x_fac, 1-y_fac, 1-z_fac);
        }
    }
}
