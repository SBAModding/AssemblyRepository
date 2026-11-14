using System.Reflection;
using System.Security.Cryptography;
using System.Text;
// Same algorithm as BepInEx's Il2CppInteropManager.ComputeHash, for a device with no unity-libs folder and no rename map.
string gameAssembly = args[0];
string generatorDll = args[1];
string cpp2ilDll = args[2];
using var md = MD5.Create();
void HashFile(string f) { using var s = File.OpenRead(f); var b = new byte[81920]; int n; while ((n = s.Read(b)) > 0) md.TransformBlock(b, 0, n, b, 0); }
void HashString(string str) { var b = Encoding.UTF8.GetBytes(str); md.TransformBlock(b, 0, b.Length, b, 0); }
HashFile(gameAssembly);
string gv = AssemblyName.GetAssemblyName(generatorDll).Version!.ToString();
string cv = AssemblyName.GetAssemblyName(cpp2ilDll).Version!.ToString();
HashString(gv); HashString(cv);
md.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
Console.Error.WriteLine($"generator {gv}, cpp2il {cv}");
Console.WriteLine(Convert.ToHexString(md.Hash!).ToLowerInvariant());
