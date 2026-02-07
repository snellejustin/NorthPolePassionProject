using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; 
using Meta.XR.MRUtilityKit;

public class RuntimeNavmeshBuilder : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;

    void Start()
    {
        
        if (navMeshSurface == null)
        {
            navMeshSurface = GetComponent<NavMeshSurface>();
        }

        if (MRUK.Instance != null)
        {
            MRUK.Instance.RegisterSceneLoadedCallback(buildNavMesh);
        }
    }

    public void buildNavMesh()
    {
        StartCoroutine(buildNavMeshRoutine());
    }
    
    public IEnumerator buildNavMeshRoutine()
    {
        if (navMeshSurface != null)
        {
            yield return new WaitForEndOfFrame();
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh Built Successfully!");
        }
    }
}