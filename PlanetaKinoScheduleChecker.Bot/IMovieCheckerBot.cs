using System.ComponentModel.Composition;
using PlanetaKinoScheduleChecker.Service.Implementation;

namespace PlanetaKinoScheduleChecker.Bot
{
    [InheritedExport]
    public interface IMovieCheckerBot
    {
        void InitalizeBot();
        void MovieCheckerOnOnRelease(object sender, MoveRealesReleaseArgs args);
        string GetName();
        void StopReceiving();
    }
}