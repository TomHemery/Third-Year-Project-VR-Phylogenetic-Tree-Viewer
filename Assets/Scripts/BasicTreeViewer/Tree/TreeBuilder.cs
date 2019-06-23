using UnityEngine;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Layout.Incremental;
using System.Collections.Generic;
using UnityEngine.UI;

public class TreeBuilder : MonoBehaviour {
    private GameObject treeContainer = null;
    private GameObject treeHandler = null;
    public static TreeBuilder Instance;
    public GameObject nodePrefab;
    private InputEventManager eventManager;
    private GameObject treeTerrainObject;
    private GameObject player;

    private bool generateTerrainTree = false;
    private GameObject[] leafNodes;
    private int numForceLayoutPassesPerformed = 0;
    private int numLeafNodeTerrainRaised = 0;
    private TerrainBuildStep currentBuildStep;
    private Queue<GameObject> bfsQueue = null;

    public GameObject loadingTextPanel;
    private Text loadingText;

    public void Awake()
    {
        if(Instance == null) { Instance = this; }
        treeContainer = GameObject.Find("TreeContainer");
        treeHandler = GameObject.Find("TreeHandler");
        eventManager = GameObject.Find("_Scene").GetComponent<InputEventManager>();
        treeTerrainObject = GameObject.Find("TreeTerrain");
        player = GameObject.Find("WaveVR");
    }

    public void Start()
    {
        loadingText = loadingTextPanel.GetComponentInChildren<Text>();
        loadingTextPanel.SetActive(false);
    }

    //used to generate terrain trees at run time without crashing the app on android (guaranteed not to make the app hang for too long)
    public void Update()
    {
        if (generateTerrainTree) {
            switch (currentBuildStep) {
                case TerrainBuildStep.forcePasses:
                    TreeForceApplicator.Pass(leafNodes);
                    numForceLayoutPassesPerformed++;
                    if (numForceLayoutPassesPerformed >= TreeForceApplicator.numPasses)
                    {
                        currentBuildStep = TerrainBuildStep.checkForCachedHeights;
                        loadingText.text = "Loading...\nChecking for cached heights";
                        numForceLayoutPassesPerformed = 0;
                    }
                    else
                        loadingText.text = "Loading...\nArranging Nodes\n" + 
                            (float)numForceLayoutPassesPerformed / TreeForceApplicator.numPasses * 100 + "%";
                    break;
                case TerrainBuildStep.checkForCachedHeights:
                    //here we check to see if the terrain has already been generated and cached
                    if (TreeTerrainBehaviour.instance.TryLoadFromCache())
                    { //if it has we can skip over raising and smoothing
                        currentBuildStep = TerrainBuildStep.nodeRaising;
                        loadingText.text = "Loading...\nRaising Nodes";
                    }
                    else {
                        currentBuildStep = TerrainBuildStep.terrainRasing;
                        loadingText.text = "Loading...\nRaising Terrain\n0%";
                    }
                    break;
                case TerrainBuildStep.terrainRasing:
                    //create a queue to store nodes in if one doesn't already exist
                    if (bfsQueue == null)
                    {
                        bfsQueue = new Queue<GameObject>();
                        //enqueue the root node - the first game object in the hierachy beneath the tree container
                        bfsQueue.Enqueue(treeContainer.transform.GetChild(0).gameObject);
                    }

                    //breadth first raise terrain at leaf nodes in chunks of 10
                    for (int numReps = 0; numReps < 10; numReps++)
                    {
                        if (bfsQueue.Count != 0)
                        {
                            GameObject node = bfsQueue.Dequeue();
                            Node info = node.GetComponent<BasicNodeBehaviour>().getAttachedNodeInfo();

                            if (info.getNumberOfChildren() == 0)
                            {
                                for (int i = 0; i <= info.nodeDepth; i++)
                                    TreeTerrainBehaviour.instance.RaiseTerrainTo(node, i, info.nodeDepth);
                                numLeafNodeTerrainRaised++;
                            }

                            foreach (Transform child in node.transform)
                            {
                                if (child.gameObject.tag == "NodeObject")
                                {
                                    bfsQueue.Enqueue(child.gameObject);
                                }
                            }
                        }
                        else
                        {
                            currentBuildStep = TerrainBuildStep.terrainSmoothing;
                            numLeafNodeTerrainRaised = 0;
                            loadingText.text = "Loading...\nSmoothing Terrain";
                            break;
                        }

                        if (currentBuildStep == TerrainBuildStep.terrainRasing)
                        {
                            loadingText.text = "Loading...\nRaising Terrain\n" +
                            (float)numLeafNodeTerrainRaised / leafNodes.Length * 100 + "%";
                        }
                    }
                    break;
                case TerrainBuildStep.terrainSmoothing:
                    TreeTerrainBehaviour.instance.SmoothTerrain();
                    //here we cache the terrain heights so we don't have to do these computations again
                    TreeTerrainBehaviour.instance.CacheTerrainHeights();
                    currentBuildStep = TerrainBuildStep.nodeRaising;
                    loadingText.text = "Loading...\nRaising Nodes";
                    break;
                case TerrainBuildStep.nodeRaising:
                    bfsQueue = new Queue<GameObject>();
                    //enqueue the root node bring nodes above the terrain
                    bfsQueue.Enqueue(treeContainer.transform.GetChild(0).gameObject);

                    //scale is the scale of the sphere renderer component of each node -> identical to the sphere's diameter
                    //hierachy: 
                    //                   0        0             0       1       1       2           n-1       n
                    //TreeContainer -> Node -> {Renderer -> {Label, Sphere}, Child1, Child2 ... Child n-1, Child n}
                    float scale = treeContainer.transform.GetChild(0).transform.GetChild(0).GetChild(1).localScale.y;
                    float radius = scale / 2;

                    //do the bfs again, this time raise all the non leaf nodes to a height above the terrain
                    // raise all the parent nodes just above the terrain so the tree remains clear
                    float parentNodeHeight = TreeTerrainBehaviour.instance.maxTerrainHeight + 0.5f + radius;

                    while (bfsQueue.Count != 0)
                    {
                        GameObject node = bfsQueue.Dequeue();
                        Node info = node.GetComponent<BasicNodeBehaviour>().getAttachedNodeInfo();
                        if (info.getNumberOfChildren() > 0)
                        {
                            Vector3 pos = node.transform.position;
                            pos.y = parentNodeHeight;
                            node.transform.position = pos;
                        }
                        else
                        {
                            Vector3 pos = node.transform.position;
                            pos.y = TreeTerrainBehaviour.instance.GlobalHeightAt(node.transform.position) + radius;
                            node.transform.position = pos;
                        }

                        foreach (Transform child in node.transform)
                        {
                            if (child.gameObject.tag == "NodeObject")
                            {
                                bfsQueue.Enqueue(child.gameObject);
                            }
                        }
                    }

                    //put the player nice and high in the air so they don't end up inside the terrain
                    Vector3 playerPos = player.transform.position;
                    playerPos.y = parentNodeHeight + 1f;
                    player.transform.position = playerPos;
                    currentBuildStep = TerrainBuildStep.terrainColouring;
                    bfsQueue = null;
                    break;
                case TerrainBuildStep.terrainColouring:
                    TreeTerrainBehaviour.instance.ColourTerrain();
                    DisableOnFrameLoading();
                    break;
            }
        }
    }

