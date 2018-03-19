using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetaKinoScheduleChecker.Bot
{
    public class CallBack
    {
        public BotAction BotAction { get; set; }
    }

    public enum BotAction
    {
        GetMovies,
        GetSubscription,
        GetSchedule
    }
}
