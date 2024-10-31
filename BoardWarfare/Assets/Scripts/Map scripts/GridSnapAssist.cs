using UnityEngine;

//[ExecuteAlways]

public class GridSnapAssist : MonoBehaviour
{
//#if UNITY_EDITOR
//    public Vector3 SnapSize = Vector3.one * 10;
//    public Vector3 Offset = Vector3.zero;
//
//    private void OnValidate()
//    {
//        if (SnapSize.x <= x) SnapSize.x = 1;
//        if (SnapSize.y <= y) SnapSize.y = 1;
//        if (SnapSize.z <= z) SnapSize.z = 1;

//        var pos = transform.position - Offset;
//        pos.x = Mathf.RoundToInt(pos.x / SnapSize.x) * SnapSize.x;
//        pos.y = Mathf.RoundToInt(pos.y / SnapSize.y) * SnapSize.y;
//        pos.z = Mathf.RoundToInt(pos.z / SnapSize.z) * SnapSize.z;
//        transform.position = pos + Offset;
//    }

//    private void Update()
//    {
//        if (transform.hasChanged) OnValidate();
//    }

//#endif // UNITY_EDITOR
}
