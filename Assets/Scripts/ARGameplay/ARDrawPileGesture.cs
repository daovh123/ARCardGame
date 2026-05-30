using UnityEngine;

public class ARDrawPileGesture : MonoBehaviour
{
    public ARHandController Controller { get; private set; }

    public void Initialize(ARHandController controller)
    {
        Controller = controller;
        EnsureCollider();
    }

    private void EnsureCollider()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            box = gameObject.AddComponent<BoxCollider>();
        }

        box.isTrigger = true;
        box.center = new Vector3(0f, 0.02f, 0f);
        box.size = new Vector3(0.18f, 0.06f, 0.22f);
    }
}
