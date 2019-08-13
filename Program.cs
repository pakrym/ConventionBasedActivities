using System;
using System.Threading;

namespace ConventionBasedActivities
{
    class Program
    {
        private static ActivityListener s_activityListener = new ActivityListener("My.Custom.Thing");

        static void Main(string[] args)
        {
            using (new ConsoleConventionBasedListener("My.Custom.*"))
            {
                var activity = s_activityListener.CreateActivity("Operation");
                activity?.AddTag("title", "Processing " + string.Join(",", args) + " arguments");
                activity?.AddTag("count", args.Length.ToString());
                s_activityListener.Write("SetColor", (activity: activity, color: ConsoleColor.Red));

                using (s_activityListener.StartActivityScope(activity))
                {
                    Thread.Sleep(1000);

                    var otherActivity = s_activityListener.CreateActivity("InnerOperation");
                    using (s_activityListener.StartActivityScope(otherActivity))
                    {
                        Thread.Sleep(1000);
                    }

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
