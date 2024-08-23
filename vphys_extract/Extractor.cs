using System.Numerics;
using System.Runtime.InteropServices;
using SteamDatabase.ValvePak;
using ValveKeyValue;
using ValveResourceFormat;
using ValveResourceFormat.IO;
using ValveResourceFormat.ResourceTypes;

namespace vphys_extract;

public static class Extractor
{
    public static (List<Vector3>, List<int>) Extract(string vpkPath, string vmdlCPath)
    {
        var package = new Package();
        package.Read(vpkPath);
        var entry = package.FindEntry(vmdlCPath);

        using var entryStream = GameFileLoader.GetPackageEntryStream(package, entry);

        var resource = new Resource();
        resource.Read(entryStream);

        if (resource.GetBlockByType(BlockType.PHYS) is not PhysAggregateData phys)
            throw new Exception("No physics data found in the model");

      
        // Copied from
        // https://github.com/ValveResourceFormat/ValveResourceFormat/blob/05f522551dcde704fa3faa9088570070380648f7/GUI/Types/Renderer/PhysSceneNode.cs
        
        var groupCount = phys.CollisionAttributes.Count;
        var verts = new List<Vector3>[groupCount];
        var inds = new List<int>[groupCount];
       
        
        for (var i = 0; i < groupCount; i++)
        {
            verts[i] = [];
            inds[i] = [];
        }
        
        var bindPose = phys.BindPose;
        
        for (var p = 0; p < phys.Parts.Length; p++)
        {
            var shape = phys.Parts[p].Shape;
            
            //Hulls
            foreach (var hull in shape.Hulls)
            {
                var collisionAttributeIndex = hull.CollisionAttributeIndex;
                //var surfacePropertyIndex = capsule.SurfacePropertyIndex;

                var vertexPositions = hull.Shape.GetVertexPositions();

                var pose = bindPose.Length == 0 ? Matrix4x4.Identity : bindPose[p];

                var shapeVerts = verts[collisionAttributeIndex];
                var shapeInds = inds[collisionAttributeIndex];

                // vertex positions
                var positions = new Vector3[vertexPositions.Length];
                for (var i = 0; i < vertexPositions.Length; i++)
                {
                    positions[i] = Vector3.Transform(vertexPositions[i], pose);
                }
                
                var faces = hull.Shape.GetFaces();
                var edges = hull.Shape.GetEdges();

                var numTriangles = edges.Length - faces.Length * 2;
                shapeVerts.EnsureCapacity(shapeVerts.Count + numTriangles * 3);
                shapeInds.EnsureCapacity(shapeInds.Count + numTriangles * 6);

                foreach (var face in faces)
                {
                    var startEdge = face.Edge;

                    for (var edge = edges[startEdge].Next; edge != startEdge;)
                    {
                        var nextEdge = edges[edge].Next;

                        if (nextEdge == startEdge)
                        {
                            break;
                        }

                        var a = positions[edges[startEdge].Origin];
                        var b = positions[edges[edge].Origin];
                        var c = positions[edges[nextEdge].Origin];

                        var offset = shapeVerts.Count;
                        shapeVerts.Add(a);
                        shapeVerts.Add(b);
                        shapeVerts.Add(c);

                        AddTriangle(shapeInds, offset, 0, 1, 2);

                        edge = nextEdge;
                    }
                }
            }
            
            // Meshes
            foreach (var mesh in shape.Meshes)
            {
                var collisionAttributeIndex = mesh.CollisionAttributeIndex;

                var triangles = mesh.Shape.GetTriangles();
                var vertices = mesh.Shape.GetVertices();

                var pose = bindPose.Length == 0 ? Matrix4x4.Identity : bindPose[p];

                var shapeVerts = verts[collisionAttributeIndex];
                var shapeInds = inds[collisionAttributeIndex];

                var numTriangles = triangles.Length;
                shapeVerts.EnsureCapacity(shapeVerts.Count + numTriangles * 3);
                shapeInds.EnsureCapacity(shapeInds.Count + numTriangles * 6);

                // vertex positions
                var positions = new Vector3[vertices.Length];
                for (var i = 0; i < vertices.Length; i++)
                {
                    positions[i] = Vector3.Transform(vertices[i], pose);
                }

                foreach (var tri in triangles)
                {
                    var a = positions[tri.X];
                    var b = positions[tri.Y];
                    var c = positions[tri.Z];
                    

                    var offset = shapeVerts.Count;
                    shapeVerts.Add(a);
                    shapeVerts.Add(b);
                    shapeVerts.Add(c);

                    AddTriangle(shapeInds, offset, 0, 1, 2);
                }
            }
        }

        return (verts[0], inds[0]);
    }
    
    private static void AddTriangle(List<int> inds, int baseVertex, int a, int b, int c)
    {
        inds.Add(baseVertex + a);
        inds.Add(baseVertex + b);
        inds.Add(baseVertex + b);
        inds.Add(baseVertex + c);
        inds.Add(baseVertex + c);
        inds.Add(baseVertex + a);
    }
}