using System;
using System.Collections.Generic;
using System.Linq;
using BuildEngineMapReader.Objects;
using DefaultNamespace;
using UnityEngine;
using static BuildEngine.BuildEngineToUnityUnitConverter;
using Vector2 = UnityEngine.Vector2;

namespace BuildEngine
{
    public class MapRenderer
    {
        private readonly GameObject _rootNode;
        private readonly TextureManager _textureManager;
        
        private int _wallCounter;
        private int _sectorCounter;

        public MapRenderer(GameObject rootNode, TextureManager textureManager)
        {
            _rootNode = rootNode;
            _textureManager = textureManager;
        }

        public void Render(Map map)
        {
            _wallCounter = 0;
            _sectorCounter = 0;
            InstantiateMap(map);
            RotateMap();
        }
        
        private void InstantiateMap(Map map)
        {
            var mapSectors = map.Sectors;

            foreach (var sector in mapSectors)
            {
                CreateSector(sector, map);
                _sectorCounter++;
            }
        }
        
        private void CreateSector(Sector sector, Map map)
        {
            var sectorGameObject = new GameObject("Sector_" + _sectorCounter);
            sectorGameObject.isStatic = true;
            sectorGameObject.transform.parent = _rootNode.transform;
        
            var sectorWalls = map.Walls.Skip(sector.FirstWallIndex).Take(sector.NumWalls);
            var sectorVertices = new List<Vector2>();

            var floorHeight = ScaleHeight(sector.Floor.Z); //;
            var ceilingHeight = ScaleHeight(sector.Ceiling.Z); //;
        
            foreach (var wall in sectorWalls)
            {
                sectorVertices.Add(new Vector2(ScaleWidth(wall.X), ScaleWidth(wall.Y)));
            
                var nextWall = map.Walls[wall.NextWallPoint2];
                var wallStart = new Vector2(ScaleWidth(wall.X), ScaleWidth(wall.Y));
                var wallEnd = new Vector2(ScaleWidth(nextWall.X), ScaleWidth(nextWall.Y));
            
                if (IsRegularWall(wall))
                {
                    if (IsAWallToTheRightOf(wall))
                    {
                        var wallTexture = _textureManager.LoadMaterialWithPicnum(wall.PicNum);
                        CreateWall(sectorGameObject.transform, wallStart, wallEnd, floorHeight, ceilingHeight, wallTexture);    
                    } else {
                        // This case does not appear to happen in the current test map
                        throw new Exception("Wall has no next sector or next wall point 2");
                    }
                } else {
                    /*
                 * This sector has a next sector, so we need to leave it blank and create a wall between this sector and
                 * the next sector if there's a height difference. 
                 */
                    var wallTexture = _textureManager.LoadMaterialWithPicnum(wall.PicNum);
                
                    var nextSector = map.Sectors[wall.NextSector];
                    var nextSectorFloorHeight = ScaleHeight(nextSector.Floor.Z);
                
                    if (floorHeight > nextSectorFloorHeight)
                    {
                        CreateWall(sectorGameObject.transform, wallStart, wallEnd, nextSectorFloorHeight, floorHeight, wallTexture);
                    }
                    else if (floorHeight < nextSectorFloorHeight)
                    {
                        CreateWall(sectorGameObject.transform, wallStart, wallEnd, floorHeight, nextSectorFloorHeight, wallTexture);
                    }
                
                    var nextSectorCeilingHeight = ScaleHeight(nextSector.Ceiling.Z);
                
                    if (ceilingHeight > nextSectorCeilingHeight)
                    {
                        CreateWall(sectorGameObject.transform, wallStart, wallEnd, nextSectorCeilingHeight, ceilingHeight, wallTexture);
                    }
                    else if (ceilingHeight < nextSectorCeilingHeight)
                    {
                        CreateWall(sectorGameObject.transform, wallStart, wallEnd, ceilingHeight, nextSectorCeilingHeight, wallTexture);
                    }
                }
            }
        
            
            var texture2d = _textureManager.LoadMaterialWithPicnum(sector.Floor.PicNum);
            CreateSectorFloor(sectorGameObject.transform, sectorVertices, floorHeight, texture2d);
        
            texture2d = _textureManager.LoadMaterialWithPicnum(sector.Ceiling.PicNum);
            CreateSectorCeiling(sectorGameObject.transform, sectorVertices, ceilingHeight, texture2d);
        }

