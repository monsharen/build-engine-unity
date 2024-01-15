using System.Collections.Generic;
using System.Linq;
using BuildEngineMapReader;
using BuildEngineMapReader.Objects;
using UnityEngine;
using static BuildEngine.BuildEngineToUnityUnitConverter;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LoadMap : MonoBehaviour
{

    public Material wallMaterial;
    public Material floorMaterial;
    
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private int _wallCounter = 0;
    private int _sectorCounter = 0;
    void Start()
    {

        floorMaterial.color = Color.red;
        
        _meshFilter = GetComponent<MeshFilter>();
        _mesh = _meshFilter.mesh = new Mesh();
        
        var mapFileReader = new MapFileReader();
        string fileName = "/Users/thomas.rosenquist/git/BuildEngineMapReader/Maps/THE_BASE.MAP";
        Map map = mapFileReader.ReadFile(fileName);
        Debug.Log(map);
        InstantiateMap(map);
    }

    void InstantiateMap(Map map)
    {
        _mesh.name = "Map";

        var mapSectors = map.Sectors;

        foreach (var sector in mapSectors)
        {
            CreateSector(sector, map);
        }
    }
    
    private void CreateSector(Sector sector, Map map)
    {
        var sectorWalls = map.Walls.Skip(sector.FirstWallIndex).Take(sector.NumWalls);
        var sectorVertices = new List<Vector2>();

        var floorHeight = ScaleHeight(sector.Floor.Z); //;
        var ceilingHeight = ScaleHeight(sector.Ceiling.Z); //;
        
        Debug.Log("Floor height: " + floorHeight + ", Ceiling height: " + ceilingHeight);
        
        foreach (var wall in sectorWalls)
        {
            sectorVertices.Add(new Vector2(ScaleWidth(wall.X), ScaleWidth(wall.Y)));
            
            var nextWall = map.Walls[wall.NextWallPoint2];
            var wallStart = new Vector2(ScaleWidth(wall.X), ScaleWidth(wall.Y));
            var wallEnd = new Vector2(ScaleWidth(nextWall.X), ScaleWidth(nextWall.Y));

            if (wall.NextSector == -1 && wall.NextWallPoint2 != -1)
            {
                CreateWall(wallStart, wallEnd, floorHeight, ceilingHeight, wallMaterial);
            }
        }
        
        
        CreateSectorFloor(sectorVertices, floorHeight, floorMaterial);
    }
    
    public void CreateSectorFloor(List<Vector2> vertices, float floorHeight, Material material)
    {
        GameObject floor = new GameObject("SectorFloor_" + _sectorCounter);

        MeshRenderer meshRenderer = floor.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = floor.AddComponent<MeshFilter>();

        // Convert 2D vertices to 3D vertices
        List<Vector3> vertices3D = new List<Vector3>();
        foreach (Vector2 vertex in vertices)
        {
            vertices3D.Add(new Vector3(vertex.x, floorHeight, vertex.y));
        }

        // Triangulate the sector (for simplicity, assuming convex polygon here)
        int[] triangles = TriangulateConvexPolygon(vertices3D);

        // Create the mesh
        var mesh = new Mesh();
        mesh.vertices = vertices3D.ToArray();
        mesh.triangles = triangles;
        meshFilter.mesh = mesh;
        meshRenderer.material = material;
        
        _sectorCounter++;
    }
    
    private int[] TriangulateConvexPolygon(List<Vector3> vertices)
    {
        List<int> triangles = new List<int>();
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        triangles.Reverse(); // make them face the right way
        return triangles.ToArray();
    }
    
    public void CreateWall(Vector2 start, Vector2 end, float floorHeight, float ceilingHeight, Material wallMaterial)
    {
        GameObject wall = new GameObject("Wall_" + _wallCounter);
        MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = wall.AddComponent<MeshFilter>();

        Vector3[] vertices = new Vector3[4]
        {
            new (start.x, floorHeight, start.y),
            new (end.x, floorHeight, end.y),
            new (start.x, ceilingHeight, start.y),
            new (end.x, ceilingHeight, end.y)
        };
        
        int[] triangles = { 1, 3, 2, 1, 2, 0 };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        meshFilter.mesh = mesh;
        meshRenderer.material = wallMaterial;

        _wallCounter++;
    }

 }
