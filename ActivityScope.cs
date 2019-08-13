// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;

namespace ConventionBasedActivities
{
    public struct ActivityScope: IDisposable
    {
        private readonly ActivityListener _activityListener;

        private readonly Activity _activity;

        internal ActivityScope(ActivityListener activityListener, Activity activity)
        {
            _activityListener = activityListener;
            _activity = activity;
        }

        public void Dispose()
        {
            if (_activity != null)
            {
                _activityListener.Write("Stop", _activity);
            }
        }
    }
}