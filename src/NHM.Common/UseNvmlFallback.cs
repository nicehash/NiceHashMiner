
namespace NHM.Common
{
    public static class UseNvmlFallback
    {
        public static bool Enabled { get; set; } = true;

        //static UseNvmlFallback()
        //{
        //    Enabled = File.Exists(Paths.InternalsPath("UseNvmlFallback"));
        //}
    }
}
