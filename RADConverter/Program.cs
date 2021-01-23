using System;

namespace RADConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var deserializer = new RadToSnConverter();
            deserializer.Convert(@"D:\Development\Projects\recastnavigation\RecastDemo\Bin\all_tiles_navmesh.bin");
        }
    }
}
