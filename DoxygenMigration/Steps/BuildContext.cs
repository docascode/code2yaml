namespace Microsoft.Content.Build.DoxygenMigration.Steps
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Microsoft.Content.Build.DoxygenMigration.Utility;

    public class BuildContext
    {
        private ConcurrentDictionary<string, object> _sharedObjects = new ConcurrentDictionary<string, object>();

        #region Shared object
        public object GetSharedObject(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            object item;
            this._sharedObjects.TryGetValue(name, out item);
            return item;
        }

        public bool TryGetSharedObject(string name, out object item)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return this._sharedObjects.TryGetValue(name, out item);
        }

        public bool TryGetSharedObject<T>(string name, out T item)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            object output;
            if (this._sharedObjects.TryGetValue(name, out output))
            {
                item = TypeUtility.ChangeTypeLoose<T>(output);
                return true;
            }

            item = default(T);
            return false;
        }

        public void SetSharedObject(string name, object item)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this._sharedObjects[name] = item;
        }
        #endregion

        public BuildContext Clone()
        {
            BuildContext clone = (BuildContext)this.MemberwiseClone();

            // shallow copy shared objects
            clone._sharedObjects = new ConcurrentDictionary<string, object>(this._sharedObjects);
            return clone;
        }
    }
}
