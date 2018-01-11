using System.ComponentModel.Composition;
using System.Web.Http;

namespace PlanetaKinoScheduleChecker.Api.Host.Controllers
{
    public class TestController : ApiController
    {
        private readonly ITestInterface _testInterface;

        public TestController(ITestInterface testInterface)
        {
            _testInterface = testInterface;
        }

        public string Get()
        {
            return _testInterface.Get();
        }
    }

    [InheritedExport]
    public interface ITestInterface
    {
        string Get();
    }

    public class TestInterface : ITestInterface
    {
        public string Get()
        {
            return "test test";
        }
    }
}
