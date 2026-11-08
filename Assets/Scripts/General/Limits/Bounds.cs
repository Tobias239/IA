using UnityEngine;

public class Bounds : MonoBehaviour
{
    public static Bounds Instance {get; private set;}

    [SerializeField] private float height = 50f;
    [SerializeField] private float width = 50f;
    [SerializeField] private float fixedY = 1f;
    [SerializeField] private bool drawGizmos;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Vector3 OutOfBounds(Vector3 position)
    {
        Vector3 newPosition = position;
        if(position.x > width / 2) newPosition.x = -width / 2;
        if(position.x < -width / 2) newPosition.x = width / 2;
        if(position.z > height / 2) newPosition.z = -height / 2;
        if(position.z < -height / 2) newPosition.z = height / 2;

        newPosition.y = fixedY;
        return newPosition;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, 0, height));
    }
}