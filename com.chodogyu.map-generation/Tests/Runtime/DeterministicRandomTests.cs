using System;
using NUnit.Framework;

namespace CDG.MapGeneration.Tests.Runtime
{
    public sealed class DeterministicRandomTests
    {
        [Test]
        public void NextUInt_동일한Seed는동일한순서를생성한다()
        {
            DeterministicRandom first = new DeterministicRandom(12345);
            DeterministicRandom second = new DeterministicRandom(12345);

            for (int i = 0; i < 100; i++)
            {
                Assert.That(first.NextUInt(), Is.EqualTo(second.NextUInt()));
            }
        }

        [Test]
        public void NextUInt_알고리즘의고정된순서를생성한다()
        {
            DeterministicRandom random = new DeterministicRandom(12345);

            Assert.That(random.NextUInt(), Is.EqualTo(3336926330u));
            Assert.That(random.NextUInt(), Is.EqualTo(1697253807u));
            Assert.That(random.NextUInt(), Is.EqualTo(2816511904u));
            Assert.That(random.NextUInt(), Is.EqualTo(1955480042u));
            Assert.That(random.NextUInt(), Is.EqualTo(718842323u));
        }

        [Test]
        public void NextUInt_Seed가0이어도고정상태에머물지않는다()
        {
            DeterministicRandom random = new DeterministicRandom(0);

            Assert.That(random.NextUInt(), Is.Not.EqualTo(0u));
            Assert.That(random.NextUInt(), Is.Not.EqualTo(0u));
        }

        [Test]
        public void NextInt_지정한범위안의값만생성한다()
        {
            DeterministicRandom random = new DeterministicRandom(12345);

            for (int i = 0; i < 1000; i++)
            {
                int value = random.NextInt(-5, 8);
                Assert.That(value, Is.GreaterThanOrEqualTo(-5));
                Assert.That(value, Is.LessThan(8));
            }
        }

        [Test]
        public void NextInt_동일한Seed는동일한순서를생성한다()
        {
            DeterministicRandom first = new DeterministicRandom(54321);
            DeterministicRandom second = new DeterministicRandom(54321);

            for (int i = 0; i < 100; i++)
            {
                Assert.That(first.NextInt(5, 20), Is.EqualTo(second.NextInt(5, 20)));
            }
        }

        [Test]
        public void NextInt_잘못된범위를전달하면예외가발생한다()
        {
            DeterministicRandom random = new DeterministicRandom(12345);

            Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(10, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(10, 5));
        }
    }
}