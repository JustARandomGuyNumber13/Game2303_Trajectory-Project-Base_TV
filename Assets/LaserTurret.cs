using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;

    List<Vector3> laserPoints = new List<Vector3>();


    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();

        laserPoints.Clear();
        laserPoints.Add(barrelEnd.position);

        if(Physics.Raycast(barrelEnd.position, barrelEnd.forward, out RaycastHit hit, 1000.0f, targetLayer))
        {
            laserPoints.Add(hit.point);

            // TODO: Implement laser rebounce direction here, then add to laserPoints list 
            Vector3 directionToTarget = (hit.point - barrelEnd.position).normalized;
            Vector3 reflectionNormal = hit.normal;  // Perpendicular line from the hit point surface
            float dotProduct = Vector3.Dot(reflectionNormal, directionToTarget);    // Not understand what it is for, but I'm following the formula
            Vector3 reflectionDirection = directionToTarget - 2 * dotProduct * reflectionNormal;
            Vector3 reflectionPoint = reflectionDirection * 1000f + hit.point;

            //Debug.DrawRay(barrelEnd.position, directionToTarget, Color.red);    // Shoot line
            //Debug.DrawRay(hit.point, reflectionDirection, Color.yellow);            // Reflection line
            //Debug.DrawRay(hit.point, reflectionPoint - hit.point, Color.black);    // Double check reflection line for laserPoint

            laserPoints.Add(reflectionPoint);
            // End TODO task
        }

        line.positionCount = laserPoints.Count;
        for(int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, laserPoints[i]);
        }
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer ))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, directionToTarget.y, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }
}
