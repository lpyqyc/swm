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
        public async Task ��������_��������յ���ͬ_����׳��쳣()
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
        public async Task ��������_�����һ��������������������_����׳��쳣()
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
        public async Task ��������_��������յ����վ������_����׳��쳣()
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
        public async Task ��������_����������ĳ�վ������_����׳��쳣()
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
        public async Task ��������_���ĳ�վ��������()
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
        public async Task ��������_�յ����վ��������()
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
        public async Task ��������_�����������()
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
        public async Task ��������_���ػᱻ���Ϊ�����ƶ�()
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
        public async Task ��������_��ʽָ��ForWcs����_��������ForWcs����()
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
        public async Task ��������_�������ֹ��վ_�򲻿�����Wcs����()
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
        public async Task ��������_����յ��ֹ��վ_�򲻿�����WCS����()
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
        public async Task ��������_�������ֹ��վ_��������ɷ�WCS����()
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
        public async Task ��������_����յ��ֹ��վ_��������ɷ�WCS����()
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
        public async Task ��������_����ظ�ʹ���������_����׳��쳣()
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
        public async Task ��������_��������ڷ�Nλ����_����������������ص�ǰλ��һ��()
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
        public async Task ��������_���������Nλ����_������������ΪKλ��()
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
        public async Task ��������_���������Nλ����_��������㲻����Sλ��()
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
        public async Task ��������_�����Nλ����Ϊ�������WCS����_����׳��쳣()
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
        public async Task ��������_�����Nλ����Ϊ�յ�����WCS����_����׳��쳣()
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
        public async Task ��������_�����Nλ����Ϊ������ɷ�WCS����_�򲻻��׳��쳣()
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
        public async Task ��������_�����Nλ����Ϊ�յ����ɷ�WCS����_�򲻻��׳��쳣()
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
        public async Task �������_���ĳ�վ�������()
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
        public async Task �������_�յ����վ�������()
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
        public async Task �������_���������()
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
        public async Task �������_���ָ����ʵ���յ�_����ر�����ָ����λ����()
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
        public async Task ȡ������_�յ����վ�������()
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
        public async Task ȡ������_���ĳ�վ�������()
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
        public async Task ȡ������_����������()
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
        public async Task ȡ������_�����㲻��Nλ��_����ر�����ԭλ����()
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
        public async Task ȡ������_���������Nλ��_��ȡ������ر�����Nλ����()
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
