using System;
using System.Collections.Generic;

public partial class COP<T>
{
    /// <summary>
    /// Single instance of a variable for this COP
    /// </summary>
    /// <typeparam name="V">Datatype of value, same as COP</typeparam>
    [Serializable]
    public class Variable<V>
    {
        public int id; // Unique Identifier
        public List<V> domain; // Use list in case of dynamic domains
        public V value;

        // Constraints can be unitary to n-ary, soft or hard
        public delegate float Constraints(int[] ids, V[] values, float utility);

        // Should return a utility amount for each possible assigned value
        public Constraints UtilityFunction;

        public void AssignRandom(System.Random rng)
        {
            if (domain.Count < 1)
                throw new ArgumentOutOfRangeException();

            value = domain[rng.Next(0, domain.Count)];
        }

        public bool AssignValue(V value, bool checkDomain = false)
        {
            if (!checkDomain)
                this.value = value;
            // Check domain
            else if (!domain.Contains(value))
                return false;

            return true;
        }

        public bool ValueInDomain()
        {
            return domain.Contains(value);
        }
    }
}
