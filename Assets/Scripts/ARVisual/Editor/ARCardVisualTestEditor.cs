using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [MenuItem("Window/AR Visual/Generate Table Prefab")]
    public static void GenerateTablePrefab()
    {
        // 1. Create directories
        string parentFolder = "Assets/Prefabs";
        string targetFolder = "Assets/Prefabs/ARVisual";
        string matParentFolder = "Assets/Materials";
        string matTargetFolder = "Assets/Materials/ARVisual";

        if (!AssetDatabase.IsValidFolder(parentFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(targetFolder))
            AssetDatabase.CreateFolder(parentFolder, "ARVisual");
        if (!AssetDatabase.IsValidFolder(matParentFolder))
            AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(matTargetFolder))
            AssetDatabase.CreateFolder(matParentFolder, "ARVisual");

        // 2. Create the table GameObject setup
        GameObject tableRoot = new GameObject("ARTableRoot");
        
        // Attach scripts
        ARTableController controller = tableRoot.AddComponent<ARTableController>();
        tableRoot.AddComponent<ARGameEventBridge>();

        // 3. Table Surface
        GameObject surfaceObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surfaceObj.name = "TableSurface";
        surfaceObj.transform.SetParent(tableRoot.transform);
        surfaceObj.transform.localScale = new Vector3(0.5f, 0.02f, 0.35f);
        surfaceObj.transform.localPosition = new Vector3(0f, -0.01f, 0f);
        
        // Remove cube collider to avoid interference, or convert to trigger
        Collider surfaceCollider = surfaceObj.GetComponent<Collider>();
        if (surfaceCollider != null)
        {
            Object.DestroyImmediate(surfaceCollider);
        }

        // Create sleek surface material
        string matPath = matTargetFolder + "/TableSurfaceMat.mat";
        Material tableMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (tableMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            tableMat = new Material(shader);
            tableMat.color = new Color(0.12f, 0.12f, 0.14f); // Carbon/Slate Dark Grey
            tableMat.SetFloat("_Metallic", 0.6f);
            if (shader.name.Contains("Universal"))
                tableMat.SetFloat("_Smoothness", 0.7f);
            else
                tableMat.SetFloat("_Glossiness", 0.7f);
            AssetDatabase.CreateAsset(tableMat, matPath);
        }
        surfaceObj.GetComponent<Renderer>().sharedMaterial = tableMat;
        controller.tableSurface = surfaceObj.transform;

        // 4. ARDrawPile
        GameObject drawPileObj = new GameObject("ARDrawPile");
        drawPileObj.transform.SetParent(tableRoot.transform);
        drawPileObj.transform.localPosition = new Vector3(-0.18f, 0f, 0.12f);
        
        // Add a simple visual deck (stack of cards)
        GameObject drawPileVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        drawPileVisual.name = "VisualDeck";
        drawPileVisual.transform.SetParent(drawPileObj.transform);
        drawPileVisual.transform.localPosition = Vector3.zero;
        drawPileVisual.transform.localScale = new Vector3(0.06f, 0.015f, 0.09f);
        
        Collider deckCollider = drawPileVisual.GetComponent<Collider>();
        if (deckCollider != null) Object.DestroyImmediate(deckCollider);

        string deckMatPath = matTargetFolder + "/DeckMat.mat";
        Material deckMat = AssetDatabase.LoadAssetAtPath<Material>(deckMatPath);
        if (deckMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            deckMat = new Material(shader);
            deckMat.color = new Color(0.15f, 0.35f, 0.7f); // Modern Blue deck back
            AssetDatabase.CreateAsset(deckMat, deckMatPath);
        }
        drawPileVisual.GetComponent<Renderer>().sharedMaterial = deckMat;
        controller.drawPile = drawPileObj.transform;

        // 5. ARDiscardPile
        GameObject discardPileObj = new GameObject("ARDiscardPile");
        discardPileObj.transform.SetParent(tableRoot.transform);
        discardPileObj.transform.localPosition = Vector3.zero;
        controller.discardPile = discardPileObj.transform;

        // 6. PlayerSlots
        GameObject playerSlotsRoot = new GameObject("PlayerSlots");
        playerSlotsRoot.transform.SetParent(tableRoot.transform);
        playerSlotsRoot.transform.localPosition = Vector3.zero;

        Transform[] slots = new Transform[4];
        
        // Player 0 (South - Bottom)
        GameObject slot0 = new GameObject("PlayerSlot_0");
        slot0.transform.SetParent(playerSlotsRoot.transform);
        slot0.transform.localPosition = new Vector3(0f, 0.01f, -0.22f); // Slightly above surface
        slot0.transform.localRotation = Quaternion.LookRotation(Vector3.forward); // Facing north (towards center)
        slots[0] = slot0.transform;

        // Player 1 (West - Left)
        GameObject slot1 = new GameObject("PlayerSlot_1");
        slot1.transform.SetParent(playerSlotsRoot.transform);
        slot1.transform.localPosition = new Vector3(-0.3f, 0.01f, 0f);
        slot1.transform.localRotation = Quaternion.LookRotation(Vector3.right); // Facing east (towards center)
        slots[1] = slot1.transform;

        // Player 2 (North - Top)
        GameObject slot2 = new GameObject("PlayerSlot_2");
        slot2.transform.SetParent(playerSlotsRoot.transform);
        slot2.transform.localPosition = new Vector3(0f, 0.01f, 0.22f);
        slot2.transform.localRotation = Quaternion.LookRotation(Vector3.back); // Facing south (towards center)
        slots[2] = slot2.transform;

        // Player 3 (East - Right)
        GameObject slot3 = new GameObject("PlayerSlot_3");
        slot3.transform.SetParent(playerSlotsRoot.transform);
        slot3.transform.localPosition = new Vector3(0.3f, 0.01f, 0f);
        slot3.transform.localRotation = Quaternion.LookRotation(Vector3.left); // Facing west (towards center)
        slots[3] = slot3.transform;

        controller.playerSlots = slots;

        // 7. ARTurnIndicator
        GameObject turnIndicatorObj = new GameObject("ARTurnIndicator");
        turnIndicatorObj.transform.SetParent(tableRoot.transform);
        turnIndicatorObj.transform.localPosition = new Vector3(0f, 0.05f, -0.22f);
        
        GameObject indicatorVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicatorVisual.name = "VisualRing";
        indicatorVisual.transform.SetParent(turnIndicatorObj.transform);
        indicatorVisual.transform.localPosition = Vector3.zero;
        indicatorVisual.transform.localScale = new Vector3(0.08f, 0.002f, 0.08f);
        
        Collider ringCollider = indicatorVisual.GetComponent<Collider>();
        if (ringCollider != null) Object.DestroyImmediate(ringCollider);

        string indicatorMatPath = matTargetFolder + "/TurnIndicatorMat.mat";
        Material indicatorMat = AssetDatabase.LoadAssetAtPath<Material>(indicatorMatPath);
        if (indicatorMat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            indicatorMat = new Material(shader);
            indicatorMat.color = new Color(0f, 1f, 0.5f); // Neon green
            indicatorMat.EnableKeyword("_EMISSION");
            indicatorMat.SetColor("_EmissionColor", new Color(0f, 0.8f, 0.4f) * 2f);
            AssetDatabase.CreateAsset(indicatorMat, indicatorMatPath);
        }
        indicatorVisual.GetComponent<Renderer>().sharedMaterial = indicatorMat;
        controller.turnIndicator = turnIndicatorObj.transform;

        // 8. AREffectRoot
        GameObject effectRootObj = new GameObject("AREffectRoot");
        effectRootObj.transform.SetParent(tableRoot.transform);
        effectRootObj.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        controller.effectRoot = effectRootObj.transform;

        // 9. Attach Card Prefab (if existing)
        string cardPrefabPath = targetFolder + "/ARCardPrefab.prefab";
        GameObject cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(cardPrefabPath);
        if (cardPrefab != null)
        {
            controller.cardPrefab = cardPrefab;
        }
        else
        {
            string fallbackPath = "Assets/Prefabs/AR/ARCardVisualPrefab.prefab";
            GameObject fallbackPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fallbackPath);
            if (fallbackPrefab != null)
            {
                controller.cardPrefab = fallbackPrefab;
            }
        }

        // 10. Save table as prefab
        string tablePrefabPath = targetFolder + "/ARTableRoot.prefab";
        PrefabUtility.SaveAsPrefabAsset(tableRoot, tablePrefabPath);

        // Clean up
        Object.DestroyImmediate(tableRoot);
        AssetDatabase.Refresh();

        Debug.Log($"[ARCardVisualTestEditor] Successfully generated and saved table prefab to: {tablePrefabPath}");
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

    [MenuItem("Window/AR Visual/Generate Test Scene")]
    public static void GenerateTestScene()
    {
        // 1. Ensure scene folder exists
        string sceneFolder = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(sceneFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        // 2. Create new blank scene with default lights & camera
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 3. Position Main Camera tilted looking down at the table
        GameObject cameraObj = GameObject.FindWithTag("MainCamera");
        if (cameraObj != null)
        {
            cameraObj.transform.position = new Vector3(0f, 0.35f, -0.45f);
            cameraObj.transform.rotation = Quaternion.Euler(38f, 0f, 0f);
        }

        // 4. Load and Instantiate Table Prefab
        string tablePrefabPath = "Assets/Prefabs/ARVisual/ARTableRoot.prefab";
        GameObject tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(tablePrefabPath);
        if (tablePrefab != null)
        {
            GameObject tableInstance = (GameObject)PrefabUtility.InstantiatePrefab(tablePrefab);
            tableInstance.transform.position = Vector3.zero;
            tableInstance.transform.rotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("[ARCardVisualTestEditor] ARTableRoot prefab not found. Please generate the table prefab first.");
        }

        // 5. Create Mock Helper GameObject and attach test helper script
        GameObject testHelperObj = new GameObject("ARVisualTestHelper");
        testHelperObj.AddComponent<ARVisualTestHelper>();

        // 6. Save scene
        string scenePath = sceneFolder + "/ARVisualTestScene.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log($"[ARCardVisualTestEditor] Successfully generated test scene: {scenePath}");
    }
}
