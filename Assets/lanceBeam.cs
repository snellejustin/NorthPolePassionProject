using UnityEngine;

public class lanceBeam : MonoBehaviour
{
    public OVRInput.RawButton shootingButton;
    public LineRenderer linePrefab;
    public Transform shootingPoint;
    public float maxLineDistance = 5;
    public float lineShowTimer = 0.3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(shootingButton))
        {
            ShootHeatBeam();
        }
    }

    public void ShootHeatBeam()
    {
        LineRenderer line = Instantiate(linePrefab);
        line.positionCount = 2;
        line.SetPosition(0, shootingPoint.position);
        
        Vector3 endPoint = shootingPoint.position + shootingPoint.forward * maxLineDistance;
        
        line.SetPosition(1, endPoint);

        Destroy(line.gameObject, lineShowTimer);
    }
}
