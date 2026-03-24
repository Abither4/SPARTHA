using UnityEngine;

namespace Spartha.Data
{
    public enum NPCRole
    {
        StoryCharacter,
        StreetTrainer,
        ResonanceKeeper,
        AuraluxOperative,
        ShopKeeper,
        QuestGiver
    }

    [CreateAssetMenu(fileName = "NewNPC", menuName = "Spartha/NPC")]
    public class NPCData : ScriptableObject
    {
        [Header("Identity")]
        public string npcName;
        public NPCRole role;
        public Sprite portrait;
        [TextArea(3, 6)]
        public string description;

        [Header("Location")]
        public string homeRegion;

        [Header("Battle")]
        public bool isTrainer;
        public SparkSpecies[] partySpecies;
        public int[] partyLevels;

        [Header("Dialogue")]
        public DialogueEntry[] dialogueEntries;
    }

    [System.Serializable]
    public class DialogueEntry
    {
        public string id;
        public int chapter;
        public string questId;
        [TextArea(2, 5)]
        public string text;
        public string[] choices;
    }
}
