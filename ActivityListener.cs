// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;

namespace ConventionBasedActivities
{
    public class ActivityListener : DiagnosticListener
    {
        public ActivityListener(string name) : base(name + ".Activities")
        {
        }

        public Activity CreateActivity(string operationName)
        {
            if (IsEnabled() && IsEnabled(operationName))
            {
                return new Activity(operationName);
            }

            return null;
        }

        public ActivityScope StartActivityScope(Activity activity)
        {
            if (activity != null)
            {
                this.Write("Start", activity);
            }
            return new ActivityScope(this, activity);
        }
    }
}