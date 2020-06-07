using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Objects;
using Newtonsoft.Json;
using JsonConverters;
using InBattle;

public class MeshToBoard : EditorWindow
{

    static float boardSize = BoardSpace.BOARD_SIZE;

    Mesh mesh;
    int rows = 1;
    int columns = 1;
    string saveTo = "Assets/Maps/map{0}.json";
    float cornerSize = 0.5f;
    float discontinuityHeight = 1f;

    [MenuItem("Window/Map to Board")]
    public static void ShowWindow(){
        MeshToBoard window = (MeshToBoard) EditorWindow.GetWindow(typeof(MeshToBoard));
        window.Show();
    }

    void OnGUI(){
        EditorGUILayout.PrefixLabel("Mesh to Convert");
        mesh = (Mesh) EditorGUILayout.ObjectField(obj: mesh, objType: typeof(Mesh), allowSceneObjects: true);
        EditorGUILayout.Space();
        rows = EditorGUILayout.IntField("Rows", rows);
        columns = EditorGUILayout.IntField("Columns", columns);
        if (GUILayout.Button("Auto-Set Dimensions")){
            if (mesh != null){
                AutoSetDimensions();
            }
        }
        EditorGUILayout.Space();
        EditorGUILayout.PrefixLabel("Corner Size");
        cornerSize = EditorGUILayout.Slider(cornerSize, 0.3f, 0.8f);
        discontinuityHeight = EditorGUILayout.FloatField("Discontinuity Height", discontinuityHeight);
        if (GUILayout.Button("To Text")){
            if (mesh != null){
                MeshToText();
            }
        }
        if (GUILayout.Button("Create Board")){
            if (mesh != null){
                GenerateBoard();
            }
        }
    }

    void MeshToText(){
        string json = JsonConvert.SerializeObject(mesh.vertices, Formatting.Indented, new VectorConverter());
        File.WriteAllText("map.txt", json);
    }
    void AutoSetDimensions(){
        /**Automatically sets the number of rows and columns to generate according to
        the bounding box of the selected mesh.!--*/
        Vector3 size = mesh.bounds.size;
        rows = (int) Math.Round(size.x / boardSize);
        columns = (int) Math.Round(size.y / boardSize);
    }

    void SaveToFile(string json){
        int i = 0;
        string filepath = string.Format(saveTo, i);
        bool written = false;
        while (!written){
            if (!File.Exists(filepath)){
                File.WriteAllText(filepath, json);
                written = true;
            } else {
                i++;
                filepath = string.Format(saveTo, i);
            }
            if (i > 100){
                Debug.Log("could not name a file");
                break;
            }
        }
        Debug.Log("Saved to " + filepath);
    }

    void GenerateBoard(){
        //keep only vertices with positive heights
        HashSet<Vector3> vertices = new HashSet<Vector3>(from vertex in mesh.vertices where vertex.z >= 0 select vertex);
        List<BoardSpace> spaces = new List<BoardSpace>();
        for (int i = 0; i < columns; i++){
            for (int j = 0; j < rows; j++){
                spaces.Add(CreateSpace(vertices, i, j));
            }
        }
        GameObject obj = new GameObject();
        BoardManager board = obj.AddComponent<BoardManager>();
        board.rows = rows;
        board.columns = columns;
        board.boardSpaces = new BoardSpace[columns, rows];
        foreach (BoardSpace space in spaces){
            board.boardSpaces[(int) space.boardPosition.x, (int) space.boardPosition.y] = space;
        }
        string json = JsonConvert.SerializeObject(obj, Formatting.Indented, new BoardConverter());
        SaveToFile(json);
        DestroyImmediate(obj);
    }

    HashSet<Vector3> VerticesInSquare(HashSet<Vector3> vertices, Vector3 meshCenter, float size){
        /**Returns the list of mesh-space points in vertices that are within a square of side length size
        centered on center, in the horizontal plane. Vertical position is not considered.!--*/
        HashSet<Vector3> inSquare = new HashSet<Vector3>();
        foreach (Vector3 vertex in vertices){
            if ((Math.Abs(vertex.x - meshCenter.x) < 0.5 * size) && (Math.Abs(vertex.y - meshCenter.y) < 0.5 * size)){
                inSquare.Add(vertex);
            }
        }
        return inSquare;
    }

    float AverageHeight(HashSet<Vector3> vertices, bool worldSpace = false){
        /**Returns the average (y) height of the points in vertices, in the specified space.!--*/
        float height = 0;
        if (worldSpace){
            foreach (Vector3 vertex in vertices){
                height += vertex.y;
            }
        } else {
            foreach (Vector3 vertex in vertices){
                height += vertex.z;
            }
        }
        
        return height / vertices.Count;
    }

    bool IsContinuous(HashSet<Vector3> vertices){
        /**Returns true if the set vertices in mesh space should represent continuous spaces.!--*/
        float[] heights = (from vertex in vertices select vertex.z).ToArray();
        Debug.Log(heights.Max() - heights.Min());
        return (heights.Max() - heights.Min() <= discontinuityHeight * boardSize);
    }

    BoardSpace CreateSpace(HashSet<Vector3> meshVerts, int i, int j){
        /**Creates the BoardSpace at board coordinates (i, j) from the given mesh vertices.!--*/
        
        //in world space
        Vector3 centerPosition = new Vector3((float) (i + 0.5) * boardSize, 0, (float) (j + 0.5) * boardSize);
        //To avoid looping through the entire mesh multiple times, keep only the close points
        HashSet<Vector3> relevantVerts = VerticesInSquare(meshVerts, SwapMeshWorldSpace(centerPosition), boardSize + cornerSize);
        Vector3[] corners = new Vector3[4] {new Vector3(i * boardSize, 0, j * boardSize),
            new Vector3((i + 1) * boardSize, 0, j * boardSize),
            new Vector3((i + 1) * boardSize, 0, (j + 1) * boardSize),
            new Vector3(i * boardSize, 0, (j + 1) * boardSize)};
        
        BoardSpace space = new BoardSpace();
        space.corners = (from corner in corners select SwapMeshWorldSpace(CreateCorner(relevantVerts,
                                                                    SwapMeshWorldSpace(centerPosition),
                                                                    SwapMeshWorldSpace(corner)))).ToArray();
        //space height will be the average height of the corners
        space.anchorPosition = centerPosition + Vector3.up * AverageHeight(new HashSet<Vector3>(space.corners), true);
        space.boardPosition = new Vector2(i, j);
        return space;
    }

    Vector3 CreateCorner(HashSet<Vector3> relevantVerts, Vector3 centerPosition, Vector3 cornerPosition){
        /**Returns the mesh-space corner point, with calculated height, of the space of given center and corner,
        where both centerPosition and cornerPosition are in mesh space. */
        HashSet<Vector3> cornerVerts = VerticesInSquare(relevantVerts, cornerPosition, cornerSize * boardSize);
        float height;
        if (IsContinuous(cornerVerts)){
            //if the area is continuous, use vertices around the corner
            height = AverageHeight(cornerVerts);
        } else {
            //in case of discontinuity, use the vertices towards the center of the space
            height = AverageHeight(VerticesInSquare(relevantVerts,
                                                    Vector3.Lerp(cornerPosition, centerPosition, cornerSize),
                                                    boardSize * cornerSize));
        }
        cornerPosition.z = height;
        return cornerPosition;
    }

    Vector3 SwapMeshWorldSpace(Vector3 point){
        return new Vector3(point.x, point.z, point.y);
    }

}
