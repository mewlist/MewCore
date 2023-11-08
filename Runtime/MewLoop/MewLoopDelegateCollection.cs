#nullable enable
using System.Collections.Generic;

namespace Mew.Core
{
    public class MewLoopDelegateCollection
    {
        public delegate void UpdateFunction();
        private readonly List<UpdateFunction> updateFunctions = new();

        public void Invoke()
        {
            foreach (var updateFunction in updateFunctions.ToArray())
                updateFunction.Invoke();
        }

        public void Add(UpdateFunction updateFunction)
        {
            updateFunctions.Add(updateFunction);
        }

        public void Remove(UpdateFunction updateFunction)
        {
            updateFunctions.Remove(updateFunction);
        }
        
        public static MewLoopDelegateCollection operator +(MewLoopDelegateCollection collection, UpdateFunction func)
        {
            collection.Add(func);
            return collection;
        }
        
        public static MewLoopDelegateCollection operator -(MewLoopDelegateCollection collection, UpdateFunction func)
        {
            collection.Remove(func);
            return collection;
        }
    }
}