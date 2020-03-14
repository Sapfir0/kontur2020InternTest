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