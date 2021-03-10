using Arctic.AppSeqs;
using NHibernate;
using Serilog;
using Swm.Locations;
using Swm.Palletization;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static NSubstitute.Substitute;

namespace Swm.TransportTasks.Tests
{
    using static TestDataUtil;

    public class TaskHelperTest
    {

        public TaskHelperTest(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.TestOutput(output)
                .CreateLogger();
        }

        [Fact]
        public async Task 生成任务_如果起点和终点相同_则会抛出异常()
        {
            Location start = NewK();
            Location end = start;
            Unitload unitload = new Unitload();
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            const string taskType = "T";

            var ex = await Assert.ThrowsAsync<FailToBuildTaskException>(() => taskHelper.BuildAsync(task, taskType, start, end, unitload));
            Assert.Equal(FailtoBuildTaskReason.StartAndEndArdTheSame, ex.Reason);
        }

        [Fact]
        public async Task 生成任务_如果在一个货载上生成两个任务_则会抛出异常()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload = new Unitload();
            TransportTask task = new TransportTask();
            TransportTask task2 = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            const string taskType = "T";

            await taskHelper.BuildAsync(task, taskType, start, end, unitload);

            var ex = await Assert.ThrowsAsync<FailToBuildTaskException>(() => taskHelper.BuildAsync(task2, taskType, start, end, unitload));
            Assert.Equal(FailtoBuildTaskReason.UnitloadBeingMoved, ex.Reason);
        }

        [Fact]
        public async Task 生成任务_如果超过终点的入站数限制_则会抛出异常()
        {
            var start = NewK();
            var end = NewS();
            Unitload unitload = new Unitload();
            Unitload unitload2 = new Unitload();
            TransportTask task = new TransportTask();
            TransportTask task2 = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            const string taskType = "T";

            await taskHelper.BuildAsync(task, taskType, start, end, unitload);

            var ex = await Assert.ThrowsAsync<FailToBuildTaskException>(() => taskHelper.BuildAsync(task2, taskType, start, end, unitload2));
            Assert.Equal(FailtoBuildTaskReason.InboundLimitReached, ex.Reason);
        }

        [Fact]
        public async Task 生成任务_如果超过起点的出站数限制_则会抛出异常()
        {
            var start = NewK();
            start.OutboundLimit = 1;
            var end = NewS();
            Unitload unitload = new Unitload();
            Unitload unitload2 = new Unitload();
            TransportTask task1 = new TransportTask();
            TransportTask task2 = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            const string taskType = "T";

            await taskHelper.BuildAsync(task1, taskType, start, end, unitload);

            var ex = await Assert.ThrowsAsync<FailToBuildTaskException>(() => taskHelper.BuildAsync(task2, taskType, start, end, unitload2));
            Assert.Equal(FailtoBuildTaskReason.OutboundLimitReached, ex.Reason);
        }

        [Fact]
        public async Task 生成任务_起点的出站数会增加()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload = new Unitload();
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            const string taskType = "T";

            await taskHelper.BuildAsync(task, taskType, start, end, unitload);

