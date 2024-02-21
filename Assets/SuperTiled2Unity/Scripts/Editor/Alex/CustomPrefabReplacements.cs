using UnityEngine;

namespace SuperTiled2Unity.Editor.Alex
{
    public static class CustomPrefabReplacements
    {
        public const string PREFIX = "C_";

        public static void Replace(GameObject newPrefab, SuperObject superObj)
        {
            Vector2 size = new Vector2(superObj.m_Width, superObj.m_Height);
            newPrefab.GetComponentInChildren<BoxCollider2D>().size = size;
            newPrefab.GetComponentInChildren<SpriteRenderer>().size = size;
            newPrefab.transform.position += (Vector3)size/2;
        }
    }
}