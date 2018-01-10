namespace PlanetaKinoScheduleChecker.DataAccess
{
    public class SqlText
    {
        public const string Insert_Movie = @"INSERT INTO Movies ([CinemaMovieId],[Title],[StartDate],[EndDate]) Values (@CinemaMovieId, @Title, @StartDate, @EndDate);
                                             SELECT CAST(SCOPE_IDENTITY() as int)";
        public const string Insert_Subscription = "INSERT INTO UserSubscriptions ([ChatId],[MovieId],[IsNotified],[City]) Values (@ChatId,@MovieId,@IsNotified,@City)";
        public const string GetAll_Movie = "SELECT * FROM Movies ";
        public const string Get_Movie = "SELECT * FROM Movies WHERE MovieId = @MovieId ";
        public const string GetSubscriptionByMovieId = "SELECT * FROM UserSubscriptions WHERE MovieId = @MovieId and [IsNotified] = @IsNotified";
        public const string UpdateSubscription = "UPDATE [PlanetaKinoScheduleChecker].[dbo].[UserSubscriptions] SET [ChatId] = @ChatId,[MovieId] = @MovieId,[IsNotified] = @IsNotified,[City] = @City WHERE UserSubscriptionId = @UserSubscriptionId";
        public const string GetAvailiableSubscription = @"SELECT *
                                                          FROM Movies m
                                                          join UserSubscriptions u on m.MovieId = u.MovieId
                                                          Where u.IsNotified = 0";
    }
}