        private static bool IsAWallToTheRightOf(Wall wall)
        {
            return wall.NextWallPoint2 != -1;
        }

        private static bool IsRegularWall(Wall wall)
        {
            return wall.NextSector == -1;
        }

        private GameObject CreateHorizontalPlane(Transform rootNode, List<Vector2> vertices, float planeHeight, Material texture, String prefix)
        {
            var plane = new GameObject(prefix + _sectorCounter);
            plane.transform.parent = rootNode.transform;
            var meshRenderer = plane.AddComponent<MeshRenderer>();
            meshRenderer.material = texture;
            var meshFilter = plane.AddComponent<MeshFilter>();

            // Convert 2D vertices to 3D vertices
            var vertices3D = new List<Vector3>();
            var uvs = new List<Vector2>(); // List for UVs
        
            foreach (Vector2 vertex in vertices)
            {
                vertices3D.Add(new Vector3(vertex.x, planeHeight, vertex.y));
                uvs.Add(new Vector2(vertex.x, vertex.y)); // Use x and y for UVs
            }

            // Create the mesh
            var mesh = new Mesh();
            mesh.vertices = vertices3D.ToArray();
            mesh.triangles = TriangulateConvexPolygon(vertices3D);
            mesh.uv = uvs.ToArray();
            meshFilter.mesh = mesh;
            return plane;
        }

        private void CreateSectorFloor(Transform rootNode, List<Vector2> vertices, float floorHeight, Material texture)
        {
            CreateHorizontalPlane(rootNode, vertices, floorHeight, texture, "SectorFloor_");
        }

        private void CreateSectorCeiling(Transform rootNode, List<Vector2> vertices, float ceilingHeight, Material texture)
        {
            var ceiling = CreateHorizontalPlane(rootNode, vertices, ceilingHeight, texture, "SectorCeiling_");
            MirrorPlaneVertically(ceiling.transform, ceilingHeight);
        }

        private void MirrorPlaneVertically(Transform transform, float planeHeight)
        {
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(1f, -1f, 1f));
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 2 * planeHeight, transform.localPosition.z);
        }
    
        private static int[] TriangulateConvexPolygon(List<Vector3> vertices)
        {
            var triangles = new List<int>();
            for (var i = 1; i < vertices.Count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }
        
            return triangles.ToArray();
        }
    
        private void CreateWall(Transform rootNode, Vector2 start, Vector2 end, float floorHeight, float ceilingHeight, Material texture)
        {
            var wall = new GameObject("Wall_" + _wallCounter);
            wall.transform.parent = rootNode.transform;
            var meshRenderer = wall.AddComponent<MeshRenderer>();
            meshRenderer.material = texture;
            var meshFilter = wall.AddComponent<MeshFilter>();

            var vertices = new Vector3[4]
            {
                new (start.x, floorHeight, start.y),
                new (end.x, floorHeight, end.y),
                new (start.x, ceilingHeight, start.y),
                new (end.x, ceilingHeight, end.y)
            };

            var uvs = GetUvs(start, end, floorHeight, ceilingHeight);

            var triangles = new int[6] { 0, 2, 1, 1, 2, 3 };
        
            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            meshFilter.mesh = mesh;

            _wallCounter++;
        }

        private static Vector2[] GetUvs(Vector2 start, Vector2 end, float floorHeight, float ceilingHeight)
        {
            var wallWidth = Vector3.Distance(new Vector3(start.x, 0, start.y), new Vector3(end.x, 0, end.y));
            var wallHeight = ceilingHeight - floorHeight;

            // Set UVs based on wall dimensions
            return new Vector2[4]
            {
                new Vector2(0, 0), // Bottom left
                new Vector2(wallWidth, 0), // Bottom right
                new Vector2(0, wallHeight), // Top left
                new Vector2(wallWidth, wallHeight) // Top right
            };
        }
        
        private void RotateMap()
        {
            _rootNode.transform.Rotate(0, 0, 180);
        }
    }
}