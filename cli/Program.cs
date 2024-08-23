// See https://aka.ms/new-console-template for more information

using vphys_extract;

var extracted = Extractor.Extract("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Counter-Strike Global Offensive\\game\\csgo\\maps\\de_dust2.vpk", "maps/de_dust2/world_physics.vmdl_c");
Console.WriteLine("Finish");