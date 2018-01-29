using UnityEngine;

public class CameraControl : MonoBehaviour
{
    //DampTime is approximate time it takes for camera to move to the position set. Instead of instantly, the movement is dampened.
    public float m_DampTime = 0.2f; 
    //Add number to the siudes to make sure the camera isn't at the edge of the screen and tnaks are in screen        
    public float m_ScreenEdgeBuffer = 4f;
    //Keeps it from getting craxy zoomed in and looking silly           
    public float m_MinSize = 6.5f;   
    //This is an array of transforms to include all of the transforms of the tanks, which are targets.               
    /*[HideInInspector]*/ public Transform[] m_Targets; 


    private Camera m_Camera;                        
    private float m_ZoomSpeed;                      
    private Vector3 m_MoveVelocity;                 
    private Vector3 m_DesiredPosition;              


    private void Awake()
    {
        //FInd the actual camera since it's a child of CameraRig
        m_Camera = GetComponentInChildren<Camera>();
    }

    //We want to Zoom and Move with the tanks, and sicne we call Move() in FixedUpdate in the Tank script, we do that here too.
    private void FixedUpdate()
    {
        Move();
        Zoom();
    }


    private void Move()
    {
        FindAveragePosition();
        //Once found average position it sets desired position to that
        transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
    }


    private void FindAveragePosition()
    {
        Vector3 averagePos = new Vector3();
        int numTargets = 0;
        //for loop creater an iterator i and it will add 1 to i for each loop it does (target is the list of tanks)
        for (int i = 0; i < m_Targets.Length; i++)
        {
            //if tank is not active, then continue on to the next loop iteration and look for next iteration in list unless it is right so then it will continue.
            if (!m_Targets[i].gameObject.activeSelf)
                continue;
            //if we are on active tank, take that positon and add the new position to the average position with the number of tanks there are
            averagePos += m_Targets[i].position;
            numTargets++;
        }
        //if there are some active targets, then divide that position by the number of targets there are
        if (numTargets > 0)
            averagePos /= numTargets;
        //here we just say the y component of the average is the y position of the rig to keep it where the rig moves (this is a safety check, we already have restraints on the rigidbody)
        averagePos.y = transform.position.y;

        m_DesiredPosition = averagePos;
    }

    //
    private void Zoom()
    {
        float requiredSize = FindRequiredSize();
        //smoothly damping towards the requiredSize using the orthographics size - smothdamp moves between orthographic size and required size and move/zoom over the same period of time
        m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
    }


    private float FindRequiredSize()
    {
        //find desired position in the cmaera rig
        Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

        float size = 0f;

        for (int i = 0; i < m_Targets.Length; i++)
        {
            //when we enter the loop we keep looking for a target that is active
            if (!m_Targets[i].gameObject.activeSelf)
                continue;
            //finding target in local position of the camera rig 
            Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);
            //we found desired position and tank in camera rig local space
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;
            //size is the max of its current value or the y axis of that vector
            size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.y));
            //choose whether the size is the largest or the x factor of that vector divided by the aspect
            size = Mathf.Max (size, Mathf.Abs (desiredPosToTarget.x) / m_Camera.aspect);
        }
        
        size += m_ScreenEdgeBuffer;

        size = Mathf.Max(size, m_MinSize);

        return size;
    }


    public void SetStartPositionAndSize()
    {
        //once we have a gamemanager we want to reset
        FindAveragePosition();

        transform.position = m_DesiredPosition;
        //setting it to no smooth damp
        m_Camera.orthographicSize = FindRequiredSize();
    }
}