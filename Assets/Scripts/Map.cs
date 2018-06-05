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
    private int[,] collisionMap;

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
        collisionMap = new int[height, width];

        //Background layer
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

        TmxLayer layer = map.Layers[0];
        Collection<TmxLayerTile> tiles = layer.Tiles;
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

        //Collision layer
        tileset = map.Tilesets[1];
        startId = tileset.FirstGid;
        layer = map.Layers[1];
        tiles = layer.Tiles;

        for (int i = 0; i < numTiles; ++i)
        {
            int id = tiles[i].Gid;
            if (id > 0) id -= startId - 1;

            int row = height - (i / width) - 1;
            int col = i % width;

            collisionMap[row, col] = id;
        }
    }

    private void FindCellLocation(Vector2 position, out int row, out int col)
    {
        col = Mathf.Clamp(Mathf.FloorToInt(position.x / tileRealWidth), 0, width);
        row = Mathf.Clamp(Mathf.FloorToInt(position.y / tileRealHeight), 0, height);
    }

    private int FindCellX(float x)
    {
        return Mathf.Clamp(Mathf.FloorToInt(x / tileRealWidth), 0, width - 1);
    }

    private int FindCellY(float y)
    {
        return Mathf.Clamp(Mathf.FloorToInt(y / tileRealHeight), 0, height - 1);
    }

    public Vector2 GetMove(Vector2 position, float objWidth, float objHeight, Vector2 move)
    {
        //Clip with bounds
        if (move.x > 0.0f) move.x = Mathf.Min(move.x, width * tileRealWidth - position.x - objWidth / 2.0f);
        else move.x = Mathf.Max(move.x, objWidth / 2.0f - position.x);

        if (move.y > 0.0f) move.y = Mathf.Min(move.y, height * tileRealHeight - position.y - objHeight);
        else move.y = Mathf.Max(move.y, -position.y);

        int y0 = FindCellY(position.y + Constants.EPS);
        int y1 = FindCellY(position.y + objHeight - Constants.EPS);

        int xDir = (move.x > 0.0f ? 1 : -1);
        float xBound = position.x + objWidth / 2.0f * xDir;

        int xStart = FindCellX(xBound - Constants.EPS * xDir);
        int xEnd = FindCellX(xBound + move.x - Constants.EPS * xDir);

        bool blocked = false;

        while (xStart != xEnd)
        {
            xStart += xDir;

            for (int y = y0; y <= y1; ++y)
            {
                if (collisionMap[y, xStart] > 0)
                {
                    blocked = true;
                    break;
                }
            }

            if (blocked)
            {
                move.x = (xStart + (xDir < 0 ? 1 : 0)) * tileRealWidth - xBound;
                break;
            }
        }

        int x0 = FindCellX(position.x - objWidth / 2.0f + Constants.EPS);
        int x1 = FindCellX(position.x + objWidth / 2.0f - Constants.EPS);

        int yDir = (move.y > 0.0f ? 1 : -1);
        float yBound = position.y + objHeight * (yDir > 0 ? 1 : 0);

        int yStart = FindCellY(yBound - Constants.EPS * yDir);
        int yEnd = FindCellY(yBound + move.y - Constants.EPS * yDir);

        blocked = false;

        while (yStart != yEnd)
        {
            yStart += yDir;

            for (int x = x0; x <= x1; ++x)
            {
                if (collisionMap[yStart, x] > 0)
                {
                    blocked = true;
                    break;
                }
            }

            if (blocked)
            {
                move.y = (yStart + (yDir < 0 ? 1 : 0)) * tileRealHeight - yBound;
                break;
            }
        }

        return move;
    }
}
