using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XeF4Core;

/// <summary>
/// 表示一个可等待的任务
/// </summary>
public interface ITask
{
    /// <summary>
    /// await
    /// </summary>
    /// <returns></returns>
    public TaskAwaiter GetAwaiter();
    /// <summary>
    /// 同步等待任务完成
    /// </summary>
    public void Wait();
    /// <summary>
    /// 提供一个异步等待任务完成的方法
    /// </summary>
    /// <returns></returns>
    public Task WaitAsync();
}


/// <summary>
/// 表示一个自定义任务
/// </summary>
public interface IMyTask : ITask
{
    /// <summary>
    /// 任务进度
    /// </summary>
    public double Progress { get; }
    /// <summary>
    /// 任务名称
    /// </summary>
    public string? Name { get; }
}
/// <summary>
/// 表示一个自定义任务集合
/// </summary>
public interface IMyTaskList : IMyTask
{
    /// <summary>
    /// 子任务
    /// </summary>
    public IReadOnlyList<IMyTask> Children { get; }
}

/// <summary>
/// 自定义任务集合
/// </summary>
public class MyTaskList : IMyTaskList
{
    /// <summary>
    /// 子任务
    /// </summary>
    public List<IMyTask> Children { get; } = new();
    IReadOnlyList<IMyTask> IMyTaskList.Children => Children;
    /// <inheritdoc/>
    public double Progress
    {
        get
        {
            double progress = 0;
            foreach (IMyTask task in Children)
            {
                if (task.Progress is not double.NaN)
                    progress += task.Progress;
            }
            progress /= Children.Count;
            return progress;
        }
    }
    /// <inheritdoc/>
    public string? Name { get; }
    private Task Waiter
    {
        get
        {
            if (_w is null)
            {
                List<Task> tasks = new();
                foreach (IMyTask task in Children)
                {
                    tasks.Add(task.WaitAsync());
                }
                _w = Task.WhenAll(tasks);
            }
            return _w;
        }
    }
    private Task? _w = null;
    /// <inheritdoc/>
    public TaskAwaiter GetAwaiter() => Waiter.GetAwaiter();
    /// <inheritdoc/>
    public void Wait() => Waiter.Wait();
    /// <inheritdoc/>
    public Task WaitAsync() => Waiter;
}