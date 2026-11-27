using UnityEngine;

public class Marker3D : MonoBehaviour
{
    public Color gizmoColor = Color.cyan;
    public float gizmoSize = 0.5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 0.25f);
        Gizmos.DrawLine(transform.position - transform.right * gizmoSize, transform.position + transform.right * gizmoSize);
        Gizmos.DrawLine(transform.position - transform.up * gizmoSize, transform.position + transform.up * gizmoSize);
        Gizmos.DrawLine(transform.position - transform.forward * gizmoSize, transform.position + transform.forward * gizmoSize);
    }
}