using UnityEngine;
public class DestroyAfterTime : MonoBehaviour {
    void Start() { Destroy(gameObject, 5f); } // Zniszcz po 5 sekundach
}