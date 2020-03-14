using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ToDoList
{
    public class Add
    {
        private const int userA = 10;
        private const int userB = 11;
        private const int userC = 14;

        private IToDoList list;

        [SetUp]
        public void SetUp()
        {
            list = new ToDoList();
        }

        [Test]
        public void SimpleAdd()
        {
            list.AddEntry(42, userA, "iv", 100);
            
            AssertEntries(Entry.Undone(42, "iv"));
        }
        
        [Test]
        public void TimestampIsExistsUserIdLower()
        {
            list.AddEntry(42, userB, "iv", 100);
            list.AddEntry(42, userA, "sing", 100);

            AssertEntries(Entry.Undone(42, "sing"));
        }
        
        [Test]
        public void TimestampIsExistsUserIdBigger()
        {
            list.AddEntry(42, userA, "iv", 100);
            list.AddEntry(42, userC, "sing", 100);

            AssertEntries(Entry.Undone(42, "iv"));
        }
        
        [Test]
        public void TimestampIsExistsRemove()
        {
            list.AddEntry(42, userA, "iv", 100);
            list.RemoveEntry(42, userB, 100);

            AssertListEmpty();
        }
        
        [Test]
        public void TimestampIsExistsMarkDone()
        {
            list.AddEntry(42, userA, "iv", 100);
            list.MarkDone(42, userB, 100);

            AssertEntries(Entry.Done(42, "iv"));
        }
        
        [Test]
        public void TimestampIsExistsMarkDoneBefore()
        {
            list.MarkDone(42, userB, 100);
            list.AddEntry(42, userA, "iv", 100);

            AssertEntries(Entry.Done(42, "iv"));
        }

        
        [Test]
        public void Mark_Done_After_Removal(
            [Values(userA, userB, userC)] int removingUserId,
            [Values(99, 101, 100)] long removingTimestamp)
        {
            list.AddEntry(10, removingUserId, "MyCase", 99);
            list.RemoveEntry(10, removingUserId, removingTimestamp);
            list.MarkDone(10, removingUserId, 99);

            list.AddEntry(10, removingUserId, "MyCase", 104);
            
            AssertEntries(Entry.Done(10, "MyCase"));
        } 
        
        
        [Test]
        public void Mark_Done_Before_Removal(
            [Values(userA, userB, userC)] int removingUserId,
            [Values(99, 101, 100)] long removingTimestamp)
        {
            list.MarkDone(10, removingUserId, 99);
            list.AddEntry(10, removingUserId, "MyCase", 99);
            list.RemoveEntry(10, removingUserId, removingTimestamp);

            list.AddEntry(10, removingUserId, "MyCase", 104);
            
            AssertEntries(Entry.Done(10, "MyCase"));
        } 
        
        [Test]
        public void Mark_Undone_After_Removal()
        {
            list.AddEntry(10, userA, "MyCase", 100);
            list.MarkDone(10, userA, 101);
            list.MarkUndone(10, userA, 102);
            list.RemoveEntry(10, userA, 102);
            list.AddEntry(10, userA, "MyCase", 104);
            
            AssertEntries(Entry.Undone(10, "MyCase"));
        }
        
        
        private void AssertListEmpty()
        {
            list.Should().BeEmpty();
            list.Count.Should().Be(0);
        }

        private void AssertEntries(params Entry[] expected)
        {
            list.Should().BeEquivalentTo(expected.AsEnumerable());
            list.Count.Should().Be(expected.Length);
        }
        
        
        
    }
}