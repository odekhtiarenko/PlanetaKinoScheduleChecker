using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace PlanetaKinoScheduleChecker.Data
{
    public class UserSubscription
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public int MovieId { get; set; }
        public bool IsNotified { get; set; }
        public string City { get; set; } = "kiev";
    }
}
