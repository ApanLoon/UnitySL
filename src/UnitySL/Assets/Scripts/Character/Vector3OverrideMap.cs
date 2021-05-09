
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Character
{
    public class Vector3OverrideMap : IEquatable<Vector3OverrideMap>
    {
        public Vector3OverrideMap()
        {
        }

        public bool FindActiveOverride(out Guid meshId, out Vector3 pos)
        {
            pos = new Vector3(0, 0, 0);
            meshId = Guid.Empty;
            bool found = false;

            // TODO: Searching for the greatest GUID makes no sense to me, but I am pretty sure that this is what the LL code does.
            if (Map.Count != 0)
            {
                meshId = Map.Keys.Max();
                pos = Map[meshId];
                found = true;
            }
            return found;
        }

        public string ShowJointVector3Overrides()
        {
            if (Map.Count == 0)
            {
                return "";
            }

            Guid activeOverride = Map.Keys.Max();
            return Map.Aggregate("", (current, kv) => current + $" [{kv.Key}: {kv.Value}]{(kv.Key == activeOverride ? "*" : "")}");
        }

        public int Count => Map.Count;

        public void Add(Guid meshId, Vector3 pos)
        {
            Map[meshId] = pos;
        }

        public bool Remove(Guid meshId)
        {
            bool removed = Map.ContainsKey(meshId);
            Map.Remove(meshId);
            return removed;
        }

        public void Clear()
        {
            Map.Clear();
        }

        public Dictionary<Guid, Vector3> Map { get; } = new Dictionary<Guid, Vector3>();

        public bool Equals(Vector3OverrideMap other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Map, other.Map);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Vector3OverrideMap) obj);
        }

        public override int GetHashCode()
        {
            return (Map != null ? Map.GetHashCode() : 0);
        }
    }
}
