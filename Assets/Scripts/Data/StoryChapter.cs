using UnityEngine;

namespace Spartha.Data
{
    [CreateAssetMenu(fileName = "NewChapter", menuName = "Spartha/Story Chapter")]
    public class StoryChapter : ScriptableObject
    {
        [Header("Chapter Info")]
        public int chapterNumber;
        public string chapterTitle;
        public string regionName;

        [Header("Narrative")]
        [TextArea(5, 15)]
        public string synopsis;

        [Header("Quests")]
        public QuestData[] quests;

        [Header("Requirements")]
        public int requiredLevel;
        public int requiredChapter; // must complete this chapter first
    }

    [System.Serializable]
    public class QuestData
    {
        public string questId;
        public string questTitle;
        [TextArea(3, 8)]
        public string description;
        public QuestObjective[] objectives;
        public string[] rewardItems;
        public int rewardXP;
    }

    [System.Serializable]
    public class QuestObjective
    {
        public string description;
        public string objectiveType; // "talk", "battle", "explore", "collect", "seal"
        public string targetId;
        public int targetCount = 1;
    }
}
