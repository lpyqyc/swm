using Arctic.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Threading.Tasks;

namespace Swm.Ops
{
    /// <summary>
    /// 提供操作记录相关的方法
    /// </summary>
    public class OpHelper
    {
        NHibernate.ISession _session;
        IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 初始化新实例
        /// </summary>
        /// <param name="session"></param>
        /// <param name="httpContextAccessor"></param>
        public OpHelper(NHibernate.ISession session, IHttpContextAccessor httpContextAccessor)
        {
            _session = session;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 当 action 使用 <see cref="OperationTypeAttribute"/> 标记时，根据 <see cref="OperationTypeAttribute.OperationType"/> 的值创建一个 Op 对象并保存到数据库。
        /// </summary>
        /// <param name="format">用于填充 <see cref="Op.Comment"/> 属性的格式化字符串。</param>
        /// <param name="args">用于填充 <see cref="Op.Comment"/> 属性的格式化参数。</param>
        /// <returns></returns>
        public async Task<Op> SaveOpAsync(string format, params object[] args)
        {
            Op op = new Op();
            op.OperationType = GetOperationType();
            op.Comment = string.Format(format, args);
            op.Url = _httpContextAccessor.HttpContext?.Request?.GetDisplayUrl();
            await _session.SaveAsync(op);

            return op;
        }

        /// <summary>
        /// 当 action 使用 <see cref="OperationTypeAttribute"/> 标记时，获取 <see cref="OperationTypeAttribute.OperationType"/> 的值。
        /// </summary>
        /// <returns></returns>
        public string? GetOperationType()
        {
            return (string?)_httpContextAccessor.HttpContext?.Items[typeof(OperationTypeAttribute)];
        }
    }

}