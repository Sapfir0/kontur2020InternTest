using System.Collections;
using System.Collections.Generic;

namespace ToDoList
{
    public class ToDoList : IToDoList
    {
        List<Dictionary<int, (string, bool)>> todolist = new List<Dictionary<int, (string, bool)>>();
        
        public void AddEntry(int entryId, int userId, string name, long timestamp)
        {
            var entry = new Dictionary<int, (string, bool)>();
            entry.Add(entryId, (name, false)); //потом будем учитывать был ли такой туду с таким же имене м
            
            //todolist.Add()
        }

        public void RemoveEntry(int entryId, int userId, long timestamp)
        {

        }

        public void MarkDone(int entryId, int userId, long timestamp)
        {
            throw new System.NotImplementedException();
        }

        public void MarkUndone(int entryId, int userId, long timestamp)
        {
            throw new System.NotImplementedException();
        }

        public void DismissUser(int userId)
        {
            throw new System.NotImplementedException();
        }

        public void AllowUser(int userId)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<Entry> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get; }
    }
}