using UnityEditor;
using UnityEngine;

public static class ARCardVisualTestEditor
{
    [MenuItem("Window/AR Visual/Test Create Card")]
    public static void CreateTestCard()
    {
        // 1. Create root object
        GameObject cardObj = new GameObject("Test_ARCard");
        
        // 2. Create front face (pointing up +Y)
        GameObject frontObj = new GameObject("Front");
        frontObj.transform.SetParent(cardObj.transform);
        frontObj.transform.localPosition = new Vector3(0f, 0.0005f, 0f);
        frontObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        SpriteRenderer frontRenderer = frontObj.AddComponent<SpriteRenderer>();

        // 3. Create back face (pointing down -Y)
        GameObject backObj = new GameObject("Back");
        backObj.transform.SetParent(cardObj.transform);
        backObj.transform.localPosition = new Vector3(0f, -0.0005f, 0f);
        backObj.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);
        SpriteRenderer backRenderer = backObj.AddComponent<SpriteRenderer>();

        // 4. Attach ARCardVisual component
        ARCardVisual visual = cardObj.AddComponent<ARCardVisual>();
        visual.frontRenderer = frontRenderer;
        visual.backRenderer = backRenderer;

        // 5. Initialize with a mock card (Red 7)
        CardData mockCard = new CardData(100, CardColor.Red, CardType.Number, 7);
        visual.Initialize(mockCard);

        // 6. Highlight and select the newly created object in the hierarchy
        Selection.activeGameObject = cardObj;
        
        // Match camera viewport focus if possible
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }

        Debug.Log("[ARCardVisualTestEditor] Created mock Card 'Red 7' with dimensions 0.06m x 0.09m.");
    }
}