    //takes a target Tree object and builds actual game nodes for it - using MSAGL as an intermediate representation for layout
    public void BuildTree(Tree target) {

        if (target == null) return; //do not attempt to build a null tree - this means that a syntax error happened in a newick file

        GameManager.DebugLog("Building tree with layout: " + eventManager.Current_Tree_State);

        ResetTreeContainer();

        GeometryGraph asMSAGL = ToMSALGraph(target);

        List<GameObject> generatedNodes = new List<GameObject>();

        //define how we want the tree to be layed out 
        LayoutAlgorithmSettings settings;
        switch (eventManager.Current_Tree_State) {
            default:
            case InputEventManager.TreeState.BasicTree:
                settings = new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings();
                break;
            case InputEventManager.TreeState.TerrainTree:
            case InputEventManager.TreeState.CircularTree:
                settings = new FastIncrementalLayoutSettings();
                break;
        }

        //apply some extra settings and layout the graph according to all settings
        settings.EdgeRoutingSettings.EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.StraightLine;
        settings.PackingMethod = PackingMethod.Compact;
        LayoutHelpers.CalculateLayout(asMSAGL, settings, null);

        //we want world space 0, 0 to be equivalent to graph space 0, 0
        float offSetX = (float)asMSAGL.Nodes[0].Center.X;
        float offSetY = (float)asMSAGL.Nodes[0].Center.Y;
        //msal graph edge weights are enormous, like hundreds of meters of in world space - scale it down
        float scaleFactor = eventManager.Current_Tree_State == InputEventManager.TreeState.TerrainTree ? 
            1f/30f : 1f/250f;

        //build game objects using msagl graph as spatial layout
        foreach (Microsoft.Msagl.Core.Layout.Node node in asMSAGL.Nodes) {
            float x = ((float)node.Center.X - offSetX) * scaleFactor;
            float y = ((float)node.Center.Y - offSetY) * scaleFactor;

            GameObject newNode = Instantiate(nodePrefab, new Vector3(
                    treeContainer.transform.position.x + x, 
                    treeContainer.transform.position.y + y, 
                    treeContainer.transform.position.z
                    ), 
                Quaternion.identity);
            newNode.transform.SetParent(treeContainer.transform);
            newNode.GetComponent<BasicNodeBehaviour>().attachNodeInfo((Node)node.UserData);
            newNode.GetComponentInChildren<InfoOnHover>()._Init();
            node.UserData = newNode;

            generatedNodes.Add(newNode);

            foreach (Edge edge in node.Edges) {
                if (edge.Target == node) {
                    Line l = newNode.transform.Find("Renderer").gameObject.AddComponent<Line>();
                    l.gameObject1 = newNode;
                    l.gameObject2 = (GameObject)edge.Source.UserData;
                    newNode.transform.SetParent(((GameObject)edge.Source.UserData).transform);
                    if (newNode.GetComponent<BasicNodeBehaviour>().getAttachedNodeInfo().weightToParentSet())
                    {
                        l.setLabel("" + newNode.GetComponent<BasicNodeBehaviour>().getAttachedNodeInfo().getWeightToParent());
                    }
                    else
                    {
                        l.setLabel("");
                    }
                }
            }
        }

        if (eventManager.Current_Tree_State == InputEventManager.TreeState.TerrainTree)
        {
            Vector3 translation = new Vector3(0, -4, 0);
            treeContainer.transform.Translate(translation);
            treeContainer.transform.Rotate(90, 0, 0);
            
            List<GameObject> leafNodesList = new List<GameObject>();

            foreach (GameObject node in generatedNodes){
                //scale up the nodes a bit to make them clearer within the terrain
                float scale = node.GetComponentInChildren<MaintainNodeScale>().targetScale;
                node.GetComponentInChildren<MaintainNodeScale>().targetScale = 2 * scale;
                node.transform.hasChanged = true;
                if (node.GetComponent<BasicNodeBehaviour>().getAttachedNodeInfo().getNumberOfChildren() == 0)
                {
                    leafNodesList.Add(node);
                }
            }

            generatedNodes[0].GetComponentInChildren<MaintainNodeScale>().targetScale = 4f;

            GameManager.DebugLog("Found " + leafNodesList.Count + " leaf nodes");

            TreeTerrainBehaviour.instance.Activate();
            TreeTerrainBehaviour.instance.ResetHeights();

            leafNodes = leafNodesList.ToArray();
            EnableOnFrameLoading();
        }
        else
        {
            DisableOnFrameLoading();
            //the player won't be moving while we aren't looking at terrain so we'll reset their position
        }
        
        if (eventManager.Draw_Tree_In_3D)
        {
            TreeConverter3D.Convert(treeContainer.transform.GetChild(0).gameObject);
        }

        //as a last step, reset the position of the player so they are close to the root of whatever tree they built
        player.GetComponent<PositionResetter>().ResetPosition();
    }

