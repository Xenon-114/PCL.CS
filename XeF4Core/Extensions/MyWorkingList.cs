using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XeF4Core;

/// <summary>
/// 工作任务管理
/// </summary>
public class MyWorkingList
{
    /// <summary>
    /// 异步任务调度限制。默认16
    /// </summary>
    public int AsyncWorkLimit { get; set; } = 16;
    private Queue<WorkApply> WorkApplies { get; } = new();
    private int WorkingTaskCounter = 0;
    /// <summary>
    /// 工作数
    /// </summary>
    public int WorkingTaskCount => WorkingTaskCounter;
    private readonly object Locker = new();
    /// <summary>
    /// 用于检查任务限制的函数，返回true会继续分配任务。不重写会使用默认逻辑。
    /// </summary>
    public WorkingListLimitChecker? LimitChecker { get; set; } = null;
    private HashSet<WorkApply> WorkingTasks { get; } = new();
    private bool CanAddWorker() => LimitChecker?.Invoke(WorkingTaskCounter) ?? WorkingTaskCounter < AsyncWorkLimit;
    /// <summary>
    /// 申请一个任务
    /// </summary>
    /// <returns></returns>
    public WorkApply Apply()
    {
        WorkApply apply;
        lock (Locker)
        {
            apply = new(this);
            WorkApplies.Enqueue(apply);
            RefreshAsyncWork();
        }
        return apply;
    }
    /// <summary>
    /// 刷新异步任务
    /// </summary>
    public void RefreshAsyncWork()
    {
        lock (Locker)
        {
            while (true)
            {
                while (WorkApplies.Count > 0 && WorkApplies.Peek().IsFinished) WorkApplies.Dequeue();
                if (CanAddWorker() && WorkApplies.Count > 0)
                {
                    WorkApply apply = WorkApplies.Dequeue();
                    WorkingTaskCounter++;
                    WorkingTasks.Add(apply);
                    apply.Waiter.TrySetResult(null);
                }
                else break;
            }
        }
    }
    internal void Finish(WorkApply apply)
    {
        lock (Locker)
        {
            if (WorkingTasks.Contains(apply))
            {
                WorkingTaskCounter--;
                WorkingTasks.Remove(apply);
            }
            RefreshAsyncWork();
        }
        
    }
}
/// <summary>
/// 表示检查任务限制使用的函数
/// </summary>
/// <param name="WorkingTaskCount">当前内部正在运行的任务的数量</param>
/// <returns>返回一个值，表示是否允许继续分派任务</returns>
public delegate bool WorkingListLimitChecker(int WorkingTaskCount);
/// <summary>
/// 工作任务申请
/// </summary>
public class WorkApply : ITask
{
    internal WorkApply(MyWorkingList myWorkingList)
    {
        Owner = myWorkingList;
    }
    internal readonly TaskCompletionSource<object?> Waiter = new();
    private MyWorkingList? Owner;
    /// <inheritdoc/>
    public TaskAwaiter GetAwaiter() => (Waiter.Task as Task).GetAwaiter();
    /// <inheritdoc/>
    public void Wait() => Waiter.Task.Wait();
    /// <inheritdoc/>
    public Task WaitAsync() => Waiter.Task;
    internal bool IsFinished = false;
    /// <summary>
    /// 完成该任务
    /// </summary>
    public void Finish()
    {
        if (IsFinished) return;
        IsFinished = true;
        Waiter.TrySetResult(null);
        Owner?.Finish(this);
        Owner = null;
    }
}
