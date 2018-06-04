using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TiledSharp;
using System.IO;
using System.Collections.ObjectModel;

public class Map
{
    private Dictionary<int, Sprite> spriteMap;

    private int width;
    private int height;

    private int tileWidth;
    private int tileHeight;

    private float tileRealWidth;
    private float tileRealHeight;

    private SpriteRenderer[,] cellRenderers;

    private string basePath = string.Empty;

    public int GetNumCellX()
    {
        return width;
    }

    public int GetNumCellY()
    {
        return height;
    }

    public int GetWidth()
    {
        return width * tileWidth;
    }

    public int GetHeight()
    {
        return height * tileHeight;
    }

    public float GetRealWidth()
    {
        return width * tileRealWidth;
    }

    public float GetRealHeight()
    {
        return height * tileRealHeight;
    }

    private Stream GetStream(string name)
    {
        string path = Utils.PathSimplify(basePath + "/" + name);
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        return new MemoryStream(textAsset.bytes);
    }

    private SpriteRenderer CreateRenderer(float x, float y)
    {
        GameObject obj = new GameObject();
        obj.transform.localPosition = new Vector3(x, y, 0.0f);
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();

        return renderer;
    }

    public void Load(int levelId)
    {
        basePath = "Levels/" + levelId.ToString();
        
        TmxMap map = new TmxMap("map", GetStream);
        width = map.Width;
        height = map.Height;

        cellRenderers = new SpriteRenderer[height, width];

        TmxTileset tileset = map.Tilesets[0];

        int tileSetWidth = tileset.Columns.GetValueOrDefault();
        int tileSetHeight = tileset.TileCount.GetValueOrDefault() / tileSetWidth;

        spriteMap = new Dictionary<int, Sprite>();

        tileWidth = tileset.TileWidth;
        tileHeight = tileset.TileHeight;

        int startId = tileset.FirstGid;

        Sprite[] spriteList = Resources.LoadAll<Sprite>(Utils.PathSimplify(basePath + "/" + tileset.Name));
        float pixelsPerUnit = spriteList[0].pixelsPerUnit;

        tileRealWidth = tileWidth / pixelsPerUnit;
        tileRealHeight = tileHeight / pixelsPerUnit;

        foreach (Sprite sprite in spriteList)
        {
            Vector2 center = sprite.rect.center;
            int column = Mathf.FloorToInt(center.x / tileWidth);
            int row = tileSetHeight - Mathf.FloorToInt(center.y / tileHeight) - 1;
            int id = row * tileSetWidth + column;

            spriteMap.Add(id, sprite);
        }

        TmxLayer backgroundLayer = map.Layers[0];
        Collection<TmxLayerTile> tiles = backgroundLayer.Tiles;
        int numTiles = tiles.Count;

        for (int i = 0; i < numTiles; ++i)
        {
            int id = tiles[i].Gid - startId;

            int row = height - (i / width) - 1;
            int col = i % width;

            SpriteRenderer cellRenderer = null;

            Sprite sprite;
            if (spriteMap.TryGetValue(id, out sprite))
            {
                cellRenderer = CreateRenderer((col + 0.5f) * tileRealWidth, (row + 0.5f) * tileRealHeight);
                cellRenderer.sprite = sprite;
            }

            cellRenderers[row, col] = cellRenderer;
        }
    }
}
