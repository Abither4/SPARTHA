using UnityEngine;
using System.Collections.Generic;

namespace Spartha.Data
{
    [CreateAssetMenu(fileName = "SparkDatabase", menuName = "Spartha/Spark Database")]
    public class SparkDatabase : ScriptableObject
    {
        public List<SparkSpecies> allSpecies = new();

        public SparkSpecies GetByName(string name)
        {
            return allSpecies.Find(s => s.speciesName == name);
        }

        public List<SparkSpecies> GetByFamily(SparkFamily family)
        {
            return allSpecies.FindAll(s => s.family == family);
        }

        public List<SparkSpecies> GetByElement(ElementType element)
        {
            return allSpecies.FindAll(s => s.elementType == element);
        }

        public List<SparkSpecies> GetByRegion(string region)
        {
            return allSpecies.FindAll(s => s.homeRegion == region);
        }

        public List<SparkSpecies> GetStarters()
        {
            return allSpecies.FindAll(s =>
                s.speciesName == "Voltpup" ||
                s.speciesName == "Murkhound" ||
                s.speciesName == "Cindersnout");
        }
    }
}
