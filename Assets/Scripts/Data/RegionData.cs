using UnityEngine;

namespace Spartha.Data
{
    [CreateAssetMenu(fileName = "NewRegion", menuName = "Spartha/Region")]
    public class RegionData : ScriptableObject
    {
        [Header("Identity")]
        public string regionName;
        public string realWorldLocation;
        [TextArea(3, 6)]
        public string description;

        [Header("Level Range")]
        public int minLevel;
        public int maxLevel;

        [Header("Dominant Elements")]
        public ElementType[] dominantElements;

        [Header("Sparks Found Here")]
        public SparkSpecies[] availableSparks;

        [Header("Story")]
        public int storyChapter;
        [TextArea(5, 10)]
        public string storySummary;
    }
}
