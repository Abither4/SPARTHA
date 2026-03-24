using UnityEngine;
using System.Collections.Generic;

namespace Spartha.Data
{
    [CreateAssetMenu(fileName = "StoryDatabase", menuName = "Spartha/Story Database")]
    public class StoryDatabase : ScriptableObject
    {
        public List<StoryChapter> chapters = new();
        public List<NPCData> allNPCs = new();
        public List<RegionData> allRegions = new();
    }
}
