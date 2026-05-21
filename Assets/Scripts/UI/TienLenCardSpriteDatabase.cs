using System.Collections.Generic;
using UnityEngine;

public static class TienLenCardSpriteDatabase
{
    private const string DatabasePath = "TienLen/TienLenCardAssetDatabase";
    private const float SpritePixelsPerUnit = 100f;
    private const int NormalFaceWidth = 417;
    private const int NormalFaceTextureHeight = 654;
    private const int NormalFaceVisibleHeight = 625;

    private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
    private static TienLenCardAssetDatabase database;
    private static bool missingDatabaseLogged;

    public static Sprite GetSprite(PlayingCardData card)
    {
        if (card == null)
        {
            return null;
        }

        TienLenCardAssetDatabase assetDatabase = GetDatabase();
        return assetDatabase == null ? null : CreateSprite(assetDatabase.GetTexture(card));
    }

    public static Sprite GetBackSprite()
    {
        TienLenCardAssetDatabase assetDatabase = GetDatabase();
        return assetDatabase == null ? null : CreateSprite(assetDatabase.cardBack);
    }

    private static TienLenCardAssetDatabase GetDatabase()
    {
        if (database != null)
        {
            return database;
        }

        database = Resources.Load<TienLenCardAssetDatabase>(DatabasePath);
        if (database == null && !missingDatabaseLogged)
        {
            missingDatabaseLogged = true;
            Debug.LogWarning("TienLenCardAssetDatabase was not found in Resources. Falling back to generated card faces.");
        }

        return database;
    }

    private static Sprite CreateSprite(Texture2D texture)
    {
        if (texture == null)
        {
            return null;
        }

        Rect visibleRect = GetVisibleRect(texture);
        string cacheKey = texture.name + ":" + visibleRect.x + ":" + visibleRect.y + ":" + visibleRect.width + ":" + visibleRect.height;

        if (SpriteCache.TryGetValue(cacheKey, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Sprite sprite = Sprite.Create(
            texture,
            visibleRect,
            new Vector2(0.5f, 0.5f),
            SpritePixelsPerUnit);

        SpriteCache[cacheKey] = sprite;
        return sprite;
    }

    private static Rect GetVisibleRect(Texture2D texture)
    {
        if (texture.width == NormalFaceWidth && texture.height == NormalFaceTextureHeight)
        {
            return new Rect(0f, 0f, texture.width, NormalFaceVisibleHeight);
        }

        return new Rect(0f, 0f, texture.width, texture.height);
    }
}
