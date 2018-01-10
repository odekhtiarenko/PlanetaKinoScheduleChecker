namespace PlanetaKinoScheduleChecker.DataAccess
{
    public class SqlText
    {
        public const string Insert_Movie = "INSERT INTO Movies ([MovieId],[Title],[StartDate],[EndDate]) Values (@MovieId, @Title, @StartDate, @EndDate)";
        public const string Insert_Subscription = "INSERT INTO UserSubscriptions ([ChatId],[MovieId],[IsNotified],[City]) Values (@ChatId,@MovieId,@IsNotified,@City)";
        public const string GetAll_Movie = "SELECT * FROM Movies ";
        public const string Get_Movie = "SELECT * FROM Movies WHERE MovieId = @MovieId ";
        public const string GetSubscriptionByMovieId = "SELECT * FROM UserSubscriptions WHERE MovieId = @MovieId and [IsNotified] = @IsNotified";
        public const string UpdateSubscription = "UPDATE [PlanetaKinoScheduleChecker].[dbo].[UserSubscriptions] SET [ChatId] = @ChatId,[MovieId] = @MovieId,[IsNotified] = @IsNotified,[City] = @City WHERE Id = @Id";
    }
}