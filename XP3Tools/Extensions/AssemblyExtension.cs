using System.IO;
using System.Reflection;

namespace XP3Tools.Extensions
{
    public static class AssemblyExtension
    {
        public static byte[] GetManifestResource(this Assembly assembly, string name)
        {
            using (Stream resourceStream = assembly.GetManifestResourceStream(name))
            {
                if (resourceStream == null) return null;

                byte[] data = new byte[resourceStream.Length];
                resourceStream.Read(data, 0, data.Length);
                return data;
            }
        }
    }
}
