namespace PlanetaKinoScheduleChecker.Data
{
    public class UserSubscription
    {
        public int UserSubscriptionId { get; set; }
        public long ChatId { get; set; }
        public int MovieId { get; set; }
        public bool IsNotified { get; set; }
        public string City { get; set; } = "kiev";
    }
}
