using System.IO;

namespace XP3Tools.Extensions
{
    public static class StreamExtension
    {
        #region Methods

        public static byte[] ToArray(this Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        #endregion
    }
}
