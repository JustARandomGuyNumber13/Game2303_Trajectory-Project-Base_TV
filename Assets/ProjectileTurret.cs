using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ProjectileTurret : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 1;
    [SerializeField] Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] bool useLowAngle;

    List<Vector3> points = new List<Vector3>();

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();
        RotateGun();

        if (Input.GetButtonDown("Fire1"))
            Fire();
    }

    private void SimulateTrajectory()   // Revised version: 13
    {
        points.Clear();
        float timeStep = 0.01f;
        float lineLength = 1f;  // Measurement unit is second in time _TV
        Vector3 shootDir = barrelEnd.forward;
        Vector3 velocity = shootDir * projectileSpeed;
        Vector3 currentPosition = barrelEnd.position;       // Should be constant as simulating by time _TV (Main issue)
        Vector3 nextPointDirection, nextPosition;

        for (float time = 0; time < lineLength; time += timeStep)
        {
            nextPointDirection = velocity * time + (-gravity * Mathf.Pow(time, 2)) / 2; // Flip Vector3 gravity to match reality, only adjust Y-Axis _TV
            nextPosition = currentPosition + nextPointDirection;
            points.Add(nextPosition);
        }

        line.positionCount = points.Count;     // Load points into line for visual _TV
        for (int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, points[i]);
        }
    }


    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, gun.transform.rotation);
        projectile.GetComponent<Rigidbody>().velocity = projectileSpeed * barrelEnd.transform.forward;
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void RotateGun()
    {
        float? angle = CalculateTrajectory(crosshair.transform.position, useLowAngle);
        SimulateTrajectory();

        if (angle != null)
        {
            gun.transform.localEulerAngles = new Vector3(360f - (float)angle, 0, 0);
        }
    }

    float? CalculateTrajectory(Vector3 target, bool useLow)
    {
        Vector3 targetDir = target - barrelEnd.position;
        
        float y = targetDir.y;
        targetDir.y = 0;

        float x = targetDir.magnitude;

        float v = projectileSpeed;
        float v2 = Mathf.Pow(v, 2);
        float v4 = Mathf.Pow(v, 4);
        float g = gravity.y;
        float x2 = Mathf.Pow(x, 2);

        float underRoot = v4 - g * ((g * x2) + (2 * y * v2));

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = v2 + root;
            float lowAngle = v2 - root;

            if (useLow)
                return (Mathf.Atan2(lowAngle, g * x) * Mathf.Rad2Deg);
            else
                return (Mathf.Atan2(highAngle, g * x) * Mathf.Rad2Deg);
        }
        else
            return null;
    }
}
