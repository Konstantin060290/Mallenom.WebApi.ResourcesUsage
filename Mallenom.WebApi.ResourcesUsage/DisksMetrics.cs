namespace Mallenom.WebApi.ResourcesUsage
{
    public class DisksMetrics
    {
        public List<Drive> Drives { get; set; } = new List<Drive>();
        public class Drive
        {
            public string? Name { get; set; }
            public string? FullSize { get; set; }
            public string? FreeSize { get; set; }

            public string? UsedSize { get; set; }

            public Drive(string _name, string _freesize, string _fullsize, string _usedsize)
            {
                Name = _name;
                FullSize = _fullsize;
                FreeSize = _freesize;
                UsedSize = _usedsize;
            }

        }
    }
}