    private void EnableOnFrameLoading() {
        generateTerrainTree = true;
        loadingTextPanel.SetActive(true);
        currentBuildStep = TerrainBuildStep.forcePasses;
        loadingText.text = "Loading...\nArranging Nodes\n0%";
    }

    private void DisableOnFrameLoading() {
        generateTerrainTree = false;
        loadingTextPanel.SetActive(false);
    }

    //Resets the tree container, removing all nodes and resetting all transforms
    public void ResetTreeContainer()
    {
        foreach (Transform child in treeContainer.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = treeContainer.transform.childCount - 1; i >= 0; i--)
        {
            treeContainer.transform.GetChild(i).parent = null;
        }

        treeContainer.GetComponent<TreeScaler>().ResetTransform();
        treeHandler.GetComponent<PositionResetter>().ResetPosition();
        TreeTerrainBehaviour.instance.Deactivate();
    }

    //Takes a Tree object and returns an equivalent MSAL compatible GeometryGraph object
    public static GeometryGraph ToMSALGraph(Tree target) {
        GeometryGraph result = new GeometryGraph();
        //setup nodes
        RecursiveCompleteMSALGraphNodes(result, target.getRoot());
        //setup edges from child to parent - hence why we don't just start at the root
        foreach (Node child in target.getRoot().getChildren())
        {
            RecursiveCompleteMSALGraphEdges(result, child);
        }
        return result;
    }

    //helper function for ToMSALGraph
    private static void RecursiveCompleteMSALGraphNodes(GeometryGraph target, Node current) {
        Microsoft.Msagl.Core.Layout.Node msaglNode = new Microsoft.Msagl.Core.Layout.Node(
            CurveFactory.CreateCircle(20, new Microsoft.Msagl.Core.Geometry.Point()), current);
        // Add node into MSAGL model.
        target.Nodes.Add(msaglNode);

        foreach (Node child in current.getChildren()) {
            RecursiveCompleteMSALGraphNodes(target, child);
        }
    }

    //helper function for ToMSALGraph
    private static void RecursiveCompleteMSALGraphEdges(GeometryGraph target, Node current) {
        target.Edges.Add(
            new Edge(
                // Set source and target by finding MSAGL node based on SfDiagram node.
                target.FindNodeByUserData(current.getParent()),
                target.FindNodeByUserData(current))
            {
                Weight = 1,
            });
        foreach (Node child in current.getChildren()) {
            RecursiveCompleteMSALGraphEdges(target, child);
        }
    }

    private enum TerrainBuildStep {
        forcePasses = 0,
        checkForCachedHeights = 1,
        terrainRasing = 2,
        terrainSmoothing = 3,
        nodeRaising = 4,
        terrainColouring = 5
    }

}