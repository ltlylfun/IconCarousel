using System.Collections.Generic;

namespace IconCarousel
{
    public class ConfigModel
    {
        public List<string> IconPaths { get; set; } = new List<string>();
        public int IntervalMilliseconds { get; set; } = 3000;
        public bool AutoStart { get; set; } = false;
    }
}
