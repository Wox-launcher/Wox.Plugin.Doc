namespace Wox.Plugin.Doc
{

    public enum DocType
    {
        DASH,
        ZDASH
    }

    public class Doc
    {
        public string DBPath { get; set; }
        public DocType DBType { get; set; }
        public string Name { get; set; }
        public string IconPath { get; set; }
    }
}