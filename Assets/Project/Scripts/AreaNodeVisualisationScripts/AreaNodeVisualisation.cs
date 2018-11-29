using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
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

    // Private fields
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;

    private List<GameObject> agentsInZone; // This area node visualiser will maintain a collection of all the characters which are inside this area.

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
        meshRenderer.material = uncontrolledAreaMaterial;
    }
	
	// Update is called once per frame
	void Update () {
		
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
