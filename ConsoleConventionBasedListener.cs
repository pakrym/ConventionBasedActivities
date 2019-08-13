// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConventionBasedActivities
{
    public class ConsoleConventionBasedListener: IDisposable, IObserver<KeyValuePair<string, object>>, IObserver<DiagnosticListener>
    {
        private const string ActivitySuffix = ".Activities";

        private readonly string[] _categories;
        
        private readonly List<IDisposable> _subscription;
        private readonly ConcurrentDictionary<Activity, ConsoleSpan> _activeSpans;

        public ConsoleConventionBasedListener(params string[] categories)
        {
            _subscription = new List<IDisposable>();
            _activeSpans = new ConcurrentDictionary<Activity, ConsoleSpan>();

            _categories = categories;
            
            _subscription.Add(DiagnosticListener.AllListeners.Subscribe(this));
        }

        public void Dispose()
        {
            foreach (var disposable in _subscription)
            {
                disposable.Dispose();
            }
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener diagnosticListener)
        {
            if (ShouldSubscribe(diagnosticListener))
            {
                _subscription.Add(diagnosticListener.Subscribe(this));
            }
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            switch (value.Key)
            {
                case "Start":
                    OnStartActivity((Activity)value.Value);
                    break;
                case "Stop":
                    OnStopActivity((Activity)value.Value);
                    break;
                case "SetColor":
                    var tuple = ((ValueTuple<Activity, ConsoleColor>)value.Value);
                    OnSetColor(tuple.Item1, tuple.Item2);
                    break;
            }
        }

        private bool ShouldSubscribe(DiagnosticListener diagnosticListener)
        {
            if (!diagnosticListener.Name.EndsWith(ActivitySuffix))
            {
                return false;
            }

            var noSuffixName = diagnosticListener.Name.Substring(0, diagnosticListener.Name.Length - ActivitySuffix.Length);
            foreach (var category in _categories)
            {
                if (category == noSuffixName)
                {
                    return true;
                }

                if (category.EndsWith("*") && noSuffixName.StartsWith(category.Substring(0, category.Length - 1)))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnSetColor(Activity activity, ConsoleColor color)
        {
            if (activity != null)
            {                
                var span = GetSpan(activity);
                span.Color = color;
            }
        }

        private void OnStopActivity(Activity activity)
        {
            if (_activeSpans.TryGetValue(activity, out var span))
            {
                Console.ForegroundColor = span.Color;
                Console.WriteLine("<" + span.Title + " " + span.Tags);
            }
        }

        private void OnStartActivity(Activity activity)
        {
            var span = GetSpan(activity);

            span.Title = activity.Tags.SingleOrDefault(t => t.Key == "title").Value ?? activity.OperationName;
            span.Tags = string.Join(" ", activity.Tags);

            
            Console.ForegroundColor = span.Color;
            Console.WriteLine(">" + span.Title);
        }

        private ConsoleSpan GetSpan(Activity activity)
        {
            return _activeSpans.AddOrUpdate(activity, new ConsoleSpan(), (_, consoleSpan) => consoleSpan);
        }

        internal class ConsoleSpan
        {
            public string Title { get; set; }
            public string Tags { get; set; }
            public ConsoleColor Color { get; set; } = ConsoleColor.White;
        }
    }
}