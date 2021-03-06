﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Net.TPLExtentions
{
    using System.Threading;
    using Telegraphy.Net.TPLExtentions;

    public static class TaskExtentionMethods
    {
        #region Timeouts
        /// <summary>Creates a new Task that mirrors the supplied task but that will be canceled after the specified timeout.</summary>
        /// <typeparam name="TResult">Specifies the type of data contained in the task.</typeparam>
        /// <param name="task">The task.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The new Task that may time out.</returns>
        public static Task WithTimeout(this Task task, TimeSpan timeout)
        {
            var result = new TaskCompletionSource<object>(task.AsyncState);
            var timer = new Timer(state => ((TaskCompletionSource<object>)state).TrySetCanceled(), result, timeout, TimeSpan.FromMilliseconds(-1));
            task.ContinueWith(t =>
            {
                timer.Dispose();
                result.TrySetFromTask(t);
            }, TaskContinuationOptions.ExecuteSynchronously);
            return result.Task;
        }

        /// <summary>Creates a new Task that mirrors the supplied task but that will be canceled after the specified timeout.</summary>
        /// <typeparam name="TResult">Specifies the type of data contained in the task.</typeparam>
        /// <param name="task">The task.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The new Task that may time out.</returns>
        public static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            var result = new TaskCompletionSource<TResult>(task.AsyncState);
            var timer = new Timer(state => ((TaskCompletionSource<TResult>)state).TrySetCanceled(), result, timeout, TimeSpan.FromMilliseconds(-1));
            task.ContinueWith(t =>
            {
                timer.Dispose();
                result.TrySetFromTask(t);
            }, TaskContinuationOptions.ExecuteSynchronously);
            return result.Task;
        }
        #endregion

        #region Children
        /// <summary>
        /// Ensures that a parent task can't transition into a completed state
        /// until the specified task has also completed, even if it's not
        /// already a child task.
        /// </summary>
        /// <param name="task">The task to attach to the current task as a child.</param>
        public static void AttachToParent(this Task task)
        {
            if (task == null) throw new ArgumentNullException("task");
            task.ContinueWith(t => t.Wait(), CancellationToken.None,
                TaskContinuationOptions.AttachedToParent |
                TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }
        #endregion

        #region Exception Handling
        /// <summary>Suppresses default exception handling of a Task that would otherwise reraise the exception on the finalizer thread.</summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task IgnoreExceptions(this Task task)
        {
            task.ContinueWith(t => { var ignored = t.Exception; },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
            return task;
        }

        /// <summary>Suppresses default exception handling of a Task that would otherwise reraise the exception on the finalizer thread.</summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task<T> IgnoreExceptions<T>(this Task<T> task)
        {
            return (Task<T>)((Task)task).IgnoreExceptions();
        }

        /// <summary>Fails immediately when an exception is encountered.</summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task FailFastOnException(this Task task)
        {
            task.ContinueWith(t => Environment.FailFast("A task faulted.", t.Exception),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
            return task;
        }

        /// <summary>Fails immediately when an exception is encountered.</summary>
        /// <param name="task">The Task to be monitored.</param>
        /// <returns>The original Task.</returns>
        public static Task<T> FailFastOnException<T>(this Task<T> task)
        {
            return (Task<T>)((Task)task).FailFastOnException();
        }

        /// <summary>Propagates any exceptions that occurred on the specified task.</summary>
        /// <param name="task">The Task whose exceptions are to be propagated.</param>
        public static void PropagateExceptions(this Task task)
        {
            if (!task.IsCompleted) throw new InvalidOperationException("The task has not completed.");
            if (task.IsFaulted) task.Wait();
        }

        /// <summary>Propagates any exceptions that occurred on the specified tasks.</summary>
        /// <param name="task">The Tassk whose exceptions are to be propagated.</param>
        public static void PropagateExceptions(this Task[] tasks)
        {
            if (tasks == null) throw new ArgumentNullException("tasks");
            if (tasks.Any(t => t == null)) throw new ArgumentException("tasks");
            if (tasks.Any(t => !t.IsCompleted)) throw new InvalidOperationException("A task has not completed.");
            Task.WaitAll(tasks);
        }
        #endregion

        #region Waiting
//#if WIN32
// NOTE NEED TO INCLUDE WINDOWSBASE ref and 
// using System.Windows.Threading;
//        /// <summary>Waits for the task to complete execution, pumping in the meantime.</summary>
//        /// <param name="task">The task for which to wait.</param>
//        /// <remarks>This method is intended for usage with Windows Presentation Foundation.</remarks>
//        public static void WaitWithPumping(this Task task)
//        {
//            if (task == null) throw new ArgumentNullException("task");
//            var nestedFrame = new DispatcherFrame();
//            task.ContinueWith(_ => nestedFrame.Continue = false);
//            Dispatcher.PushFrame(nestedFrame);
//            task.Wait();
//        }
//#endif

        /// <summary>Waits for the task to complete execution, returning the task's final status.</summary>
        /// <param name="task">The task for which to wait.</param>
        /// <returns>The completion status of the task.</returns>
        /// <remarks>Unlike Wait, this method will not throw an exception if the task ends in the Faulted or Canceled state.</remarks>
        public static TaskStatus WaitForCompletionStatus(this Task task)
        {
            if (task == null) throw new ArgumentNullException("task");
            ((IAsyncResult)task).AsyncWaitHandle.WaitOne();
            return task.Status;
        }
        #endregion


        #region Then
        /// <summary>Creates a task that represents the completion of a follow-up action when a task completes.</summary>
        /// <param name="task">The task.</param>
        /// <param name="next">The action to run when the task completes.</param>
        /// <returns>The task that represents the completion of both the task and the action.</returns>
        public static Task Then(this Task task, Action next)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<object>();
            task.ContinueWith(delegate
            {
                if (task.IsFaulted) tcs.TrySetException(task.Exception.InnerExceptions);
                else if (task.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        next();
                        tcs.TrySetResult(null);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, TaskScheduler.Default);
            return tcs.Task;
        }

        /// <summary>Creates a task that represents the completion of a follow-up function when a task completes.</summary>
        /// <param name="task">The task.</param>
        /// <param name="next">The function to run when the task completes.</param>
        /// <returns>The task that represents the completion of both the task and the function.</returns>
        public static Task<TResult> Then<TResult>(this Task task, Func<TResult> next)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<TResult>();
            task.ContinueWith(delegate
            {
                if (task.IsFaulted) tcs.TrySetException(task.Exception.InnerExceptions);
                else if (task.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        var result = next();
                        tcs.TrySetResult(result);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, TaskScheduler.Default);
            return tcs.Task;
        }

        /// <summary>Creates a task that represents the completion of a follow-up action when a task completes.</summary>
        /// <param name="task">The task.</param>
        /// <param name="next">The action to run when the task completes.</param>
        /// <returns>The task that represents the completion of both the task and the action.</returns>
        public static Task Then<TResult>(this Task<TResult> task, Action<TResult> next)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<object>();
            task.ContinueWith(delegate
            {
                if (task.IsFaulted) tcs.TrySetException(task.Exception.InnerExceptions);
                else if (task.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        next(task.Result);
                        tcs.TrySetResult(null);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, TaskScheduler.Default);
            return tcs.Task;
        }

        /// <summary>Creates a task that represents the completion of a follow-up function when a task completes.</summary>
        /// <param name="task">The task.</param>
        /// <param name="next">The function to run when the task completes.</param>
        /// <returns>The task that represents the completion of both the task and the function.</returns>
        public static Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> task, Func<TResult, TNewResult> next)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<TNewResult>();
            task.ContinueWith(delegate
            {
                if (task.IsFaulted) tcs.TrySetException(task.Exception.InnerExceptions);
                else if (task.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    try
                    {
                        var result = next(task.Result);
                        tcs.TrySetResult(result);
                    }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, TaskScheduler.Default);
            return tcs.Task;
        }

        /// <summary>Creates a task that represents the completion of a second task when a first task completes.</summary>
        /// <param name="task">The first task.</param>
        /// <param name="next">The function that produces the second task.</param>
        /// <returns>The task that represents the completion of both the first and second task.</returns>
        public static Task Then(this Task task, Func<Task> next)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<object>();
            task.ContinueWith(delegate
            {
                // When the first task completes, if it faulted or was canceled, bail
                if (task.IsFaulted) tcs.TrySetException(task.Exception.InnerExceptions);
                else if (task.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    // Otherwise, get the next task.  If it's null, bail.  If not,
                    // when it's done we'll have our result.
                    try { next().ContinueWith(t => tcs.TrySetFromTask(t), TaskScheduler.Default); }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, TaskScheduler.Default);
            return tcs.Task;
        }

        /// <summary>Creates a task that represents the completion of a second task when a first task completes.</summary>
        /// <param name="task">The first task.</param>
        /// <param name="next">The function that produces the second task based on the result of the first task.</param>
        /// <returns>The task that represents the completion of both the first and second task.</returns>
        public static Task Then<T>(this Task<T> task, Func<T, Task> next)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<object>();
            task.ContinueWith(delegate
            {
                // When the first task completes, if it faulted or was canceled, bail
                if (task.IsFaulted) tcs.TrySetException(task.Exception.InnerExceptions);
                else if (task.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    // Otherwise, get the next task.  If it's null, bail.  If not,
                    // when it's done we'll have our result.
                    try { next(task.Result).ContinueWith(t => tcs.TrySetFromTask(t), TaskScheduler.Default); }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, TaskScheduler.Default);
            return tcs.Task;
        }

        /// <summary>Creates a task that represents the completion of a second task when a first task completes.</summary>
        /// <param name="task">The first task.</param>
        /// <param name="next">The function that produces the second task.</param>
        /// <returns>The task that represents the completion of both the first and second task.</returns>
        public static Task<TResult> Then<TResult>(this Task task, Func<Task<TResult>> next)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<TResult>();
            task.ContinueWith(delegate
            {
                // When the first task completes, if it faulted or was canceled, bail
                if (task.IsFaulted) tcs.TrySetException(task.Exception.InnerExceptions);
                else if (task.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    // Otherwise, get the next task.  If it's null, bail.  If not,
                    // when it's done we'll have our result.
                    try { next().ContinueWith(t => tcs.TrySetFromTask(t), TaskScheduler.Default); }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, TaskScheduler.Default);
            return tcs.Task;
        }

        /// <summary>Creates a task that represents the completion of a second task when a first task completes.</summary>
        /// <param name="task">The first task.</param>
        /// <param name="next">The function that produces the second task based on the result of the first.</param>
        /// <returns>The task that represents the completion of both the first and second task.</returns>
        public static Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> task, Func<TResult, Task<TNewResult>> next)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (next == null) throw new ArgumentNullException("next");

            var tcs = new TaskCompletionSource<TNewResult>();
            task.ContinueWith(delegate
            {
                // When the first task completes, if it faulted or was canceled, bail
                if (task.IsFaulted) tcs.TrySetException(task.Exception.InnerExceptions);
                else if (task.IsCanceled) tcs.TrySetCanceled();
                else
                {
                    // Otherwise, get the next task.  If it's null, bail.  If not,
                    // when it's done we'll have our result.
                    try { next(task.Result).ContinueWith(t => tcs.TrySetFromTask(t), TaskScheduler.Default); }
                    catch (Exception exc) { tcs.TrySetException(exc); }
                }
            }, TaskScheduler.Default);
            return tcs.Task;
        }
        #endregion
    }
}
