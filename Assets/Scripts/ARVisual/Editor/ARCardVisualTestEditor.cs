using UnityEditor;
using UnityEngine;

public static class ARCardVisualTestEditor
{
    [MenuItem("Window/AR Visual/Test Create Card")]
    public static void CreateTestCard()
    {
        GameObject cardObj = CreateCardObject();
        
        // Highlight and select the newly created object in the hierarchy
        Selection.activeGameObject = cardObj;
        
        // Match camera viewport focus if possible
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }

        Debug.Log("[ARCardVisualTestEditor] Created mock Card 'Red 7' in scene with dimensions 0.06m x 0.09m.");
    }

    [MenuItem("Window/AR Visual/Generate Card Prefab")]
    public static void GenerateCardPrefab()
    {
        // 1. Create the card GameObject setup
        GameObject cardObj = CreateCardObject();

        // 2. Ensure directories exist
        string parentFolder = "Assets/Prefabs";
        string targetFolder = "Assets/Prefabs/ARVisual";
        
        if (!AssetDatabase.IsValidFolder(parentFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            AssetDatabase.CreateFolder(parentFolder, "ARVisual");
        }

        // 3. Save as prefab
        string prefabPath = targetFolder + "/ARCardPrefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(cardObj, prefabPath);

        // 4. Clean up scene
        Object.DestroyImmediate(cardObj);

        // 5. Refresh asset database to show the new prefab
        AssetDatabase.Refresh();

        Debug.Log($"[ARCardVisualTestEditor] Successfully generated and saved prefab to: {prefabPath}");
    }

    private static GameObject CreateCardObject()
    {
        // 1. Create root object
        GameObject cardObj = new GameObject("ARCardPrefab");
        
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

        // 5. Initialize with a mock card (Red 7) to set up standard scaling/sprites
        CardData mockCard = new CardData(100, CardColor.Red, CardType.Number, 7);
        visual.Initialize(mockCard);

        return cardObj;
    }
}
