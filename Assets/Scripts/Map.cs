using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TiledSharp;
using System.IO;
using System.Collections.ObjectModel;

public class Map
{
    private Sprite[] sprites;

    private int width;
    private int height;

    private int[,] tileIds;

    private string basePath = string.Empty;

    private Stream GetStream(string name)
    {
        string path = Utils.PathSimplify(basePath + "/" + name);
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        return new MemoryStream(textAsset.bytes);
    }

    public void Load(int levelId)
    {
        basePath = "Levels/" + levelId.ToString();
        
        TmxMap map = new TmxMap("map", GetStream);
        width = map.Width;
        height = map.Height;

        tileIds = new int[height, width];

        TmxLayer backgroundLayer = map.Layers[0];
        Collection<TmxLayerTile> tiles = backgroundLayer.Tiles;
        int numTiles = tiles.Count;

        for (int i = 0; i < numTiles; ++i)
        {
            int id = tiles[i].Gid;

            int row = i / width;
            int col = i % width;
            tileIds[row, col] = id;

            Debug.Log(id);
        }
    }
}
