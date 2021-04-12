using Serilog;
using Swm.Device;
using Swm.Device.Rgv;
using Swm.Device.SRgv;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Swm.SRgv.GS
{
    /// <summary>
    /// 单工位 RGV
    /// </summary>
    public class SRgv
    {
        IDeviceTaskNoGenerator _deviceTaskNoGenerator;

        // TODO 改为可配置
        public static readonly TimeSpan RespondTimeout = TimeSpan.FromMilliseconds(30000);
        readonly ICommunicator<SRgvDirective, SRgvState> _rgvCommunicator;

        readonly ILogger _logger;

        public SRgv(IDeviceTaskNoGenerator deviceTaskNoGenerator, ICommunicator<SRgvDirective, SRgvState> rgvCommunicator, ILogger logger)
        {
            _deviceTaskNoGenerator = deviceTaskNoGenerator;
            _rgvCommunicator = rgvCommunicator;
            _logger = logger;
            _rgvCommunicator.StateReceived += _rgvCommunicator_StateReceived;
        }

        private void _rgvCommunicator_StateReceived(object? sender, SRgvState e)
        {
            // TODO：rgvCommunicator_MessageReceived 是否使用 async
            // 1，如果使用 async 标记，那么一旦某个报文的处理程序卡住，则后到的报文会先处理完成，出现错误的次序
            // 2，如果不使用 async 标记，那么一旦某个报文的处理程序卡住，则其他设备会一起卡住收不到报文

            //if (this.Name == "穿梭车1")
            //{
            //    Console.WriteLine(ReceivedMessageCount);
            //    if (random.NextDouble() < 0.2)
            //    {
            //        int x = Interlocked.Increment(ref i);

            //        Console.WriteLine($"开始处理 {x}");
            //        int d = random.Next(100, 1500);
            //        await Task.Delay(d);
            //        Console.WriteLine($"处理完成 {x}, 耗时 {d}");
            //    }
            //}

            SetState(e);
        }



        public string Name { get; set; }


        public event EventHandler<SRgvStateChangedEventArgs> RgvStateChanged;

        internal void OnRgvStateChanged(SRgvStateChangedEventArgs e)
        {
            RgvStateChanged?.Invoke(this, e);
        }

        public Task ConnectAsync()
        {
            return _rgvCommunicator.ConnectAsync();
        }

        public Task DisconnectAsync()
        {
            return _rgvCommunicator.DisconnectAsync();
        }

        public void SetState(SRgvState state)
        {
            if (Equals(PreviousState, state))
            {
                return;
            }
            SRgvStateChangedEventArgs e = new SRgvStateChangedEventArgs(PreviousState, state);
            PreviousState = state;
            OnRgvStateChanged(e);
        }

        /// <summary>
        /// 上一个状态，刚启动时为 null
        /// </summary>
        public SRgvState? PreviousState { get; private set; }

        /// <summary>
        /// 当前状态，刚启动时为 null
        /// </summary>
        public SRgvState? CurrentState { get; private set; }

        /// <summary>
        /// 无货行走
        /// </summary>
        /// <param name="toStation"></param>
        /// <returns></returns>
        public async Task<int> WalkWithoutPalletAsync(int toStation)
        {
            var station = this.CurrentState?.StationNo;
            if (station == null)
            {
                throw new InvalidOperationException("不在站点");
            }

            var taskNo = _deviceTaskNoGenerator.GetNextTaskNo();
            var directive = new SRgvDirective.SendTask(
                new SRgvTaskInfo.WalkWithoutPallet(taskNo, null, toStation)
                );

            await SendAsync(directive, RespondTimeout);
            return taskNo;
        }

        /// <summary>
        /// 带货行走
        /// </summary>
        /// <param name="palletCode"></param>
        /// <param name="toStation"></param>
        /// <returns></returns>
        public async Task<int> WalkWithPalletAsync(string palletCode, int toStation)
        {
            var station = this.CurrentState?.StationNo;
            if (station == null)
            {
                throw new InvalidOperationException("不在站点");
            }

            var taskNo = _deviceTaskNoGenerator.GetNextTaskNo();
            var directive = new SRgvDirective.SendTask(
                new SRgvTaskInfo.WalkWithPallet(taskNo, palletCode, null, toStation)
                );

            await SendAsync(directive, RespondTimeout);
            return taskNo;
        }

        /// <summary>
        /// 左取货
        /// </summary>
        /// <param name="palletCode"></param>
        /// <returns></returns>
        public async Task<int> LeftLoadAsync(string palletCode)
        {
            var station = this.CurrentState?.StationNo;
            if (station == null)
            {
                throw new InvalidOperationException("不在站点");
            }

            var taskNo = _deviceTaskNoGenerator.GetNextTaskNo();
            var directive = new SRgvDirective.SendTask(
                new SRgvTaskInfo.LeftLoad(taskNo, palletCode, station.Value)
                );

            await SendAsync(directive, RespondTimeout);
            return taskNo;
        }


        /// <summary>
        /// 左放货
        /// </summary>
        /// <param name="palletCode"></param>
        /// <returns></returns>
        public async Task<int> LeftUnloadAsync(string palletCode)
        {
            var station = this.CurrentState?.StationNo;
            if (station == null)
            {
                throw new InvalidOperationException("不在站点");
            }

            var taskNo = _deviceTaskNoGenerator.GetNextTaskNo();
            var directive = new SRgvDirective.SendTask(
                new SRgvTaskInfo.LeftUnload(taskNo, palletCode, station.Value)
                );

            await SendAsync(directive, RespondTimeout);
            return taskNo;
        }


        /// <summary>
        /// 右取货
        /// </summary>
        /// <param name="palletCode"></param>
        /// <returns></returns>
        public async Task<int> RightLoadAsync(string palletCode)
        {
            var station = this.CurrentState?.StationNo;
            if (station == null)
            {
                throw new InvalidOperationException("不在站点");
            }

            var taskNo = _deviceTaskNoGenerator.GetNextTaskNo();
            var directive = new SRgvDirective.SendTask(
                new SRgvTaskInfo.RightLoad(taskNo, palletCode, station.Value)
                );

            await SendAsync(directive, RespondTimeout);
            return taskNo;
        }

        /// <summary>
        /// 右放货
        /// </summary>
        /// <param name="palletCode"></param>
        /// <returns></returns>
        public async Task<int> RightUnloadAsync(string palletCode)
        {
            var station = this.CurrentState?.StationNo;
            if (station == null)
            {
                throw new InvalidOperationException("不在站点");
            }

            var taskNo = _deviceTaskNoGenerator.GetNextTaskNo();
            var directive = new SRgvDirective.SendTask(
                new SRgvTaskInfo.RightUnload(taskNo, palletCode, station.Value)
                );

            await SendAsync(directive, RespondTimeout);
            return taskNo;
        }

        /// <summary>
        /// 清除任务
        /// </summary>
        /// <returns></returns>
        public async Task ClearTaskAsync()
        {
            var directive = new SRgvDirective.ClearTask();
            await SendAsync(directive, RespondTimeout);
        }

        // TODO 重试逻辑
        /// <summary>
        /// 
        /// </summary>
        /// <param name="directive">要下发的指令</param>
        /// <param name="timeout">超时时间</param>
        /// <exception cref="NotRespondedException">在超时时间内未收到成功回复</exception>
        /// <returns></returns>
        private async Task SendAsync(SRgvDirective directive, TimeSpan timeout)
        {
            _logger.Debug("正在下发指令：" + directive);

            if (directive.IsSuccessfulResponse(this.PreviousState))
            {
                _logger.Warning("不需下发的指令 {directive}", directive);
                return;
            }

            if (CanSendDirective(out CannotSendDirectiveReason reason) == false)
            {
                //TODO 执行方式需要循环执行
                throw new CannotSendDirectiveException(reason);
            }


            DateTime startTime = DateTime.Now;

            await _rgvCommunicator.SendDirectiveAsync(directive);
            _logger.Debug("已写入");

            while (true)
            {
                if (directive.IsSuccessfulResponse(PreviousState))
                {
                    _logger.Debug("已收到响应，当前状态：" + PreviousState);
                    //lock (Outlet)
                    //{
                    //    Outlet.State = DirectiveState.Successful;
                    //}
                    break;
                }

                if (DateTime.Now.Subtract(startTime) > timeout)
                {
                    //lock (Outlet)
                    //{
                    //    Outlet.State = DirectiveState.Timedout;
                    //}

                    throw new NotRespondedException();
                }

                Thread.Sleep(30);
            }
        }


        public bool CanSendDirective(out CannotSendDirectiveReason reason)
        {
            // TODO 设备锁概念
            reason = CannotSendDirectiveReason.可以下发指令;
            if (!this.IsConnected)
            {
                reason |= CannotSendDirectiveReason.未连接到设备;
            }
            //TODO 穿梭车下发取消指令货堆垛机下发急停指令时 设备本身可能存在任务
            //lock (Outlet)
            //{
            //    if (Outlet.Directive != null && Outlet.State == DirectiveState.Pending)
            //    {
            //        reason |= CannotSendDirectiveReason.有正在处理的指令;
            //    }
            //}

            var currentState = this.PreviousState;
            if (currentState == null)
            {
                reason |= CannotSendDirectiveReason.设备状态未知;
            }
            else
            {
                //设备处于手动模式
                if (currentState.InManualMode)
                {
                    reason |= CannotSendDirectiveReason.设备处于手动模式;
                }

                if (currentState.ErrorCode != null)
                {
                    reason |= CannotSendDirectiveReason.设备报警停机;
                }

                // TODO 防止 二次下发穿梭车任务
                //if (currentState.Event == SRgvEvent.TaskCompletionByManual || currentState.Event == SRgvEvent.AutomaticTaskCompletion)
                //{
                //    reason |= CannotSendDirectiveReason.任务手动或自动完成;
                //}
                //TODO 与上面提到的 <有正在处理的指令> 可能存在重复判断
                if (currentState.TaskInfo != null)
                {
                    reason |= CannotSendDirectiveReason.设备有任务;
                }

                if (currentState.ErrorCode != 0)
                {
                    reason |= CannotSendDirectiveReason.设备出错;
                }
            }
            return reason == CannotSendDirectiveReason.可以下发指令;
        }


        public bool IsConnected
        {
            get
            {
                return _rgvCommunicator.IsConnected;
            }
        }

    }
}

