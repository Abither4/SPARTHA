using UnityEngine;

namespace Spartha.World
{
    public class NPCInteract : MonoBehaviour
    {
        public string npcName = "NPC";
        public string[] dialogueLines;
        public Color nameColor = Color.yellow;

        private bool playerNear;
        private int currentLine;
        private bool talking;
        private OverworldUI overworldUI;

        void Start()
        {
            overworldUI = Object.FindAnyObjectByType<OverworldUI>();
        }

        void Update()
        {
            if (playerNear && Input.GetKeyDown(KeyCode.E))
            {
                if (!talking)
                {
                    talking = true;
                    currentLine = 0;
                    if (overworldUI != null)
                        overworldUI.ShowDialogue(npcName, dialogueLines[currentLine]);
                }
                else
                {
                    currentLine++;
                    if (currentLine >= dialogueLines.Length)
                    {
                        talking = false;
                        if (overworldUI != null)
                            overworldUI.HideDialogue();
                    }
                    else
                    {
                        if (overworldUI != null)
                            overworldUI.ShowDialogue(npcName, dialogueLines[currentLine]);
                    }
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                playerNear = true;
                if (overworldUI != null)
                    overworldUI.ShowPrompt($"Press [E] to talk to {npcName}");
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                playerNear = false;
                talking = false;
                if (overworldUI != null)
                {
                    overworldUI.HideDialogue();
                    overworldUI.HidePrompt();
                }
            }
        }
    }
}
