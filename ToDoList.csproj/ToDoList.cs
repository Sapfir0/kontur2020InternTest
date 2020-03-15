using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ToDoList
{
    public class ToDoList : IToDoList
    {
        List<int> dismissedUsers = new List<int>(); // если юзер в списке, то мы не отображаем все что было сделано им

        public enum Operations
        {
            Add,
            Remove,
            Done,
            Undone,
            Rename
        }

        public class History
        {
            public Operations operation { get; }
            public long timestamp;
            public int userId;
            public EntryState state;
            public string name;

            public History(Operations operation, long timestamp, int userId, EntryState state, string name)
            {
                this.operation = operation;
                this.timestamp = timestamp;
                this.userId = userId;
                this.state = state;
                this.name = name;
            }
        }

        List<Entry> entrySet = new List<Entry>();

        private Dictionary<int, List<History>> history = new Dictionary<int, List<History>>();


        public void AddEntry(int entryId, int userId, string name, long timestamp)
        {
            var historyState = EntryState.Undone;

            if (HasUserAccess(userId))
            {
                // проверим, есть ли элемент с таким айдишником
                if (IsElementWithIdExists(entryId))
                {
                    var index = IndexOfElement(entryId);
                    var element = GetElement(entryId);
                    
                    var historyElement = history[entryId].LastOrDefault();
                  
                    if (userId < historyElement.userId && historyElement.timestamp == timestamp) // а с таким же таймстемпом
                    {
                        entrySet[index] = new Entry(element.Id, name, element.State);
                    }
                    else if (historyElement.timestamp < timestamp)
                    {
                        // а теперь посмотрим, может были изменения раньше по времени
                        entrySet[index] = new Entry(element.Id, name, element.State);
                    }
                }
                else
                {
                    // а тут нужно сделать запрос и проверить, был ли удален коммит с таким же айдишником

                    history.TryGetValue(entryId, out var existedMarkDone);
                    // обрабатываем ситуацию, когда произошли какие-то изменения над списком, когда не был вызван
                    if (existedMarkDone != null) 
                    {
                        var state = existedMarkDone.LastOrDefault(x =>
                            x.operation == Operations.Done || x.operation == Operations.Remove);
                        historyState = state.state;

                        var removes = existedMarkDone.FindLast(x => x.operation == Operations.Remove);
                        if (removes != null && removes.timestamp >= timestamp)
                        {
                            return;
                        }

                        // TODO тупой костыль
                        if (existedMarkDone[0].operation == Operations.Done)
                        {
                            historyState = EntryState.Done;
                        }
                    }

                    AddToEntryList(entryId, name, historyState);
                    historyState = EntryState.Undone;
                }
            }

            var operation = history.ContainsKey(entryId) ? Operations.Rename : Operations.Add;

            HistoryAdd(operation, entryId, userId, timestamp, name, historyState);
        }


        public void RemoveEntry(int entryId, int userId, long timestamp) 
        {
            if (IsElementWithIdExists(entryId))
            {
                var index = IndexOfElement(entryId);
                var metaelement = history[entryId].FirstOrDefault();
                if (metaelement.timestamp <= timestamp) // а с таким же таймстемпом
                {
                    RemoveFromEntryList(index); // преимущество удаления
                }
            }

            HistoryAdd(Operations.Remove, entryId, userId, timestamp);
        }


        public void MarkDone(int entryId, int userId, long timestamp)
        {
            // проверим, есть ли элемент с таким айдишником
            if (IsElementWithIdExists(entryId))
            {
                var index = IndexOfElement(entryId);
                var element = entrySet[index];

                var greaterTimestamp = history[entryId].LastOrDefault(x =>
                    x.operation == Operations.Undone && x.timestamp >= timestamp);
                if (greaterTimestamp is null)
                {
                    entrySet[index] = element.MarkDone();
                }
            }

            // а если нет, то порешаем потом при доабавлении этого элемента
            HistoryAdd(Operations.Done, entryId, userId, timestamp, state: EntryState.Done);
        }

        public void MarkUndone(int entryId, int userId, long timestamp)
        {
            if (IsElementWithIdExists(entryId))
            {
                var index = IndexOfElement(entryId);
                var element = entrySet[index];

                // был ли элемент с большим таймстемпом с операцией markdone? если был, то ничего не делаем
                var greaterTimestamp = history[entryId].LastOrDefault(x =>
                    x.operation == Operations.Done && x.timestamp > timestamp); //просто больше, т.к. доминация андана
                if (greaterTimestamp is null)
                {
                    entrySet[index] = element.MarkUndone();
                }
            }

            HistoryAdd(Operations.Undone, entryId, userId, timestamp, state: EntryState.Undone);
        }


        public void DismissUser(int userId)
        {
            foreach (var historyList in history)
            {
                for (int i = 0; i < historyList.Value.Count; i++)
                {
                    if (historyList.Value[i].userId != userId)
                    {
                        continue;
                    }

                    if (historyList.Value[i].operation == Operations.Add)
                    {
                        RemoveFromEntryList(i);
                    }
                    else
                    {
                        // пробегаемся второй раз по всему списку изменений по этому айдишнику
                        for (int j = historyList.Value.Count - 1; j >= 0; j--)
                        {
                            if (historyList.Value[j].userId != userId)
                            {
                                var elem = historyList.Value[j];
                                UpdateEntry(elem.name, elem.state);
                                break; 
                            }
                        }
                    }
                }
            }

            dismissedUsers.Add(userId);
        }

        public void AllowUser(int userId)
        {
            dismissedUsers.Remove(userId);
            foreach (var historyList in history)
            {
                foreach (var currentAction in historyList.Value)
                {
                    if (currentAction.userId != userId)
                    {
                        continue;
                    }

                    if (currentAction.operation == Operations.Add)
                    {
                        AddToEntryList(historyList.Key, currentAction.name,  currentAction.state);
                    }

                    foreach (var action in historyList.Value)
                    {
                        UpdateEntry(action.name, action.state);
                    }
                }
            }
        }


        public void HistoryAdd(Operations operation, int entryId, int userId, long timestamp, string name = " ",
            EntryState? state = null)
        {
            if (name == " ")
            {
                history.TryGetValue(entryId, out var histValue);
                if (histValue != null)
                {
                    name = histValue.ToList().LastOrDefault().name;
                }
            }

            if (state is null)
            {
                history.TryGetValue(entryId, out var histValue);
                if (histValue != null)
                {
                    state = histValue.ToList().LastOrDefault().state;
                }
                else
                {
                    state = EntryState.Undone;
                }
            }

            
            var fixedState = (EntryState) state;

            var currentAction = new History(operation, timestamp, userId, fixedState, name);


            if (history.TryGetValue(entryId, out List<History> historyList))
            {
                bool isInserted = false;	
                // теперь нам нужно инсертнуть в нужное место	
                for (int i = 0; i < historyList.Count; i++)	
                {	
                    var historyTimestamp = historyList[i].timestamp;	
                    if (historyTimestamp > timestamp)	
                    {	
                        isInserted = true;	
                        InsertBefore(entryId, i, currentAction);	
                        break;	
                    }	
                }	
                // теперь правим ремув, если возможно, т.е. если дальше по таймтемпам что-то произошло, но фактичски вызов был раньше, и в ремув не до записались параметры	
                // ставлю что этот блок приведет к ошибке, 	
                if (historyList.Where(x => x.operation == Operations.Remove).ToList().Count == 1)	
                {	
                    var fixRemove = history[entryId].LastOrDefault(x => x.operation == Operations.Remove);	
                    fixRemove.state = (EntryState) state;	
                }	
                if (!isInserted)	
                {	
                    history[entryId].Add(currentAction);	
                }
                

            }
            else
            {
                history.Add(entryId, new List<History>());
                history[entryId].Add(currentAction);
            }
            
           
        }


        public IEnumerator<Entry> GetEnumerator()
            => entrySet.Take(entrySet.Count).GetEnumerator();


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private bool HasUserAccess(int userId)
            => !dismissedUsers.Contains(userId);


        public int Count { get; set; }

        private int IndexOfElement(int entryId)
            => entrySet.IndexOf(entrySet.Where(entry => entry.Id == entryId).FirstOrDefault());

        private Entry GetElement(int entryId)
            => entrySet.Find(x => x.Id == entryId);

        private void AddToEntryList(int entryId, string name, EntryState state)
        {
            entrySet.Add(new Entry(entryId, name, state));
            Count++;
        }

        private void UpdateEntry(string name, EntryState state)
            => entrySet[entrySet.Count - 1] = new Entry(entrySet[entrySet.Count - 1].Id, name, state);

        private void RemoveFromEntryList(int entryId)
        {
            entrySet.RemoveAt(entryId);
            Count--;
        }


        private void InsertBefore(int entryCode, int myindex, History action)
        {
            history[entryCode].Add(action);
            for (int i = history[entryCode].Count - 1; i > myindex; i--)
            {
                history[entryCode][i] = history[entryCode][i - 1];
            }

            history[entryCode][myindex] = action;
        }

        private bool IsElementWithIdExists(int entryId) //не должно быть больше 1
            => entrySet.Where(x => x.Id == entryId).ToList().Count == 1; //мне кажется, не оч
    }
}