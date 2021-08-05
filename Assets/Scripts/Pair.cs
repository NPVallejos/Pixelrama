using System;
using UnityEngine;

namespace GuamboCollections {

    [Serializable]
    public class Pair<X, Y>
    {
        [SerializeField] private X _x;
        [SerializeField] private Y _y;

        public Pair(X first, Y second)
        {
            _x = first;
            _y = second;
        }

        [SerializeField] public X first { get { return _x; } }

        [SerializeField] public Y second {
            get { return _y; }
            set { _y = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            return false;
            if (obj == this)
            return true;
            Pair<X, Y> other = obj as Pair<X, Y>;
            if (other == null)
            return false;

            return
            (((first == null) && (other.first == null))
            || ((first != null) && first.Equals(other.first)))
            &&
            (((second == null) && (other.second == null))
            || ((second != null) && second.Equals(other.second)));
        }

        public override int GetHashCode()
        {
            int hashcode = 0;
            if (first != null)
            hashcode += first.GetHashCode();
            if (second != null)
            hashcode += second.GetHashCode();

            return hashcode;
        }
    }
}
