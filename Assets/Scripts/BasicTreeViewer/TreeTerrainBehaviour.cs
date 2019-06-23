using System;
using System.IO;
using System.Text;
using UnityEngine;

public class TreeTerrainBehaviour : MonoBehaviour {

    public static TreeTerrainBehaviour instance { get; private set; } = null;
    private Terrain activeTerrain;
    private int hmWidth;
    private int hmHeight;
    private readonly float heightIncrement = 0.01f;
    private float globalHeightIncrement;
    //the maximum height of any node moved by this terrain - used to offset non child nodes by the tree builder
    public float maxTerrainHeight { get; private set; } = 0f;

    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start () {
        activeTerrain = Terrain.activeTerrain;
        hmWidth = activeTerrain.terrainData.heightmapWidth;
        hmHeight = activeTerrain.terrainData.heightmapHeight;
        gameObject.SetActive(false);
        globalHeightIncrement = heightIncrement * activeTerrain.terrainData.size.y;
    }

    void OnApplicationQuit() {
        //terrain heights in unity are maintained post close - we don't really want that 
        ResetHeights();
    }

    public float GlobalHeightAt(Vector3 pos) { 
        return Terrain.activeTerrain.SampleHeight(pos) + activeTerrain.transform.position.y;
    }

    public void RaiseTerrainTo(GameObject target, int currentDepth, int targetNodeDepth) {
        if (gameObject.activeInHierarchy) {

            int minSize = 20;
            int size = (targetNodeDepth - currentDepth + 1) * minSize;

            // get the position of game object relative to the terrain
            Vector3 relativePos = (target.transform.position - activeTerrain.gameObject.transform.position);
            Vector3 coord;
            coord.x = relativePos.x / activeTerrain.terrainData.size.x;
            coord.y = relativePos.y / activeTerrain.terrainData.size.y;
            coord.z = relativePos.z / activeTerrain.terrainData.size.z;

            // get the position of the terrain heightmap where this game object is
            int posXInTerrain = (int)(coord.x * hmWidth);
            int posYInTerrain = (int)(coord.z * hmHeight);

            int offset = size / 2;

            float[,] heights = activeTerrain.terrainData.GetHeights(posXInTerrain - offset, posYInTerrain - offset, size, size);

            int radiusSquared = size / 2;
            radiusSquared = radiusSquared * radiusSquared;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Vector2 center = new Vector2(size / 2, size / 2);
                    Vector2 indexPos = new Vector2(i, j);
                    float distSquared = (center - indexPos).SqrMagnitude();
                    if (distSquared < radiusSquared)
                    {
                        heights[i, j] = heightIncrement * currentDepth > heights[i, j] ?
                            heightIncrement * currentDepth : heights[i, j];
                    }
                }
            }

            // set the new height
            activeTerrain.terrainData.SetHeights(posXInTerrain - offset, posYInTerrain - offset, heights);

