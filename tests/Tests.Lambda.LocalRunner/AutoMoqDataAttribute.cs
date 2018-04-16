using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Hosting;

namespace Tests
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(CreateFixture) { }

        private static IFixture CreateFixture()
        {
            IFixture fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization{ ConfigureMembers = true});

            return fixture;
        }
    }
}