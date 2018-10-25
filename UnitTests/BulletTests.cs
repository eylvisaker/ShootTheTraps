using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ShootTheTraps
{
    public class BulletTests
    {
        [Fact]
        public void SomeTest()
        {
            var b = new Bullet();

            b.ShouldCreateDebris.Should().BeFalse();
        }
    }
}
