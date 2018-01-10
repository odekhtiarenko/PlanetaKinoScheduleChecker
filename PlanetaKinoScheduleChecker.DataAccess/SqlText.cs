namespace PlanetaKinoScheduleChecker.DataAccess
{
    public class SqlText
    {
        public const string InsertMovie = @"INSERT INTO Movies ([CinemaMovieId],[Title],[StartDate],[EndDate]) Values (@CinemaMovieId, @Title, @StartDate, @EndDate);
                                             SELECT CAST(SCOPE_IDENTITY() as int)";
        public const string InsertSubscription = "INSERT INTO UserSubscriptions ([ChatId],[MovieId],[IsNotified],[City]) Values (@ChatId,@MovieId,@IsNotified,@City)";
        public const string SubscriptionCheck = "SELECT * FROM UserSubscriptions WHERE MovieId = @MovieId and ChatId = @ChatId";
        public const string GetAllMovie = "SELECT * FROM Movies ";
        public const string GetMovieByCinemaMovieId = "SELECT * FROM Movies WHERE CinemaMovieId = @CinemaMovieId ";
        public const string GetMovieByMovieId = "SELECT * FROM Movies WHERE MovieId = @MovieId ";
        public const string GetSubscriptionByMovieId = "SELECT * FROM UserSubscriptions WHERE MovieId = @MovieId and [IsNotified] = @IsNotified";
        public const string UpdateSubscription = "UPDATE [PlanetaKinoScheduleChecker].[dbo].[UserSubscriptions] SET [ChatId] = @ChatId,[MovieId] = @MovieId,[IsNotified] = @IsNotified,[City] = @City WHERE UserSubscriptionId = @UserSubscriptionId";
        public const string GetAvailiableSubscription = @"SELECT *
                                                          FROM Movies m
                                                          join UserSubscriptions u on m.MovieId = u.MovieId
                                                          Where u.IsNotified = 0";

    }
}