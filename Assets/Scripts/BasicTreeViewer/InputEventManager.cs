using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WaveVR_Log;

public class InputEventManager : MonoBehaviour
{

    private Text buttonInfoText;
    private TreeBuilder treeBuilder;
    private GameObject moverCanvas;
    private TreeContainer_EventHandler treeContainerEventHandler;
    private Text currentSpeedMultText;

    private Movement treeHandlerMovement;
    private Movement playerMovement;

    private Toggle viewIn3DToggle;
    private GameObject controllerFunctionPanel;

    private GameObject treeTargetingSphere;

    private Camera mainCamera;
    
    readonly private string rotateInfoText = "Target the tree and rotate with touchpad";
    readonly private string scaleInfoText = "Target the tree and scale with touchpad";
    readonly private string moveInfoText = "Move Tree With Movement Controls";
    private const string UP = "up";
    private const string DOWN = "down";
    private const string LEFT = "left";
    private const string RIGHT = "right";
    private const string FORWARD = "forward";
    private const string BACKWARD = "backward";
    private const float MOVE_SPEED = 1f;
    private int speedMultiplier = 1;
    private string moveDir = "";

    public ControllerState Current_Controller_State { get; private set; }
    public TreeState Current_Tree_State { get; private set; }
    public bool Draw_Tree_In_3D { get; private set; }

    private void Awake()
    {
        Current_Controller_State = ControllerState.Rotate;
        Current_Tree_State = TreeState.CircularTree;
        Draw_Tree_In_3D = false;
        controllerFunctionPanel = GameObject.Find("ControllerFunctionPanel");
    }

    // Use this for initialization
    void Start()
    {
        buttonInfoText = GameObject.Find("ButtonInfoText").GetComponent<Text>();
        treeBuilder = GameObject.Find("_Scene").GetComponent<TreeBuilder>();
        moverCanvas = GameObject.Find("TreeMoverCanvas");
        treeContainerEventHandler = GameObject.Find("TreeHandler").GetComponent<TreeContainer_EventHandler>();

        treeHandlerMovement = GameObject.Find("TreeHandler").GetComponent<Movement>();
        playerMovement = GameObject.Find("WaveVR").GetComponent<Movement>();

        viewIn3DToggle = GameObject.Find("ViewIn3DToggle").GetComponent<Toggle>();

        currentSpeedMultText = GameObject.Find("TextCurrentSpeedMult").GetComponent<Text>();

        treeTargetingSphere = GameObject.Find("TargetingSphere");

        mainCamera = Camera.main;

        buttonInfoText.text = rotateInfoText;
        ShowMoverCanvas(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerMovement.doMovement) {
            OnMoveTreeDown(moveDir);
        }
    }

    public void OnSelectRotate()
    {
        buttonInfoText.text = rotateInfoText;
        Current_Controller_State = ControllerState.Rotate;
        ShowMoverCanvas(false);
    }

    public void OnSelectMove()
    {
        buttonInfoText.text = moveInfoText;
        Current_Controller_State = ControllerState.Move;
        ShowMoverCanvas(true);
    }

    public void OnSelectScale()
    {
        buttonInfoText.text = scaleInfoText;
        Current_Controller_State = ControllerState.Scale;
        ShowMoverCanvas(false);
    }

    public void OnSelectSpeedMult(int mult)
    {
        speedMultiplier = mult;
        currentSpeedMultText.text = "Speed Multiplier: x" + mult;
    }

    public void OnMoveTreeDown(string direction) {
        Vector3 dir;
        switch (direction) {
            case UP:
                dir = new Vector3(0, MOVE_SPEED, 0);
                break;
            case DOWN:
                dir = new Vector3(0, -MOVE_SPEED, 0);
                break;
            case LEFT:
                dir = new Vector3(mainCamera.transform.right.x, 0, mainCamera.transform.right.z);
                dir = dir * -1;
                dir.Normalize();
                break;
            case RIGHT:
                dir = new Vector3(mainCamera.transform.right.x, 0, mainCamera.transform.right.z);
                dir.Normalize();
                break;
            case FORWARD:
                dir = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
                dir.Normalize();
                break;
            case BACKWARD:
                dir = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
                dir = dir * -1;
                dir.Normalize();
                break;
            default:
                dir = new Vector3(0, 0, 0);
                break;
        }
        moveDir = direction;
        dir = dir * speedMultiplier;
        playerMovement.movementVector = dir;
        playerMovement.doMovement = true;
    }

    public void OnMoveTreeUp() {
        playerMovement.doMovement = false;
    }

    private void ShowMoverCanvas(bool bEnable) {
        moverCanvas.SetActive(bEnable);
    }

    public void OnSelectTreeView(int state)
    {
        GameManager.DebugLog("Selecting new view: " + state + " or " + (TreeState)state);
        Current_Tree_State = (TreeState)state;

        switch (Current_Tree_State)
        {
            case TreeState.BasicTree:
                viewIn3DToggle.isOn = false;
                Draw_Tree_In_3D = false;
                controllerFunctionPanel.SetActive(true);
                break;
            case TreeState.TerrainTree:
                viewIn3DToggle.isOn = false;
                Draw_Tree_In_3D = false;
                OnSelectMove();
                controllerFunctionPanel.SetActive(false);
                break;
            case TreeState.CircularTree:
                controllerFunctionPanel.SetActive(true);
                break;
        }

        

        treeBuilder.BuildTree(TreeLoader.CurrentTree);
    }

    public void OnCheckTree3D()
    {
        if (Current_Tree_State == TreeState.CircularTree)
        {
            Draw_Tree_In_3D = !Draw_Tree_In_3D;
            treeBuilder.BuildTree(TreeLoader.CurrentTree);
        }
        else {
            Draw_Tree_In_3D = false;
            viewIn3DToggle.isOn = false;
        }
    }

    public enum ControllerState
    {
        Rotate = 0,
        Scale = 1,
        Move = 2
    }

    public enum TreeState {
        BasicTree = 0,
        CircularTree = 1,
        TerrainTree = 2
    }
}