using System.Numerics;
using System.Runtime.InteropServices;
using ValveKeyValue;

namespace vphys_extract;

public static class Exports
{
    [UnmanagedCallersOnly(EntryPoint = "ExtractVPK")]
    public static int ExtractVPK(IntPtr VpkPath, IntPtr VmdlcPath, IntPtr Out)
    {
        try
        {
            var vpkPath = Marshal.PtrToStringAnsi(VpkPath);
            var vmdlcPath = Marshal.PtrToStringAnsi(VmdlcPath);

            var extracted = Extractor.Extract(vpkPath, vmdlcPath);
            var export = CreateTrianglesExport(extracted.Item1, extracted.Item2);

            Marshal.StructureToPtr(export, Out, true);

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return -1;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "FreeVPK")]
    public static int FreeVPK(IntPtr Vertices, IntPtr Indices)
    {
        try
        {
            Marshal.FreeHGlobal(Vertices);
            Marshal.FreeHGlobal(Indices);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            return -1;
        }

        return 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TrianglesExport
    {
        public Int32 VertexCount;
        public IntPtr Vertices;


        public Int32 IndexCount;
        public IntPtr Indices;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct rVector3
    {
        public float X;
        public float Y;
        public float Z;
    }

    private static TrianglesExport CreateTrianglesExport(List<Vector3> vertices, List<int> indices)
    {
        var mappedVertices = vertices.Select(v => new rVector3
        {
            X = v.X,
            Y = v.Y,
            Z = v.Z
        }).ToList();

        var memoryVertices = Marshal.AllocHGlobal(Marshal.SizeOf<rVector3>() * mappedVertices.Count);

        for (var i = 0; i < mappedVertices.Count; i++)
        {
            Marshal.StructureToPtr(mappedVertices[i], memoryVertices + i * Marshal.SizeOf<rVector3>(), true);
        }

        var memoryIndices = Marshal.AllocHGlobal(Marshal.SizeOf<int>() * indices.Count);
        for (var i = 0; i < indices.Count; i++)
        {
            Marshal.StructureToPtr(indices[i], memoryIndices + i * Marshal.SizeOf<int>(), true);
        }


        var t = new TrianglesExport
        {
            VertexCount = mappedVertices.Count,
            Vertices = memoryVertices,

            IndexCount = indices.Count,
            Indices = memoryIndices,
        };

        return t;
    }
}