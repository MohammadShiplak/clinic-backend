using System.Reflection.Metadata.Ecma335;

namespace ClinicManagementSystem.API.Services
{
    public class SlotGenerator
    {

        public static IReadOnlyList<DateTime>Generate(DateOnly date, TimeOnly start, TimeOnly end,int slotMinutes
            , IReadOnlySet<DateTime> booked, DateTime now)
        {


            var slots =new List<DateTime>();    
            var step=TimeSpan.FromMinutes(slotMinutes);
            var dayEnd = date.ToDateTime(end);

            for(var t=date.ToDateTime(start); t+ step<=dayEnd; t+=step)
            {

if(t> now && !booked.Contains(t))
                    slots.Add(t);


            }
            return slots;
        }
    }
}
