namespace PlanetaKinoScheduleChecker.Domain.Implementation
{
    public class MoveRealesReleaseArgs
    {
        public int MovieId { get; set; }
        
        public MoveRealesReleaseArgs(int movieId)
        {
            MovieId = movieId;
        }
    }
}