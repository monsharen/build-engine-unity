namespace BuildEngine
{
    /*
     * Source: https://wiki.eduke32.com/wiki/Build_units
     * 
     * Build units expressed in terms of length, width, and height. One block of the largest grid size in Build editor equal to 1024.
     *
     * Unity Build Engine units are:
     *  - 512 Build Engine Unit = 1 Unity unit 
     */
    public class BuildEngineToUnityUnitConverter
    {
        
        private const float OneMeterInUnits = 512; // 1024 units = 2 real world meters

        
        /*
         * 1024 units equal the default (non-stretched) size of an 64 pixels tile (i.e. pavement texture, ventilation duct texture, etc).
         */
        private const float ArtTileInUnits = 1024; // Art tiles are 64 pixels and match 1:1 with a 1024 map units.

        
        /*
         * The height scale is different. A z coordinate is 16 times that of x-y coordinates. In other words, a wall with 1024 of length equal in height for a value of 16384.
         */
        public static float ScaleHeight(float value)
        {
            return (value / 16) / OneMeterInUnits;
        }
        
        public static float ScaleWidth(float value)
        {
            return value / OneMeterInUnits;
        }

        /*
         * For orientation in the rotational direction, the Build engine uses a 2048-degree scale (as opposed to 360 degrees.)
         * This means that a 90 degree angle is equal to 512 build units.
         * In relation to the direction a sprite/actor is facing, a value of 0 means the actor/sprite is facing due East.
         * The numbering goes up clockwise until 2047 (just counterclockwise from facing due East.)
         */
        public static float ScaleRotation(float value)
        {
            // (2048 / 360) * 90 = 512
            // 512 
            return value / 2048;
        }
    }
}