            Assert.Equal(1, start.OutboundCount);
        }

        [Fact]
        public async Task 生成任务_终点的入站数会增加()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload = new Unitload();
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            const string taskType = "T";

            await taskHelper.BuildAsync(task, taskType, start, end, unitload);

            Assert.Equal(1, end.InboundCount);
        }

        [Fact]
        public async Task 生成任务_检查任务属性()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload = new Unitload();
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            const string taskType = "T";

            await taskHelper.BuildAsync(task, taskType, start, end, unitload);

            Assert.Same(start, task.Start);
            Assert.Same(end, task.End);
            Assert.Same(unitload, task.Unitload);
            Assert.Equal(taskType, task.TaskType);
        }

        [Fact]
        public async Task 生成任务_货载会被标记为正在移动()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload = new Unitload();
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            const string taskType = "T";

            await taskHelper.BuildAsync(task, taskType, start, end, unitload);

            Assert.True(unitload.BeingMoved);
        }

        [Fact]
        public async Task 生成任务_显式指定ForWcs参数_检查任务的ForWcs属性()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload1 = new Unitload();
            Unitload unitload2 = new Unitload();
            TransportTask task1 = new TransportTask();
            TransportTask task2 = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            const string taskType = "T";

            await taskHelper.BuildAsync(task1, taskType, start, end, unitload1, true);
            await taskHelper.BuildAsync(task2, taskType, start, end, unitload2, false);

            Assert.True(task1.ForWcs);
            Assert.False(task2.ForWcs);
        }

        [Fact]
        public async Task 生成任务_如果起点禁止出站_则不可生成Wcs任务()
        {
            var start = NewK();
            start.OutboundDisabled = true;
            var end = NewK();
            Unitload unitload1 = new Unitload();
            TransportTask task1 = new TransportTask();
            TransportTask task2 = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            var ex = await Assert.ThrowsAsync<FailToBuildTaskException>(() => taskHelper.BuildAsync(task1, "T", start, end, unitload1));
            Assert.Equal(FailtoBuildTaskReason.OutboundDisabled, ex.Reason);
        }

        [Fact]
        public async Task 生成任务_如果终点禁止入站_则不可生成WCS任务()
        {
            var start = NewK();
            var end = NewK();
            end.InboundDisabled = true;
            Unitload unitload1 = new Unitload();
            TransportTask task1 = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            var ex = await Assert.ThrowsAsync<FailToBuildTaskException>(() => taskHelper.BuildAsync(task1, "T", start, end, unitload1));

            Assert.Equal(FailtoBuildTaskReason.InboundDisabled, ex.Reason);
        }

        [Fact]
        public async Task 生成任务_如果起点禁止出站_则可以生成非WCS任务()
        {
            var start = NewK();
            start.OutboundDisabled = true;
            var end = NewK();
            Unitload unitload1 = new Unitload();
            TransportTask task1 = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            await taskHelper.BuildAsync(task1, "T", start, end, unitload1, false);
        }

        [Fact]
        public async Task 生成任务_如果终点禁止入站_则可以生成非WCS任务()
        {
            var start = NewK();
            var end = NewK();
            end.InboundDisabled = true;
            Unitload unitload1 = new Unitload();
            TransportTask task1 = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            await taskHelper.BuildAsync(task1, "T", start, end, unitload1, false);
        }

        [Fact]
        public async Task 生成任务_如果重复使用任务对象_则会抛出异常()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload1 = new Unitload();
            Unitload unitload2 = new Unitload();
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            await taskHelper.BuildAsync(task, "test", start, end, unitload1);

            await Assert.ThrowsAsync<ArgumentException>(() => taskHelper.BuildAsync(task, "test", start, end, unitload2));
        }

        [Fact]
        public async Task 生成任务_如果货载在非N位置上_则任务起点必须与货载当前位置一致()
        {
            var start = NewK();
            var end = NewS();
            Unitload unitload = new Unitload();
            unitload.Enter(NewK());
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            var ex = await Assert.ThrowsAsync<FailToBuildTaskException>(() => taskHelper.BuildAsync(task, "test", start, end, unitload));

            Assert.Equal(FailtoBuildTaskReason.InvalidStart, ex.Reason);
        }

        [Fact]
        public async Task 生成任务_如果货载在N位置上_则任务起点可以为K位置()
        {
            var start = NewK();
            var end = NewS();
            var n = NewN();
            Unitload unitload = new Unitload();
            unitload.Enter(n);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            await taskHelper.BuildAsync(task, "A", start, end, unitload);

            Assert.Same(n, unitload.CurrentLocation);
            Assert.Same(start, task.Start);
            Assert.Equal(0, n.UnitloadCount);
            Assert.Equal(0, n.OutboundCount);
            Assert.Equal(1, start.OutboundCount);
        }

        [Fact]
        public async Task 生成任务_如果货载在N位置上_则任务起点不可以S位置()
        {
            var start = NewS();
            var end = NewK();
            var n = NewN();
            Unitload unitload = new Unitload();
            unitload.Enter(n);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            var ex = await Assert.ThrowsAsync<FailToBuildTaskException>(() => taskHelper.BuildAsync(task, "A", start, end, unitload));

            Assert.Equal(FailtoBuildTaskReason.InvalidStart, ex.Reason);
        }


        [Fact]
        public async Task 生成任务_如果以N位置作为起点生成WCS任务_则会抛出异常()
        {
            var start = NewN();
            var end = NewK();
            var n = NewN();
            Unitload unitload = new Unitload();
            unitload.Enter(n);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            var ex = await Assert.ThrowsAsync<FailToBuildTaskException>(() => taskHelper.BuildAsync(task, "A", start, end, unitload));

            Assert.Equal(FailtoBuildTaskReason.NForWcsTask, ex.Reason);
        }

        [Fact]
        public async Task 生成任务_如果以N位置作为终点生成WCS任务_则会抛出异常()
        {
            var start = NewK();
            var end = NewN();
            Unitload unitload = new Unitload();
            unitload.Enter(start);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            var ex = await Assert.ThrowsAsync<FailToBuildTaskException>(() => taskHelper.BuildAsync(task, "A", start, end, unitload));

            Assert.Equal(FailtoBuildTaskReason.NForWcsTask, ex.Reason);
        }

        [Fact]
        public async Task 生成任务_如果以N位置作为起点生成非WCS任务_则不会抛出异常()
        {
            var start = NewN();
            var end = NewK();
            Unitload unitload = new Unitload();
            unitload.Enter(start);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            await taskHelper.BuildAsync(task, "A", start, end, unitload, false);
        }

        [Fact]
        public async Task 生成任务_如果以N位置作为终点生成非WCS任务_则不会抛出异常()
        {
            var start = NewK();
            var end = NewN();
            Unitload unitload = new Unitload();
            unitload.Enter(start);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            await taskHelper.BuildAsync(task, "A", start, end, unitload, false);
        }

        [Fact]
        public async Task 完成任务_起点的出站数会减少()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload = new Unitload();
            unitload.Enter(start);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            await taskHelper.BuildAsync(task, "A", start, end, unitload);

            await taskHelper.CompleteAsync(task, end);

            Assert.Equal(0, start.OutboundCount);
        }

        [Fact]
        public async Task 完成任务_终点的入站数会减少()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload = new Unitload();
            unitload.Enter(start);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);

            await taskHelper.BuildAsync(task, "A", start, end, unitload);

            await taskHelper.CompleteAsync(task, end);

            Assert.Equal(0, end.InboundCount);
        }

        [Fact]
        public async Task 完成任务_检查载属性()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload = new Unitload();
            unitload.Enter(start);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            await taskHelper.BuildAsync(task, "A", start, end, unitload);

            await taskHelper.CompleteAsync(task, end);

            Assert.False(unitload.BeingMoved);
            Assert.Same(end, unitload.CurrentLocation);
        }


        [Fact]
        public async Task 完成任务_如果指定了实际终点_则货载被放在指定的位置上()
        {
            var start = NewK();
            var end = NewS();
            var actualEnd = NewS();
            Unitload unitload = new Unitload();
            unitload.Enter(start);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            await taskHelper.BuildAsync(task, "A", start, end, unitload);

            await taskHelper.CompleteAsync(task, actualEnd);

            Assert.Same(actualEnd, unitload.CurrentLocation);
        }


        [Fact]
        public async Task 取消任务_终点的入站数会减少()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload = new Unitload();
            unitload.Enter(start);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            await taskHelper.BuildAsync(task, "A", start, end, unitload);

            await taskHelper.CancelAsync(task);

            Assert.Equal(0, end.InboundCount);
        }

        [Fact]
        public async Task 取消任务_起点的出站数会减少()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload = new Unitload();
            unitload.Enter(start);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            await taskHelper.BuildAsync(task, "A", start, end, unitload);

            await taskHelper.CancelAsync(task);

            Assert.Equal(0, start.OutboundCount);
        }

        [Fact]
        public async Task 取消任务_检查货载属性()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload = new Unitload();
            unitload.Enter(start);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            await taskHelper.BuildAsync(task, "A", start, end, unitload);

            await taskHelper.CancelAsync(task);

            Assert.False(unitload.BeingMoved);
        }

        [Fact]
        public async Task 取消任务_如果起点不是N位置_则货载保持在原位置上()
        {
            var start = NewK();
            var end = NewK();
            Unitload unitload = new Unitload();
            unitload.Enter(start);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            await taskHelper.BuildAsync(task, "A", start, end, unitload);

            await taskHelper.CancelAsync(task);

            Assert.Same(start, unitload.CurrentLocation);
        }


        [Fact]
        public async Task 取消任务_如果货载在N位置_则取消后货载保持在N位置上()
        {
            var start = NewK();
            var end = NewK();
            var n = NewN();
            Unitload unitload = new Unitload();
            unitload.Enter(n);
            TransportTask task = new TransportTask();
            TaskHelper taskHelper = new TaskHelper(For<ISession>(),
                                                   For<IAppSeqService>(),
                                                   new UnitloadSnapshopHelper(new DefaultUnitloadSnapshotFactory(), Log.Logger),
                                                   Log.Logger);
            await taskHelper.BuildAsync(task, "A", start, end, unitload);

            await taskHelper.CancelAsync(task);

            Assert.Same(n, unitload.CurrentLocation);
        }

    }

}
