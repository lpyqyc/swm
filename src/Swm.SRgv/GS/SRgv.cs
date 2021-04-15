using Serilog;
using Swm.Device;
using Swm.Device.Rgv;
using Swm.Device.SRgv;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Swm.SRgv
{
    /// <summary>
    /// 单工位 RGV
    /// </summary>
    public class SRgv
    {
        readonly IDeviceTaskNoGenerator _deviceTaskNoGenerator;
        readonly ILogger _logger;
        readonly ICommunicator<SRgvDirective, SRgvState> _rgvCommunicator;

        // TODO 改为可配置
        public static readonly TimeSpan RespondTimeout = TimeSpan.FromMilliseconds(30000);


        public SRgv(string name, IDeviceTaskNoGenerator deviceTaskNoGenerator, ICommunicator<SRgvDirective, SRgvState> rgvCommunicator, ILogger logger)
        {
            this.Name = name;
            _deviceTaskNoGenerator = deviceTaskNoGenerator;
            _rgvCommunicator = rgvCommunicator;
            _logger = logger;
            _rgvCommunicator.StateMessageReceived += _rgvCommunicator_StateMessageReceived;
            _rgvCommunicator.Disconnected += _rgvCommunicator_Disconnected;
        }

        private void _rgvCommunicator_Disconnected(object? sender, EventArgs e)
        {
            CurrentState = null;
        }

        private void _rgvCommunicator_StateMessageReceived(object? sender, SRgvState e)
        {
            SetState(e);
        }



        public string Name { get; init; }

        /// <summary>
        /// 在设备上报的状态发送变化时引发。如果设备连续发送两个相同的状态，则不引发此事件。
        /// </summary>
        public event EventHandler<SRgvStateChangedEventArgs>? RgvStateChanged;

        internal void OnRgvStateChanged(SRgvStateChangedEventArgs e)
        {
            Console.WriteLine(e.NewState);
            RgvStateChanged?.Invoke(this, e);
        }

        public Task ConnectAsync()
        {
            return _rgvCommunicator.ConnectAsync();
        }

        public async Task DisconnectAsync()
        {
            await _rgvCommunicator.DisconnectAsync();
        }

        private void SetState(SRgvState? newState)
        {
            if (Equals(CurrentState?.Value, newState))
            {
                CurrentState = new Timestamped<SRgvState?>(newState, DateTime.Now);
                return;
            }
            SRgvStateChangedEventArgs e = new SRgvStateChangedEventArgs(CurrentState?.Value, newState);
            CurrentState = new Timestamped<SRgvState?>(newState, DateTime.Now);
            OnRgvStateChanged(e);
        }

        /// <summary>
        /// 当前状态，刚启动时为 null
        /// </summary>
        public Timestamped<SRgvState?>? CurrentState { get; private set; }


        /// <summary>
        /// 获取正在下发、尚未确定是否成功的指令，下发成功或者失败后设为 null。此属性用于避免下发重叠的指令。
        /// </summary>
        public Timestamped<SRgvDirective>? PendingDirective { get; private set; }


        /// <summary>
        /// 无货行走
        /// </summary>
        /// <param name="toStation"></param>
        /// <returns></returns>
        public async Task<int> WalkWithoutPalletAsync(int toStation)
        {
            var station = this.CurrentState?.Value?.StationNo;
            if (station == null)
            {
                throw new InvalidOperationException("不在站点");
            }

            var taskNo = _deviceTaskNoGenerator.GetNextTaskNo();
            var directive = new SRgvDirective.SendTask(
                new SRgvTaskInfo.Walk.WithoutPallet(taskNo, toStation)
                );

            await SendDirectiveAsync(directive, RespondTimeout);
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
            var station = this.CurrentState?.Value?.StationNo;
            if (station == null)
            {
                throw new InvalidOperationException("不在站点");
            }

            var taskNo = _deviceTaskNoGenerator.GetNextTaskNo();
            var directive = new SRgvDirective.SendTask(
                new SRgvTaskInfo.Walk.WithPallet(taskNo, palletCode, toStation)
                );

            await SendDirectiveAsync(directive, RespondTimeout);
            return taskNo;
        }

        private async Task<int> ConveyAsync(RgvConveyingType conveyingType, string palletCode)
        {
            var station = this.CurrentState?.Value?.StationNo;
            if (station == null)
            {
                throw new InvalidOperationException("不在站点");
            }

            var taskNo = _deviceTaskNoGenerator.GetNextTaskNo();
            var directive = new SRgvDirective.SendTask(
                new SRgvTaskInfo.Convey(taskNo, conveyingType, palletCode, station.Value)
                );

            await SendDirectiveAsync(directive, RespondTimeout);
            return taskNo;
        }


        /// <summary>
        /// 左取货
        /// </summary>
        /// <param name="palletCode"></param>
        /// <returns></returns>
        public Task<int> LeftLoadAsync(string palletCode)
        {
            return ConveyAsync(RgvConveyingType.LeftLoad, palletCode);
        }

        /// <summary>
        /// 左放货
        /// </summary>
        /// <param name="palletCode"></param>
        /// <returns></returns>
        public Task<int> LeftUnloadAsync(string palletCode)
        {
            return ConveyAsync(RgvConveyingType.LeftUnload, palletCode);
        }


        /// <summary>
        /// 右取货
        /// </summary>
        /// <param name="palletCode"></param>
        /// <returns></returns>
        public Task<int> RightLoadAsync(string palletCode)
        {
            return ConveyAsync(RgvConveyingType.RightLoad, palletCode);
        }

        /// <summary>
        /// 右放货
        /// </summary>
        /// <param name="palletCode"></param>
        /// <returns></returns>
        public Task<int> RightUnloadAsync(string palletCode)
        {
            return ConveyAsync(RgvConveyingType.RightUnload, palletCode);
        }

        /// <summary>
        /// 清除任务
        /// </summary>
        /// <returns></returns>
        public async Task ClearTaskAsync()
        {
            var directive = new SRgvDirective.ClearTask();
            await SendDirectiveAsync(directive, RespondTimeout);
        }

        // TODO 重试逻辑
        /// <summary>
        /// 
        /// </summary>
        /// <param name="directive">要下发的指令</param>
        /// <param name="timeout">超时时间</param>
        /// <exception cref="NotRespondedException">在超时时间内未收到成功回复</exception>
        /// <returns></returns>
        private async Task SendDirectiveAsync(SRgvDirective directive, TimeSpan timeout)
        {
            _logger.Debug("正在下发指令 {directive}", directive);

            if (directive.IsSuccessfulResponse(this.CurrentState?.Value))
            {
                _logger.Warning("不需下发的指令");
                return;
            }

            if (PendingDirective != null)
            {
                throw new CannotSendDirectiveException(new List<string> { "有一个挂起的指令" });
            }

            if (CanSendDirective(directive, out List<string> errors) == false)
            {
                // TODO 执行方式需要循环执行
                throw new CannotSendDirectiveException(errors);
            }

            try
            {
                DateTime time = DateTime.Now;
                PendingDirective = new Timestamped<SRgvDirective>(directive, time);
                _logger.Verbose("将 PendingDirective 属性设置为了 {PendingDirective}", PendingDirective);
                await _rgvCommunicator.SendDirectiveAsync(directive);
                _logger.Debug("已向网络写入指令数据");

                while (true)
                {
                    await Task.Delay(1);
                    _logger.Verbose("轮询 CurrentState 属性判断设备是否已成功响应 {CurrentState}", CurrentState);
                    if (directive.IsSuccessfulResponse(CurrentState?.Value))
                    {
                        _logger.Debug("收到设备的成功响应");
                        break;
                    }
                    if (_rgvCommunicator.ConnectionState == DeviceConnectionState.Disconnected)
                    {
                        _logger.Debug("已和设备断开连接");
                        break;
                    }

                    if (DateTime.Now.Subtract(time) > timeout)
                    {
                        throw new NotRespondedException();
                    }
                }
            }
            finally
            {
                PendingDirective = null;
            }
        }

        public CommunicatorStatistics Statistics => _rgvCommunicator.Statistics;

        public bool CanSendDirective(SRgvDirective directive, out List<string> errors)
        {
            errors = new List<string>();
            if (this.ConnectionState != DeviceConnectionState.Connected)
            {
                errors.Add("未连接到设备");
                return false;
            }

            switch (directive)
            {
                case SRgvDirective.Inquire:
                    {
                        return true;
                    }
                case SRgvDirective.SendTask:
                    {
                        if (CurrentState == null || CurrentState.Value == null)
                        {
                            errors.Add("设备状态未知");
                            return false;
                        }

                        //设备处于手动模式
                        if (CurrentState.Value.InManualMode)
                        {
                            errors.Add("设备处于手动模式");
                        }

                        if (CurrentState.Value.HasError)
                        {
                            errors.Add("设备报错");
                        }

                        if (CurrentState.Value.HasTask)
                        {
                            errors.Add("设备已有任务");
                        }

                        return errors.Count == 0;
                    }
                case SRgvDirective.ClearError:
                    {
                        if (CurrentState == null || CurrentState.Value == null)
                        {
                            errors.Add("设备状态未知");
                            return false;
                        }

                        if (CurrentState.Value.HasError == false)
                        {
                            errors.Add("设备无错误");
                        }

                        return errors.Count == 0;
                    }
                case SRgvDirective.ClearTask:
                    {
                        if (CurrentState == null || CurrentState.Value == null)
                        {
                            errors.Add("设备状态未知");
                            return false;
                        }

                        if (CurrentState.Value.HasTask == false)
                        {
                            errors.Add("设备无任务");
                        }

                        return errors.Count == 0;
                    }
                default:
                    throw new();
            };
        }


        public DeviceConnectionState ConnectionState
        {
            get
            {
                return _rgvCommunicator.ConnectionState;
            }
        }

        public async Task ShutdownAsync()
        {
            await _rgvCommunicator.ShutdownAsync();
        }
    }


    /// <summary>
    /// 表示带时间戳的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public record Timestamped<T>(T Value, DateTime Time);
}

