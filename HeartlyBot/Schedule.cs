using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VarietyScheduler
{
    /// <summary>
    /// Contains all positions in a specific day's schedule.
    /// </summary>
    class Schedule
    {
        string Weekday;
        DateTime StartDate;
        DateTime EndDate;
        string Session;
        Position[] Positions;
        public static Schedule[] Schedules;
        
        /// <summary>
        /// Constructor for the Schedule object.
        /// </summary>
        /// <param name="scheduleLines">The lines of a .csv version of a Variety Village weekday employee schedule.</param>
        public Schedule(string[] scheduleLines)
        {
            var schedule = new string[scheduleLines.Count()][];
            for (int i = 0; i < scheduleLines.Count(); i++)
            {
                schedule[i] = scheduleLines[i].Split(',');
            }
            Weekday = schedule[0][0];
            var dateInfo = (schedule[1][0] + schedule[1][1]).Replace("\"","").Split(' ');
            Session = dateInfo[0];

            string[] months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            bool first = true;
            for(int i = 0; i < dateInfo.Count(); i++)
            {
                if (months.Contains(dateInfo[i]))
                {
                    int monthIndex = Array.IndexOf(months, dateInfo[i]) + 1;
                    int day = Convert.ToInt32(dateInfo[i + 1].Replace("th", "").Replace("rd","").Replace("st",""));
                    var date = new DateTime(DateTime.Now.Year, monthIndex, day);
                    if (first)
                    {
                        StartDate = date;
                        first = false;
                    }
                    else
                    {
                        EndDate = date;
                        break;
                    }
                }
            }

            List<Position> positions = new List<Position>();
            for (int i = 1; i < schedule[0].Count(); i++)
            {
                string posName = schedule[2][i];
                bool pm = false;
                bool firstPm = true;
                for (int o = 3; o < schedule.Count(); o++)
                {
                    string name = schedule[o][i];
                    string times = schedule[o][0];
                    var splitTimes = times.Split('-');
                    var startSplit = splitTimes[0].Split(':').Select(x=>Convert.ToInt32(x)).ToArray();
                    var endSplit = splitTimes[1].Split(':').Select(x => Convert.ToInt32(x)).ToArray();
                    if (!pm && endSplit[0] == 1)
                    {
                        pm = true;
                    }
                    var startTime = new DateTime(DateTime.Now.Year, 1, 1, startSplit[0], startSplit[1], 0);
                    var endTime = new DateTime(DateTime.Now.Year, 1, 1, endSplit[0], endSplit[1], 0);
                    if (pm)
                    {
                        if (!firstPm) startTime = startTime.AddHours(12);
                        else firstPm = false;
                        endTime = endTime.AddHours(12);
                    }
                    positions.Add(new Position(posName, name, Weekday, startTime,endTime));
                }
            }
            Positions = positions.ToArray();

        }

        /// <summary>
        /// Get all positions in the Schedule.
        /// </summary>
        /// <returns>An array of all Position objects in the Schedule.</returns>
        public Position[] GetPositions()
        {
            return Positions;
        }

        /// <summary>
        /// Get all shifts that belong to a specified employee.
        /// </summary>
        /// <param name="name">The employee name to search for.</param>
        /// <returns>Returns an array of shifts in which the employee name matches the name parameter.</returns>
        public static Shift[] GetEmployeeShifts(string name)
        {
            var allPos = new List<Position>();
            foreach (var sched in Schedules)
                allPos.AddRange(sched.GetPositions());

            var employeePositions = allPos.Where(x => x.EmployeeName.ToLower() == name.ToLower());
            var employeeWorkDays = employeePositions.GroupBy(x => x.Weekday).ToArray();
            var shifts = new List<Shift>();

            foreach (var day in employeeWorkDays)
            {
                var shift = new Shift(day.ToArray());
                shifts.Add(shift);
            }


            string[] weekdayOrder = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            return shifts.OrderBy(x => Array.IndexOf(weekdayOrder, x.Weekday)).ToArray();
        }
    }

    /// <summary>
    /// Stores information about a single position block.
    /// </summary>
    class Position
    {
        public DateTime StartTime { get; }
        public DateTime EndTime { get; }
        public string PositionName { get; }
        public string EmployeeName { get; }
        public string Weekday { get; }

        public Position(string title, string name, string weekday, DateTime start, DateTime end)
        {
            PositionName = title;
            EmployeeName = name;
            StartTime = start;
            EndTime = end;
            Weekday = weekday;
        }

    }

    /// <summary>
    /// Used to group position blocks that are connected via area and time.
    /// </summary>
    class GroupedPositions
    {
        public string PositionName { get; private set; }
        public Position[] Positions { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        public void Add(Position position)
        {
            List<Position> existingPos;
            if (Positions != null) existingPos = Positions.ToList();
            else existingPos = new List<Position>();

            existingPos.Add(position);
            Positions = existingPos.OrderBy(x => x.StartTime).ToArray();

            PositionName = Positions.First().PositionName;
            StartTime = Positions.First().StartTime;
            EndTime = Positions.Last().EndTime;
        }

    }

    /// <summary>
    /// Used to store information about an employee's entire shift for the day.
    /// </summary>
    class Shift
    {
        public string Weekday { get; }
        public string EmployeeName { get; }
        public Position[] AllPositions { get; }
        public GroupedPositions[] GroupedPositions { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        /// <summary>
        /// Constructor for the Shift object.
        /// </summary>
        /// <param name="dayPositions">Array of Position objects all belonging to the same employee on the same day.</param>
        public Shift(Position[] dayPositions)
        {
            EmployeeName = dayPositions[0].EmployeeName;
            Weekday = dayPositions[0].Weekday;
            AllPositions = dayPositions;
            GroupPositions();
        }

        private void GroupPositions()
        {
            var groups = new List<GroupedPositions>();
            foreach(var position in AllPositions)
            {
                bool added = false;
                var similarGroups = groups.Where(x => x.PositionName == position.PositionName);
                if (similarGroups.Count() > 0)
                {
                    foreach(var similarGroup in similarGroups)
                    {
                        if (similarGroup.Positions.Where(x => x.StartTime == position.EndTime || x.EndTime == position.StartTime).Count() > 0)
                        {
                            similarGroup.Add(position);
                            added = true;
                            break;
                        }
                    }
                }

                if (!added)
                {
                    var newGroup = new GroupedPositions();
                    newGroup.Add(position);
                    groups.Add(newGroup);
                }
            }

            GroupedPositions = groups.OrderBy(x=>x.StartTime).ToArray();
            StartTime = GroupedPositions.First().StartTime;
            EndTime = GroupedPositions.Last().EndTime;
        }
    }
}
