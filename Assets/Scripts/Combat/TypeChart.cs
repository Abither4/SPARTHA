using Spartha.Data;

namespace Spartha.Combat
{
    public static class TypeChart
    {
        // 8x8 type effectiveness matrix
        // Order: SURGE, TIDE, NULL, ECHO, RIFT, EMBER, FLUX, VEIL
        private static readonly float[,] chart = new float[8, 8]
        {
            //          SURGE  TIDE   NULL   ECHO   RIFT   EMBER  FLUX   VEIL
            /*SURGE*/  { 0.5f, 2.0f,  1.0f,  1.0f,  0.5f,  1.0f,  2.0f,  1.0f },
            /*TIDE*/   { 0.5f, 0.5f,  1.0f,  2.0f,  1.0f,  2.0f,  1.0f,  0.5f },
            /*NULL*/   { 1.0f, 1.0f,  0.5f,  0.5f,  2.0f,  1.0f,  1.0f,  2.0f },
            /*ECHO*/   { 1.0f, 0.5f,  2.0f,  0.5f,  1.0f,  1.0f,  2.0f,  1.0f },
            /*RIFT*/   { 2.0f, 1.0f,  0.5f,  1.0f,  0.5f,  2.0f,  1.0f,  1.0f },
            /*EMBER*/  { 1.0f, 0.5f,  1.0f,  1.0f,  0.5f,  0.5f,  2.0f,  2.0f },
            /*FLUX*/   { 0.5f, 1.0f,  1.0f,  0.5f,  1.0f,  0.5f,  0.5f,  2.0f },
            /*VEIL*/   { 1.0f, 2.0f,  0.5f,  1.0f,  1.0f,  0.5f,  0.5f,  0.5f },
        };

        public static float GetEffectiveness(ElementType attacker, ElementType defender)
        {
            return chart[(int)attacker, (int)defender];
        }
    }
}
