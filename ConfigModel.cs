using System.Collections.Generic;

namespace IconCarousel
{
    public class ConfigModel
    {
        public List<string> IconPaths { get; set; } = new List<string>();
        public int IntervalSeconds { get; set; } = 3;
        public bool AutoStart { get; set; } = false;
    }
}