            //update the maximum height of any point within the generated terrain
            Vector3 pos = target.transform.position;
            float scale = target.transform.GetChild(0).GetChild(1).localScale.y;
            pos.y = Terrain.activeTerrain.SampleHeight(pos) + activeTerrain.transform.position.y + scale / 2;
            if (pos.y > maxTerrainHeight) maxTerrainHeight = pos.y;
        }
    }

    public void SmoothTerrain() {
        int smoothRadius = 1;
        int numPasses = 4;
        int hmWidth = activeTerrain.terrainData.heightmapWidth;
        int hmHeight = activeTerrain.terrainData.heightmapHeight;
        float[,] heights = activeTerrain.terrainData.GetHeights(0, 0, hmWidth, hmHeight);
        for (int i = 0; i < numPasses; i++) {
            heights = SmoothPass(heights, smoothRadius);
        }
        activeTerrain.terrainData.SetHeights(0, 0, heights);
    }

    public static float[,] SmoothPass(float[,] heights, int smoothRadius) {
        float[,] result = new float[heights.GetLength(0), heights.GetLength(1)];
        int hmWidth = heights.GetLength(0);
        int hmHeight = heights.GetLength(1);
        for (int x = 0; x < heights.GetLength(0); x++)
        {
            for (int y = 0; y < heights.GetLength(1); y++)
            {

                int count = 0;
                float total = 0;

                for (int x1 = -smoothRadius; x1 <= smoothRadius; x1++)
                {
                    for (int y1 = -smoothRadius; y1 <= smoothRadius; y1++)
                    {
                        if (x1 + x >= 0 && x1 + x < hmWidth && y1 + y >= 0 && y1 + y < hmHeight)
                        {
                            count++;
                            total += heights[x1 + x, y1 + y];
                        }
                    }
                }
                result[x, y] = total / count;
            }
        }
        return result;
    }

    public void ColourTerrain()
    {
        TerrainData terrainData = activeTerrain.terrainData;

        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
        
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y / terrainData.alphamapHeight;
                float x_01 = (float)x / terrainData.alphamapWidth;

                // Sample the height at this location
                float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth));

                float level = height / globalHeightIncrement - globalHeightIncrement / 2;
                level = level > 0 ? level : 0;

                int colourIndex = Mathf.FloorToInt(level) % terrainData.alphamapLayers;

                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    splatmapData[x, y, i] = i == colourIndex ? 1f : 0f;
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    public void ResetHeights() {
        float[,] heights = activeTerrain.terrainData.GetHeights(0, 0,
            activeTerrain.terrainData.heightmapWidth, activeTerrain.terrainData.heightmapHeight);
        for (int i = 0; i < heights.GetLength(0); i++)
        {
            for (int j = 0; j < heights.GetLength(1); j++)
            {
                heights[i, j] = 0;
            }
        }

        activeTerrain.terrainData.SetHeights(0, 0, heights);
    }

    public void CacheTerrainHeights() {
        string path = null;

#if UNITY_EDITOR
        path = Application.dataPath + "/Resources/NewickTrees";
#elif UNITY_ANDROID
        path = Application.persistentDataPath;
#endif
        path += "/" + TreeLoader.CurrentTreeFileName + "-heightmap.txt";
        if (path != null)
        {
            float[,] heights = activeTerrain.terrainData.GetHeights(0, 0,
                activeTerrain.terrainData.heightmapWidth, activeTerrain.terrainData.heightmapHeight);
            GameManager.DebugLog("Caching terrain heights using path: " + path);
            StringBuilder builder = new StringBuilder();
            for (int x = 0; x < heights.GetLength(0); x++) {
                int y = 0;
                for (; y < heights.GetLength(1) - 1; y++) {
                    builder.Append(heights[x, y]);
                    builder.Append(",");
                }
                builder.Append(heights[x, y]);
                builder.Append("\n");
            }
            File.WriteAllText(path, builder.ToString());
        }
        else
        {
            GameManager.DebugLog("Unrecognised platform - terrain heights NOT cached");
        }
    }

    public bool TryLoadFromCache() {
        string textHeightsString;

        string sFileName = TreeLoader.NEWICK_RELATIVE_DIRECTORY_PATH + "/" + TreeLoader.CurrentTreeFileName + "-heightmap";
        TextAsset textHeightsFile = Resources.Load<TextAsset>(sFileName);

        if (textHeightsFile == null) {
            GameManager.DebugLog("Didn't find a text heights file in the assets, checking in app storage");
            //we didn't find anything in the assets file, but there might be something in the app files created by the android app
            sFileName = Application.persistentDataPath + "/" + TreeLoader.CurrentTreeFileName + "-heightmap.txt";
            GameManager.DebugLog("Reading from: " + sFileName);
            try
            {
                textHeightsString = File.ReadAllText(sFileName);
            } catch(FileNotFoundException e)
            {
                GameManager.DebugLog("Failed to find cache file, building tree algorithmically");
                return false;
            }
        }
        else{
            textHeightsString = textHeightsFile.ToString();
        }
        string[] lines = textHeightsString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        float[,] heights = activeTerrain.terrainData.GetHeights(0, 0,
            activeTerrain.terrainData.heightmapWidth, activeTerrain.terrainData.heightmapHeight);
        for (int x = 0; x < lines.Length; x++) {
            string[] currentLineHeights = lines[x].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int y = 0; y < currentLineHeights.Length; y++) {
                float h = float.Parse(currentLineHeights[y]);
                float absoluteHeight = h * activeTerrain.terrainData.size.y + activeTerrain.transform.position.y;
                heights[x, y] = h;
                if (absoluteHeight > maxTerrainHeight) maxTerrainHeight = absoluteHeight;
            }
        }

        activeTerrain.terrainData.SetHeights(0, 0, heights);
        return true;
    }

    public void Activate() {
        gameObject.SetActive(true);
        maxTerrainHeight = 0;
    }

    public void Deactivate() {
        gameObject.SetActive(false);
    }
}
