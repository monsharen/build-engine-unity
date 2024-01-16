using System.Collections.Generic;
using System.Linq;
using BuildEngineMapReader;
using BuildEngineMapReader.Objects;
using DefaultNamespace;
using UnityEngine;
using static BuildEngine.BuildEngineToUnityUnitConverter;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LoadMap : MonoBehaviour
{

    public GameObject rootNode;
    
    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private int _wallCounter = 0;
    private int _sectorCounter = 0;
    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _mesh = _meshFilter.mesh = new Mesh();
        
        var mapFileReader = new MapFileReader();
        string fileName = "/Users/thomas.rosenquist/git/BuildEngineMapReader/Maps/THE_BASE.MAP";
        Map map = mapFileReader.ReadFile(fileName);
        Debug.Log(map);
        InstantiateMap(map);
        RotateMap();
    }

    void RotateMap()
    {
        rootNode.transform.Rotate(0, 0, 180);
    }

    void InstantiateMap(Map map)
    {
        _mesh.name = "Map";

        var mapSectors = map.Sectors;

        foreach (var sector in mapSectors)
        {
            CreateSector(sector, map);
            _sectorCounter++;
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
                var wallTexture = TextureUtil.LoadTextureWithPicnum(wall.PicNum);
                CreateWall(wallStart, wallEnd, floorHeight, ceilingHeight, wallTexture);
            }

            if (wall.NextSector != -1)
            {
                Sector nextSector = map.Sectors[wall.NextSector];
                var nextSectorFloorHeight = ScaleHeight(nextSector.Floor.Z);
                var wallTexture = TextureUtil.LoadTextureWithPicnum(wall.PicNum);
                if (floorHeight > nextSectorFloorHeight)
                {
                    CreateWall(wallStart, wallEnd, nextSectorFloorHeight, floorHeight, wallTexture);
                }
                else
                {
                    CreateWall(wallStart, wallEnd, floorHeight, nextSectorFloorHeight, wallTexture);
                }
            }
        }
        
        var texture2d = TextureUtil.LoadTextureWithPicnum(sector.Floor.PicNum);
        CreateSectorFloor(sectorVertices, floorHeight, texture2d);
        
        texture2d = TextureUtil.LoadTextureWithPicnum(sector.Ceiling.PicNum);
        CreateSectorCeiling(sectorVertices, ceilingHeight, texture2d);
    }
    
    public void CreateSectorFloor(List<Vector2> vertices, float floorHeight, Texture2D texture)
    {
        GameObject floor = new GameObject("SectorFloor_" + _sectorCounter);
        floor.transform.parent = rootNode.transform;
        MeshRenderer meshRenderer = floor.AddComponent<MeshRenderer>();
        meshRenderer.material.mainTexture = texture;
        MeshFilter meshFilter = floor.AddComponent<MeshFilter>();

        // Convert 2D vertices to 3D vertices
        var vertices3D = new List<Vector3>();
        var uvs = new List<Vector2>(); // List for UVs
        
        foreach (Vector2 vertex in vertices)
        {
            vertices3D.Add(new Vector3(vertex.x, floorHeight, vertex.y));
            uvs.Add(new Vector2(vertex.x, vertex.y)); // Use x and y for UVs
        }
        
        int[] triangles = TriangulateConvexPolygon(vertices3D);

        // Create the mesh
        var mesh = new Mesh();
        mesh.vertices = vertices3D.ToArray();
        mesh.triangles = triangles;
        mesh.uv = uvs.ToArray();
        meshFilter.mesh = mesh;
    }
    
    public void CreateSectorCeiling(List<Vector2> vertices, float ceilingHeight, Texture2D texture)
    {
        GameObject ceiling = new GameObject("SectorCeiling_" + _sectorCounter);
        ceiling.transform.parent = rootNode.transform;
        MeshRenderer meshRenderer = ceiling.AddComponent<MeshRenderer>();
        meshRenderer.material.mainTexture = texture;
        MeshFilter meshFilter = ceiling.AddComponent<MeshFilter>();

        // Convert 2D vertices to 3D vertices
        var vertices3D = new List<Vector3>();
        var uvs = new List<Vector2>(); // List for UVs
        
        foreach (Vector2 vertex in vertices)
        {
            vertices3D.Add(new Vector3(vertex.x, ceilingHeight, vertex.y));
            uvs.Add(new Vector2(vertex.x, vertex.y)); // Use x and y for UVs
        }

        vertices3D.Reverse();
        uvs.Reverse();
        
        int[] triangles = TriangulateConvexPolygon(vertices3D);

        // Create the mesh
        var mesh = new Mesh();
        mesh.vertices = vertices3D.ToArray();
        mesh.triangles = triangles;
        mesh.uv = uvs.ToArray();
        meshFilter.mesh = mesh;
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
        
        return triangles.ToArray();
    }
    
    public void CreateWall(Vector2 start, Vector2 end, float floorHeight, float ceilingHeight, Texture2D texture)
    {
        GameObject wall = new GameObject("Wall_" + _wallCounter);
        wall.transform.parent = rootNode.transform;
        MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();
        meshRenderer.material.mainTexture = texture;
        MeshFilter meshFilter = wall.AddComponent<MeshFilter>();

        Vector3[] vertices = new Vector3[4]
        {
            new (start.x, floorHeight, start.y),
            new (end.x, floorHeight, end.y),
            new (start.x, ceilingHeight, start.y),
            new (end.x, ceilingHeight, end.y)
        };

        var uvs = GetUvs(start, end, floorHeight, ceilingHeight);

        var triangles = new int[6] { 0, 2, 1, 1, 2, 3 };

        //int[] triangles = { 1, 3, 2, 1, 2, 0 }; 
        //int[] triangles = { 0, 2, 1, 2, 3, 1 }; // Define the two triangles
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        meshFilter.mesh = mesh;

        _wallCounter++;
    }

    private Vector2[] GetUvs(Vector2 start, Vector2 end, float floorHeight, float ceilingHeight)
    {
        var wallWidth = Vector3.Distance(new Vector3(start.x, 0, start.y), new Vector3(end.x, 0, end.y));
        var wallHeight = ceilingHeight - floorHeight;

        // Scale factor for texture tiling
        var textureScaleX = wallWidth; // Adjust this factor as needed
        var textureScaleY = wallHeight; // Adjust this factor as needed

        // Set UVs based on wall dimensions
        return new Vector2[4]
        {
            new Vector2(0, 0), // Bottom left
            new Vector2(textureScaleX, 0), // Bottom right
            new Vector2(0, textureScaleY), // Top left
            new Vector2(textureScaleX, textureScaleY) // Top right
        };
    }

 }
