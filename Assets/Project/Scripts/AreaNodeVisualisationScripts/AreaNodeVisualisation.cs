using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
[DisallowMultipleComponent]
public class AreaNodeVisualisation : MonoBehaviour {

    [Header("Define the position of each corner of the area, and the height")]
    [Tooltip("A transform which has the position of the first corner of the area, on the floor")]
    public Transform areaCorner1;
    [Tooltip("A transform which has the position of the second corner of the area, on the floor")]
    public Transform areaCorner2;
    [Tooltip("A transform which has the position of the third corner of the area, on the floor")]
    public Transform areaCorner3;
    [Tooltip("A transform which has the position of the fourth corner of the area, on the floor")]
    public Transform areaCorner4;
    [Tooltip("The Y coordinate value which the top face of the area will be at. Used to control the 'height' of the area")]
    public float areaTopYValue;

    [Header("Materials for defining the colour/look of the visualisation for each 'area state'")]
    [Tooltip("Material for when the 'Main' Agent character enters this zone")]
    public Material mainAgentInAreaMaterial;
    [Tooltip("Enemy agent character is known to be in this area")]
    public Material confirmedEnemiesMaterial;
    [Tooltip("Material for contact with enemies area (Direct combat engagement)")]
    public Material combatContactMaterial;
    [Tooltip("Material for when known enemies could reach this area without the main agent seeing them do so")]
    public Material enemyControlledMaterial;
    [Tooltip("Material for when there is the potential for enemies or danger in the area, but is unknown")]
    public Material uncontrolledAreaMaterial;
    [Tooltip("Immediate Danger in this area")]
    public Material dangerAreaMaterial;
    [Tooltip("Material for areas which are directly controlled by friendly agents")]
    public Material controlledAreaMaterial;

    // Events the area node manager will be interested in.
    public event EventHandler<ICharacter> CharacterEnteredZone;
    public event EventHandler<ICharacter> CharacterExitedZone;

    private HashSet<ICharacter> agentsInZone;
    public HashSet<ICharacter> AgentsInZone {
        get {
            return new HashSet<ICharacter>(agentsInZone);
        }
    }

    public AreaNodeVisualisationStates CurrentState {
        get { return CurrentState; }
        set {
            CurrentState = value;
            if (value == AreaNodeVisualisationStates.MAIN_AGENT_IN_AREA) { meshRenderer.material = mainAgentInAreaMaterial; }
            else if (value == AreaNodeVisualisationStates.COMBAT_CONTACT) { meshRenderer.material = combatContactMaterial; }
            else if (value == AreaNodeVisualisationStates.CONFIRMED_ENEMIES) { meshRenderer.material = confirmedEnemiesMaterial; }
            else if (value == AreaNodeVisualisationStates.ENEMY_CONTROLLED) { meshRenderer.material = enemyControlledMaterial; }
            else if (value == AreaNodeVisualisationStates.UNCONTROLLED) { meshRenderer.material = uncontrolledAreaMaterial; }
            else if (value == AreaNodeVisualisationStates.DANGER) { meshRenderer.material = dangerAreaMaterial; }
            else if (value == AreaNodeVisualisationStates.CONTROLLED) { meshRenderer.material = controlledAreaMaterial; }
            else { throw new System.Exception("Tried to set an AreaNodeVisualisation to a state without a corresponding material update!"); }
        }
    }

    // Private fields
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;

    private void Awake() {
        // Gain access to the game controller in the scene, so that we can get the area node manager and register to it.
        GameObject.FindGameObjectWithTag("GameController").GetComponent<AreaNodeManager>().RegisterAreaNode(this);
    }

    // Use this for initialization
    void Start () {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        Mesh mesh = GenerateAreaMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshCollider.isTrigger = true;
        meshRenderer.material = uncontrolledAreaMaterial;
        agentsInZone = new HashSet<ICharacter>();
        CurrentState = AreaNodeVisualisationStates.UNCONTROLLED;
    }

    // Define logic for tracking when character objects enter and exit the area.
    void OnTriggerEnter(Collider other) {
        ICharacter character = other.GetComponent<ICharacter>();
        if (character != null) {
            // The thing which entered the trigger is a character!
            agentsInZone.Add(character);

            // Inform our manager! (or anyone who cares..)
            CharacterEnteredZone?.Invoke(this, character);
        }
    }
    void OnTriggerExit(Collider other) {
        ICharacter character = other.GetComponent<ICharacter>();
        if (character != null) {
            // The thing which left the trigger is a character!
            agentsInZone.Remove(character);

            // Inform our manager! (or anyone who cares..)
            CharacterExitedZone?.Invoke(this, character);
        }
    }

    public enum AreaNodeVisualisationStates {
        MAIN_AGENT_IN_AREA,
        CONFIRMED_ENEMIES,
        COMBAT_CONTACT,
        ENEMY_CONTROLLED,
        UNCONTROLLED,
        DANGER,
        CONTROLLED
    }

    private Mesh GenerateAreaMesh() {
        Mesh mesh = new Mesh();
        Vector3[] verts = new Vector3[8]; // Boxes have 8 vertices on them
        verts[0] = verts[4] = areaCorner1.localPosition;
        verts[1] = verts[5] = areaCorner2.localPosition;
        verts[2] = verts[6] = areaCorner3.localPosition;
        verts[3] = verts[7] = areaCorner4.localPosition;
        for (int i = 4; i < 8; i++) verts[i].y = areaTopYValue;
        mesh.vertices = verts;
        mesh.triangles = new int[]{0,3,1,1,3,2,4,5,7,5,6,7,3,7,2,2,7,6,0,1,4,1,5,4,0,4,7,0,7,3,1,6,5,1,2,6};
        mesh.uv = new Vector2[] {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),
            new Vector2(0,1),
            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(1,1)
        };
        return mesh;
    }
}
