using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace RunAfterProviderNS;

public static class RunAfterProvider
{
    public static IObservable<long> RunAtOrAfter(DateTimeOffset dateTimeOffset, IScheduler scheduler)
    {
        return Observable
            .Timer(
                dueTime: dateTimeOffset,
                scheduler: scheduler
            )
            .Select(selector: l =>
                scheduler.Now < dateTimeOffset
                    ? RunAtOrAfter(
                        dateTimeOffset: dateTimeOffset,
                        scheduler: scheduler
                    )
                    : Observable.Return(value: l)
            )
            .Switch();
    }

    public static IObservable<long> RunInOrAfter(TimeSpan timeSpan, IScheduler scheduler)
    {
        return RunAtOrAfter(
            dateTimeOffset: scheduler.Now + timeSpan,
            scheduler: scheduler
        );
    }

    /// <summary>
    ///     todo understand how to pass <see cref="IScheduler"/>-like schedulers to `Task`-s and this method
    /// </summary>
    public static async Task RunInOrAfter(
        TimeSpan timeSpan,
        CancellationToken cancellationToken
    )
    {
        var utcNowSnapshot = DateTimeOffset.UtcNow;
        var finishTime = utcNowSnapshot + timeSpan;
        while (utcNowSnapshot < finishTime)
        {
            await Task.Delay(
                delay: finishTime - utcNowSnapshot,
                cancellationToken: cancellationToken
            );
            utcNowSnapshot = DateTimeOffset.UtcNow;
        }
    }
}