using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation; 
using Meta.XR.MRUtilityKit;

public class RuntimeNavmeshBuilder : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
    
    // Update is called once per frame
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