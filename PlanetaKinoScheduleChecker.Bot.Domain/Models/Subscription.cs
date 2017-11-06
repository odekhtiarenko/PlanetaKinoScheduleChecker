namespace PlanetaKinoScheduleChecker.Bot.Domain.Models
{
    public class Subscription
    {
        public long ChatId { get; set; }
        public string City { get; set; } = "kiev";
        public int MovieId { get; set; }
        public bool IsNotyfied { get; set; }
        public bool ShouldNotify { get; set; }
        public bool IsReleased { get; set; }
    }
}