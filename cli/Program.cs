// See https://aka.ms/new-console-template for more information

using vphys_extract;
using StreamWriter = System.IO.StreamWriter;

var extracted =
    Extractor.Extract(
        "D:\\SteamLibrary\\steamapps\\common\\Counter-Strike Global Offensive\\game\\csgo\\maps\\de_dust2.vpk",
        "maps/de_dust2/world_physics.vmdl_c");

using var f = File.Create("physics.obj");

using var stream = new StreamWriter(f);

stream.WriteLine("o physics");
foreach (var vector3 in extracted.Item1)
{
    stream.WriteLine($"v {vector3.X} {vector3.Y} {vector3.Z}");
}
//'f ' + str(ind[0] + 1) + '/' + str(ind[0] + 1) +'/ ' + str(ind[1]+1) + '/'+ str(ind[1] + 1) + '/ ' + str(ind[2]+1)+ '/'+ str(ind[2] + 1) + '/
for (var i = 0; i < extracted.Item2.Count; i += 3)
{
    stream.WriteLine($"f {extracted.Item2[i] + 1} {extracted.Item2[i + 1] + 1} {extracted.Item2[i + 2] + 1}");
}

f.Flush();