using UnityEngine;

public class SceneLikeCamera : MonoBehaviour
{
    [Header("Focus Object")]
    [SerializeField, Tooltip("Double click to focus on objects with colliders?")]
    private bool doubleClickFocus = false;
    [SerializeField] private float focusLimit = 100f;
    [SerializeField] private float minFocusDistance = 5.0f;
    [SerializeField, Tooltip("Time in which a double click will be registered, reduce to require faster double clicks")]
    private float doubleClickTime = .25f;
    private float cooldown = 0;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = .5f;
    [SerializeField] private float rotationSpeed = 4.0f;
    [SerializeField] private float zoomSpeed = 5.0f;    

    [Header("Move Keys")]
    [SerializeField] private KeyCode forwardKey = KeyCode.W;
    [SerializeField] private KeyCode backKey = KeyCode.S;
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;

    [SerializeField] private KeyCode anchoredMoveKey = KeyCode.Mouse2;

    [SerializeField] private KeyCode anchoredRotateKey = KeyCode.Mouse1;

    void Update()
    {
        if (!doubleClickFocus)
            return;

        //Double click for focus 
        if (cooldown > 0 && Input.GetKeyDown(KeyCode.Mouse0))
            FocusObject();
        if (Input.GetKeyDown(KeyCode.Mouse0))
            cooldown = doubleClickTime;

        cooldown -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(forwardKey))
            move += Vector3.forward * moveSpeed;
        if (Input.GetKey(backKey))
            move += Vector3.back * moveSpeed;
        if (Input.GetKey(leftKey))
            move += Vector3.left * moveSpeed;
        if (Input.GetKey(rightKey))
            move += Vector3.right * moveSpeed;

        float mouseMoveY = Input.GetAxis("Mouse Y");
        float mouseMoveX = Input.GetAxis("Mouse X");

        //Move the camera when anchored
        if (Input.GetKey(anchoredMoveKey))
        {
            move += Vector3.up * -mouseMoveY * moveSpeed;
            move += Vector3.right * -mouseMoveX * moveSpeed;
        }

        //Rotate the camera when anchored
        if (Input.GetKey(anchoredRotateKey))
        {
            transform.RotateAround(transform.position, transform.right, -mouseMoveY * rotationSpeed);
            transform.RotateAround(transform.position, Vector3.up, mouseMoveX * rotationSpeed);
        }

        transform.Translate(move);
    }

    private void LateUpdate()
    {
        //Scroll to zoom
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(Vector3.forward * mouseScroll * zoomSpeed);
    }

    private void FocusObject()
    {
        //If we double-clicked an object in the scene, go to its position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, focusLimit))
        {
            GameObject target = hit.collider.gameObject;
            Vector3 targetPos = target.transform.position;
            Vector3 targetSize = hit.collider.bounds.size;

            transform.position = targetPos + GetOffset(targetPos, targetSize);

            transform.LookAt(target.transform);
        }
    }

    private Vector3 GetOffset(Vector3 targetPos, Vector3 targetSize)
    {
        Vector3 dirToTarget = targetPos - transform.position;

        float focusDistance = Mathf.Max(targetSize.x, targetSize.z);
        focusDistance = Mathf.Clamp(focusDistance, minFocusDistance, focusDistance);

        return -dirToTarget.normalized * focusDistance;
    }
}
